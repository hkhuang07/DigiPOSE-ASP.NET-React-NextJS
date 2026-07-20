BUỔI 2: TÙY BIẾN GIAO DIỆN (VIEW) VÀ XỬ LÝ LOGIC (CONTROLLER) CHO ADMIN CMS DIGIPOSE(Ghi chú: Thay vì làm thủ công cho toàn bộ các bảng, tài liệu này sẽ hướng dẫn tùy biến 3 bảng cốt lõi mang tính đại diện nhất: Category (Bảng từ điển), User (Bảng chứa logic mã hóa) và Product (Bảng chứa nhiều Khóa ngoại và ràng buộc số liệu). Các bảng khác áp dụng tư duy tương tự). 


Bước 1: Cập nhật giao diện Trang chủ Admin (Dashboard)Chỉnh sửa tập tin Views/Home/Index.cshtml để tạo điểm neo giao diện cho hệ thống quản trị.  HTML@{
    ViewData["Title"] = "Trang chủ Admin";
}

<div class="card shadow-sm border-0">
    <h5 class="card-header bg-dark text-white">@ViewData["Title"] - DigiPOSE ERP</h5>
    <div class="card-body">
        <h4 class="card-title">Hệ thống quản lý điểm bán hàng tập trung</h4>
        <p class="card-text text-muted">Vui lòng sử dụng thanh điều hướng bên trên để thao tác với các danh mục, nhân sự và hàng hóa.</p>
        <div class="row mt-4">
            <div class="col-md-4">
                <div class="alert alert-info">
                    <i class="fa fa-users"></i> Quản lý Nhân sự & Phân quyền
                </div>
            </div>
            <div class="col-md-4">
                <div class="alert alert-success">
                    <i class="fa fa-cubes"></i> Quản trị Hàng hóa & Kho bãi
                </div>
            </div>
            <div class="col-md-4">
                <div class="alert alert-warning">
                    <i class="fa fa-bar-chart"></i> Giám sát Giao dịch & Dòng tiền
                </div>
            </div>
        </div>
    </div>
</div>
Bước 2: Tùy biến View và Controller cho Danh mục (Category)Đại diện cho các bảng dữ liệu tĩnh (Từ điển). 
 1. Giao diện Danh sách (Views/Categories/Index.cshtml):  HTML@model IEnumerable<DigiPOSE.Web.Models.Category>
@{
    ViewData["Title"] = "Nhóm hàng hóa";
}
<div class="card border-0 shadow-sm">
    <h5 class="card-header bg-primary text-white">@ViewData["Title"]</h5>
    <div class="card-body">
        <p><a asp-action="Create" class="btn btn-success"><i class="fa-light fa-plus"></i> Thêm nhóm hàng mới</a></p>
        <table class="table table-sm table-hover table-striped table-bordered mb-0">
            <thead class="table-light">
                <tr>
                    <th width="5%">#</th>
                    <th>@Html.DisplayNameFor(model => model.CategoryName)</th>
                    <th width="5%" class="text-center">Sửa</th>
                    <th width="5%" class="text-center">Xóa</th>
                </tr>
            </thead>
            <tbody>
                @{ int stt = 1; }
                @foreach (var item in Model) {
                    <tr>
                        <td>@stt</td>
                        <td class="fw-bold text-primary">@Html.DisplayFor(modelItem => item.CategoryName)</td>
                        <td class="text-center">
                            <a asp-action="Edit" asp-route-id="@item.CategoryId" class="btn btn-sm btn-outline-warning">Sửa</a>
                        </td>
                        <td class="text-center">
                            <!-- Nút Xóa ở đây sẽ gọi hàm xử lý Xóa Mềm thay vì xóa cứng -->
                            <a asp-action="Delete" asp-route-id="@item.CategoryId" class="btn btn-sm btn-outline-danger">Xóa</a>
                        </td>
                    </tr>
                    stt++;
                }
            </tbody>
        </table>
    </div>
