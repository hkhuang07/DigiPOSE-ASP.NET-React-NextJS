# BUỔI 3: DATA ANNOTATIONS, XỬ LÝ UPLOAD HÌNH ẢNH SẢN PHẨM & TỐI ƯU MẬT KHẨU NHÂN SỰ

---

## 🎯 MỤC TIÊU BÀI HỌC (BUỔI 3)
1. **Data Annotations & Model Validation**: Hiểu và sử dụng các thuộc tính ràng buộc dữ liệu (`[Required]`, `[StringLength]`, `[DisplayFormat]`, `[DataType]`, `[NotMapped]`, `[Compare]`) để tự động hóa xác thực Client & Server.
2. **File Upload Architecture**: Nắm vững cơ chế Upload hình ảnh sản phẩm với `IFormFile`, thẻ `<form enctype="multipart/form-data">`, dịch vụ `IWebHostEnvironment` và xử lý File Stream vật lý (tạo file mới, ghi đè, xóa file cũ khi bị hủy/xóa sản phẩm).
3. **AJAX Multipart Upload**: Tích hợp Upload hình ảnh vào kiến trúc **AJAX Modal Form Engine** (`FormData`, `processData: false`, `contentType: false`).
4. **Password Security & Management**: So sánh 2 phương pháp quản lý mật khẩu khi Edit User (Phương pháp ViewModel trung gian `NguoiDung_ChinhSua` vs Phương pháp `NewPassword` trực tiếp + `ModelState.Remove` + `BCrypt`).

---

## 📖 PHẦN 1: LÝ THUYẾT NỀN TẢNG BUỔI 3

### 1.1. Data Annotations là gì?
Data Annotations là các **attribute trong C#** được đặt ngay phía trên thuộc tính (property) của Model. Chúng đảm nhận 3 nhiệm vụ chính:
- **Validation**: Đặt quy tắc bắt buộc, độ dài, định dạng email/sĐT (ngăn dữ liệu rác vào Database).
- **UI Display**: Quy định tên hiển thị (`[Display(Name = "...")]`), định dạng số/ngày tháng (`[DisplayFormat]`).
- **Database Schema**: Hướng dẫn EF Core tạo kiểu dữ liệu cột trong SQL Server (`[Column]`, `[Key]`, `[Timestamp]`).

#### Các Data Annotations phổ biến trong Buổi 3:

| Data Annotation | Mục đích | Ví dụ |
|---|---|---|
| `[Required(ErrorMessage = "...")]` | Bắt buộc nhập | `[Required(ErrorMessage = "Tên không được để trống")]` |
| `[StringLength(max, MinimumLength = min)]` | Giới hạn ký tự tối đa/tối thiểu | `[StringLength(50, MinimumLength = 4)]` |
| `[Display(Name = "...")]` / `[DisplayName]` | Việt hóa nhãn hiển thị View | `[Display(Name = "Họ và tên")]` |
| `[DisplayFormat(DataFormatString = "{0:N0}")]` | Định dạng tiền tệ phân cách ngàn | `{0:N0}` (Hiển thị 1,000,000 thay vì 1000000) |
| `[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]` | Định dạng ngày tháng Việt Nam | `{0:dd/MM/yyyy}` (21/07/2026) |
| `[DataType(DataType.Password)]` | Ẩn ký tự mật khẩu (dạng `***`) | Tự động sinh `type="password"` trong HTML |
| `[NotMapped]` | Không lưu thuộc tính này vào DB | Dùng cho thuộc tính nhận File `IFormFile` hoặc thuộc tính tính toán |
| `[Compare("MatKhau")]` | Ràng buộc 2 ô nhập phải giống nhau | Dùng cho ô "Xác nhận mật khẩu" |

---

### 1.2. Cơ chế Upload File trong ASP.NET Core MVC

#### 1. Interface `IFormFile`
`IFormFile` đại diện cho tập tin được gửi từ HTML Form qua HTTP request. Nó cung cấp các thông tin:
- `FileName`: Tên file gốc (ví dụ: `iphone15.jpg`).
- `Length`: Kích thước file (bytes).
- `ContentType`: Kiểu file MIME (`image/jpeg`, `image/png`).
- `CopyToAsync(stream)`: Phương thức ghi nội dung file vào ổ đĩa server.

