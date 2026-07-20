BUỔI 3: DATA ANNOTATIONS, XỬ LÝ UPLOAD HÌNH ẢNH & TỐI ƯU HÓA MẬT KHẨU (DIGIPOSE)
Bước 1: Ràng buộc Dữ liệu (Data Annotations) cho các Model cốt lõiViệc bổ sung Data Annotations sẽ giúp tự động hóa việc xác thực dữ liệu (Validation) ở cả phía Client (trình duyệt) và Server, giúp hệ thống tránh được các lỗi nhập liệu ngớ ngẩn.  1. Bảng Customer (Khách hàng)
Mở file Models/Customer.cs, cập nhật các thuộc tính:  C#using System.ComponentModel.DataAnnotations;

namespace DigiPOSE.Web.Models
{
    public class Customer
    {
        [Key] public int CustomerId { get; set; }
        
        [Display(Name = "Loại khách hàng")]
        [Required(ErrorMessage = "Vui lòng chọn phân loại khách hàng.")]
        public int CustomerTypeId { get; set; }
        
        [Display(Name = "Họ và tên")]
        [Required(ErrorMessage = "Họ và tên không được bỏ trống.")]
        [StringLength(100)]
        public string FullName { get; set; } = null!;
        
        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "Số điện thoại không được bỏ trống.")]
        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ.")]
        [StringLength(20)]
        public string? Phone { get; set; }
        
        [Display(Name = "Điểm thưởng")]
        public int RewardPoints { get; set; } = 0;

        [Display(Name = "Đang hoạt động")]
        public bool IsActive { get; set; } = true;

        public CustomerType? CustomerType { get; set; }
    }
}
2. Bảng Product (Hàng hóa - Đã đồng bộ V2.0)
Mở file Models/Product.cs, cập nhật định dạng hiển thị tiền tệ, chuẩn hóa mã vạch và bổ sung ManufacturerId cùng IsActive:  C#using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiPOSE.Web.Models
{
    public class Product
    {
        [Key] public int ProductId { get; set; }
        
        [Display(Name = "Danh mục")]
        [Required(ErrorMessage = "Vui lòng chọn danh mục.")]
        public int CategoryId { get; set; }
        
        [Display(Name = "Đơn vị tính")]
        [Required(ErrorMessage = "Vui lòng chọn đơn vị tính.")]
        public int UnitId { get; set; }

        [Display(Name = "Hãng sản xuất")]
        public int? ManufacturerId { get; set; }
        
        [Display(Name = "Mã SKU (Barcode)")]
        [Required(ErrorMessage = "Mã SKU không được bỏ trống.")]
        [StringLength(50)]
        public string SKU { get; set; } = null!;
        
        [Display(Name = "Tên sản phẩm")]
        [Required(ErrorMessage = "Tên sản phẩm không được bỏ trống.")]
        [StringLength(150)]
        public string ProductName { get; set; } = null!;
        
        [Display(Name = "Giá cơ sở (VNĐ)")]
        [Required(ErrorMessage = "Giá bán không được bỏ trống.")]
        [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = false)]
        [Column(TypeName = "decimal(18,0)")]
        public decimal BasePrice { get; set; }

        // Trường dùng để chứa tên File ảnh lưu trong Database
        [Display(Name = "Đường dẫn ảnh")]
        [StringLength(255)]
        public string? ImageUrl { get; set; }

        // Trường ảo dùng để bắt file Upload từ Form (Không lưu vào DB)
        [NotMapped]
        [Display(Name = "Tải ảnh sản phẩm")]
        public IFormFile? ImageUpload { get; set; }
        
        [Display(Name = "Đang kinh doanh")]
        public bool IsActive { get; set; } = true;

        [Timestamp] public byte[]? RowVersion { get; set; }

        public Category? Category { get; set; }
        public Unit? Unit { get; set; }
        public Manufacturer? Manufacturer { get; set; }
    }
}
Bước 2: Xử lý Upload Hình ảnh Sản phẩm1. Cập nhật View Thêm mới Sản phẩm (Views/Products/Create.cshtml):
Cần phải thêm thuộc tính enctype="multipart/form-data" vào thẻ <form> để cho phép upload file.  HTML@model DigiPOSE.Web.Models.Product
@{
    ViewData["Title"] = "Thêm sản phẩm mới";
}
<div class="card border-0 shadow-sm">
    <h5 class="card-header bg-primary text-white">@ViewData["Title"]</h5>
    <div class="card-body">
        <form asp-action="Create" enctype="multipart/form-data">
            <div asp-validation-summary="ModelOnly" class="text-danger"></div>
            
            <div class="row">
                <div class="col-md-4 mb-3">
                    <label asp-for="CategoryId" class="control-label"></label>
                    <select asp-for="CategoryId" class="form-select" asp-items="ViewBag.CategoryId">
                        <option value="">-- Chọn danh mục --</option>
                    </select>
                    <span asp-validation-for="CategoryId" class="text-danger"></span>
                </div>
                <div class="col-md-4 mb-3">
                    <label asp-for="ManufacturerId" class="control-label"></label>
                    <select asp-for="ManufacturerId" class="form-select" asp-items="ViewBag.ManufacturerId">
                        <option value="">-- Chọn Hãng SX --</option>
                    </select>
                    <span asp-validation-for="ManufacturerId" class="text-danger"></span>
                </div>
                <div class="col-md-4 mb-3">
                    <label asp-for="UnitId" class="control-label"></label>
                    <select asp-for="UnitId" class="form-select" asp-items="ViewBag.UnitId">
                        <option value="">-- Chọn ĐVT --</option>
                    </select>
                    <span asp-validation-for="UnitId" class="text-danger"></span>
                </div>
            </div>

            <div class="row">
                <div class="col-md-4 mb-3">
                    <label asp-for="SKU" class="control-label"></label>
                    <input asp-for="SKU" class="form-control" />
                    <span asp-validation-for="SKU" class="text-danger"></span>
                </div>
                <div class="col-md-8 mb-3">
                    <label asp-for="ProductName" class="control-label"></label>
                    <input asp-for="ProductName" class="form-control" />
                    <span asp-validation-for="ProductName" class="text-danger"></span>
                </div>
            </div>

            <div class="row">
                <div class="col-md-6 mb-3">
                    <label asp-for="BasePrice" class="control-label"></label>
                    <input asp-for="BasePrice" class="form-control" type="number" />
                    <span asp-validation-for="BasePrice" class="text-danger"></span>
                </div>
                <div class="col-md-6 mb-3">
                    <label asp-for="ImageUpload" class="control-label"></label>
                    <input asp-for="ImageUpload" type="file" accept="image/jpeg, image/png" class="form-control" />
                    <span asp-validation-for="ImageUpload" class="text-danger"></span>
                </div>
            </div>

            <div class="mb-3 form-check">
                <label class="form-check-label">
                    <input class="form-check-input" asp-for="IsActive" /> @Html.DisplayNameFor(model => model.IsActive)
                </label>
            </div>

            <div class="mb-0">
                <button type="submit" class="btn btn-primary"><i class="fa fa-save"></i> Lưu sản phẩm</button>
            </div>
        </form>
    </div>
