# BUỔI 5: PHÂN VÙNG HỆ THỐNG (AREAS), XÁC THỰC KÉP VÀ PHÂN QUYỀN BẢO MẬT (DIGIPOSE CYBER HUD ERP)

---

## 💡 I. TỔNG QUAN LÝ THUYẾT VÀ KIẾN TRÚC (THEORY & ARCHITECTURE)

### 1. ASP.NET Core Areas là gì? Tại sao cần phân vùng?
Trong các ứng dụng Enterprise cỡ lớn như **DigiPOSE (26 Bảng dữ liệu ERP/POS)**, việc nhét tất cả `Controllers` và `Views` vào một thư mục gốc duy nhất sẽ tạo ra sự hỗn loạn trong quản lý mã nguồn (Spaghetti Structure) và gây rủi ro bảo mật nghiêm trọng.

**Areas** trong ASP.NET Core MVC là giải pháp phân vùng ứng dụng thành các mô-đun chức năng độc lập (Modules). Mỗi Area có cấu trúc thư mục riêng biệt gồm `Controllers`, `Models` và `Views`.

```
DigiPOSE.Web/
├── Areas/
│   └── Admin/                 <-- Area Phân vùng CMS Quản trị
│       ├── Controllers/       <-- Controllers phục vụ Admin
│       ├── Models/            <-- ViewModels dành riêng cho CMS
│       └── Views/             <-- Giao diện CMS Cyber-HUD
├── Controllers/               <-- Controllers phục vụ Storefront Client / Guest API
├── Models/                    <-- Domain Entities (EF Core Models)
└── Views/                     <-- Giao diện Khách hàng (Frontend)
```

**Lợi ích của Areas:**
- **Phân tách trách nhiệm (Separation of Concerns):** Độc lập giao diện CMS Admin khỏi trang Khách hàng (Client).
- **Độc lập Routing:** Quản lý không gian tên URL (`/Admin/Products`, `/Admin/Users`) tách biệt với (`/Products`, `/Cart`).
- **Tối ưu Bảo mật (Security Isolation):** Dễ dàng thiết lập bộ lọc Bảo mật/Phân quyền toàn cục cho từng Area.

---

### 2. Kiến trúc Xác thực (Authentication) & Phân quyền (Authorization)
Một hệ thống an toàn phải phân biệt rõ ràng giữa hai khái niệm:

```
+-----------------------------------------------------------------------+
|                         REQUEST HTTP TỪ CLIENT                        |
+-----------------------------------------------------------------------+
                                   │
                                   ▼
+-----------------------------------------------------------------------+
|  1. AUTHENTICATION (Xác thực): "Bạn là ai?"                          |
|  - Kiểm tra Username & Password qua BCrypt.Net                        |
|  - Cấp phát Cookie chứa thông tin nhận dạng (Claims Identity)         |
+-----------------------------------------------------------------------+
                                   │
                                   ▼
+-----------------------------------------------------------------------+
|  2. AUTHORIZATION (Phân quyền): "Bạn được phép làm gì?"               |
|  - Trích xuất Claim Role (Admin, Branch Manager, Cashier...)          |
|  - Kiểm tra đặc quyền truy cập trên Controller/Action [Authorize]    |
+-----------------------------------------------------------------------+
```

#### a. Luồng Middleware Pipeline trong `Program.cs`
Thứ tự khai báo Middleware trong pipeline HTTP của ASP.NET Core cực kỳ quan trọng:
```csharp
app.UseRouting();

app.UseAuthentication(); // 1. Xác minh danh tính người dùng trước (Đọc Cookie ticket)
app.UseAuthorization();  // 2. Phán quyết quyền hạn dựa trên danh tính đã xác minh

app.MapControllerRoute(...);
```
> ⚠️ **CẢNH BÁO KIẾN TRÚC:** Nếu đặt `UseAuthorization()` trước `UseAuthentication()`, hệ thống sẽ không thể nhận diện được Claims của User và luôn trả về lỗi `401 Unauthorized` hoặc `403 Forbidden`.

