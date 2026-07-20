
Dưới góc độ của một Hacker (Red Team) hoặc Kỹ sư hệ thống, kiến trúc hiện tại có 3 lỗ hổng tử huyệt về mặt hiệu năng và bảo mật khi đưa lên môi trường Production:

Chai sạn I/O Database: Khi siêu thị đông khách, hàng chục máy PoS liên tục gọi API ScanBarcode. Nếu mỗi nhịp quét mã đều query trực tiếp xuống SQL Server, Database sẽ quá tải (Connection Pool Exhaustion).

DDoS / Brute-force API: Các Endpoint API mở ra ngoài internet rất dễ bị tấn công rải thảm làm sập băng thông.

Trải nghiệm người dùng (UX) đứt gãy: JWT có thời hạn ngắn (ví dụ: 1 tiếng). Nếu thu ngân đang tính tiền cho khách mà JWT hết hạn, họ sẽ bị văng ra màn hình đăng nhập.

Dưới đây là các giải pháp công nghệ cấp Enterprise để vá các lỗ hổng này, chuẩn bị nền tảng hạ tầng vững chắc nhất trước khi chúng ta kết nối với Front-end SPA (Next.js/React).

BUỔI 14: BẢO MẬT NÂNG CAO & TỐI ƯU HIỆU NĂNG TẦNG API (SECURITY & OPTIMIZATION)
Bước 1: Tối ưu API Quét mã vạch bằng IMemoryCache (In-Memory Caching)Chúng ta sẽ lưu trữ dữ liệu sản phẩm tĩnh (Tên, Giá, ĐVT) trực tiếp trên RAM của Server. Khi thu ngân quét mã, API sẽ bốc dữ liệu từ RAM với độ trễ $\mathcal{O}(1)$ (dưới 10ms) thay vì phải xuống ổ cứng truy vấn DB.Cập nhật Controllers/Api/PosController.cs (Hoặc ProductsApiController):C#using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory; // Thêm thư viện Caching
using DigiPOSE.Web.Models;

namespace DigiPOSE.Web.Controllers.Api
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class PosController : ControllerBase
    {
        private readonly DigiPoseDbContext _context;
        private readonly IMemoryCache _cache; // Inject MemoryCache

        public PosController(DigiPoseDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet("scan-barcode/{sku}")]
        public async Task<IActionResult> ScanBarcode(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku)) return BadRequest();

            string cacheKey = $"Product_SKU_{sku}";

            // 1. Tìm trong RAM trước. Nếu có, trả về ngay lập tức!
            if (_cache.TryGetValue(cacheKey, out object? cachedProduct))
            {
                return Ok(cachedProduct);
            }

            // 2. Nếu RAM không có (Cache Miss), mới truy vấn xuống Database
            var product = await _context.Products
                .Include(p => p.Unit)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.SKU == sku);

            if (product == null) 
                return NotFound(new { Message = "Không tìm thấy sản phẩm." });

            var productPayload = new
            {
                ProductId = product.ProductId,
                SKU = product.SKU,
                ProductName = product.ProductName,
                BasePrice = product.BasePrice,
                UnitName = product.Unit?.UnitName
            };

            // 3. Nạp dữ liệu vừa tìm được lên RAM, thiết lập thời gian sống (Absolute Expiration) là 4 tiếng
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(4));
            
            _cache.Set(cacheKey, productPayload, cacheOptions);

            return Ok(productPayload);
        }
    }
}
(Ghi chú: Cần đăng ký builder.Services.AddMemoryCache(); trong Program.cs).Bước 2: Triển khai Rate Limiting (Chống DDoS API)Từ .NET 8, Microsoft đã tích hợp sẵn Middleware Rate Limiting cực kỳ mạnh mẽ. Chúng ta sẽ thiết lập giới hạn: Mỗi địa chỉ IP chỉ được phép gọi tối đa 100 requests trong mỗi 10 giây. Vượt mức này, Server trả về mã lỗi 429 Too Many Requests.Mở file Program.cs và cấu hình:C#using Microsoft.AspNetCore.RateLimiting; // Bổ sung thư viện
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// ... Các cấu hình hiện tại