</div>
2. Cập nhật CategoriesController.cs:  
Controller tự động sinh bằng Scaffolding đã khá hoàn chỉnh. Ta chỉ cần tinh chỉnh lại phần Edit để bẫy lỗi Concurrency cho an toàn, đồng thời chèn thêm logic lọc IsActive == true để bảo vệ hệ thống khỏi rác dữ liệu.  C#using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Web.Models;

namespace DigiPOSE.Web.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly DigiPoseDbContext _context;

        public CategoriesController(DigiPoseDbContext context)
        {
            _context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            // [CẬP NHẬT KIẾN TRÚC]: Chỉ lấy các danh mục đang hoạt động (Xóa Mềm)
            var activeCategories = await _context.Categories
                .Where(c => c.IsActive)
                .ToListAsync();
            return View(activeCategories);
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryId,CategoryName,IsActive")] Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryId,CategoryName,IsActive")] Category category)
        {
            if (id != category.CategoryId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Categories.Any(e => e.CategoryId == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }
    }
}
Bước 3: Tùy biến View và Controller cho Nhân sự (User)Đại diện cho các bảng chứa tính toán logic ngầm (Mã hóa mật khẩu) và liên kết nhiều Khóa ngoại (BranchId, RoleId).  1. Giao diện Thêm mới (Views/Users/Create.cshtml):  
(Lưu ý: Không hiển thị PasswordHash ở chế độ plain text. Sử dụng thẻ input password).  HTML@model DigiPOSE.Web.Models.User
@{
    ViewData["Title"] = "Thêm nhân sự";
}
<div class="card border-0 shadow-sm">
    <h5 class="card-header bg-primary text-white">@ViewData["Title"]</h5>
    <div class="card-body">
        <form asp-action="Create">
            <div asp-validation-summary="ModelOnly" class="text-danger"></div>
            
            <div class="row">
                <div class="col-md-6 mb-3">
                    <label asp-for="BranchId" class="control-label"></label>
                    <select asp-for="BranchId" class="form-select" asp-items="ViewBag.BranchId">
                        <option value="">-- Chọn Chi nhánh --</option>
                    </select>
                </div>
                <div class="col-md-6 mb-3">
                    <label asp-for="RoleId" class="control-label"></label>
                    <select asp-for="RoleId" class="form-select" asp-items="ViewBag.RoleId">
                        <option value="">-- Chọn Quyền hạn --</option>
                    </select>
                </div>
            </div>

            <div class="row">
                <div class="col-md-6 mb-3">
                    <label asp-for="Username" class="control-label"></label>
                    <input asp-for="Username" class="form-control" placeholder="Tên đăng nhập hệ thống" />
                    <span asp-validation-for="Username" class="text-danger"></span>
                </div>
                <div class="col-md-6 mb-3">
                    <label asp-for="PasswordHash" class="control-label"></label>
                    <input asp-for="PasswordHash" type="password" class="form-control" placeholder="Mật khẩu" />
                    <span asp-validation-for="PasswordHash" class="text-danger"></span>
                </div>
            </div>

            <div class="mb-3 form-check">
                <label class="form-check-label">
                    <input class="form-check-input" asp-for="IsActive" /> Đang hoạt động
                </label>
            </div>

            <div class="mb-0">
                <button type="submit" class="btn btn-primary"><i class="fa fa-save"></i> Lưu nhân sự</button> 
                <a asp-action="Index" class="btn btn-light ms-2">Hủy</a>
            </div>
        </form>
    </div>
</div>
2. Cập nhật UsersController.cs xử lý mã hóa BCrypt:  C#using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Web.Models;
using BC = BCrypt.Net.BCrypt;

namespace DigiPOSE.Web.Controllers
{
    public class UsersController : Controller
    {
        private readonly DigiPoseDbContext _context;