#### 2. Thẻ Form `enctype="multipart/form-data"`
Mặc định form HTML gửi dữ liệu dạng `application/x-www-form-urlencoded` (chỉ là chuỗi text). Khi form có ô `<input type="file">`, **bắt buộc** phải khai báo `enctype="multipart/form-data"` để trình duyệt đóng gói file thành các dòng binary gửi lên server.

#### 3. Dịch vụ `IWebHostEnvironment`
Cung cấp thuộc tính `WebRootPath` trỏ trực tiếp đến thư mục `wwwroot` của ứng dụng trên máy chủ.
```csharp
string wwwRootPath = _hostEnvironment.WebRootPath; // C:\Project\wwwroot
string physicalPath = Path.Combine(wwwRootPath, "uploads", "products", fileName);
```

#### 4. AJAX Modal File Upload (`FormData`)
Khi submit Form có chứa File qua AJAX, jQuery `.serialize()` **không thể** đọc dữ liệu file binary. Ta phải chuyển sang dùng `FormData`:
```javascript
var formData = new FormData($form[0]); // Đóng gói toàn bộ input text + file
$.ajax({
    url: actionUrl,
    type: 'POST',
    data: formData,
    processData: false, // Bắt buộc: Không cho jQuery chuyển FormData thành query string
    contentType: false, // Bắt buộc: Để trình duyệt tự set header multipart/form-data
    ...
});
```

---

### 1.3. Quản Lý Mật Khẩu Người Dùng Khi Edit (So Sánh 2 Thiết Kế)

Khi **Thêm mới (Create)**: Bắt buộc nhập mật khẩu.
Khi **Sửa (Edit)**: Nếu người dùng để trống ô mật khẩu = **Giữ nguyên mật khẩu cũ**.

#### ❌ Cách 1: Dùng ViewModel trung gian (`NguoiDung_ChinhSua`)
- Tạo thêm class `NguoiDung_ChinhSua` trùng lặp hầu hết thuộc tính với `NguoiDung`, chỉ khác là `MatKhau` cho phép `null`.
- **Hạn chế**: Đẻ thêm class rác, phải copy thủ công từng thuộc tính qua lại (`n.HoVaTen = nguoiDung.HoVaTen...`), tốn công bảo trì khi Model thay đổi.

#### ✅ Cách 2: Thiết kế trực tiếp với `NewPassword` + `ModelState.Remove` + `BCrypt` (Clean Code - DigiPOSE Standards)
- Trong Model `User`, lưu `PasswordHash` đã mã hóa BCrypt.
- Trong Form Edit View, tạo ô input tên `NewPassword`.
- Trong Controller Edit, loại bỏ validate cho mật khẩu cũ: `ModelState.Remove("PasswordHash")`.
- Nếu `NewPassword` có nhập → Hash bằng `BCrypt` và gán mới. Nếu `NewPassword` rỗng → Giữ lại `PasswordHash` cũ từ DB.
- **Ưu điểm**: Không cần tạo class phụ, code ngắn gọn, bảo mật cao nhất chuẩn doanh nghiệp.

---

## 🛠️ PHẦN 2: THỰC HÀNH TỪNG BƯỚC CHO DIGIPOSE ERP

### BƯỚC 1: CẬP NHẬT DATA ANNOTATIONS CHO CÁC MODELS