</div>
2. Cập nhật View Sửa Sản phẩm (Views/Products/Edit.cshtml):
Cung cấp khu vực hiển thị ảnh cũ và cho phép cập nhật ảnh mới.  HTML@model DigiPOSE.Web.Models.Product
@{
    ViewData["Title"] = "Cập nhật sản phẩm";
}
<div class="card border-0 shadow-sm">
    <h5 class="card-header bg-warning text-dark">@ViewData["Title"]</h5>
    <div class="card-body">
        <form asp-action="Edit" enctype="multipart/form-data">
            <input type="hidden" asp-for="ProductId" />
            <input type="hidden" asp-for="RowVersion" />
            <input type="hidden" asp-for="ImageUrl" /> <!-- Giữ lại đường dẫn ảnh cũ -->
            
            <div class="row">
                <div class="col-md-4 mb-3">
                    <label asp-for="CategoryId" class="control-label"></label>
                    <select asp-for="CategoryId" class="form-select" asp-items="ViewBag.CategoryId"></select>
                </div>
                <div class="col-md-4 mb-3">
                    <label asp-for="ManufacturerId" class="control-label"></label>
                    <select asp-for="ManufacturerId" class="form-select" asp-items="ViewBag.ManufacturerId">
                        <option value="">-- Không có --</option>
                    </select>
                </div>
                <div class="col-md-4 mb-3">
                    <label asp-for="UnitId" class="control-label"></label>
                    <select asp-for="UnitId" class="form-select" asp-items="ViewBag.UnitId"></select>
                </div>
            </div>

            <div class="row">
                <div class="col-md-4 mb-3">
                    <label asp-for="SKU" class="control-label"></label>
                    <input asp-for="SKU" class="form-control" />
                    <span asp-validation-for="SKU" class="text-danger"></span>
                </div>
                <div class="col-md-8 mb-3">
                    <label asp-for="ProductName" class="control-label"></label>
                    <input asp-for="ProductName" class="form-control" />
                    <span asp-validation-for="ProductName" class="text-danger"></span>
                </div>
            </div>

            <div class="row">
                <div class="col-md-6 mb-3">
                    <label asp-for="BasePrice" class="control-label"></label>
                    <input asp-for="BasePrice" class="form-control" type="number" />
                </div>
                <div class="col-md-6 mb-3">
                    <label asp-for="ImageUpload" class="control-label"></label>
                    @if (!string.IsNullOrEmpty(Model.ImageUrl))
                    {
                        <div class="mb-2">
                            <img src="~/uploads/products/@Model.ImageUrl" width="80" class="img-thumbnail" />
                            <small class="text-muted d-block mt-1">Ảnh hiện tại (Bỏ trống nếu không muốn đổi ảnh)</small>
                        </div>
                    }
                    <input asp-for="ImageUpload" type="file" accept="image/jpeg, image/png" class="form-control" />
                </div>
            </div>

            <div class="mb-3 form-check">
                <label class="form-check-label">
                    <input class="form-check-input" asp-for="IsActive" /> @Html.DisplayNameFor(model => model.IsActive)
                </label>
            </div>

            <div class="mb-0">
                <button type="submit" class="btn btn-warning"><i class="fa fa-save"></i> Cập nhật</button>
            </div>
        </form>
    </div>
