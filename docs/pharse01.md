BUỔI 1: KHỞI TẠO DỰ ÁN VỚI ASP.NET CORE MVC & THIẾT KẾ DATABASE DIGIPOSE (.NET 8.0)
(Ghi chú chiến lược: Tầng MVC này sẽ được sử dụng độc quyền làm khu vực Admin CMS để sinh giao diện quản trị tự động, phục vụ cho việc nhập liệu danh mục, cấu hình hệ thống nhanh chóng mà không cần code tay Front-end).  

Bước 1: Tạo project ASP.NET Core MVC
Mở Visual Studio 2022. Chọn mẫu ASP.NET Core Web App (Model - View - Controller).
Đặt tên project là: DigiPOSE.Web.
Sử dụng các tùy chọn sau:  Framework: .NET 8.0 (Long Term Support).  Authentication type: None.  Bỏ chọn Configure for HTTPS (Để test local đơn giản hơn).  Chọn Do not use top-level statements.  

Bước 2: Cài đặt các gói NuGet cần thiết
Mở cửa sổ Package Manager Console. Lần lượt chạy các lệnh sau để cài đặt Entity Framework Core và thư viện mã hóa bảo mật:  
PowerShell
Install-Package Microsoft.EntityFrameworkCore.SqlServer
Install-Package Microsoft.EntityFrameworkCore.Tools
Install-Package Microsoft.VisualStudio.Web.CodeGeneration.Design
Install-Package BCrypt.Net-Next

Bước 3: Tạo các tập tin Entities và DbContext trong thư mục Models
Trong thư mục Models, tiến hành tạo các class thực thể (Entities) chia theo 4 phân hệ lõi của DigiPOSE (Tổng cộng 26 Thực thể Database).

1. Phân hệ Cấu hình & Nhân sự (IAM & Org)
- Branch (Chi nhánh)
- Role (Vai trò)
- User (Nhân sự)
- Counter (Quầy thu ngân)
- Shift (Ca làm việc)
- ShiftStatus (Trạng thái ca làm việc)

2. Phân hệ Đối tác & Khách hàng (CRM & Suppliers)
- CustomerType / CustomeType (Loại khách hàng)
- Customer (Khách hàng)
- Supplier (Nhà cung cấp)

3. Phân hệ Từ điển Hàng hóa & Kho bãi (Catalog & Inventory)
- Category (Nhóm hàng)
- Unit (Đơn vị tính)
- Manufacturer (Hãng sản xuất)
- TaxType (Loại thuế)
- ProductType (Loại sản phẩm)
- ItemNature (Tính chất hàng hóa)
- Product (Hàng hóa / Sản phẩm)
- ProductInventory (Tồn kho chi nhánh)
- StockVoucher (Phiếu kho nhập/xuất)
- StockVoucherDetail (Chi tiết phiếu kho)

4. Phân hệ Giao dịch Bán hàng & Hóa đơn (Sales & Billing)
- OrderStatus (Trạng thái đơn hàng)
- PaymentMethod (Phương thức thanh toán)
- Order (Đơn hàng bán)
- OrderDetail (Chi tiết đơn hàng)
- InvoiceStatus (Trạng thái hóa đơn)
- InvoiceType (Loại hóa đơn)
- Invoice (Hóa đơn tài chính GTGT)

