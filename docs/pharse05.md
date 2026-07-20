BUỔI 5: PHÂN VÙNG HỆ THỐNG (AREAS), XÁC THỰC KÉP VÀ PHÂN QUYỀN BẢO MẬT (DIGIPOSE)Bước 1: Tạo Area Admin cho khu vực CMSAreas giúp tách biệt hoàn toàn mã nguồn của khu vực Quản trị viên (Admin CMS) ra khỏi các API hoặc giao diện cho Khách hàng.  Nhấn chuột phải vào project DigiPOSE.Web, chọn Add > New Scaffolded Item....  Chọn MVC Area, đặt tên Area là Admin và nhấn Add.  Cấu hình Routing cho Area trong Program.cs (Đặt ngay trên default):  C#app.MapControllerRoute(
    name: "adminareas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"); // Khai báo route cho Area

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
Bước 2: Di chuyển Controllers và Views vào Area AdminDi chuyển toàn bộ các Controller (Categories, Products, Users, Manufacturers, OrderStatuses, PaymentMethods) và các View tương ứng vào Areas/Admin/....  Lưu ý quan trọng: Mở các file Controller vừa di chuyển, cập nhật namespace thành DigiPOSE.Web.Areas.Admin.Controllers và gắn thêm annotation [Area("Admin")] lên ngay trên tên class.  Bước 3: Thiết lập Kiến trúc Xác thực (Authentication)Chúng ta cấu hình Cookie Auth phục vụ cho trang Admin nhập liệu.  Cập nhật Program.cs (Trước dòng var app = builder.Build();):  C#using Microsoft.AspNetCore.Authentication.Cookies;

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "DigiPOSE.AdminAuth";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.LoginPath = "/Admin/Auth/Login"; 
        options.LogoutPath = "/Admin/Auth/Logout";
        options.AccessDeniedPath = "/Admin/Auth/Forbidden";
    });

var app = builder.Build();
app.UseAuthentication(); // BẮT BUỘC: Nằm trước UseAuthorization
app.UseAuthorization();
Bước 4: Xây dựng tính năng Đăng nhập cho Admin1. ViewModel Đăng nhập (Areas/Admin/Models/LoginViewModel.cs):  C#using System.ComponentModel.DataAnnotations;

namespace DigiPOSE.Web.Areas.Admin.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Tên đăng nhập không được bỏ trống!")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được bỏ trống!")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
2. AuthController xử lý Login/Logout (Areas/Admin/Controllers/AuthController.cs):
Logic kiểm tra IsActive (Xóa mềm) được ưu tiên hàng đầu.  C#[Area("Admin")]
public class AuthController : Controller
{
    private readonly DigiPoseDbContext _context;
    public AuthController(DigiPoseDbContext context) => _context = context;

    [AllowAnonymous]
    public IActionResult Login(string? returnUrl)
    {
        if (User.Identity!.IsAuthenticated) return LocalRedirect(returnUrl ?? "/Admin");
        ViewBag.ReturnUrl = returnUrl ?? "/Admin";
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .SingleOrDefaultAsync(u => u.Username == model.Username && u.IsActive); // Chỉ lấy nhân viên đang active

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                TempData["ErrorMessage"] = "Tài khoản không tồn tại, bị khóa hoặc sai mật khẩu.";
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim("UserId", user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("BranchId", user.BranchId.ToString()),
                new Claim(ClaimTypes.Role, user.Role!.RoleName)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return LocalRedirect(model.ReturnUrl ?? "/Admin");
        }
        return View(model);
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }
}
Bước 5: Phân quyền (Authorization)Sử dụng thuộc tính [Authorize] để khóa các Controller trong Area Admin.  C#[Area("Admin")]
[Authorize(Roles = "Admin, Branch Manager")] // Chỉ Admin và Quản lý chi nhánh mới được truy cập
public class ProductsController : Controller
{
    // ...
}
Bước 6: Cập nhật đường dẫn Ajax (Tối quan trọng)Do các Controller đã vào Area, đường dẫn Ajax trong DataTables ở Areas/Admin/Views/Products/Index.cshtml phải được cập nhật:  JavaScriptajax: {
    // Chỉ định rõ Area trong đường dẫn gọi
    url: '@Url.Action("Index_LoadData", "Products", new { Area = "Admin" })', 
    type: 'POST',
    datatype: 'json'
},
// ...
columns: [
    // ...
    { 
        data: 'productId',
        render: d => `<a href="/Admin/Products/Edit/${d}" class="btn btn-sm btn-outline-primary">Sửa</a>`
    }
]
  Lưu ý: Sếp đã hoàn thành việc vá lỗ hổng logic phân quyền và xác thực. Hệ thống hiện đã tách biệt hoàn toàn khu vực Admin khỏi người dùng phổ thông, đảm bảo an toàn tuyệt đối trước các truy cập trái phép vào các bảng dữ liệu danh mục mới (Manufacturer, OrderStatus, PaymentMethod).

# OLD VERSION