</div>
3. Cập nhật ProductsController.cs để xử lý File I/O:
Tạo thư mục wwwroot/uploads/products trong Project trước khi chạy code.  C#using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Web.Models;

namespace DigiPOSE.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly DigiPoseDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductsController(DigiPoseDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,CategoryId,UnitId,ManufacturerId,SKU,ProductName,BasePrice,ImageUpload,IsActive")] Product product)
        {
            if (ModelState.IsValid)
            {
                // Xử lý upload ảnh
                if (product.ImageUpload != null)
                {
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageUpload.FileName);
                    string path = Path.Combine(wwwRootPath + "/uploads/products/", fileName);
                    
                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await product.ImageUpload.CopyToAsync(fileStream);
                    }
                    product.ImageUrl = fileName;
                }

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories.Where(c => c.IsActive), "CategoryId", "CategoryName", product.CategoryId);
            ViewData["UnitId"] = new SelectList(_context.Units, "UnitId", "UnitName", product.UnitId);
            ViewData["ManufacturerId"] = new SelectList(_context.Manufacturers.Where(m => m.IsActive), "ManufacturerId", "ManufacturerName", product.ManufacturerId);
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,CategoryId,UnitId,ManufacturerId,SKU,ProductName,BasePrice,ImageUrl,ImageUpload,IsActive,RowVersion")] Product product)
        {
            if (id != product.ProductId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý ghi đè ảnh mới nếu có
                    if (product.ImageUpload != null)
                    {
                        string wwwRootPath = _hostEnvironment.WebRootPath;
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageUpload.FileName);
                        string path = Path.Combine(wwwRootPath + "/uploads/products/", fileName);
                        
                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await product.ImageUpload.CopyToAsync(fileStream);
                        }
                        
                        // Xóa file cũ (Tùy chọn)[cite: 4]
                        if (!string.IsNullOrEmpty(product.ImageUrl))
                        {
                            var oldPath = Path.Combine(wwwRootPath + "/uploads/products/", product.ImageUrl);
                            if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                        }
                        
                        product.ImageUrl = fileName; // Cập nhật đường dẫn mới[cite: 4]
                    }

                    _context.Update(product);
                    // Bỏ qua không track trường ImageUpload ảo[cite: 4]
                    _context.Entry(product).Property(x => x.ImageUpload).IsModified = false; 

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Products.Any(e => e.ProductId == product.ProductId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories.Where(c => c.IsActive), "CategoryId", "CategoryName", product.CategoryId);
            ViewData["UnitId"] = new SelectList(_context.Units, "UnitId", "UnitName", product.UnitId);
            ViewData["ManufacturerId"] = new SelectList(_context.Manufacturers.Where(m => m.IsActive), "ManufacturerId", "ManufacturerName", product.ManufacturerId);
            return View(product);
        }
    }
}
Bước 3: Tối ưu hóa tính năng Cập nhật Mật khẩu Nhân sự (User)[cite: 4]Trong thực tế doanh nghiệp, Form chỉnh sửa nhân viên không bắt buộc nhập lại mật khẩu nếu họ không muốn đổi. Thay vì phải tạo thêm một class ViewModel trung gian lỉnh kỉnh, ta sẽ xử lý trực tiếp ngay tại Controller.[cite: 4]Cập nhật UsersController.cs:[cite: 4]C#// POST: Users/Edit/5[cite: 4]
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, [Bind("UserId,RoleId,BranchId,Username,PasswordHash,IsActive")] User user, string? NewPassword)
{
    if (id != user.UserId) return NotFound();

    // Loại bỏ validation cho PasswordHash vì ta sẽ xử lý thủ công[cite: 4]
    ModelState.Remove("PasswordHash");

    if (ModelState.IsValid)
    {
        try
        {
            var existingUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == id);
            if (existingUser == null) return NotFound();

            // Nếu người dùng có nhập mật khẩu mới ở Form[cite: 4]
            if (!string.IsNullOrEmpty(NewPassword))
            {
                user.PasswordHash = BC.HashPassword(NewPassword);
            }
            else
            {
                // Giữ lại mật khẩu cũ[cite: 4]
                user.PasswordHash = existingUser.PasswordHash;
            }

            _context.Update(user);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Users.Any(e => e.UserId == user.UserId)) return NotFound();
            else throw;
        }
        return RedirectToAction(nameof(Index));
    }
    ViewData["BranchId"] = new SelectList(_context.Branches.Where(b => b.IsActive), "BranchId", "BranchName", user.BranchId);
    ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleName", user.RoleId);
    return View(user);
}
  (Trong File Views/Users/Edit.cshtml, Sếp chỉ cần đổi thẻ <input asp-for="PasswordHash"> thành <input name="NewPassword" type="password" class="form-control" placeholder="Để trống nếu giữ nguyên mật khẩu cũ" />)[cite: 4]