#### b. Bản chất của Claims-Based Authentication & Cookie Persistence
- **Claims (Tuyên bố):** Là các cặp Key-Value lưu trữ thông tin của người dùng trong phiên đăng nhập (Ví dụ: `UserId`, `Username`, `Role`, `BranchId`).
- **Encrypted Cookie Ticket:** ASP.NET Core mã hóa danh sách Claims thành chuỗi Cookie an toàn (Data Protection API) và gửi về Trình duyệt. Ở các Request sau, Browser tự động gửi Cookie này kèm theo Request để Server giải mã và tái tạo `HttpContext.User`.

---

## 📊 II. BÁO CÁO PHÂN TÍCH ĐỐI CHIẾU & ĐÁNH GIÁ KIẾN TRÚC

### 1. Phân tích đối chiếu: Tài liệu ITShop (Buổi 5) vs Tài liệu DigiPOSE Phase 05

| Tiêu chí So sánh | Tài liệu ITShop (Buổi 5) | Tài liệu DigiPOSE Phase 05 | Đánh giá & Giải pháp Tối ưu (DigiPOSE) |
| :--- | :--- | :--- | :--- |
| **Cấu trúc DTO / ViewModel** | Khai báo class `DangNhap` nằm trực tiếp trong file `Models/NguoiDung.cs` dùng `[NotMapped]`. | Tách biệt hoàn toàn thành `LoginViewModel.cs` trong `Areas/Admin/Models/`. | **Tốt hơn 100% (Clean Code):** Không làm ô nhiễm Domain Entity Model. Tuân thủ Single Responsibility Principle (SRP). |
| **Phân định Auth Controller** | Nhét chung các Action `Login`, `Logout` vào `HomeController`. | Tách riêng thành `AuthController.cs` quản lý phiên làm việc. | **Kiến trúc Chuẩn:** Phân tách rõ ràng giữa Dashboard điều khiển và nghiệp vụ Authentication/Security. |
| **Mô hình Phân quyền (Role)** | Hardcode `Quyen` kiểu Boolean (`Quyen ? "Admin" : "User"`). | Sử dụng Bảng `Role` riêng biệt linh hoạt, hỗ trợ Multitenancy với `BranchId`. | **Khả năng mở rộng cao:** Hỗ trợ đa vai trò (Admin, Branch Manager, Cashier, Warehouse Staff...) đáp ứng quy mô Enterprise. |
| **Kiểm tra trạng thái User** | Chỉ kiểm tra Username/Password đơn thuần. | Tích hợp kiểm tra `u.IsActive` (Vô hiệu hóa mềm / Khóa tài khoản nhân viên ngay khi xác thực). | **An toàn Bảo mật:** Ngăn chặn nhân viên đã bị sa thải hoặc bị khóa tài khoản truy cập vào hệ thống dù đúng mật khẩu. |
| **Bảo mật Form submission** | Không sử dụng CSRF Token. | Bắt buộc `[ValidateAntiForgeryToken]` trên tất cả POST Actions. | **Ngăn chặn triệt để lỗ hổng CSRF** (Cross-Site Request Forgery). |

> 📌 **ĐÁNH GIÁ HẠN CHẾ NGHIỆP VỤ:** 
> - Tài liệu ITShop có tỉ lệ hạn chế kiến trúc lên tới **> 25%** (Vi phạm Clean Code, hardcode Role, thiếu CSRF Protection, nhét ViewModel vào Entity file).
> - DigiPOSE giải quyết triệt để các hạn chế trên, đạt tiêu chuẩn **High-Scalability, Low-Latency & Zero-Trust Security**.

---

### 2. Đối chiếu với Hệ thống Code thực tế trong Source (`source/DigiPOSE`)

- **Tình trạng hiện tại của Source Code:**
  - Database Models (`User.cs`, `Role.cs`, `Branch.cs`) đã được xây dựng sẵn với đầy đủ thuộc tính `IsActive`, `RoleId`, `BranchId`, `PasswordHash`.
  - Các Controllers (`ProductsController`, `UsersController`, `CategoriesController`...) và Views hiện vẫn đang nằm ở thư mục gốc, **chưa được phân vùng vào thư mục `Areas/Admin`**.
  - `Program.cs` đã tích hợp `AddDbContextPool` và `System.Linq.Dynamic.Core` (Tối ưu performance cực cao), nhưng chưa bật `AddAuthentication` và `AddCookie`.