        public UsersController(DigiPoseDbContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            // Eager Loading các khóa ngoại và lọc Xóa mềm
            var users = _context.Users
                .Include(u => u.Branch)
                .Include(u => u.Role)
                .Where(u => u.IsActive);
                
            return View(await users.ToListAsync());
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            // Chỉ load những Branch và Role đang active
            ViewData["BranchId"] = new SelectList(_context.Branches.Where(b => b.IsActive), "BranchId", "BranchName");
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleName");
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,RoleId,BranchId,Username,PasswordHash,IsActive")] User user)
        {
            if (ModelState.IsValid)
            {
                // Băm mật khẩu trước khi lưu vào Database
                user.PasswordHash = BC.HashPassword(user.PasswordHash);
                
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BranchId"] = new SelectList(_context.Branches.Where(b => b.IsActive), "BranchId", "BranchName", user.BranchId);
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleName", user.RoleId);
            return View(user);
        }
    }
}
Bước 4: Tùy biến View và Controller cho Hàng hóa (Product)Đại diện cho bảng trung tâm, chứa thông tin giá cả, mã vạch và cần format tiền tệ chuẩn xác. (Đã tích hợp bảng Manufacturer từ V2.0).  1. Giao diện Danh sách (Views/Products/Index.cshtml):  HTML@model IEnumerable<DigiPOSE.Web.Models.Product>
@{
    ViewData["Title"] = "Hàng hóa";
}
<div class="card border-0 shadow-sm">
    <h5 class="card-header bg-success text-white">@ViewData["Title"]</h5>
    <div class="card-body">
        <p><a asp-action="Create" class="btn btn-success"><i class="fa fa-plus"></i> Thêm hàng hóa</a></p>
        <table class="table table-sm table-hover table-bordered mb-0">
            <thead class="table-light">
                <tr>
                    <th width="5%">#</th>
                    <th width="10%">Mã SKU</th>
                    <th>@Html.DisplayNameFor(model => model.ProductName)</th>
                    <th width="12%">Danh mục</th>
                    <th width="12%">Hãng SX</th>
                    <th width="8%">ĐVT</th>
                    <th width="12%" class="text-end">Giá cơ sở (VNĐ)</th>
                    <th width="10%" class="text-center">Thao tác</th>
                </tr>
            </thead>
            <tbody>
                @{ int stt = 1; }
                @foreach (var item in Model) {
                    <tr>
                        <td>@stt</td>
                        <td><span class="badge bg-secondary">@item.SKU</span></td>
                        <td class="fw-bold">@item.ProductName</td>
                        <td>@item.Category?.CategoryName</td>
                        <td>@item.Manufacturer?.ManufacturerName</td>
                        <td>@item.Unit?.UnitName</td>
                        <td class="text-end text-danger fw-bold">
                            @string.Format("{0:N0}", item.BasePrice)
                        </td>
                        <td class="text-center">
                            <a asp-action="Edit" asp-route-id="@item.ProductId" class="btn btn-sm btn-outline-primary">Sửa</a>
                        </td>
                    </tr>
                    stt++;
                }
            </tbody>
        </table>
    </div>
</div>
2. Cập nhật ProductsController.cs:[cite: 3]C#using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Web.Models;

namespace DigiPOSE.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly DigiPoseDbContext _context;

        public ProductsController(DigiPoseDbContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            // Tối ưu hóa truy vấn Eager Loading để lấy Name của Category, Unit VÀ Manufacturer[cite: 3]
            var products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Unit)
                .Include(p => p.Manufacturer) // Bổ sung liên kết Hãng sản xuất
                .Where(p => p.IsActive); // Áp dụng cơ chế Xóa mềm
                