# OLD VERSION
BUỔI 3: DATA ANNOTATIONS, XỬ LÝ UPLOAD HÌNH ẢNH & TỐI ƯU HÓA MẬT KHẨU (DIGIPOSE)
Bước 1: Ràng buộc Dữ liệu (Data Annotations) cho các Model cốt lõi
Việc bổ sung Data Annotations sẽ giúp tự động hóa việc xác thực dữ liệu (Validation) ở cả phía Client (trình duyệt) và Server, giúp hệ thống tránh được các lỗi nhập liệu ngớ ngẩn.

1. Bảng Customer (Khách hàng)
Mở file Models/Customer.cs, cập nhật các thuộc tính:

C#
using System.ComponentModel.DataAnnotations;

namespace DigiPOSE.Web.Models
{
    public class Customer
    {
        [Key] public int CustomerId { get; set; }
        
        [Display(Name = "Loại khách hàng")]
        [Required(ErrorMessage = "Vui lòng chọn phân loại khách hàng.")]
        public int CustomerTypeId { get; set; }
        
        [Display(Name = "Họ và tên")]
        [Required(ErrorMessage = "Họ và tên không được bỏ trống.")]
        [StringLength(100)]
        public string FullName { get; set; } = null!;
        
        [Display(Name = "Số điện thoại")]
        [Required(ErrorMessage = "Số điện thoại không được bỏ trống.")]
        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ.")]
        [StringLength(20)]
        public string? Phone { get; set; }
        