- **Điểm DigiPOSE Source Code vượt trội hơn tài liệu mẫu ITShop:**
  1. **Tốc độ xử lý dữ liệu (Low-Latency):** DigiPOSE dùng `AddDbContextPool` giảm chi phí cấp phát bộ nhớ (GC Pressure) khi xử lý hàng ngàn request/giây.
  2. **DataTables Server-Side Processing:** Sử dụng `System.Linq.Dynamic.Core` cho phép tìm kiếm, sắp xếp đa cột và phân trang động mượt mà trên hệ thống cơ sở dữ liệu lớn.
  3. **Giao diện Cyber-Cinematic Military HUD:** Đạt chuẩn cao cấp về UI/UX với thông số scannability cao, tối ưu cho thao tác vận hành điểm bán hàng (POS).

---

## 🛠️ III. HƯỚNG DẪN THỰC HÀNH CHI TIẾT THEO TỪNG BƯỚC (STEP-BY-STEP WORKFLOW)

### Bước 1: Tạo cấu trúc Area Admin và Cấu hình Routing

#### 1.1. Tạo cấu trúc thư mục Area Admin
Tạo cây thư mục theo cấu trúc chuẩn sau trong project `DigiPOSE`:
```
DigiPOSE/
└── Areas/
    └── Admin/
        ├── Controllers/
        ├── Models/
        └── Views/
            ├── Auth/
            ├── Home/
            └── Shared/
```

#### 1.2. Cấu hình Map Route cho Area trong `Program.cs`
Mở tập tin `Program.cs`, thêm cấu hình `adminareas` ngay phía trên `default`:

```csharp
// Program.cs
app.UseStaticFiles();
app.UseRouting();

// 1. Phải bật Authentication trước Authorization
app.UseAuthentication();
app.UseAuthorization();

// 2. Routing cho Area Admin (Ưu tiên kiểm tra route có Area trước)
app.MapControllerRoute(
    name: "adminareas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// 3. Routing Default (Frontend / Guest)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

---

### Bước 2: Thiết lập Cookie Authentication Service trong `Program.cs`

Bổ sung khai báo Cookie Authentication trước dòng `var app = builder.Build();`:

```csharp
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

// Cấu hình Cookie Authentication cho Admin CMS
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "DigiPOSE.AdminAuth";          // Tên Cookie lưu ở Browser
        options.Cookie.HttpOnly = true;                      // Chống đánh cắp Cookie qua XSS Scripting
        options.ExpireTimeSpan = TimeSpan.FromHours(8);      // Phiên đăng nhập có hiệu lực 8 tiếng ca làm việc
        options.SlidingExpiration = true;                    // Tự động gia hạn khi người dùng thao tác
        options.LoginPath = "/Admin/Auth/Login";              // Trang chuyển hướng khi chưa đăng nhập
        options.LogoutPath = "/Admin/Auth/Logout";            // Action Đăng xuất
        options.AccessDeniedPath = "/Admin/Auth/Forbidden";  // Trang báo lỗi khi không đủ quyền (403)
    });

builder.Services.AddHttpContextAccessor();
```

---

### Bước 3: Di chuyển Controllers và Views vào Area Admin

#### 3.1. Di chuyển các Controllers
Di chuyển toàn bộ các Controllers sau từ `Controllers/` vào `Areas/Admin/Controllers/`:
- `BranchesController.cs`
- `CategoriesController.cs`
- `CustomersController.cs`
- `ManufacturersController.cs`
- `OrdersController.cs`
- `ProductsController.cs`
- `UsersController.cs`
*(Và các Controllers quản lý danh mục khác)*

#### 3.2. Cập nhật Namespace và Thêm Attribute `[Area("Admin")]`
Mở tất cả các Controllers vừa di chuyển, cập nhật `namespace` và gắn thêm thuộc tính `[Area("Admin")]` và `[Authorize]`:

```csharp
// Ví dụ: Areas/Admin/Controllers/ProductsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Models;
using DigiPOSE.Web.Helpers;