// 1. ĐĂNG KÝ RATE LIMITING
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    
    // Cấu hình Fixed Window (Cửa sổ cố định)
    options.AddFixedWindowLimiter("FixedPolicy", opt =>
    {
        opt.PermitLimit = 100; // Tối đa 100 requests
        opt.Window = TimeSpan.FromSeconds(10); // Trong vòng 10 giây
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2; // Hàng đợi chờ nếu vượt giới hạn
    });
});

var app = builder.Build();

// 2. KÍCH HOẠT MIDDLEWARE (Nên đặt sau UseRouting và trước UseAuthorization)
app.UseRouting();
app.UseRateLimiter(); 
// ...
Để áp dụng cho các API, Sếp chỉ cần thêm Annotation [EnableRateLimiting("FixedPolicy")] lên trên đỉnh của các ApiController.Bước 3: Cấu hình Cơ chế Refresh Token (Duy trì phiên làm việc không gián đoạn)Để bảo mật, JWT (Access Token) chỉ nên có tuổi thọ rất ngắn (ví dụ 30 phút). Khi Token chết, Frontend (Next.js) sẽ tự động dùng một "Refresh Token" (tuổi thọ 7 ngày) gửi ngầm xuống Server để xin một Access Token mới mà không bắt thu ngân phải gõ lại mật khẩu.1. Cập nhật bảng User (Models/User.cs):Cần bổ sung 2 cột để lưu trữ Refresh Token dưới DB.C#public class User
{
    // ... Các trường hiện có
    [StringLength(255)]
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
}
(Chạy Migration để cập nhật CSDL).2. Cập nhật AuthApiController.cs:Bổ sung hàm sinh chuỗi ngẫu nhiên cho Refresh Token và Endpoint xử lý việc cấp lại Token mới.C#using System.Security.Cryptography; // Để dùng RNGCryptoServiceProvider
// ...

namespace DigiPOSE.Web.Controllers.Api
{
    [Route("api/v1/auth")]
    [ApiController]
    public class AuthApiController : ControllerBase
    {
        // ... Các mã nguồn Login ở Buổi 10

        // Sinh chuỗi Refresh Token an toàn
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        // Endpoint: api/v1/auth/refresh-token
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenModel tokenModel)
        {
            if (tokenModel is null) return BadRequest("Yêu cầu không hợp lệ.");

            // 1. Tìm User có chứa Refresh Token này
            var user = await _context.Users
                .Include(u => u.Role)
                .SingleOrDefaultAsync(u => u.RefreshToken == tokenModel.RefreshToken);

            if (user == null || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return Unauthorized(new { Message = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại." });
            }

            // 2. Tạo Access Token MỚI (Logic giống hàm Login ở Buổi 10)
            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("UserId", user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role!.RoleName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtConfig:Key"]!));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var newAccessToken = new JwtSecurityToken(
                issuer: _configuration["JwtConfig:Issuer"],
                audience: _configuration["JwtConfig:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30), // Cấp mới sống 30 phút
                signingCredentials: signIn
            );

            // 3. Xoay vòng (Rotate) Refresh Token MỚI để tăng cường bảo mật
            var newRefreshToken = GenerateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7); // Duy trì 7 ngày
            
            await _context.SaveChangesAsync();

            return Ok(new { 
                AccessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                RefreshToken = newRefreshToken 
            });
        }
    }

    public class TokenModel
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}
Ghi chú: Trong hàm Login ở Buổi 10, Sếp cũng cần gọi GenerateRefreshToken(), gán vào Database và trả về cho Frontend kèm theo Access Token.Toàn bộ hệ thống Backend API lúc này đã đáp ứng đầy đủ tiêu chuẩn khắt khe nhất về an toàn dữ liệu và tối ưu băng thông. Framework cấu trúc dữ liệu cho dự án DigiPOSE Phase 14 đã hoàn tất. Tầng lõi đã vững chắc hoàn toàn và sẵn sàng kết nối giao thức Frontend.