        [Display(Name = "Điểm thưởng")]
        public int RewardPoints { get; set; } = 0;

        public CustomerType? CustomerType { get; set; }
    }
}
2. Bảng Product (Hàng hóa)
Mở file Models/Product.cs, cập nhật định dạng hiển thị tiền tệ và chuẩn hóa mã vạch:

C#
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiPOSE.Web.Models
{
    public class Product
    {
        [Key] public int ProductId { get; set; }
        
        [Display(Name = "Danh mục")]
        [Required(ErrorMessage = "Vui lòng chọn danh mục.")]
        public int CategoryId { get; set; }
        
        [Display(Name = "Đơn vị tính")]
        [Required(ErrorMessage = "Vui lòng chọn đơn vị tính.")]
        public int UnitId { get; set; }
        
        [Display(Name = "Mã SKU (Barcode)")]
        [Required(ErrorMessage = "Mã SKU không được bỏ trống.")]
        [StringLength(50)]
        public string SKU { get; set; } = null!;
        
        [Display(Name = "Tên sản phẩm")]
        [Required(ErrorMessage = "Tên sản phẩm không được bỏ trống.")]
        [StringLength(150)]
        public string ProductName { get; set; } = null!;
        
        [Display(Name = "Giá cơ sở (VNĐ)")]
        [Required(ErrorMessage = "Giá bán không được bỏ trống.")]
        [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = false)]
        [Column(TypeName = "decimal(18,0)")]
        public decimal BasePrice { get; set; }

        // Trường dùng để chứa tên File ảnh lưu trong Database
        [Display(Name = "Đường dẫn ảnh")]
        [StringLength(255)]
        public string? ImageUrl { get; set; }

        // Trường ảo dùng để bắt file Upload từ Form (Không lưu vào DB)
        [NotMapped]
        [Display(Name = "Tải ảnh sản phẩm")]
        public IFormFile? ImageUpload { get; set; }
        
        [Timestamp] public byte[]? RowVersion { get; set; }

        public Category? Category { get; set; }
        public Unit? Unit { get; set; }
    }
}
Bước 2: Xử lý Upload Hình ảnh Sản phẩm
1. Cập nhật View Thêm mới Sản phẩm (Views/Products/Create.cshtml):
Cần phải thêm thuộc tính enctype="multipart/form-data" vào thẻ <form> để cho phép upload file.

