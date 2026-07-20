BUỔI 9: CHUẨN HÓA API ROUTING VÀ XUẤT BẢN HỆ THỐNG LÊN MÁY CHỦ IIS (DEPLOYMENT)Bước 1: Thiết lập cấu trúc RESTful API RoutingChúng ta quy hoạch lại các Endpoint giao tiếp với máy PoS theo chuẩn RESTful. Mở các Controller trong Controllers/Api/ và thiết lập [Route].1. ProductsApiController.cs (Quản lý hàng hóa API):  
Đã bổ sung việc trả về thông tin Manufacturer và lọc IsActive cho dữ liệu hàng hóa.  C#[Route("api/v1/products")] 
[ApiController]
public class ProductsApiController : ControllerBase
{
    private readonly DigiPoseDbContext _context;
    public ProductsApiController(DigiPoseDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] int? categoryId, [FromQuery] int page = 1)
    {
        var products = _context.Products
            .Include(p => p.Manufacturer) // Bổ sung Manufacturer
            .Where(p => p.IsActive);      // Chỉ lấy hàng còn kinh doanh
            
        if (categoryId.HasValue) 
            products = products.Where(p => p.CategoryId == categoryId);

        return Ok(await products.ToListAsync());
    }

    [HttpGet("sku/{sku}")]
    public async Task<IActionResult> GetProductBySku(string sku)
    {
        var product = await _context.Products
            .Include(p => p.Manufacturer)
            .FirstOrDefaultAsync(p => p.SKU == sku && p.IsActive);
            
        if (product == null) return NotFound();
        return Ok(product);
    }
}
Bước 2: Cài đặt môi trường thực thi (.NET 8 Core Runtime)Để máy chủ Windows Server chạy được ứng dụng, tải .NET 8 Hosting Bundle:
https://dotnet.microsoft.com/download/dotnet/8.0  Bước 3: Cấu hình IIS ServerNhấn Win + R, gõ optionalfeatures, tích chọn Internet Information Services.  Mở inetmgr (IIS Manager), chọn Default Web Site > Bindings... > Đổi Port sang 8080.  Nếu cần bảo mật HTTPS cho API PoS, cấu hình Add... > Type: https, Port: 443, gán SSL Certificate.  Bước 4: Xuất bản (Publish) dự ánChuột phải project DigiPOSE.Web > Publish....  Target: Folder. Location: C:\inetpub\wwwroot\DigiPOSE\.  Nhấn Publish.  Bước 5: Cấp quyền Cơ sở dữ liệu (Vá lỗi HTTP 500)IIS chạy dưới quyền IIS APPPOOL\DefaultAppPool. Nếu không cấp quyền, ứng dụng sẽ bị từ chối truy cập DB.  Mở SSMS (SQL Server Management Studio), kết nối DB DigiPOSE_Db.  Vào Security > Logins, chuột phải chọn New Login....  Login name: IIS APPPOOL\DefaultAppPool.  Chọn trang User Mapping, tick chọn DigiPOSE_Db và cấp quyền db_owner.  Nhấn OK để lưu lại.  Bước 6: Kiểm tra vận hành  Mở trình duyệt truy cập: http://localhost:8080/Admin/Auth/Login

# OLD VERSION
BUỔI 9: CHUẨN HÓA API ROUTING VÀ XUẤT BẢN HỆ THỐNG LÊN MÁY CHỦ IIS (DEPLOYMENT)

Bước 1: Thiết lập cấu trúc RESTful API Routing (Thay thế SEO Routing)
Thay vì dùng đường dẫn ảo .html, chúng ta sẽ quy hoạch lại toàn bộ các Endpoint giao tiếp với máy PoS theo chuẩn RESTful. 
Mở các Controller trong thư mục Controllers/Api/ và thiết lập [Route].  1. Ví dụ với ProductsApiController.cs (Quản lý hàng hóa API):C#using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Web.Models;

namespace DigiPOSE.Web.Controllers.Api
{
    // Định tuyến chuẩn RESTful: /api/{phiên_bản}/{tài_nguyên}
    [Route("api/v1/products")] 
    [ApiController]
    public class ProductsApiController : ControllerBase
    {
        // ...
        
        // Cũ (SEO): /san-pham/dien-thoai/1
        // Mới (API): GET /api/v1/products?categoryId=1&page=1
        [HttpGet]
        public IActionResult GetProducts([FromQuery] int? categoryId, [FromQuery] int page = 1)
        {
            // Logic lấy danh sách sản phẩm...
            return Ok();
        }