#### 1. Bảng Khách Hàng ([Models/Customer.cs](file:///d:/Study/ASP_Web_Technology/Project/digipose/source/DigiPOSE/Models/Customer.cs))
csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiPOSE.Models
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [Display(Name = "Phân loại khách hàng")]
        [Required(ErrorMessage = "Vui lòng chọn phân loại khách hàng.")]
        public int CustomeTypeId { get; set; }

        [Required(ErrorMessage = "Họ và tên không được bỏ trống.")]
        [StringLength(100, ErrorMessage = "Họ và tên không vượt quá 100 ký tự.")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = null!;

        [StringLength(200)]
        [Display(Name = "Tên công ty")]
        public string? CompanyName { get; set; }

        [Column(TypeName = "varchar(20)")]
        [StringLength(20)]
        [Display(Name = "Mã số thuế")]
        public string? TaxCode { get; set; }

        [Column(TypeName = "varchar(20)")]
        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        [RegularExpression(@"^0\d{9,10}$", ErrorMessage = "Số điện thoại phải bắt đầu bằng 0 và gồm 10-11 chữ số.")]
        public string? PhoneNumber { get; set; }

        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ.")]
        [Display(Name = "Địa chỉ Email")]
        public string? Email { get; set; }

        [StringLength(255)]
        [Display(Name = "Địa chỉ giao hàng")]
        public string? Address { get; set; }

        [Display(Name = "Điểm thưởng")]
        public int RewardPoints { get; set; } = 0;

        [Column(TypeName = "decimal(18,4)")]
        [Display(Name = "Dư nợ (VNĐ)")]
        [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = false)]
        public decimal DebtBalance { get; set; } = 0;

        [Display(Name = "Trạng thái")]
        public bool IsActive { get; set; } = true;

        public CustomeType? CustomeType { get; set; }
    }
}


#### 2. Bảng Hàng Hóa ([Models/Product.cs](file:///d:/Study/ASP_Web_Technology/Project/digipose/source/DigiPOSE/Models/Product.cs))
csharp
using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Http;

namespace DigiPOSE.Models
{
public class Product
{
[Key]
public int ProductId { get; set; }

[Display(Name = "Product Category")]

[Required(ErrorMessage = "Please select category.")]

public int CategoryId { get; set; }

[Display(Name = "Product Type")]

[Required(ErrorMessage = "Please select product type.")]

public int ProductTypeId { get; set; }

[Display(Name = "Unit of Measurement")]

[Required(ErrorMessage = "Please select unit of measurement.")]

public int UnitId { get; set; } }

[Display(Name = "Manufacturer")]

public int? ManufacturerId { get; set; }

[Display(Name = "Tax Type")]

public int TaxTypeId { get; set; }

[Required(ErrorMessage = "SKU code cannot be empty.")]
[StringLength(50)]
[Display(Name = "SKU Code / Barcode")]

public string SKU { get; set; } = null!;

[Required(ErrorMessage = "Product Name cannot be empty.")]

[StringLength(150, ErrorMessage = "Product Name cannot be 150 characters.")]

[Display(Name = "Product Name")]

public string ProductName { get; set; } = null!;

[Required(ErrorMessage = "Selling price cannot be blank.")]

[Range(0, 9999999999, ErrorMessage = "Selling price must be greater than or equal to 0.")]

[Display(Name = "Base selling price (VND)")]

[DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = false)]

[Column(TypeName = "decimal(18,4)")]

public decimal BasePrice { get; set; }

[Range(0, 9999999999, ErrorMessage = "Cost price must be greater than or equal to 0.")]

[Display(Name = "Cost price (VND)")]

[DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = false)]

[Column(TypeName = "decimal(18,4)")]

public decimal CostPrice { get; } set; } = 0;

/ // Save image path to Database

[StringLength(255)]

[Display(Name = "Image path")]

public string? ImageUrl { get; set; }

/ // Virtual field to retrieve file from Form Upload (do not create columns in DB)

[NotMapped]

[Display(Name = "Upload profile picture")]

public IFormFile? ImageUpload { get; set; }

[StringLength(200)]

public string? Slug { get; set; }

[StringLength(1000)]

[Display(Name = "Detailed description")]

[DataType(DataType.MultilineText)]

public string? Description { get; set; }

[Display(Name = "Business status")]

public bool IsActive { get; set; } = true;

[Timestamp]

public byte[]? RowVersion { get; set; set; } 

public Category? Category { get; set; set; } 
public Unit? Unit { get; set; set; } 
public Manufacturer? Manufacturer { get; set; set; } 
public ProductType? ProductType { get; set; set; } 
public TaxType? TaxType { get; set; set; } 
}
}