HTML
@model DigiPOSE.Web.Models.Product
@{
    ViewData["Title"] = "Thêm sản phẩm mới";
}
<div class="card border-0 shadow-sm">
    <h5 class="card-header bg-primary text-white">@ViewData["Title"]</h5>
    <div class="card-body">
        <form asp-action="Create" enctype="multipart/form-data">
            <div asp-validation-summary="ModelOnly" class="text-danger"></div>
            
            <div class="row">
                <div class="col-md-6 mb-3">
                    <label asp-for="CategoryId" class="control-label"></label>
                    <select asp-for="CategoryId" class="form-select" asp-items="ViewBag.CategoryId">
                        <option value="">-- Chọn danh mục --</option>
                    </select>
                    <span asp-validation-for="CategoryId" class="text-danger"></span>
                </div>
                <div class="col-md-6 mb-3">
                    <label asp-for="UnitId" class="control-label"></label>
                    <select asp-for="UnitId" class="form-select" asp-items="ViewBag.UnitId">
                        <option value="">-- Chọn ĐVT --</option>
                    </select>
                    <span asp-validation-for="UnitId" class="text-danger"></span>
                </div>
            </div>

            <div class="row">
                <div class="col-md-4 mb-3">
                    <label asp-for="SKU" class="control-label"></label>
                    <input asp-for="SKU" class="form-control" />
                    <span asp-validation-for="SKU" class="text-danger"></span>
                </div>
                <div class="col-md-8 mb-3">
                    <label asp-for="ProductName" class="control-label"></label>
                    <input asp-for="ProductName" class="form-control" />
                    <span asp-validation-for="ProductName" class="text-danger"></span>
                </div>
            </div>

            <div class="row">
                <div class="col-md-6 mb-3">
                    <label asp-for="BasePrice" class="control-label"></label>
                    <input asp-for="BasePrice" class="form-control" type="number" />
                    <span asp-validation-for="BasePrice" class="text-danger"></span>
                </div>
                <div class="col-md-6 mb-3">
                    <label asp-for="ImageUpload" class="control-label"></label>
                    <input asp-for="ImageUpload" type="file" accept="image/jpeg, image/png" class="form-control" />
                    <span asp-validation-for="ImageUpload" class="text-danger"></span>
                </div>
            </div>

            <div class="mb-0">
                <button type="submit" class="btn btn-primary"><i class="fa fa-save"></i> Lưu sản phẩm</button>
            </div>
        </form>
    </div>
</div>
2. Cập nhật View Sửa Sản phẩm (Views/Products/Edit.cshtml):
Cung cấp khu vực hiển thị ảnh cũ và cho phép cập nhật ảnh mới.

HTML
@model DigiPOSE.Web.Models.Product
@{
    ViewData["Title"] = "Cập nhật sản phẩm";
}
<div class="card border-0 shadow-sm">
    <h5 class="card-header bg-warning text-dark">@ViewData["Title"]</h5>
    <div class="card-body">
        <form asp-action="Edit" enctype="multipart/form-data">
            <input type="hidden" asp-for="ProductId" />
            <input type="hidden" asp-for="RowVersion" />
            <input type="hidden" asp-for="ImageUrl" /> <!-- Giữ lại đường dẫn ảnh cũ -->
            
            <div class="row">
                <!-- (Bao gồm các thẻ input như ở Create.cshtml) -->
                <!-- ... -->
                
                <div class="col-md-6 mb-3">
                    <label asp-for="ImageUpload" class="control-label"></label>
                    @if (!string.IsNullOrEmpty(Model.ImageUrl))
                    {
                        <div class="mb-2">
                            <img src="~/uploads/products/@Model.ImageUrl" width="80" class="img-thumbnail" />
                            <small class="text-muted d-block mt-1">Ảnh hiện tại (Bỏ trống nếu không muốn đổi ảnh)</small>
                        </div>
                    }
                    <input asp-for="ImageUpload" type="file" accept="image/jpeg, image/png" class="form-control" />
                </div>
            </div>

            <div class="mb-0">
                <button type="submit" class="btn btn-warning"><i class="fa fa-save"></i> Cập nhật</button>
            </div>
        </form>
    </div>