            return View(await products.ToListAsync());
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            // Đổ dữ liệu Dropdown List cho Frontend
            ViewData["CategoryId"] = new SelectList(_context.Categories.Where(c => c.IsActive), "CategoryId", "CategoryName");
            ViewData["UnitId"] = new SelectList(_context.Units, "UnitId", "UnitName");
            ViewData["ManufacturerId"] = new SelectList(_context.Manufacturers.Where(m => m.IsActive), "ManufacturerId", "ManufacturerName");
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,CategoryId,UnitId,ManufacturerId,SKU,ProductName,BasePrice,IsActive")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            // Re-bind lại dữ liệu nếu Form không hợp lệ
            ViewData["CategoryId"] = new SelectList(_context.Categories.Where(c => c.IsActive), "CategoryId", "CategoryName", product.CategoryId);
            ViewData["UnitId"] = new SelectList(_context.Units, "UnitId", "UnitName", product.UnitId);
            ViewData["ManufacturerId"] = new SelectList(_context.Manufacturers.Where(m => m.IsActive), "ManufacturerId", "ManufacturerName", product.ManufacturerId);
            return View(product);
        }
    }
}





# OLD VERSION
BUỔI 2: TÙY BIẾN GIAO DIỆN (VIEW) VÀ XỬ LÝ LOGIC (CONTROLLER) CHO ADMIN CMS DIGIPOSE
(Ghi chú: Thay vì làm thủ công cho toàn bộ 16 bảng, tài liệu này sẽ hướng dẫn tùy biến 3 bảng cốt lõi mang tính đại diện nhất: Category (Bảng từ điển), User (Bảng chứa logic mã hóa) và Product (Bảng chứa nhiều Khóa ngoại và ràng buộc số liệu). Các bảng khác áp dụng tư duy tương tự).

Bước 1: Cập nhật giao diện Trang chủ Admin (Dashboard)
Chỉnh sửa tập tin Views/Home/Index.cshtml để tạo điểm neo giao diện cho hệ thống quản trị.  
PDF

HTML
@{
    ViewData["Title"] = "Trang chủ Admin";
}

<div class="card shadow-sm border-0">
    <h5 class="card-header bg-dark text-white">@ViewData["Title"] - DigiPOSE ERP</h5>
    <div class="card-body">
        <h4 class="card-title">Hệ thống quản lý điểm bán hàng tập trung</h4>
        <p class="card-text text-muted">Vui lòng sử dụng thanh điều hướng bên trên để thao tác với các danh mục, nhân sự và hàng hóa.</p>
        <div class="row mt-4">
            <div class="col-md-4">
                <div class="alert alert-info">
                    <i class="fa fa-users"></i> Quản lý Nhân sự & Phân quyền
                </div>
            </div>
            <div class="col-md-4">
                <div class="alert alert-success">
                    <i class="fa fa-cubes"></i> Quản trị Hàng hóa & Kho bãi
                </div>
            </div>
            <div class="col-md-4">
                <div class="alert alert-warning">
                    <i class="fa fa-bar-chart"></i> Giám sát Giao dịch & Dòng tiền
                </div>
            </div>
        </div>
    </div>
</div>
Bước 2: Tùy biến View và Controller cho Danh mục (Category)
Đại diện cho các bảng dữ liệu tĩnh (Từ điển).

1. Giao diện Danh sách (Views/Categories/Index.cshtml):  
PDF