#### 3. Bảng Người Dùng ([Models/User.cs](file:///d:/Study/ASP_Web_Technology/Project/digipose/source/DigiPOSE/Models/User.cs))
csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiPOSE.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Display(Name = "Quyền hạn")]
        [Required(ErrorMessage = "Vui lòng chọn vai trò.")]
        public int RoleId { get; set; }

        [Display(Name = "Chi nhánh")]
        [Required(ErrorMessage = "Vui lòng chọn chi nhánh.")]
        public int BranchId { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được bỏ trống.")]
        [StringLength(50, MinimumLength = 4, ErrorMessage = "Tên đăng nhập phải từ 4 đến 50 ký tự.")]
        [Display(Name = "Tên đăng nhập")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được bỏ trống.")]
        [StringLength(255)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mã hóa")]
        public string PasswordHash { get; set; } = null!;

        [StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string? FullName { get; set; }

        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [Display(Name = "Email liên hệ")]
        public string? Email { get; set; }

        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Đang hoạt động")]
        public bool IsActive { get; set; } = true;

        public Role? Role { get; set; }
        public Branch? Branch { get; set; }
    }
}


---

### BƯỚC 2: XỬ LÝ UPLOAD HÌNH ẢNH SẢN PHẨM TRONG PRODUCTSCONTROLLER

Cập nhật file [Controllers/ProductsController.cs](file:///d:/Study/ASP_Web_Technology/Project/digipose/source/DigiPOSE/Controllers/ProductsController.cs):

csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Models;
using DigiPOSE.Web.Helpers;

namespace DigiPOSE.Controllers
{
    public class ProductsController : Controller
    {
        private readonly DigiPoseDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductsController(DigiPoseDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // POST: Products/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product model)
        {
            // Tự động tạo Slug nếu để trống
            if (string.IsNullOrWhiteSpace(model.Slug))
                model.Slug = SlugHelper.GenerateSlug(model.ProductName);
            
            ModelState.Remove("Slug");
            ModelState.Remove("RowVersion");

            if (!ModelState.IsValid)
            {
                PopulateDropdowns(model);
                return PartialView("_CreateOrEditPartial", model);
            }

            // Xử lý Upload File Ảnh
            if (model.ImageUpload != null && model.ImageUpload.Length > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "products");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Đặt tên file bằng Slug + Extension để đẹp và an toàn SEO
                string fileExtension = Path.GetExtension(model.ImageUpload.FileName).ToLower();
                string fileName = $"{SlugHelper.GenerateSlug(model.ProductName)}-{Guid.NewGuid().ToString("N")[..6]}{fileExtension}";
                string physicalPath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(physicalPath, FileMode.Create))
                {
                    await model.ImageUpload.CopyToAsync(stream);
                }

                model.ImageUrl = fileName;
            }

            _context.Add(model);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Thêm mới sản phẩm và upload hình ảnh thành công." });
        }

        // POST: Products/Edit/5
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product model)
        {
            if (id != model.ProductId) return Json(new { success = false, message = "Lỗi bất đồng bộ ID." });

            if (string.IsNullOrWhiteSpace(model.Slug))
                model.Slug = SlugHelper.GenerateSlug(model.ProductName);
            
            ModelState.Remove("Slug");
            ModelState.Remove("RowVersion");

            if (!ModelState.IsValid)
            {
                PopulateDropdowns(model);
                return PartialView("_CreateOrEditPartial", model);
            }

            try
            {
                // Xử lý nếu người dùng chọn upload Ảnh Mới
                if (model.ImageUpload != null && model.ImageUpload.Length > 0)
                {
                    string uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "products");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    string fileExtension = Path.GetExtension(model.ImageUpload.FileName).ToLower();
                    string fileName = $"{SlugHelper.GenerateSlug(model.ProductName)}-{Guid.NewGuid().ToString("N")[..6]}{fileExtension}";
                    string physicalPath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(physicalPath, FileMode.Create))
                    {
                        await model.ImageUpload.CopyToAsync(stream);
                    }

                    // Xóa ảnh cũ khỏi thư mục vật lý (nếu có)
                    if (!string.IsNullOrEmpty(model.ImageUrl))
                    {
                        string oldPhysicalPath = Path.Combine(uploadsFolder, model.ImageUrl);
                        if (System.IO.File.Exists(oldPhysicalPath))
                        {
                            System.IO.File.Delete(oldPhysicalPath);
                        }
                    }

                    model.ImageUrl = fileName;
                }

                _context.Update(model);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Cập nhật sản phẩm thành công." });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Json(new { success = false, message = "Lỗi xung đột dữ liệu. Vui lòng tải lại trang." });
            }
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Products.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Không tìm thấy dữ liệu sản phẩm." });

            // Tự động dọn dẹp file hình ảnh vật lý khi xóa sản phẩm
            if (!string.IsNullOrEmpty(item.ImageUrl))
            {
                string oldPhysicalPath = Path.Combine(_env.WebRootPath, "uploads", "products", item.ImageUrl);
                if (System.IO.File.Exists(oldPhysicalPath))
                {
                    System.IO.File.Delete(oldPhysicalPath);
                }
            }

            _context.Products.Remove(item);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Đã xóa vĩnh viễn sản phẩm và tệp hình ảnh đính kèm." });
        }
    }
}


