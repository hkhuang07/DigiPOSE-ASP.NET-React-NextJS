BUỔI 10: TÍCH HỢP BẢO MẬT JWT VÀ SWAGGER CHO RESTFUL API (DIGIPOSE)Bước 1: Cài đặt gói thư viện JWT BearerĐể cấp phát và xác thực chuỗi Token bảo mật cho các thiết bị ngoại vi (Máy PoS, App Mobile), hệ thống cần cài đặt thư viện JWT.  Mở cửa sổ Package Manager Console và chạy lệnh:  PowerShellInstall-Package Microsoft.AspNetCore.Authentication.JwtBearer
(Ghi chú: Gói BCrypt.Net-Next dùng để mã hóa mật khẩu đã được chúng ta cài đặt từ Buổi 1 nên không cần cài lại).  Bước 2: Khai báo cấu hình JWT trong appsettings.jsonMở tập tin appsettings.json, bổ sung block JwtConfig chứa khóa bí mật (Secret Key) để ký Token. Khóa này phải tuyệt đối bảo mật và đủ độ dài.  JSON{
  "ConnectionStrings": {
    "DigiPoseConnection": "Server=.;Database=DigiPOSE_Db;Integrated Security=True;MultipleActiveResultSets=True;TrustServerCertificate=True"
  },
  "JwtConfig": {
    "Key": "DigiPOSE_Enterprise_Secret_Key_Super_Secure_2026_@!", 
    "Issuer": "DigiPoseServer",
    "Audience": "DigiPoseClient",
    "Subject": "DigiPoseAccessToken"
  },
  "Logging": {
    // ...
  },
  "AllowedHosts": "*"
}
Bước 3: Xây dựng API Cấp phát Token (AuthApiController)Tạo một endpoint để ứng dụng Frontend (React/Vue) tại quầy thu ngân gửi Username và Password lên để lấy JWT Token.  Tạo file AuthApiController.cs trong thư mục Controllers/Api/:C#using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using DigiPOSE.Web.Models;
using BC = BCrypt.Net.BCrypt;

namespace DigiPOSE.Web.Controllers.Api
{
    [Route("api/v1/auth")]
    [ApiController]
    public class AuthApiController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly DigiPoseDbContext _context;

        public AuthApiController(IConfiguration configuration, DigiPoseDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { Message = "Tên đăng nhập và mật khẩu là bắt buộc." });

            // Kiểm tra User trong hệ thống DigiPOSE
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

            if (user == null || !BC.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { Message = "Tài khoản hoặc mật khẩu không chính xác." });
            }

            // Thiết lập các thông tin (Claims) nhúng vào Payload của Token
            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, _configuration["JwtConfig:Subject"]!),
                new Claim("UserId", user.UserId.ToString()),
                new Claim("BranchId", user.BranchId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role!.RoleName) // Quan trọng để phân quyền
            };

            // Ký Token bằng Secret Key
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtConfig:Key"]!));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtConfig:Issuer"],
                audience: _configuration["JwtConfig:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(12), // Token của ca làm việc hết hạn sau 12 tiếng
                signingCredentials: signIn
            );

            return Ok(new { 
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo
            });
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
Bước 4: Tích hợp JWT và Swagger vào Program.csChúng ta phải thiết lập hệ thống Xác thực Kép: Giữ nguyên Cookie cho MVC Admin, nhưng thêm JWT cho các Controller API. Đồng thời, cấu hình giao diện Swagger UI để có nút nhập Token kiểm thử.  Cập nhật tập tin Program.cs:  C#using System.Text;
using DigiPOSE.Web.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(); // MVC
builder.Services.AddControllers(); // Web API

builder.Services.AddDbContext<DigiPoseDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DigiPoseConnection")));

// ==========================================
// 1. CẤU HÌNH XÁC THỰC KÉP (DUAL AUTH)
// ==========================================
// Mặc định sử dụng Cookie (cho Admin CMS)
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.Name = "DigiPOSE.AdminAuth";
    options.LoginPath = "/Admin/Auth/Login";
    options.AccessDeniedPath = "/Admin/Auth/Forbidden";
})
// Cấu hình thêm JWT Bearer (Dành cho API)
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false; // Set true nếu có chứng chỉ SSL
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtConfig:Issuer"],
        ValidAudience = builder.Configuration["JwtConfig:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtConfig:Key"]!))
    };
});

// ==========================================
// 2. CẤU HÌNH SWAGGER UI CÓ NÚT AUTHORIZE
// ==========================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "DigiPOSE API", Version = "v1.0" });
    
    // Nút nhập Token trên Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập token theo định dạng: Bearer {chuỗi_token}"
    });
    
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // Kích hoạt Swagger
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "adminareas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
Bước 5: Khóa bảo mật các API bằng [Authorize]Mở các file API Controller đã tạo ở các buổi trước và yêu cầu chúng xác thực bằng Scheme JWT (bỏ qua Cookie).  Ví dụ với PosController.cs:C#using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
// ...

namespace DigiPOSE.Web.Controllers.Api
{
    [Route("api/v1/[controller]")]
    [ApiController]
    // Bắt buộc Client (React/Vue) phải đính kèm Token ở Header thì mới được gọi API
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] 
    public class PosController : ControllerBase
    {
        // ... Các endpoints Checkout, AddItem ...
    }
}
(Thực hiện tương tự thuộc tính [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] cho CustomerApiController và AnnouncementsApiController).Bước 6: Kiểm thử bằng Swagger và PostmanChạy dự án (F5). Truy cập đường dẫn http://localhost:<port>/swagger trên trình duyệt.  Sử dụng Endpoint POST /api/v1/auth/login để giả lập đăng nhập thu ngân. Nếu thành công, hệ thống sẽ trả về chuỗi Token eyJh....  Copy chuỗi Token đó.Kéo lên đầu trang Swagger, nhấn vào nút Authorize màu xanh lá. Dán Token vào (không cần gõ chữ "Bearer" vì Swagger đã tự xử lý) và bấm Save.Bây giờ, Sếp có thể test các API bị khóa bảo mật như PosController hoặc CustomerApiController một cách trơn tru, hoặc sử dụng công cụ Postman để gán Token vào tab Authorization > Bearer Token