</div>
3. Cập nhật ProductsController.cs để xử lý File I/O:
Tạo thư mục wwwroot/uploads/products trong Project trước khi chạy code.

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
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProductsController(DigiPoseDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,CategoryId,UnitId,SKU,ProductName,BasePrice,ImageUpload")] Product product)
        {
            if (ModelState.IsValid)
            {
                // Xử lý upload ảnh
                if (product.ImageUpload != null)
                {
                    string wwwRootPath = _hostEnvironment.WebRootPath;
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageUpload.FileName);
                    string path = Path.Combine(wwwRootPath + "/uploads/products/", fileName);
                    
                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await product.ImageUpload.CopyToAsync(fileStream);
                    }
                    product.ImageUrl = fileName;
                }

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            ViewData["UnitId"] = new SelectList(_context.Units, "UnitId", "UnitName", product.UnitId);
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,CategoryId,UnitId,SKU,ProductName,BasePrice,ImageUrl,ImageUpload,RowVersion")] Product product)
        {
            if (id != product.ProductId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Xử lý ghi đè ảnh mới nếu có
                    if (product.ImageUpload != null)
                    {
                        string wwwRootPath = _hostEnvironment.WebRootPath;
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageUpload.FileName);
                        string path = Path.Combine(wwwRootPath + "/uploads/products/", fileName);
                        
                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await product.ImageUpload.CopyToAsync(fileStream);
                        }
                        
                        // Xóa file cũ (Tùy chọn)
                        if (!string.IsNullOrEmpty(product.ImageUrl))
                        {
                            var oldPath = Path.Combine(wwwRootPath + "/uploads/products/", product.ImageUrl);
                            if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                        }
                        
                        product.ImageUrl = fileName; // Cập nhật đường dẫn mới
                    }

                    _context.Update(product);
                    // Bỏ qua không track trường ImageUpload ảo
                    _context.Entry(product).Property(x => x.ImageUpload).IsModified = false; 

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Products.Any(e => e.ProductId == product.ProductId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryName", product.CategoryId);
            ViewData["UnitId"] = new SelectList(_context.Units, "UnitId", "UnitName", product.UnitId);
            return View(product);
        }
    }
}
Bước 3: Tối ưu hóa tính năng Cập nhật Mật khẩu Nhân sự (User)
Trong thực tế doanh nghiệp, Form chỉnh sửa nhân viên không bắt buộc nhập lại mật khẩu nếu họ không muốn đổi. Thay vì phải tạo thêm một class ViewModel trung gian lỉnh kỉnh, ta sẽ xử lý trực tiếp ngay tại Controller.

Cập nhật UsersController.cs:

C#
// POST: Users/Edit/5
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, [Bind("UserId,RoleId,BranchId,Username,PasswordHash,IsActive")] User user, string? NewPassword)
{
    if (id != user.UserId) return NotFound();

    // Loại bỏ validation cho PasswordHash vì ta sẽ xử lý thủ công
    ModelState.Remove("PasswordHash");

    if (ModelState.IsValid)
    {
        try
        {
            var existingUser = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == id);
            if (existingUser == null) return NotFound();

            // Nếu người dùng có nhập mật khẩu mới ở Form
            if (!string.IsNullOrEmpty(NewPassword))
            {
                user.PasswordHash = BC.HashPassword(NewPassword);
            }
            else
            {
                // Giữ lại mật khẩu cũ
                user.PasswordHash = existingUser.PasswordHash;
            }

            _context.Update(user);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Users.Any(e => e.UserId == user.UserId)) return NotFound();
            else throw;
        }
        return RedirectToAction(nameof(Index));
    }
    ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "BranchName", user.BranchId);
    ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleName", user.RoleId);
    return View(user);
}
(Trong File Views/Users/Edit.cshtml, Sếp chỉ cần đổi thẻ <input asp-for="PasswordHash"> thành <input name="NewPassword" type="password" class="form-control" placeholder="Để trống nếu giữ nguyên mật khẩu cũ" />)