HTML
@model IEnumerable<DigiPOSE.Web.Models.Category>
@{
    ViewData["Title"] = "Nhóm hàng hóa";
}
<div class="card border-0 shadow-sm">
    <h5 class="card-header bg-primary text-white">@ViewData["Title"]</h5>
    <div class="card-body">
        <p><a asp-action="Create" class="btn btn-success"><i class="fa-light fa-plus"></i> Thêm nhóm hàng mới</a></p>
        <table class="table table-sm table-hover table-striped table-bordered mb-0">
            <thead class="table-light">
                <tr>
                    <th width="5%">#</th>
                    <th>@Html.DisplayNameFor(model => model.CategoryName)</th>
                    <th width="5%" class="text-center">Sửa</th>
                    <th width="5%" class="text-center">Xóa</th>
                </tr>
            </thead>
            <tbody>
                @{ int stt = 1; }
                @foreach (var item in Model) {
                    <tr>
                        <td>@stt</td>
                        <td class="fw-bold text-primary">@Html.DisplayFor(modelItem => item.CategoryName)</td>
                        <td class="text-center">
                            <a asp-action="Edit" asp-route-id="@item.CategoryId" class="btn btn-sm btn-outline-warning">Sửa</a>
                        </td>
                        <td class="text-center">
                            <a asp-action="Delete" asp-route-id="@item.CategoryId" class="btn btn-sm btn-outline-danger">Xóa</a>
                        </td>
                    </tr>
                    stt++;
                }
            </tbody>
        </table>
    </div>
</div>
2. Cập nhật CategoriesController.cs:
Controller tự động sinh bằng Scaffolding đã khá hoàn chỉnh. Ta chỉ cần tinh chỉnh lại phần Edit để bẫy lỗi Concurrency cho an toàn:  
PDF

C#
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Web.Models;

namespace DigiPOSE.Web.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly DigiPoseDbContext _context;

        public CategoriesController(DigiPoseDbContext context)
        {
            _context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories.ToListAsync());
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryId,CategoryName")] Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryId,CategoryName")] Category category)
        {
            if (id != category.CategoryId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Categories.Any(e => e.CategoryId == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }
    }
}
Bước 3: Tùy biến View và Controller cho Nhân sự (User)
Đại diện cho các bảng chứa tính toán logic ngầm (Mã hóa mật khẩu) và liên kết nhiều Khóa ngoại (BranchId, RoleId).  
PDF

1. Giao diện Thêm mới (Views/Users/Create.cshtml):
(Lưu ý: Không hiển thị PasswordHash ở chế độ plain text. Sử dụng thẻ input password).  
PDF

HTML
@model DigiPOSE.Web.Models.User
@{
    ViewData["Title"] = "Thêm nhân sự";
}
<div class="card border-0 shadow-sm">
    <h5 class="card-header bg-primary text-white">@ViewData["Title"]</h5>
    <div class="card-body">
        <form asp-action="Create">
            <div asp-validation-summary="ModelOnly" class="text-danger"></div>
            
            <div class="row">
                <div class="col-md-6 mb-3">
                    <label asp-for="BranchId" class="control-label"></label>
                    <select asp-for="BranchId" class="form-select" asp-items="ViewBag.BranchId">
                        <option value="">-- Chọn Chi nhánh --</option>
                    </select>
                </div>
                <div class="col-md-6 mb-3">
                    <label asp-for="RoleId" class="control-label"></label>
                    <select asp-for="RoleId" class="form-select" asp-items="ViewBag.RoleId">
                        <option value="">-- Chọn Quyền hạn --</option>
                    </select>
                </div>
            </div>

            <div class="row">
                <div class="col-md-6 mb-3">
                    <label asp-for="Username" class="control-label"></label>
                    <input asp-for="Username" class="form-control" placeholder="Tên đăng nhập hệ thống" />
                    <span asp-validation-for="Username" class="text-danger"></span>
                </div>
                <div class="col-md-6 mb-3">
                    <label asp-for="PasswordHash" class="control-label"></label>
                    <input asp-for="PasswordHash" type="password" class="form-control" placeholder="Mật khẩu" />
                    <span asp-validation-for="PasswordHash" class="text-danger"></span>
                </div>
            </div>

            <div class="mb-3 form-check">
                <label class="form-check-label">
                    <input class="form-check-input" asp-for="IsActive" /> Đang hoạt động
                </label>
            </div>

            <div class="mb-0">
                <button type="submit" class="btn btn-primary"><i class="fa fa-save"></i> Lưu nhân sự</button> 
                <a asp-action="Index" class="btn btn-light ms-2">Hủy</a>
            </div>
        </form>
    </div>