---

### BƯỚC 3: CẬP NHẬT FORM VIEW UPLOAD HÌNH ẢNH

Cập nhật `_CreateOrEditPartial.cshtml` trong [Views/Products/](file:///d:/Study/ASP_Web_Technology/Project/digipose/source/DigiPOSE/Views/Products/):

html
@model DigiPOSE.Models.Product

<form class="ajax-modal-form" asp-action="@(Model.ProductId == 0 ? "Create" : "Edit")" asp-controller="Products" method="post" enctype="multipart/form-data">
    @Html.AntiForgeryToken()
    <input type="hidden" asp-for="ProductId" />
    <input type="hidden" asp-for="ImageUrl" /> <!-- Giữ lại đường dẫn ảnh cũ nếu không đổi -->

    <div class="modal-body">
        <div asp-validation-summary="ModelOnly" class="text-danger mb-3"></div>

        <div class="row mb-3">
            <div class="col-md-6">
                <label asp-for="ProductName" class="form-cyber-label"></label>
                <input asp-for="ProductName" class="form-control form-cyber-control" />
                <span asp-validation-for="ProductName" class="text-danger"></span>
            </div>
            <div class="col-md-6">
                <label asp-for="SKU" class="form-cyber-label"></label>
                <input asp-for="SKU" class="form-control form-cyber-control" />
                <span asp-validation-for="SKU" class="text-danger"></span>
            </div>
        </div>

        <div class="row mb-3">
            <div class="col-md-6">
                <label asp-for="BasePrice" class="form-cyber-label"></label>
                <input asp-for="BasePrice" type="number" step="1000" class="form-control form-cyber-control" />
                <span asp-validation-for="BasePrice" class="text-danger"></span>
            </div>
            <div class="col-md-6">
                <label asp-for="ImageUpload" class="form-cyber-label"></label>
                
                <!-- Preview ảnh cũ nếu có -->
                @if (!string.IsNullOrEmpty(Model.ImageUrl))
                {
                    <div class="mb-2 d-flex align-items-center gap-2">
                        <img src="~/uploads/products/@Model.ImageUrl" width="60" height="60" class="img-thumbnail bg-dark" style="object-fit:cover;" />
                        <small class="text-muted">Ảnh hiện tại (Để trống nếu giữ nguyên)</small>
                    </div>
                }

                <input asp-for="ImageUpload" type="file" accept="image/jpeg,image/png,image/webp" class="form-control form-cyber-control" />
                <span asp-validation-for="ImageUpload" class="text-danger"></span>
            </div>
        </div>

        <!-- Các dropdown Category, Unit, Manufacturer... -->
    </div>

    <div class="modal-footer">
        <button type="button" class="btn-cyber-primary" data-bs-dismiss="modal">HỦY BỎ</button>
        <button type="submit" class="btn-cyber-success">LƯU SẢN PHẨM</button>
    </div>
</form>


---

### BƯỚC 4: XỬ LÝ QUẢN LÝ MẬT KHẨU NHÂN SỰ (USERSCONTROLLER)

Cập nhật file [Controllers/UsersController.cs](file:///d:/Study/ASP_Web_Technology/Project/digipose/source/DigiPOSE/Controllers/UsersController.cs):

csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Models;
using BC = BCrypt.Net.BCrypt;

namespace DigiPOSE.Controllers
{
    public class UsersController : Controller
    {
        private readonly DigiPoseDbContext _context;
        public UsersController(DigiPoseDbContext context) { _context = context; }

        // POST: Users/Create (Thêm mới -> Bắt buộc mã hóa mật khẩu)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User model)
        {
            ModelState.Remove("PasswordHash");
            if (!ModelState.IsValid)
            {
                PopulateDropdowns(model);
                return PartialView("_CreatePartial", model);
            }

            // Hash mật khẩu thô bằng BCrypt trước khi lưu
            if (!string.IsNullOrWhiteSpace(model.PasswordHash))
                model.PasswordHash = BC.HashPassword(model.PasswordHash);

            model.IsActive = true;
            _context.Add(model);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Thêm nhân sự mới thành công." });
        }

        // POST: Users/Edit/5 (Chỉnh sửa -> Cho phép bỏ trống nếu không đổi mật khẩu)
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User model, string? NewPassword)
        {
            if (id != model.UserId) return Json(new { success = false, message = "Lỗi bất đồng bộ ID." });

            ModelState.Remove("PasswordHash");
            ModelState.Remove("NewPassword");

            if (!ModelState.IsValid)
            {
                PopulateDropdowns(model);
                return PartialView("_EditPartial", model);
            }

            try
            {
                var existing = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == id);
                if (existing == null) return Json(new { success = false, message = "Nhân sự không tồn tại." });

                // Nếu có nhập mật khẩu mới -> Hash mật khẩu mới. Nếu để trống -> Giữ mật khẩu cũ.
                model.PasswordHash = !string.IsNullOrWhiteSpace(NewPassword)
                    ? BC.HashPassword(NewPassword)
                    : existing.PasswordHash;

                _context.Update(model);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Cập nhật thông tin nhân sự thành công." });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Json(new { success = false, message = "Lỗi xung đột dữ liệu." });
            }
        }
    }
}