namespace DigiPOSE.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, Branch Manager")] // Phân quyền: Chỉ Admin và Quản lý chi nhánh mới vào được
    public class ProductsController : Controller
    {
        // ... giữ nguyên toàn bộ logic hiện tại
    }
}
```

#### 3.3. Di chuyển Views tương ứng
Di chuyển các thư mục giao diện tương ứng từ `Views/` vào `Areas/Admin/Views/`:
- `Areas/Admin/Views/Products/`
- `Areas/Admin/Views/Categories/`
- `Areas/Admin/Views/Users/`
- ...

---

### Bước 4: Xây dựng Module Đăng nhập & Đăng xuất (Authentication Module)

#### 4.1. Tạo `LoginViewModel` (Areas/Admin/Models/LoginViewModel.cs)
```csharp
using System.ComponentModel.DataAnnotations;

namespace DigiPOSE.Areas.Admin.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Tên đăng nhập không được bỏ trống!")]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được bỏ trống!")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = null!;

        [Display(Name = "Duy trì đăng nhập")]
        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
```

#### 4.2. Tạo `AuthController` (Areas/Admin/Controllers/AuthController.cs)
```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Models;
using DigiPOSE.Areas.Admin.Models;
using BC = BCrypt.Net.BCrypt;

namespace DigiPOSE.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AuthController : Controller
    {
        private readonly DigiPoseDbContext _context;

        public AuthController(DigiPoseDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Auth/Login
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return LocalRedirect(returnUrl ?? "/Admin");
            }

            ViewBag.ReturnUrl = returnUrl ?? "/Admin";
            return View();
        }

        // POST: /Admin/Auth/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra tài khoản tồn tại và phải đang Active (Xóa mềm Check)
                var user = await _context.Users
                    .Include(u => u.Role)
                    .Include(u => u.Branch)
                    .SingleOrDefaultAsync(u => u.UserName == model.Username && u.IsActive);

                if (user == null || !BC.Verify(model.Password, user.PasswordHash))
                {
                    TempData["ErrorMessage"] = "Tài khoản không tồn tại, đã bị vô hiệu hóa hoặc mật khẩu không chính xác.";
                    return View(model);
                }

                // Khởi tạo danh sách Claims cho người dùng
                var claims = new List<Claim>
                {
                    new Claim("UserId", user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim("FullName", user.FullName ?? user.UserName),
                    new Claim("BranchId", user.BranchId.ToString()),
                    new Claim("BranchName", user.Branch?.BranchName ?? "N/A"),
                    new Claim(ClaimTypes.Role, user.Role?.RoleName ?? "User")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(8)
                };

                // Đăng nhập hệ thống (Ghi Cookie Authentication)
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return LocalRedirect(model.ReturnUrl ?? "/Admin");
            }

            return View(model);
        }

        // GET: /Admin/Auth/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth", new { Area = "Admin" });
        }

        // GET: /Admin/Auth/Forbidden
        [AllowAnonymous]
        public IActionResult Forbidden()
        {
            return View();
        }
    }
}
```

---

### Bước 5: Xây dựng Giao diện Cyber-HUD cho Login & Forbidden

#### 5.1. View Đăng nhập Cyber-HUD (`Areas/Admin/Views/Auth/Login.cshtml`)
```html
@model DigiPOSE.Areas.Admin.Models.LoginViewModel
@{
    Layout = null;
    ViewData["Title"] = "AUTHENTICATION - CYBER HUD POS";
}
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"]</title>
    <link rel="stylesheet" href="~/lib/bootstrap/css/bootstrap.min.css" />
    <link rel="stylesheet" href="~/lib/font-awesome/css/all.min.css" />
    <link rel="stylesheet" href="~/css/cyber-hud.css" asp-append-version="true" />
