# BUỔI 4: SẮP XẾP, TÌM KIẾM, PHÂN TRANG (DATATABLES), CKEDITOR VÀ BẢO MẬT BCRYPT

## 1. LÝ THUYẾT VÀ KIẾN TRÚC TỐI ƯU (ENTERPRISE STANDARD)

### 1.1. Tại sao phải phân trang ở Server-side (Server-Side Pagination)?
- **Vấn đề của Client-side:** Thông thường, thư viện DataTables sẽ load toàn bộ dữ liệu từ Server về HTML (ví dụ: 10,000 dòng), sau đó dùng Javascript để chia trang. Điều này làm tăng thời gian phản hồi (Latency), tốn băng thông và có thể làm crash trình duyệt của client.
- **Giải pháp Server-side:** Khi click trang số 2, trình duyệt chỉ gửi yêu cầu lấy đúng 10 dòng của trang 2 về. 
- **Cơ chế hoạt động của DataTables Server-Side:**
  - DataTables sẽ gửi một `POST Request` (hoặc GET) lên Server chứa các tham số: `draw` (bộ đếm bảo đảm đồng bộ), `start` (dòng bắt đầu), `length` (số lượng dòng cần lấy), `search[value]` (từ khóa tìm kiếm) và thông tin cột cần sắp xếp.
  - Server sử dụng Entity Framework kết hợp với thư viện `System.Linq.Dynamic.Core` để dịch các tham số string thành câu lệnh truy vấn SQL tương ứng (ví dụ: `.OrderBy("ProductName ASC").Skip(10).Take(10)`).
  - Server trả về JSON chứa `data` (dữ liệu hiển thị) và `recordsTotal`, `recordsFiltered`.

### 1.2. CKEditor (Rich Text Editor)
- Với những nội dung dài cần định dạng (như Mô tả sản phẩm - Description), thẻ `<textarea>` truyền thống không đáp ứng được. CKEditor biến textarea thành một trình soạn thảo văn bản giống MS Word, sau đó tự động convert nội dung thành mã HTML để lưu vào Database.

### 1.3. Băm mật khẩu (Password Hashing) với BCrypt
- **Nguyên tắc Zero-Trust Security:** Mật khẩu của người dùng tuyệt đối KHÔNG được lưu dưới dạng Plain Text (chữ rõ ràng) trong Database. Nếu Database bị hack, hacker sẽ có toàn bộ mật khẩu.
- **BCrypt:** Là thuật toán băm (hash) một chiều, tự động sinh ra `Salt` ngẫu nhiên cho mỗi lần băm. Dù 2 người dùng có cùng mật khẩu `123456`, chuỗi băm lưu trong CSDL vẫn sẽ khác biệt hoàn toàn.

---

## 2. HƯỚNG DẪN THỰC HÀNH (ÁP DỤNG VÀO DIGIPOSE)

### Bước 1: Chuẩn bị thư viện cần thiết
Mở Package Manager Console (Tools > NuGet Package Manager > Package Manager Console) và chạy 2 lệnh sau:
```powershell
Install-Package System.Linq.Dynamic.Core
Install-Package BCrypt.Net-Next
```
*(Lưu ý: Source code DigiPOSE hiện tại có thể đã cài đặt sẵn BCrypt để phục vụ UsersController)*

---

### Bước 2: Tối ưu phân trang Server-side bảng Sản Phẩm (Products)

**2.1. Cập nhật `Controllers/ProductsController.cs`**
Ta sẽ giữ nguyên hàm `Index()` chỉ trả về View rỗng, và thêm hàm `Index_LoadData` để xử lý AJAX.

```csharp
using System.Linq.Dynamic.Core; // Bắt buộc thêm

public class ProductsController : Controller
{
    // Cập nhật hàm Index chỉ trả về View (không query dữ liệu nữa)
    public IActionResult Index()
    {
        return View();
    }

    // Thêm API xử lý Server-Side DataTables
    [HttpPost]
    public IActionResult Index_LoadData()
    {
        try
        {
            // 1. Nhận các tham số từ DataTables
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            
            int pageSize = length != null ? int.Parse(length) : 0;
            int skip = start != null ? int.Parse(start) : 0;

            // 2. Query gốc
            var query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Unit)
                .AsQueryable();

            int totalRecords = query.Count();

            // 3. Tìm kiếm (Searching)
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(p => 
                    p.ProductName.Contains(searchValue) ||
                    p.SKU.Contains(searchValue) ||
                    p.Category.CategoryName.Contains(searchValue));
            }
            int filterRecords = query.Count();

            // 4. Sắp xếp (Sorting) sử dụng System.Linq.Dynamic.Core
            if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
            {
                query = query.OrderBy(sortColumn + " " + sortColumnDirection);
            }

            // 5. Phân trang (Paging) và Map dữ liệu trả về
            var dataList = query.Skip(skip).Take(pageSize).Select(p => new {
                ProductId = p.ProductId,
                ImageUrl = p.ImageUrl,
                SKU = p.SKU,
                ProductName = p.ProductName,
                CategoryName = p.Category != null ? p.Category.CategoryName : "",
                UnitName = p.Unit != null ? p.Unit.UnitName : "",
                BasePrice = p.BasePrice,
                IsActive = p.IsActive
            }).ToList();

            // 6. Format Json Data
            return Json(new { draw = draw, recordsFiltered = filterRecords, recordsTotal = totalRecords, data = dataList });
        }
        catch (Exception ex)
        {
            return Json(new { error = ex.Message });
        }
    }
}
```