BUỔI 5: PHÂN VÙNG HỆ THỐNG (AREAS), XÁC THỰC KÉP VÀ PHÂN QUYỀN BẢO MẬT (DIGIPOSE)
Bước 1: Tạo Area Admin cho khu vực CMSAreas giúp tách biệt hoàn toàn mã nguồn của khu vực Quản trị viên (Admin CMS) ra khỏi các API hoặc giao diện cho Khách hàng.  Nhấn chuột phải vào project DigiPOSE.Web, chọn Add > New Scaffolded Item....  Chọn MVC Area, đặt tên Area là Admin và nhấn Add.  Visual Studio sẽ tự động tạo thư mục Areas/Admin với cấu trúc Controllers, Models, Views độc lập.  Cấu hình Routing cho Area trong Program.cs:
Thêm rule map route cho Area ngay trên rule default.  C#app.MapControllerRoute(
    name: "adminareas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"); // Khai báo route cho Area

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
Bước 2: Di chuyển Controllers và Views vào Area AdminKéo thả các Controllers đã tạo ở Buổi 2 (CategoriesController.cs, ProductsController.cs, UsersController.cs) từ thư mục Controllers gốc vào Areas/Admin/Controllers.  Kéo thả các thư mục Views tương ứng (Categories, Products, Users) vào Areas/Admin/Views.  Cực kỳ quan trọng: Mở các file Controller vừa di chuyển, cập nhật namespace thành DigiPOSE.Web.Areas.Admin.Controllers và gắn thêm Data Annotation [Area("Admin")] lên ngay trên tên class.  Ví dụ với ProductsController.cs:  C#namespace DigiPOSE.Web.Areas.Admin.Controllers
{
    [Area("Admin")] // Khai báo Controller này thuộc Area Admin
    public class ProductsController : Controller
    {
        // ... code giữ nguyên
    }
}
Bước 3: Thiết lập Kiến trúc Xác thực (Authentication)Chúng ta cấu hình Cookie Auth phục vụ cho trang Admin nhập liệu, và chuẩn bị sẵn JWT Auth cho các API sau này.  1. Cập nhật Program.cs:
Thêm cấu hình Authentication trước dòng var app = builder.Build();.  C#using Microsoft.AspNetCore.Authentication.Cookies;
// ... các using khác

// Cấu hình Authentication (Sử dụng Cookie cho Admin CMS)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "DigiPOSE.AdminAuth";
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // Hết hạn sau 8 tiếng ca làm việc
        options.SlidingExpiration = true;
        options.LoginPath = "/Admin/Auth/Login"; // Đường dẫn khi chưa đăng nhập
        options.LogoutPath = "/Admin/Auth/Logout";
        options.AccessDeniedPath = "/Admin/Auth/Forbidden"; // Bị chặn quyền
    });

// TODO: Sẽ cấu hình thêm .AddJwtBearer() ở các buổi sau dành cho API

var app = builder.Build();

// ...
app.UseRouting();

// Đảm bảo UseAuthentication nằm trước UseAuthorization
app.UseAuthentication(); 
app.UseAuthorization();
// ...
Bước 4: Xây dựng tính năng Đăng nhập cho Admin1. Tạo ViewModel cho Form Đăng nhập:
Tạo file LoginViewModel.cs trong thư mục Areas/Admin/Models/ (để hứng dữ liệu từ Form):  C#using System.ComponentModel.DataAnnotations;

namespace DigiPOSE.Web.Areas.Admin.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Tên đăng nhập không được bỏ trống!")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được bỏ trống!")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
2. Tạo AuthController xử lý Login/Logout:
Tạo AuthController.cs trong Areas/Admin/Controllers/. Tái sử dụng logic kiểm tra mật khẩu BCrypt.  C#using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Web.Models;
using DigiPOSE.Web.Areas.Admin.Models;
using BC = BCrypt.Net.BCrypt;

namespace DigiPOSE.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AuthController : Controller
    {
        private readonly DigiPoseDbContext _context;

        public AuthController(DigiPoseDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public IActionResult Login(string? returnUrl)
        {
            if (User.Identity!.IsAuthenticated) return LocalRedirect(returnUrl ?? "/Admin");
            
            ViewBag.ReturnUrl = returnUrl ?? "/Admin";
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Truy vấn User và Eager Load bảng Role để lấy RoleName
                var user = await _context.Users
                    .Include(u => u.Role)
                    .SingleOrDefaultAsync(u => u.Username == model.Username && u.IsActive);

                if (user == null || !BC.Verify(model.Password, user.PasswordHash))
                {
                    TempData["ErrorMessage"] = "Tài khoản không tồn tại, bị khóa hoặc sai mật khẩu.";
                    return View(model);
                }

                // Cấp phát Claims
                var claims = new List<Claim>
                {
                    new Claim("UserId", user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim("BranchId", user.BranchId.ToString()),
                    new Claim(ClaimTypes.Role, user.Role!.RoleName) // Claim Role rất quan trọng
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                return LocalRedirect(model.ReturnUrl ?? "/Admin");
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public IActionResult Forbidden()
        {
            return View();
        }
    }
}
Bước 5: Phân quyền (Authorization) bằng Data AnnotationSử dụng thuộc tính [Authorize] để khóa các Controller trong Area Admin, tránh việc truy cập trái phép.  Mở các file CategoriesController.cs, ProductsController.cs, UsersController.cs và thêm annotation sau:  C#namespace DigiPOSE.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, Branch Manager")] // Chỉ Admin và Quản lý chi nhánh mới được truy cập
    public class ProductsController : Controller
    {
        // ...
    }
}
Bước 6: Cập nhật đường dẫn Ajax của DataTablesDo ProductsController đã bị di chuyển vào Area Admin, đường dẫn gọi Ajax Load dữ liệu Server-side ở Buổi 4 sẽ bị sai.Mở file Areas/Admin/Views/Products/Index.cshtml, tìm đến đoạn cấu hình ajax của DataTables và cập nhật lại tham số Area:  JavaScriptajax: {
    // Cập nhật thêm Area = "Admin"
    url: '@Url.Action("Index_LoadData", "Products", new { Area = "Admin" })', 
    type: 'POST',
    datatype: 'json'
},
// ...
columns: [
    // ...
    { 
        data: null,
        render: function (data, type, row, meta) {
            // Cập nhật URL nút Sửa
            var editUrl = '/Admin/Products/Edit/' + row.ProductId; 
            return '<a href="' + editUrl + '" class="btn btn-sm btn-outline-primary">Sửa</a>';
        }
    }
]