</div>
2. Cập nhật UsersController.cs xử lý mã hóa BCrypt:  
PDF

C#
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Web.Models;
using BC = BCrypt.Net.BCrypt;

namespace DigiPOSE.Web.Controllers
{
    public class UsersController : Controller
    {
        private readonly DigiPoseDbContext _context;

        public UsersController(DigiPoseDbContext context)
        {
            _context = context;
        }

        // GET: Users
        public async Task<IActionResult> Index()
        {
            // Eager Loading các khóa ngoại
            var users = _context.Users.Include(u => u.Branch).Include(u => u.Role);
            return View(await users.ToListAsync());
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "BranchName");
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleName");
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,RoleId,BranchId,Username,PasswordHash,IsActive")] User user)
        {
            if (ModelState.IsValid)
            {
                // Băm mật khẩu trước khi lưu vào Database
                user.PasswordHash = BC.HashPassword(user.PasswordHash);
                
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "BranchName", user.BranchId);
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleName", user.RoleId);
            return View(user);
        }
    }
}
Bước 4: Tùy biến View và Controller cho Hàng hóa (Product)
Đại diện cho bảng trung tâm, chứa thông tin giá cả, mã vạch và cần format tiền tệ chuẩn xác.  
PDF

1. Giao diện Danh sách (Views/Products/Index.cshtml):  
PDF

HTML
@model IEnumerable<DigiPOSE.Web.Models.Product>
@{
    ViewData["Title"] = "Hàng hóa";
}
<div class="card border-0 shadow-sm">
    <h5 class="card-header bg-success text-white">@ViewData["Title"]</h5>
    <div class="card-body">
        <p><a asp-action="Create" class="btn btn-success"><i class="fa fa-plus"></i> Thêm hàng hóa</a></p>
        <table class="table table-sm table-hover table-bordered mb-0">
            <thead class="table-light">
                <tr>
                    <th width="5%">#</th>
                    <th width="15%">Mã SKU</th>
                    <th>@Html.DisplayNameFor(model => model.ProductName)</th>
                    <th width="15%">Danh mục</th>
                    <th width="10%">ĐVT</th>
                    <th width="15%" class="text-end">Giá cơ sở (VNĐ)</th>
                    <th width="10%" class="text-center">Thao tác</th>
                </tr>
            </thead>
            <tbody>
                @{ int stt = 1; }
                @foreach (var item in Model) {
                    <tr>
                        <td>@stt</td>
                        <td><span class="badge bg-secondary">@item.SKU</span></td>
                        <td class="fw-bold">@item.ProductName</td>
                        <td>@item.Category?.CategoryName</td>
                        <td>@item.Unit?.UnitName</td>
                        <td class="text-end text-danger fw-bold">
                            @string.Format("{0:N0}", item.BasePrice)
                        </td>
                        <td class="text-center">
                            <a asp-action="Edit" asp-route-id="@item.ProductId" class="btn btn-sm btn-outline-primary">Sửa</a>
                        </td>
                    </tr>
                    stt++;
                }
            </tbody>
        </table>
    </div>
</div>
2. Cập nhật ProductsController.cs:  
PDF

C#
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Web.Models;

namespace DigiPOSE.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly DigiPoseDbContext _context;

        public ProductsController(DigiPoseDbContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            // Tối ưu hóa truy vấn Eager Loading để lấy Name của Category và Unit
            var products = _context.Products.Include(p => p.Category).Include(p => p.Unit);
            return View(await products.ToListAsync());
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            ViewData["UnitId"] = new SelectList(_context.Units, "UnitId", "UnitName");
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,CategoryId,UnitId,SKU,ProductName,BasePrice")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            ViewData["UnitId"] = new SelectList(_context.Units, "UnitId", "UnitName", product.UnitId);
            return View(product);
        }
    }
}