Bước 4: Thiết lập thông tin kết nối CSDL
Cập nhật tập tin appsettings.json:  
JSON
{
  "ConnectionStrings": {
    "DigiPoseConnection": "Server=.;Database=DigiPOSE_Db;Integrated Security=True;MultipleActiveResultSets=True;TrustServerCertificate=True"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}

Cập nhật tập tin Program.cs:  
C#
using DigiPOSE.Web.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<DigiPoseDbContext>(options => 
    options.UseSqlServer(builder.Configuration.GetConnectionString("DigiPoseConnection")));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

Bước 5: Tạo CSDL bằng lệnh Migration
Mở Package Manager Console và thực thi:  
PowerShell
Add-Migration Init_DigiPOSE_Database
Update-Database

Bước 6: Sử dụng Scaffolding tạo giao diện Admin CMS tự động cho đầy đủ 26 Bảng Dữ Liệu
Để sinh giao diện nhập liệu nhanh cho Admin CMS, sử dụng tính năng Scaffolding tạo Controller và các Views tương ứng cho 26 Entities trong Models:

- Phân hệ Cấu hình & Nhân sự (IAM & Org):
  1. BranchesController (`/Branches`)
  2. RolesController (`/Roles`)
  3. UsersController (`/Users`)
  4. CountersController (`/Counters`)
  5. ShiftsController (`/Shifts`)
  6. ShiftStatusController (`/ShiftStatus`)

- Phân hệ Đối tác & Khách hàng (CRM & Suppliers):
  7. CustomeTypesController (`/CustomeTypes`)
  8. CustomersController (`/Customers`)
  9. SuppliersController (`/Suppliers`)

- Phân hệ Từ điển Hàng hóa & Kho bãi (Catalog & Inventory):
  10. CategoriesController (`/Categories`)
  11. UnitsController (`/Units`)
  12. ManufacturersController (`/Manufacturers`)
  13. TaxTypesController (`/TaxTypes`)
  14. ProductTypesController (`/ProductTypes`)
  15. ItemNaturesController (`/ItemNatures`)
  16. ProductsController (`/Products`)
  17. ProductInventoriesController (`/ProductInventories`)
  18. StockVouchersController (`/StockVouchers`)
  19. StockVoucherDetailsController (`/StockVoucherDetails`)

- Phân hệ Giao dịch Bán hàng & Hóa đơn (Sales & Billing):
  20. OrderStatusController (`/OrderStatus`)
  21. PaymentMethodsController (`/PaymentMethods`)
  22. OrdersController (`/Orders`)
  23. OrderDetailsController (`/OrderDetails`)
  24. InvoiceStatusController (`/InvoiceStatus`)
  25. InvoiceTypesController (`/InvoiceTypes`)
  26. InvoicesController (`/Invoices`)

Bước 7: Quản lý thư viện Client-Side & Cyber-Cinematic HUD Design Assets
Cập nhật thư viện client-side (Libman) và bổ sung hệ thống CSS/JS Cyber-Cinematic Military HUD:
- Thư viện Libman (`libman.json`): Bootstrap 5.3.3, jQuery 3.7.1, FontAwesome 6 Icons.
- Design Tokens & Assets:
  - `wwwroot/css/cyber-hud.css`: Định nghĩa hệ thống màu Cyber HUD (#000000, #0A0A0A, #00E5FF, #00FF66, #FFB000, #FF3333), typography (Orbitron, Rajdhani, VT323, Roboto Mono), glassmorphism panel, corner reticles, neon borders & CRT scanline.
  - `wwwroot/js/cyber-hud.js`: Xử lý đóng/mở Sidebar bằng nút 3 gạch (`#sidebarToggle`), công tắc đổi theme Dark/Light mode (`#themeToggleBtn`), bộ chọn ngôn ngữ EN/VI, đồng hồ live telemetry thời gian thực.

Bước 8: Tùy biến View Master (_Layout.cshtml) theo chuẩn Cyber-Cinematic UI/UX cho DigiPOSE ERP
Cấu trúc thanh điều hướng Top Navbar và Left Sidebar quản lý đầy đủ 26 Bảng dữ liệu:

1. Thanh điều hướng Top Navbar:
   - Navbar Trái: Nút 3 gạch (`#sidebarToggle`) để ẩn/hiện Sidebar, logo_main SVG icon phát sáng, Brand Name (`DigiPOSE HUD`).
   - Navbar Phải: Bộ chọn ngôn ngữ (`EN/VI`), Công tắc Dark/Light Mode (Cyber Void / Hologram), Notification Bell với badge thông báo live, Profile người dùng.

2. Thanh Sidebar Trái:
   - Phân chia thành 4 Phân hệ quản lý khoa học với 26 danh mục điều hướng riêng biệt tương ứng với 26 Controllers/Tables.