        // Cũ (SEO): /san-pham/dien-thoai/iphone-16.html
        // Mới (API): GET /api/v1/products/sku/{sku} (Dành cho máy quét mã vạch)
        [HttpGet("sku/{sku}")]
        public IActionResult GetProductBySku(string sku)
        {
            // Logic quét mã vạch...
            return Ok();
        }
    }
}
Bước 2: Cài đặt môi trường thực thi (.NET 8 Core Runtime)Để máy chủ Windows Server có thể chạy được ứng dụng DigiPOSE, bắt buộc phải cài đặt Hosting Bundle. 
Truy cập link tải .NET 8 Hosting Bundle của Microsoft: [https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-aspnetcore-8.0.0-windows-hosting-bundle-installer](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-aspnetcore-8.0.0-windows-hosting-bundle-installer)  
Chạy file .exe vừa tải về, tick chọn "I agree to the license terms and conditions" và nhấn Install.  
(Lưu ý: Nếu máy chủ đã cài sẵn Visual Studio kèm môi trường .NET 8 thì có thể bỏ qua bước này). 

Bước 3: Cài đặt và cấu hình IIS Server (Internet Information Services)1. Kích hoạt IIS trên Windows:  Nhấn tổ hợp phím Win + R, gõ optionalfeatures và nhấn OK.  Trong hộp thoại Windows Features, tìm và tick chọn mục Internet Information Services. Nhấn OK và chờ Windows hoàn tất cài đặt.  
Sau khi cài xong, hệ thống sẽ tự sinh ra thư mục C:\inetpub\wwwroot. Đây là nơi ta sẽ chứa mã nguồn biên dịch của DigiPOSE.  
2. Cấu hình Cổng (Port) cho ứng dụng:  
Nhấn Win + R, gõ lệnh inetmgr để mở cửa sổ quản lý IIS.  Ở cột bên trái, mở rộng mục Sites > Chọn Default Web Site.  
Nhìn sang cột bên phải (Actions), chọn Bindings....  
Nhấn Edit..., đổi cổng mặc định 80 sang một cổng khác (ví dụ: 8080 hoặc 9000) để tránh xung đột với các ứng dụng khác như Skype hay XAMPP.  
Nếu hệ thống yêu cầu bảo mật HTTPS cho các API gọi từ máy PoS, nhấn Add..., chọn type là https, port 443, và gán SSL certificate tương ứng.  Nhấn Restart ở cột bên phải để khởi động lại máy chủ IIS.  

Bước 4: Xuất bản (Publish) dự án DigiPOSE lên IISBiên dịch mã nguồn từ Visual Studio ra các file thực thi (.dll).Trong Visual Studio, nhấn chuột phải vào project DigiPOSE.Web, chọn Publish....  Ở mục Target, chọn Folder (Publish your application to a local folder).  Ở mục Location, trỏ đường dẫn trực tiếp vào thư mục của IIS: C:\inetpub\wwwroot\DigiPOSE\.  Nhấn Publish và chờ Visual Studio build mã nguồn.  

Bước 5: Cấp quyền truy cập Cơ sở dữ liệu (Khắc phục lỗi HTTP 500)Đây là lỗi kinh điển khi Deloy. Máy chủ IIS chạy ứng dụng dưới quyền của một tài khoản ảo tên là IIS APPPOOL\DefaultAppPool, tài khoản này mặc định KHÔNG CÓ QUYỀN chui vào SQL Server để đọc dữ liệu.  Các bước cấp quyền trong SQL Server Management Studio (SSMS):  Mở SSMS, kết nối vào Database Engine chứa DigiPOSE_Db.  Mở thư mục CSDL DigiPOSE_Db > Chọn thư mục Security > Users.  Nhấn chuột phải vào Users, chọn New User....  Ở trang General:User type: Chọn SQL user with login.  User name: Nhập IIS APPPOOL\DefaultAppPool.  Login name: Nhập IIS APPPOOL\DefaultAppPool.  Chuyển sang trang Membership ở menu bên trái:  Tick chọn quyền db_owner để cấp toàn quyền đọc/ghi/thực thi cho ứng dụng DigiPOSE trên cơ sở dữ liệu này.  Nhấn OK để lưu lại.  Cuối cùng, mở trình duyệt và truy cập http://localhost:8080/Admin để kiểm tra kết quả Deploy. Toàn bộ nền tảng Backend API và CMS của hệ thống ERP DigiPOSE đã sẵn sàng đưa vào vận hành thực tế.