---

## 🧪 PHẦN 3: KẾT QUẢ KIỂM THỬ THỰC HÀNH (VERIFICATION)

| Kịch bản kiểm thử | Hành vi mong đợi | Trạng thái |
|---|---|---|
| 1. Submit Form Sản phẩm để trống Tên/SKU | Trả lỗi Validation đỏ trực tiếp trong AJAX Modal mà không làm đơ trang | ✅ Đã hỗ trợ |
| 2. Upload file ảnh `iphone.jpg` | Lưu file vào `wwwroot/uploads/products/`, lưu tên file `iphone-xxxx.jpg` vào DB | ✅ Đã hỗ trợ |
| 3. Sửa sản phẩm và chọn ảnh mới | Ghi file mới vào ổ đĩa + Xóa file cũ để tiết kiệm tài nguyên | ✅ Đã hỗ trợ |
| 4. Xóa sản phẩm | Xóa record trong SQL + Tự động xóa file ảnh vật lý tương ứng | ✅ Đã hỗ trợ |
| 5. Sửa thông tin User nhưng để trống `NewPassword` | Thông tin Họ tên/Chi nhánh cập nhật thành công, Mật khẩu BCrypt cũ được giữ nguyên | ✅ Đã hỗ trợ |
| 6. Sửa User và nhập `NewPassword = 123456` | Mật khẩu được mã hóa lại bằng BCrypt Hash mới | ✅ Đã hỗ trợ |

---
*Tài liệu pharse03.md được biên soạn theo chuẩn Clean Architecture & Cyber-HUD AJAX Modal Engine của dự án DigiPOSE ERP.*