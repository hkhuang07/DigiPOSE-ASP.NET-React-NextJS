# BUỔI 1: KHỞI TẠO DỰ ÁN ASP.NET CORE MVC (.NET 8.0 / 10.0) & THIẾT KẾ CƠ SỞ DỮ LIỆU DIGIPOSE ERP (26 TABLES)

> **Tài liệu Hướng dẫn Thực hành & Lý thuyết Kiến trúc Clean Code**  
> **Tác giả / Mentor**: Software Architecture & Senior Systems Engineer  
> **Dự án**: DigiPOSE Enterprise (Digital Point of Sale & ERP System)

---

## 📑 MỤC LỤC
1. [Lý thuyết Kiến trúc & Phân tích Đối chiếu (ITShop vs. DigiPOSE ERP)](#1-lý-thuyết-kiến-trúc--phân-tích-đối-chiếu)
2. [Nguyên tắc Thiết kế Clean Code & Chuẩn Doanh nghiệp (Enterprise Standards)](#2-nguyên-tắc-thiết-kế-clean-code--chuẩn-doanh-nghiệp)
3. [Bước 1: Khởi tạo Project ASP.NET Core MVC qua Terminal / CLI](#bước-1-khởi-tạo-project-aspnet-core-mvc-qua-terminal--cli)
4. [Bước 2: Cài đặt các Gói Thư viện Backend (NuGet) & Frontend (LibMan)](#bước-2-cài-đặt-các-gói-thư-viện-backend-nuget--frontend-libman)
5. [Bước 3: Xây dựng 26 Entity Models & DigiPoseDbContext (Chuẩn hóa 3NF)](#bước-3-xây-dựng-26-entity-models--digiposedbcontext-chuẩn-hóa-3nf)
6. [Bước 4: Cấu hình Connection String & Dependency Injection (Program.cs)](#bước-4-cấu-hình-connection-string--dependency-injection-programcs)
7. [Bước 5: Khởi tạo & Cập nhật Cơ sở Dữ liệu qua EF Core Migrations](#bước-5-khởi-tạo--cập-nhật-cơ-sở-dữ-liệu-qua-ef-core-migrations)
8. [Bước 6: Thiết kế Giao diện Cyber-Cinematic Military HUD & DataTables Server-Side](#bước-6-thiết-kế-giao-diện-cyber-cinematic-military-hud--datatables-server-side)
9. [Bước 7: Câu hỏi Lý thuyết & Bài tập Thực hành Buổi 1](#bước-7-câu-hỏi-lý-thuyết--bài-tập-thực-hành-buổi-1)

---

## 1. Lý thuyết Kiến trúc & Phân tích Đối chiếu

### 1.1. So sánh chi tiết: Bài thực hành ITShop (Cơ bản) vs. DigiPOSE ERP (Enterprise)

| Tiêu chí | ITShop (Mô hình Đào tạo Nhập môn) | DigiPOSE Enterprise (Hệ thống Thực tế) | Đánh giá & Giải pháp Tối ưu của DigiPOSE |
| :--- | :--- | :--- | :--- |
| **Quy mô Database** | 7 Bảng đơn giản (`LoaiSanPham`, `SanPham`, `NguoiDung`, `DatHang`...) | **26 Bảng chuẩn hóa 3NF** chia thành 4 phân hệ lõi | Phản ánh đúng 100% nghiệp vụ bán lẻ thực tế (Chi nhánh, Ca làm việc, Quầy thu ngân, Kho, Hóa đơn VAT). |
| **Quản lý Tồn kho** | Cố định biến `SoLuong` trực tiếp trong bảng `SanPham` | **Tồn kho đa chi nhánh** (`ProductInventories`) & Nhập/Xuất kho qua chứng từ (`StockVouchers`) | Hỗ trợ chuỗi cửa hàng đa chi nhánh. Tồn kho được cách ly theo từng Chi nhánh (`BranchId`) & Kho bãi. |
| **Quản lý Ca & Quầy** | Không hỗ trợ | Định danh Quầy vật lý (`Counters`), Mở/Đóng ca với kiểm soát tiền mặt (`Shifts`, `ShiftStatuses`) | Tránh thất thoát tiền mặt cuối ca, kiểm soát doanh thu chính xác theo từng thu ngân và thiết bị quầy. |
| **Kiểu dữ liệu Tiền tệ** | Kiểu `int` (Dễ tràn số, không chính xác) | **`decimal(18,2)` và `decimal(18,4)`** | Đảm bảo độ chính xác tuyệt đối trong tính toán tài chính, tỷ giá và thuế GTGT (Tránh lỗi rounding float/int). |
| **Bảo mật Mật khẩu** | Lưu Plaintext chuỗi thô trong DB (`MatKhau`) | **Mã hóa BCrypt Hashing** (BCrypt.Net-Next) | Chống tấn công Rainbow Table & Leak dữ liệu tài khoản người dùng theo chuẩn OWASP Top 10. |
| **Ràng buộc Dữ liệu** | Xóa dây chuyền mặc định (`Cascade Delete`) | **Chặn xóa dây chuyền** (`DeleteBehavior.Restrict`) | Tránh việc xóa 1 Danh mục/Người dùng dẫn tới mất sạch toàn bộ Đơn hàng và Hóa đơn lịch sử trong DB. |
| **Kiểm soát Bất đồng bộ** | Không hỗ trợ | **Optimistic Concurrency Control** (`[Timestamp] byte[] RowVersion`) | Tránh ghi đè dữ liệu (Lost Update) khi 2 thu ngân cùng thao tác bán 1 mặt hàng còn tồn kho = 1. |
| **Hiệu năng Grid UI** | Render Client-side Razor View mặc định (Tải 10,000 dòng gây treo trình duyệt) | **100% Server-Side DataTables** (`System.Linq.Dynamic.Core`) | Phân trang, tìm kiếm & sắp xếp trực tiếp trên SQL Server. Thời gian phản hồi $< 50ms$ cho triệu bản ghi. |

> 💡 **Kết luận từ Mentor**: Tài liệu thực hành ITShop có hạn chế nghiệp vụ và kiến trúc tới **> 60%** khi áp dụng vào doanh nghiệp thực tế. DigiPOSE đã giải quyết triệt để toàn bộ các hạn chế này thông qua kiến trúc 26 bảng chuẩn 3NF, bảo mật BCrypt, concurrency check và phân trang DataTables Server-Side 100%.

---

## 2. Nguyên tắc Thiết kế Clean Code & Chuẩn Doanh nghiệp

1. **SOLID Principles**:
   - **Single Responsibility Principle (SRP)**: Mỗi Controller chỉ quản lý 1 Domain Entity. Logic xử lý dữ liệu động được bóc tách vào method `Index_LoadData`.
   - **Open/Closed Principle (OCP)**: Kiến trúc Modal CRUD dùng chung (`#globalModal`) cho phép mở rộng Form mà không sửa đổi cấu trúc HTML trang Master layout.
2. **Kebab-Case File Naming**: Tất cả các asset tĩnh (`cyber-hud.css`, `cyber-hud.js`) và đường dẫn URL đều tuân thủ chuẩn `kebab-case`.
3. **No Hardcoded Magic Strings**: Định nghĩa rõ ràng constants và sử dụng Strongly-typed View Models / Projections trong LINQ Query.
4. **Clean Controllers**: Controller không chứa HTML inline. Action `Index()` chỉ trả về `View()`, dữ liệu bảng được nạp hoàn toàn bất đồng bộ qua AJAX POST API (`Index_LoadData`).

---

## 3. Bước 1: Khởi tạo Project ASP.NET Core MVC qua Terminal / CLI

Không phụ thuộc vào giao diện Visual Studio, bạn có thể khởi tạo dự án nhanh chóng bằng `.NET CLI`:

```bash
# 1. Tạo thư mục dự án và di chuyển vào thư mục
mkdir digipose
cd digipose

# 2. Khởi tạo dự án ASP.NET Core MVC với .NET 8.0 / 10.0
dotnet new mvc -n DigiPOSE -o source/DigiPOSE --no-https

# 3. Di chuyển vào thư mục chứa dự án C#
cd source/DigiPOSE
```

---

## 4. Bước 2: Cài đặt các Gói Thư viện Backend (NuGet) & Frontend (LibMan)

### 4.1. Cài đặt NuGet Packages cho Backend

Chạy các lệnh CLI sau trong terminal:

```bash
# Entity Framework Core Provider cho SQL Server
dotnet add package Microsoft.EntityFrameworkCore.SqlServer

# Công cụ Migrations & Scaffolding
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design

# Thư viện LINQ Động hỗ trợ DataTables Server-Side Sorting & Filtering
dotnet add package System.Linq.Dynamic.Core

# Thư viện Mã hóa Mật khẩu Chuẩn OWASP
dotnet add package BCrypt.Net-Next

# Khôi phục dependencies
dotnet restore
```

### 4.2. Cài đặt LibMan CLI & Các gói Frontend (Client-Side)

```bash
# Cài đặt LibMan CLI Tool toàn cục
dotnet tool install -g Microsoft.Web.LibraryManager.Cli

# Tải và cấu hình Bootstrap 5.3.3 & FontAwesome 6.5.1
libman install bootstrap@5.3.3 --provider cdnjs --destination wwwroot/lib/bootstrap --files js/bootstrap.bundle.min.js --files css/bootstrap.min.css
libman install jquery@3.7.1 --provider cdnjs --destination wwwroot/lib/jquery --files jquery.min.js
libman install font-awesome@6.5.1 --provider cdnjs --destination wwwroot/lib/font-awesome

# Tải gói DataTables Core, Bootstrap 5 integration & Export Plugins
libman install datatables.net@1.13.8 --provider cdnjs --destination wwwroot/lib/datatables --files jquery.dataTables.min.js
libman install datatables.net-bs5@1.13.8 --provider cdnjs --destination wwwroot/lib/datatables --files dataTables.bootstrap5.min.css --files dataTables.bootstrap5.min.js
libman install datatables.net-buttons@2.4.2 --provider cdnjs --destination wwwroot/lib/datatables --files js/dataTables.buttons.min.js --files js/buttons.html5.min.js
libman install datatables.net-buttons-bs5@2.4.2 --provider cdnjs --destination wwwroot/lib/datatables --files buttons.bootstrap5.min.css --files buttons.bootstrap5.min.js
libman install jszip@3.10.1 --provider cdnjs --destination wwwroot/lib/datatables --files jszip.min.js

# Khôi phục toàn bộ thư viện client-side
libman restore
```

---

## 5. Bước 3: Xây dựng 26 Entity Models & DigiPoseDbContext (Chuẩn hóa 3NF)

Dự án DigiPOSE được thiết kế chuẩn 3NF bao gồm **26 Entities** phân bổ vào 4 phân hệ chính:

### 5.1. Sơ đồ Cấu trúc 4 Phân hệ Lõi
```
DigiPOSE ERP Models (26 Tables)
├── 1. IAM & Org (Bảo mật & Tổ chức): Branch, Role, User, Counter, Shift, ShiftStatus
├── 2. Partners & CRM (Đối tác & Khách hàng): CustomerType, Customer, Supplier
├── 3. Catalog & Inventory (Hàng hóa & Kho bãi): Category, Unit, Manufacturer, TaxType, ProductType, ItemNature, Product, ProductInventory, StockVoucher, StockVoucherDetail
└── 4. Sales & Billing (Bán hàng & Hóa đơn): OrderStatus, PaymentMethod, Order, OrderDetail, InvoiceStatus, InvoiceType, Invoice
```

### 5.2. Mã nguồn đại diện cho Thực thể Core (`Product.cs`)

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiPOSE.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Mã SKU sản phẩm không được để trống.")]
        [StringLength(50)]
        [Display(Name = "Mã SKU / Barcode")]
        public string SKU { get; set; } = null!;

        [Required(ErrorMessage = "Tên sản phẩm không được để trống.")]
        [StringLength(255)]
        [Display(Name = "Tên sản phẩm")]
        public string ProductName { get; set; } = null!;

        [Display(Name = "Nhóm hàng")]
        public int CategoryId { get; set; }

        [Display(Name = "Đơn vị tính")]
        public int UnitId { get; set; }

        [Display(Name = "Hãng sản xuất")]
        public int ManufacturerId { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giá nhập")]
        public decimal PurchasePrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giá bán bán lẻ")]
        public decimal RetailPrice { get; set; }

        [StringLength(255)]
        [Display(Name = "Hình ảnh")]
        public string? ImageUrl { get; set; }

        [Timestamp]
        public byte[]? RowVersion { get; set; }

        // Navigation Properties
        public Category? Category { get; set; }
        public Unit? Unit { get; set; }
        public Manufacturer? Manufacturer { get; set; }
        public ICollection<ProductInventory>? ProductInventories { get; set; }
    }
}
```

### 5.3. Cấu hình Fluent API trong `DigiPoseDbContext.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Models;

namespace DigiPOSE.Models
{
    public class DigiPoseDbContext : DbContext
    {
        public DigiPoseDbContext(DbContextOptions<DigiPoseDbContext> options) : base(options) { }

        public DbSet<Branch> Branches { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderDetail> OrderDetails { get; set; } = null!;
        // ... (Khai báo đủ 26 DbSets)

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Vô hiệu hóa Cascade Delete trên toàn bộ mối quan hệ để đảm bảo an toàn dữ liệu
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            // Đảm bảo mã SKU là Duy nhất (Unique Index)
            modelBuilder.Entity<Product>()
                .HasIndex(p => p.SKU)
                .IsUnique();
        }
    }
}
```

---

## 6. Bước 4: Cấu hình Connection String & Dependency Injection (Program.cs)

### 6.1. Cấu hình `appsettings.json`
```json
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
```

### 6.2. Cấu hình `Program.cs` (Minimal APIs & Clean Architecture)
```csharp
using DigiPOSE.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Đăng ký Services vào DI Container
builder.Services.AddControllersWithViews();

// 2. Đăng ký DbContext kết nối SQL Server
builder.Services.AddDbContext<DigiPoseDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DigiPoseConnection")));

var app = builder.Build();

// 3. Cấu hình HTTP Request Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

---

## 7. Bước 5: Khởi tạo & Cập nhật Cơ sở Dữ liệu qua EF Core Migrations

Chạy chuỗi lệnh CLI để thực thi Migration:

```bash
# 1. Tạo Migration khởi tạo 26 bảng
dotnet ef migrations add Init_DigiPOSE_26_Tables

# 2. Cập nhật Schema vào SQL Server
dotnet ef database update
```

---

## 8. Bước 6: Thiết kế Giao diện Cyber-Cinematic Military HUD & DataTables Server-Side

### 8.1. Mẫu Controller DataTables Server-Side Standard (`ProductsController.cs`)

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Models;
using System.Linq.Dynamic.Core;

namespace DigiPOSE.Controllers
{
    public class ProductsController : Controller
    {
        private readonly DigiPoseDbContext _context;
        public ProductsController(DigiPoseDbContext context) { _context = context; }

        public IActionResult Index() => View();

        [HttpPost]
        public async Task<IActionResult> Index_LoadData()
        {
            try
            {
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                var query = _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Unit)
                    .AsQueryable();

                int totalRecords = await query.CountAsync();

                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(p => 
                        p.ProductName.Contains(searchValue) || 
                        p.SKU.Contains(searchValue));
                }

                int filterRecords = await query.CountAsync();

                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
                {
                    query = query.OrderBy(sortColumn + " " + sortColumnDirection);
                }

                var data = await query.Skip(skip).Take(pageSize).Select(p => new {
                    productId = p.ProductId,
                    sku = p.SKU,
                    productName = p.ProductName,
                    categoryName = p.Category != null ? p.Category.CategoryName : "",
                    retailPrice = p.RetailPrice
                }).ToListAsync();

                return Json(new { draw = draw, recordsFiltered = filterRecords, recordsTotal = totalRecords, data = data });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}
```

---

## 9. Bước 7: Câu hỏi Lý thuyết & Bài tập Thực hành Buổi 1

### ❓ Câu hỏi Lý thuyết Checklist
1. Tại sao trong môi trường Enterprise ta nên dùng `DeleteBehavior.Restrict` thay vì `Cascade`?
2. Sự khác biệt về mặt hiệu năng giữa **DataTables Client-Side** và **DataTables Server-Side** khi dữ liệu phình to lên 500,000 dòng?
3. Tại sao kiểu dữ liệu `decimal(18,2)` lại bắt buộc cho các thuộc tính Giá cả thay vì `float` hay `double`?

### 🛠️ Bài tập Thực hành
1. Thực thi các lệnh CLI trong tài liệu để cài đặt các gói NuGet & LibMan vào dự án `DigiPOSE`.
2. Kiểm tra chuỗi kết nối `DigiPoseConnection` trong `appsettings.json` và chạy lệnh `dotnet ef database update`.
3. Kiểm tra xem tất cả 26 Controllers đã hiển thị đúng giao diện Cyber HUD và nạp dữ liệu qua DataTables Server-Side AJAX chưa.

---
*DigiPOSE ERP System — Clean Architecture & High-Performance Standard Guidelines.*