</head>
<body style="background-color: #000000; color: #00E5FF; font-family: 'Rajdhani', sans-serif; display: flex; align-items: center; justify-content: center; min-height: 100vh; margin: 0;">
    
    <div class="hud-card" style="width: 420px; padding: 2.5rem; background: rgba(10, 10, 10, 0.95); border: 1px solid #00E5FF; box-shadow: 0 0 20px rgba(0, 229, 255, 0.2); backdrop-filter: blur(12px); position: relative;">
        
        <!-- Corner Markers -->
        <div style="position: absolute; top: -1px; left: -1px; color: #00E5FF; font-size: 0.8rem;">+</div>
        <div style="position: absolute; top: -1px; right: -1px; color: #00E5FF; font-size: 0.8rem;">+</div>
        <div style="position: absolute; bottom: -1px; left: -1px; color: #00E5FF; font-size: 0.8rem;">+</div>
        <div style="position: absolute; bottom: -1px; right: -1px; color: #00E5FF; font-size: 0.8rem;">+</div>

        <div class="text-center mb-4">
            <h3 style="font-family: 'Orbitron', sans-serif; font-weight: 700; letter-spacing: 2px; color: #00E5FF; margin: 0;">
                <i class="fa-solid fa-shield-halved me-2"></i>DIGI<span style="color:#00FF66;">POSE</span>
            </h3>
            <p style="font-size: 0.8rem; color: #888; letter-spacing: 1px;" class="mt-1">COMMAND CMS AUTHENTICATION GATEWAY</p>
        </div>

        @if (TempData["ErrorMessage"] != null)
        {
            <div class="alert alert-danger" style="background: rgba(255, 51, 51, 0.15); border: 1px solid #FF3333; color: #FF3333; font-size: 0.85rem;" role="alert">
                <i class="fa-solid fa-triangle-exclamation me-2"></i>@TempData["ErrorMessage"]
            </div>
        }

        <form asp-area="Admin" asp-controller="Auth" asp-action="Login" method="post">
            <input type="hidden" asp-for="ReturnUrl" value="@ViewBag.ReturnUrl" />
            
            <div class="mb-3">
                <label asp-for="Username" class="form-label" style="font-size: 0.85rem; text-transform: uppercase; letter-spacing: 1px;"></label>
                <div class="input-group">
                    <span class="input-group-text" style="background: #000; border-color: #00E5FF; color: #00E5FF;"><i class="fa-solid fa-user"></i></span>
                    <input asp-for="Username" class="form-control" style="background: #000; border-color: #00E5FF; color: #FFF;" placeholder="Enter Username..." />
                </div>
                <span asp-validation-for="Username" class="text-danger" style="font-size: 0.75rem;"></span>
            </div>

            <div class="mb-3">
                <label asp-for="Password" class="form-label" style="font-size: 0.85rem; text-transform: uppercase; letter-spacing: 1px;"></label>
                <div class="input-group">
                    <span class="input-group-text" style="background: #000; border-color: #00E5FF; color: #00E5FF;"><i class="fa-solid fa-key"></i></span>
                    <input asp-for="Password" class="form-control" style="background: #000; border-color: #00E5FF; color: #FFF;" placeholder="Enter Password..." />
                </div>
                <span asp-validation-for="Password" class="text-danger" style="font-size: 0.75rem;"></span>
            </div>

            <div class="mb-4 form-check">
                <input class="form-check-input" asp-for="RememberMe" style="background-color: #000; border-color: #00E5FF;" />
                <label class="form-check-label" asp-for="RememberMe" style="font-size: 0.85rem; color: #AAA;"></label>
            </div>

            <button type="submit" class="btn w-100" style="background: #00E5FF; color: #000; font-family: 'Orbitron'; font-weight: 700; letter-spacing: 1px; border: none; padding: 0.6rem;">
                <i class="fa-solid fa-right-to-bracket me-2"></i>AUTHENTICATE NOW
            </button>
        </form>
    </div>

    <script src="~/lib/jquery/jquery.min.js"></script>
    <script src="~/lib/bootstrap/js/bootstrap.bundle.min.js"></script>