**2.2. Cập nhật `Views/Products/Index.cshtml`**
Cập nhật HTML `<table id="datatable">` (xóa toàn bộ nội dung trong `<tbody>`) và thêm script khởi tạo DataTables:

```html
@section Styles {
    <link rel="stylesheet" href="https://cdn.datatables.net/1.13.8/css/dataTables.bootstrap5.min.css" />
    <link rel="stylesheet" href="https://cdn.datatables.net/buttons/2.4.2/css/buttons.bootstrap5.min.css" />
}

<table id="datatable" class="table-cyber w-100">
    <thead>
        <tr>
            <th>#</th>
            <th>Image</th>
            <th>SKU</th>
            <th>Product Name</th>
            <th>Category</th>
            <th>Unit</th>
            <th>Price</th>
            <th>Status</th>
            <th>Actions</th>
        </tr>
    </thead>
</table>

@section Scripts {
    <script src="https://cdn.datatables.net/1.13.8/js/jquery.dataTables.min.js"></script>
    <script src="https://cdn.datatables.net/1.13.8/js/dataTables.bootstrap5.min.js"></script>
    <script>
        $(document).ready(function () {
            $('#datatable').DataTable({
                serverSide: true,
                processing: true,
                filter: true,
                ajax: {
                    url: '@Url.Action("Index_LoadData", "Products")',
                    type: 'POST',
                    datatype: 'json'
                },
                columns: [
                    { data: null, name: null, sortable: false, render: (d,t,r,meta) => meta.row + meta.settings._iDisplayStart + 1 },
                    { 
                        data: 'imageUrl', 
                        name: 'ImageUrl', 
                        sortable: false, 
                        render: function(d) {
                            if(d) return `<img src="/uploads/products/${d}" style="width:38px; height:38px; object-fit:cover; border:1px solid #00E5FF;" />`;
                            return `<i class="fa-solid fa-image text-muted"></i>`;
                        }
                    },
                    { data: 'sku', name: 'SKU' },
                    { data: 'productName', name: 'ProductName' },
                    { data: 'categoryName', name: 'Category.CategoryName' },
                    { data: 'unitName', name: 'Unit.UnitName' },
                    { data: 'basePrice', name: 'BasePrice', render: $.fn.dataTable.render.number(',', '.', 0, '') },
                    { 
                        data: 'isActive', 
                        name: 'IsActive',
                        render: d => d ? '<span class="badge bg-success">ACTIVE</span>' : '<span class="badge bg-secondary">INACTIVE</span>'
                    },
                    {
                        data: null,
                        sortable: false,
                        render: function(d,t,row) {
                            return `<button class="btn-cyber-primary btn-show-modal" data-url="/Products/Edit/${row.productId}"><i class="fa-solid fa-pen"></i></button>`;
                        }
                    }
                ]
            });
        });
    </script>
}
```

---

### Bước 3: Nhúng CKEditor vào Form Sản Phẩm

**3.1. Cập nhật `Views/Products/_CreateOrEditPartial.cshtml`**
Trong trường `Description`, ta sẽ loại bỏ thẻ `<textarea>` cơ bản và cấu hình bằng ID riêng để CKEditor nhận diện. (Chú ý: vì ở DigiPOSE ta đang dùng Bootstrap Modal (PartialView) để load form, nên việc Init CKEditor phải làm tại logic Javascript load Modal).

Thêm vào `_Layout.cshtml` hoặc `Index.cshtml` phần script của CKEditor:
```html
<script src="https://cdn.ckeditor.com/ckeditor5/40.2.0/classic/ckeditor.js"></script>
```

Trong View gọi Modal, bổ sung logic khởi tạo:
```javascript
ClassicEditor.create(document.querySelector('#Description'))
    .catch(error => { console.error(error); });
```

---

### Bước 4: Kiểm chứng tính năng Bảo Mật (BCrypt) tại UsersController

Source code hiện tại của DigiPOSE **đã hoàn thiện trước** tính năng này. Hãy mở file `Controllers/UsersController.cs` và xem logic tại hàm `Create` và `Edit`:

```csharp
using BC = BCrypt.Net.BCrypt;

[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> Create(User model)
{
    // ... validation
    // Hash the raw password before saving
    if (!string.IsNullOrWhiteSpace(model.PasswordHash))
        model.PasswordHash = BC.HashPassword(model.PasswordHash); // <-- Băm mật khẩu 1 chiều

    _context.Add(model);
    await _context.SaveChangesAsync();
}
```
**Nhận xét:**
Việc mật khẩu đã được tự động Hash từ Phase 02 chứng tỏ hệ thống được thiết kế Secure-by-default (Bảo mật từ trong trứng nước). Tuy nhiên tài liệu cũ thiếu ghi chép việc này.