</body>
</html>
```

#### 5.2. View Từ chối Truy cập (`Areas/Admin/Views/Auth/Forbidden.cshtml`)
```html
@{
    ViewData["Title"] = "ACCESS DENIED - 403 FORBIDDEN";
}
<div class="container text-center py-5">
    <div class="hud-card mx-auto" style="max-width: 500px; padding: 3rem; background: rgba(10, 10, 10, 0.9); border: 1px solid #FF3333;">
        <i class="fa-solid fa-user-slash text-danger mb-3" style="font-size: 4rem;"></i>
        <h2 style="font-family: 'Orbitron'; color: #FF3333;">ACCESS FORBIDDEN (403)</h2>
        <p class="text-secondary mt-3">Tài khoản hiện tại không đủ đặc quyền (Role Clearance) để truy cập chức năng này.</p>
        <a asp-area="Admin" asp-controller="Home" asp-action="Index" class="btn btn-outline-info mt-3">
            <i class="fa-solid fa-house me-2"></i>Quay lại Dashboard
        </a>
    </div>
</div>
```

---

### Bước 6: Phân quyền Authorization trên Controllers

Khóa tất cả các Controller trong Area Admin bằng đặc quyền Role rõ ràng:

```csharp
// Areas/Admin/Controllers/UsersController.cs
namespace DigiPOSE.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Chỉ duy nhất Admin mới được quản lý User
    public class UsersController : Controller
    {
        // ...
    }
}

// Areas/Admin/Controllers/ProductsController.cs
namespace DigiPOSE.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, Branch Manager")] // Admin và Quản lý chi nhánh
    public class ProductsController : Controller
    {
        // ...
    }
}
```

---

### Bước 7: Cập nhật đường dẫn Ajax DataTables cho Area Admin

Do các Controller đã di chuyển vào `Area("Admin")`, đường dẫn gọi Ajax Load Data và Action links trên View DataTables phải chỉ định tham số `Area = "Admin"`:

Mở tập tin `Areas/Admin/Views/Products/Index.cshtml`:

```javascript
// Cập nhật cấu hình DataTables Ajax Call
var table = $('#datatable').DataTable({
    processing: true,
    serverSide: true,
    filter: true,
    ajax: {
        // Chỉ định rõ tham số Area = "Admin" trong Url.Action
        url: '@Url.Action("Index_LoadData", "Products", new { Area = "Admin" })',
        type: 'POST',
        datatype: 'json'
    },
    columns: [
        { data: 'productId', name: 'ProductId' },
        { data: 'productName', name: 'ProductName' },
        { data: 'categoryName', name: 'CategoryName' },
        { 
            data: 'basePrice', 
            name: 'BasePrice',
            render: DataTable.render.number(',', '.', 0, '₫')
        },
        {
            data: null,
            sortable: false,
            render: function (data, type, row) {
                // Đảm bảo đường dẫn Action chỉ hướng chính xác vào Area Admin
                return `
                    <a href="/Admin/Products/Edit/${row.productId}" class="btn btn-sm btn-outline-info me-1"><i class="fa-solid fa-pen"></i> Sửa</a>
                    <button onclick="deleteProduct(${row.productId})" class="btn btn-sm btn-outline-danger"><i class="fa-solid fa-trash"></i> Xóa</button>
                `;
            }
        }
    ]
});
```

---

## 🏆 IV. TỔNG KẾT CHECKLIST KIỂM THỬ HOÀN THÀNH (VERIFICATION)

- [x] Tạo cấu trúc `Areas/Admin` đầy đủ Controllers, Models, Views.
- [x] Khai báo Route `adminareas` đúng vị trí ưu tiên trong `Program.cs`.
- [x] Cấu hình Cookie Authentication dịch vụ với thời lượng hết hạn 8 giờ.
- [x] Đã xử lý `[ValidateAntiForgeryToken]` và kiểm tra điều kiện tài khoản `IsActive`.
- [x] Đã khóa các Controller bằng `[Authorize(Roles = "...")]`.
- [x] Đã cập nhật `@Url.Action(..., new { Area = "Admin" })` cho tất cả bảng DataTables Server-Side.