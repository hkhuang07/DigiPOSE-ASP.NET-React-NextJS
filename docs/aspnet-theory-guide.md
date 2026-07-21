# 📖 GIẢI THÍCH LÝ THUYẾT ASP.NET MVC — Dựa Trên Source Code DigiPOSE

> Tài liệu này giải thích **từng khái niệm** trong code theo trình tự: **Model → View → Controller → AJAX Engine**.
> Mỗi mục đều có code thực tế từ project của bạn kèm dòng tham chiếu.

---

## PHẦN 1: MODEL — Nền Tảng Dữ Liệu

### 1.1. Class Model là gì?
Model là **lớp C# đại diện cho 1 bảng trong Database**. Mỗi property = 1 cột.

📌 Ví dụ: [Category.cs](file:///d:/Study/ASP_Web_Technology/Project/digipose/source/DigiPOSE/Models/Category.cs)

```csharp
public class Category
{
    [Key]                                                           // ← Khóa chính
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Category Name is required.")]         // ← Bắt buộc nhập
    [StringLength(100, ErrorMessage = "...cannot exceed 100...")]   // ← Giới hạn ký tự
    [Display(Name = "Category Name")]                               // ← Tên hiển thị trên UI
    public string CategoryName { get; set; } = null!;

    [StringLength(150)]
    public string? Slug { get; set; }                               // ← Dấu ? = cho phép NULL

    public bool IsActive { get; set; } = true;                     // ← Mặc định = true

    public ICollection<Product>? Products { get; set; }            // ← Quan hệ 1-N với Product
}
```

### 1.2. Giải thích Data Annotations (Validate trên Model)

| Annotation | Vị trí | Tác dụng |
|---|---|---|
| `[Key]` | Dòng 8 | Đánh dấu **Primary Key** — EF Core dùng cột này làm khóa chính |
| `[Required(ErrorMessage = "...")]` | Dòng 11 | **Bắt buộc nhập** — nếu để trống sẽ hiện thông báo lỗi đỏ trên form |
| `[StringLength(100)]` | Dòng 12 | **Giới hạn độ dài** — không được quá 100 ký tự |
| `[Display(Name = "Category Name")]` | Dòng 13 | **Tên hiển thị** — khi View dùng `@Html.DisplayNameFor()` sẽ hiện "Category Name" thay vì "CategoryName" |
| `string?` (nullable) | Dòng 17 | Cho phép cột này **để trống** (NULL) trong DB |
| `= null!` | Dòng 14 | Nói với compiler "Tôi biết nó non-null, tôi sẽ gán giá trị sau" (tránh warning) |
| `= true` | Dòng 23 | **Giá trị mặc định** khi tạo mới — IsActive mặc định là `true` |

> **Cách hoạt động**: Khi user submit form → ASP.NET tự động kiểm tra tất cả annotation → nếu vi phạm → `ModelState.IsValid = false` → hiện lỗi đỏ bên cạnh input.

---

## PHẦN 2: VIEW — Giao Diện Người Dùng

### 2.1. View là gì? PartialView là gì?

```
📁 Views/
├── Categories/
│   ├── Index.cshtml              ← VIEW CHÍNH (trang danh sách, hiện trực tiếp)
│   ├── _CreatePartial.cshtml     ← PARTIAL VIEW (mảnh HTML, load vào Modal)
│   ├── _EditPartial.cshtml       ← PARTIAL VIEW
│   ├── _DeletePartial.cshtml     ← PARTIAL VIEW
│   └── _DetailsPartial.cshtml    ← PARTIAL VIEW
└── Shared/
    └── _Layout.cshtml            ← LAYOUT CHUNG (navbar, sidebar, footer)
```

| Loại | Đặc điểm | Cách gọi |
|---|---|---|
| **View** (`Index.cshtml`) | Trang đầy đủ, có `@model`, được render bằng `return View()` | `return View(data);` |
| **PartialView** (`_CreatePartial.cshtml`) | **Mảnh HTML** không có layout riêng, được nhúng vào 1 trang khác | `return PartialView("_CreatePartial", data);` |
| **Layout** (`_Layout.cshtml`) | Khung trang chung, chứa `@RenderBody()` — nơi View được nhúng vào | Tự động áp dụng qua `_ViewStart.cshtml` |

> **Tại sao dùng PartialView?** Vì ta dùng **AJAX Modal** — khi user bấm "EDIT", JS gọi AJAX lấy HTML partial → nhét vào `<div id="globalModal">` → hiện lên như popup. Không cần redirect sang trang mới.

### 2.2. Directive `@model` — Khai báo kiểu dữ liệu cho View

📌 [Categories/Index.cshtml](file:///d:/Study/ASP_Web_Technology/Project/digipose/source/DigiPOSE/Views/Categories/Index.cshtml) — Dòng 1:
```razor
@model IEnumerable<DigiPOSE.Models.Category>
```
- `IEnumerable<Category>` = **danh sách** nhiều Category → dùng cho trang danh sách
- `Model` trong View sẽ có kiểu `IEnumerable<Category>`, bạn có thể `@foreach (var item in Model)`

📌 [_CreatePartial.cshtml](file:///d:/Study/ASP_Web_Technology/Project/digipose/source/DigiPOSE/Views/Categories/_CreatePartial.cshtml) — Dòng 1:
```razor
@model DigiPOSE.Models.Category
```
- **1 object duy nhất** Category → dùng cho form thêm/sửa/xóa

### 2.3. Tag Helpers (`asp-*`) — Cầu nối giữa HTML và C#

Tag Helpers là **attribute đặc biệt** mà ASP.NET dịch ra HTML chuẩn khi render trang.

#### 📌 Form Tag Helper

[_CreatePartial.cshtml](file:///d:/Study/ASP_Web_Technology/Project/digipose/source/DigiPOSE/Views/Categories/_CreatePartial.cshtml) — Dòng 10:
```html
<form class="ajax-modal-form" asp-action="Create" asp-controller="Categories" method="post">
```

| Tag Helper | Razor viết | HTML sau khi render |
|---|---|---|
| `asp-action="Create"` | Trỏ đến Action `Create` | `action="/Categories/Create"` |
| `asp-controller="Categories"` | Trỏ đến Controller `Categories` | (kết hợp với action thành URL) |
| `asp-route-id="@Model.CategoryId"` | Truyền tham số `id` lên URL | `action="/Categories/Edit/5"` |

→ ASP.NET tự động sinh ra thuộc tính `action="/Categories/Create"` trong HTML.

#### 📌 Input Tag Helper

[_CreatePartial.cshtml](file:///d:/Study/ASP_Web_Technology/Project/digipose/source/DigiPOSE/Views/Categories/_CreatePartial.cshtml) — Dòng 17:
```html
<input asp-for="CategoryName" class="form-control" placeholder="Enter category title..." />
```

**`asp-for="CategoryName"` tự động sinh ra:**
```html
<input type="text" 
       id="CategoryName" 
       name="CategoryName" 
       value=""
       data-val="true" 
       data-val-required="Category Name is required."
       data-val-length="Category Name cannot exceed 100 characters."
       data-val-length-max="100" />
```

> **Mấu chốt**: `asp-for` đọc annotation `[Required]`, `[StringLength]` từ Model → tự sinh `data-val-*` → jQuery Unobtrusive Validation dùng các `data-val-*` này để validate ở client (trước cả khi gửi lên server).

#### 📌 Label Tag Helper

```html
<label asp-for="CategoryName" class="form-cyber-label">CATEGORY NAME</label>
```
→ Tự động sinh `for="CategoryName"` để khi click label sẽ focus vào input.

#### 📌 Validation Tag Helper

```html
<span asp-validation-for="CategoryName" class="text-danger"></span>
```
→ Hiện **thông báo lỗi validate** bên cạnh input khi vi phạm `[Required]` hoặc `[StringLength]`.

#### 📌 Validation Summary

```html
<div asp-validation-summary="ModelOnly" class="text-danger"></div>
```
→ Hiện **tổng hợp lỗi** ở đầu form (chỉ lỗi model-level, không phải lỗi từng field).

#### 📌 Hidden Input (cho Edit/Delete)

[_EditPartial.cshtml](file:///d:/Study/ASP_Web_Technology/Project/digipose/source/DigiPOSE/Views/Categories/_EditPartial.cshtml) — Dòng 12:
```html
<input type="hidden" asp-for="CategoryId" />
```
→ **Gửi ID ẩn** lên server khi submit form Edit. Nếu không có, server không biết đang sửa record nào.

#### 📌 AntiForgeryToken

[_CreatePartial.cshtml](file:///d:/Study/ASP_Web_Technology/Project/digipose/source/DigiPOSE/Views/Categories/_CreatePartial.cshtml) — Dòng 11:
```razor
@Html.AntiForgeryToken()
```
→ Sinh ra `<input type="hidden" name="__RequestVerificationToken" value="abc123..." />`
→ **Bảo mật CSRF**: Server sẽ kiểm tra token này khớp với session → chặn request giả mạo từ trang khác.

### 2.4. Razor Syntax — Viết C# trong HTML

📌 [Categories/Index.cshtml](file:///d:/Study/ASP_Web_Technology/Project/digipose/source/DigiPOSE/Views/Categories/Index.cshtml) — Dòng 40-80:

```razor
@{ int idx = 1; }                                   // ← Khai báo biến C# trong View
@foreach (var item in Model) {                       // ← Vòng lặp qua danh sách
    <tr>
        <td>@idx.ToString("D2")</td>                  // ← In biến ra HTML (01, 02, 03...)
        <td>@item.CategoryName</td>                   // ← Truy cập property của object
        <td>@(string.IsNullOrEmpty(item.Slug) ? "N/A" : item.Slug)</td>  // ← Toán tử 3 ngôi
    </tr>
    idx++;
}
```

| Cú pháp | Ý nghĩa |
|---|---|
| `@{ ... }` | **Block C#** — chạy code C# nhưng không output HTML |
| `@variable` | **In giá trị** biến ra HTML |
| `@(expression)` | **In biểu thức** phức tạp (cần ngoặc tròn) |
| `@foreach`, `@if` | **Control flow** — viết logic C# trực tiếp |

### 2.5. `@Html.DisplayNameFor()` và `@Html.DisplayFor()`

```razor
<th>@Html.DisplayNameFor(model => model.CategoryName)</th>
```
→ Đọc `[Display(Name = "Category Name")]` từ Model → hiện **"Category Name"** làm tiêu đề cột.

```razor
<td>@Html.DisplayFor(modelItem => item.CategoryName)</td>
```
→ Hiện **giá trị** của property `CategoryName` (tương đương `@item.CategoryName` nhưng có format).

### 2.6. Select Dropdown (cho bảng có khóa ngoại)

📌 [Users/_CreatePartial.cshtml](file:///d:/Study/ASP_Web_Technology/Project/digipose/source/DigiPOSE/Views/Users/_CreatePartial.cshtml):
```html
<select asp-for="BranchId" class="form-select" asp-items="ViewBag.BranchId">
    <option value="">-- Chọn Chi nhánh --</option>
</select>
```
- `asp-for="BranchId"` → bind vào property `BranchId` của Model User
- `asp-items="ViewBag.BranchId"` → lấy danh sách options từ `SelectList` mà Controller đã chuẩn bị

### 2.7. Modal Container (khung chứa PartialView)

📌 [Categories/Index.cshtml](file:///d:/Study/ASP_Web_Technology/Project/digipose/source/DigiPOSE/Views/Categories/Index.cshtml) — Dòng 87-92:
```html
<!-- SINGLE GENERIC GLOBAL MODAL CONTAINER -->
<div class="modal fade" id="globalModal" tabindex="-1" data-bs-backdrop="static">
    <div class="modal-dialog" id="globalModalContainer">
        <!-- DYNAMIC PARTIAL VIEWS LOADED VIA AJAX -->
    </div>
</div>
```
→ Đây là **khung rỗng**. Khi user bấm nút → AJAX sẽ lấy HTML partial từ server → nhét vào `#globalModalContainer` → hiện modal.

### 2.8. Nút bấm mở Modal

📌 [Categories/Index.cshtml](file:///d:/Study/ASP_Web_Technology/Project/digipose/source/DigiPOSE/Views/Categories/Index.cshtml) — Dòng 66-77:
```html
<!-- Nút ADD -->
<button class="btn-show-modal" data-url="@Url.Action("Create", "Categories")">ADD NEW</button>

<!-- Nút EDIT (truyền id của dòng hiện tại) -->
<button class="btn-show-modal" data-url="@Url.Action("Edit", "Categories", new { id = item.CategoryId })">EDIT</button>

<!-- Nút DELETE -->
<button class="btn-show-modal" data-url="@Url.Action("Delete", "Categories", new { id = item.CategoryId })">DELETE</button>
```

| Thuộc tính | Giá trị render ra | Ý nghĩa |
|---|---|---|
| `data-url` cho Create | `/Categories/Create` | Gọi Action Create (không cần id) |
| `data-url` cho Edit | `/Categories/Edit/5` | Gọi Action Edit với `id=5` |
| `data-url` cho Delete | `/Categories/Delete/5` | Gọi Action Delete với `id=5` |

> **Cách lấy ID từ 1 dòng**: `@item.CategoryId` — trong vòng `@foreach`, mỗi `item` là 1 object Category, truy cập `.CategoryId` để lấy ID của dòng đó.

---

## PHẦN 3: CONTROLLER — Bộ Não Xử Lý

### 3.1. Cấu trúc tổng quan Controller

📌 [CategoriesController.cs](file:///d:/Study/ASP_Web_Technology/Project/digipose/source/DigiPOSE/Controllers/CategoriesController.cs):

```csharp
public class CategoriesController : Controller        // ← Kế thừa class Controller của ASP.NET
{
    private readonly DigiPoseDbContext _context;        // ← Biến giữ kết nối Database

    public CategoriesController(DigiPoseDbContext context)  // ← Constructor — nhận DbContext
    {
        _context = context;                             // ← Lưu lại để dùng trong các Action
    }
    
    // Các Action methods ở dưới...
}
```

### 3.2. Dependency Injection (DI) — `_context` đến từ đâu?

```
Program.cs: builder.Services.AddDbContext<DigiPoseDbContext>(...) 
                    ↓
ASP.NET tự động tạo DigiPoseDbContext khi có request
                    ↓
Controller constructor: public CategoriesController(DigiPoseDbContext context)
                    ↓
ASP.NET "tiêm" context vào → _context = context
```

→ Bạn **không bao giờ phải new DigiPoseDbContext()** — framework tự lo.

### 3.3. DbContext (`_context`) — "Cánh cổng" vào Database

`_context` là đại diện cho toàn bộ Database. Mỗi property `DbSet` = 1 bảng.

```csharp
_context.Categories      // → Bảng Category
_context.Products        // → Bảng Product  
_context.Users           // → Bảng User
```

### 3.4. Giải thích từng Action CRUD

#### 📌 INDEX — Lấy toàn bộ danh sách (`FindAll`)

[CategoriesController.cs](file:///d:/Study/ASP_Web_Technology/Project/digipose/source/DigiPOSE/Controllers/CategoriesController.cs#L18-L22) — Dòng 18:
```csharp
public async Task<IActionResult> Index()
{
    var categories = await _context.Categories.ToListAsync();
    return View(categories);
}
```

**Phân tích từng keyword:**

| Keyword | Ý nghĩa |
|---|---|
| `public` | Phương thức công khai — ASP.NET routing có thể gọi được |
| `async` | Đánh dấu phương thức **bất đồng bộ** (không chặn thread chính) |
| `Task<IActionResult>` | **Kiểu trả về**: `Task` = "lời hứa sẽ trả kết quả trong tương lai", `IActionResult` = kết quả Action (View, JSON, Redirect...) |
| `await` | **Chờ** tác vụ bất đồng bộ hoàn thành rồi mới chạy dòng tiếp theo |
| `_context.Categories` | Truy cập bảng `Category` trong DB |
| `.ToListAsync()` | **Lấy TẤT CẢ** records → trả về `List<Category>` — tương đương `SELECT * FROM Categories` |
| `return View(categories)` | Trả về View `Index.cshtml` kèm dữ liệu `categories` |

> **Tại sao `async/await`?** Khi query DB, thread phải **chờ** DB trả kết quả. Nếu dùng đồng bộ (`ToList()`), thread bị **khóa** → server xử lý chậm. Dùng `async/await` → thread được **giải phóng** để phục vụ request khác trong lúc chờ DB.

#### 📌 DETAILS — Tìm 1 record theo ID (`FindById`)

[CategoriesController.cs](file:///d:/Study/ASP_Web_Technology/Project/digipose/source/DigiPOSE/Controllers/CategoriesController.cs#L25-L33) — Dòng 25:
```csharp
public async Task<IActionResult> Details(int? id)              // ← id? = cho phép null
{
    if (id == null) return NotFound();                          // ← Nếu không có id → 404

    var category = await _context.Categories
        .FirstOrDefaultAsync(m => m.CategoryId == id);          // ← TÌM theo điều kiện
    
    if (category == null) return NotFound();                    // ← Không tìm thấy → 404

    return PartialView("_DetailsPartial", category);            // ← Trả về PartialView
}
```

**So sánh 2 cách tìm theo ID:**

| Phương thức | Ý nghĩa | SQL tương đương |
|---|---|---|
| `FindAsync(id)` | Tìm theo **Primary Key** — nhanh nhất, có cache | `SELECT * FROM Categories WHERE CategoryId = 5` |
| `FirstOrDefaultAsync(m => m.CategoryId == id)` | Tìm theo **điều kiện bất kỳ** — linh hoạt hơn, có thể `Include()` | Tương tự nhưng cho phép JOIN |

> **`int? id`** — dấu `?` = nullable. Nếu URL không có id (ví dụ `/Categories/Details/`), `id` sẽ = `null` → ta kiểm tra và trả 404.

#### 📌 CREATE (GET) — Mở form trống

[CategoriesController.cs](file:///d:/Study/ASP_Web_Technology/Project/digipose/source/DigiPOSE/Controllers/CategoriesController.cs#L36-L39) — Dòng 36:
```csharp
public IActionResult Create()
{
    return PartialView("_CreatePartial", new Category());
}
```
- `new Category()` → tạo object rỗng → form sẽ trống
- `PartialView(...)` → trả HTML mảnh (không có layout) → AJAX nhét vào modal

#### 📌 CREATE (POST) — Xử lý thêm mới

[CategoriesController.cs](file:///d:/Study/ASP_Web_Technology/Project/digipose/source/DigiPOSE/Controllers/CategoriesController.cs#L42-L71) — Dòng 42:
```csharp
[HttpPost]                                // ← Chỉ chấp nhận HTTP POST
[ValidateAntiForgeryToken]                // ← Kiểm tra token CSRF
public async Task<IActionResult> Create(
    [Bind("CategoryId,CategoryName,Slug,Description")] Category category)  // ← Bind
{
    // 1. Auto-generate slug nếu user để trống
    if (string.IsNullOrWhiteSpace(category.Slug) && !string.IsNullOrWhiteSpace(category.CategoryName))
    {
        category.Slug = SlugHelper.GenerateSlug(category.CategoryName);
    }

    ModelState.Remove("Slug");            // ← Bỏ qua validate cho Slug (vì ta tự sinh)

    if (!ModelState.IsValid)              // ← Kiểm tra validate [Required], [StringLength]
    {
        return PartialView("_CreatePartial", category);  // ← Trả HTML có lỗi đỏ
    }

    _context.Add(category);               // ← Đánh dấu "sẽ thêm" vào DB
    await _context.SaveChangesAsync();     // ← THỰC SỰ ghi vào DB (INSERT INTO...)

    return Json(new {                     // ← Trả JSON cho AJAX
        success = true, 
        message = "Category created successfully." 
    });
}
```

**Giải thích từng attribute/keyword:**

| Keyword | Dòng | Ý nghĩa |
|---|---|---|
| `[HttpPost]` | 42 | Action này **chỉ** được gọi bằng POST (form submit), không phải GET (truy cập URL) |
| `[ValidateAntiForgeryToken]` | 43 | **Bắt buộc** request phải có token CSRF hợp lệ (từ `@Html.AntiForgeryToken()`) |
| `[Bind("CategoryId,CategoryName,Slug,Description")]` | 44 | **Chỉ cho phép** bind các field này từ form → **bảo mật**: hacker không thể inject thêm field khác |
| `ModelState.IsValid` | 53 | Kiểm tra **tất cả Data Annotation** (`[Required]`, `[StringLength]`) trên Model có pass không |
| `ModelState.Remove("Slug")` | 51 | Bỏ qua lỗi validate cho `Slug` — vì ta sẽ tự sinh nếu trống |
| `_context.Add(category)` | 62 | Đánh dấu object để **INSERT** — chưa ghi DB |
| `await _context.SaveChangesAsync()` | 63 | **Thực sự ghi** vào DB — tất cả `Add/Update/Remove` trước đó mới có hiệu lực |
| `return Json(new { success = true })` | 65 | Trả **JSON** cho JavaScript AJAX — không phải HTML |

#### 📌 EDIT (GET) — Mở form có dữ liệu sẵn

[CategoriesController.cs](file:///d:/Study/ASP_Web_Technology/Project/digipose/source/DigiPOSE/Controllers/CategoriesController.cs#L74-L82) — Dòng 74:
```csharp
public async Task<IActionResult> Edit(int? id)
{
    if (id == null) return NotFound();

    var category = await _context.Categories.FindAsync(id);    // ← Tìm theo ID
    if (category == null) return NotFound();

    return PartialView("_EditPartial", category);               // ← Trả partial CÓ DỮ LIỆU
}
```

> **Cách "đổ dữ liệu" lên form Edit**: `FindAsync(id)` lấy record từ DB → truyền vào `PartialView("_EditPartial", category)` → View nhận được `@Model` chứa dữ liệu → `asp-for="CategoryName"` tự động đổ giá trị vào `value=""` của input.

#### 📌 EDIT (POST) — Xử lý cập nhật

[CategoriesController.cs](file:///d:/Study/ASP_Web_Technology/Project/digipose/source/DigiPOSE/Controllers/CategoriesController.cs#L85-L125) — Dòng 85:
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, [Bind("...")] Category category)
{
    if (id != category.CategoryId)                  // ← Kiểm tra ID URL khớp với ID form
        return Json(new { success = false, message = "Category ID mismatch." });

    // ... validate + slug logic ...

    try
    {
        _context.Update(category);                  // ← Đánh dấu UPDATE
        await _context.SaveChangesAsync();          // ← Ghi vào DB
    }
    catch (DbUpdateConcurrencyException)            // ← Xung đột dữ liệu
    {
        if (!CategoryExists(category.CategoryId))
            return Json(new { success = false, message = "Record no longer exists." });
        throw;
    }

    return Json(new { success = true, message = "Category updated successfully." });
}
```

> **`DbUpdateConcurrencyException`**: Xảy ra khi 2 người cùng sửa 1 record. Người thứ 2 submit → DB phát hiện data đã thay đổi → ném exception → ta bắt và xử lý.

#### 📌 DELETE (GET) — Hiện xác nhận xóa

[CategoriesController.cs](file:///d:/Study/ASP_Web_Technology/Project/digipose/source/DigiPOSE/Controllers/CategoriesController.cs#L128-L136) — Dòng 128:
```csharp
public async Task<IActionResult> Delete(int? id)
{
    var category = await _context.Categories
        .FirstOrDefaultAsync(m => m.CategoryId == id);
    return PartialView("_DeletePartial", category);    // ← Hiện dialog xác nhận
}
```

#### 📌 DELETE (POST) — Thực sự xóa

[CategoriesController.cs](file:///d:/Study/ASP_Web_Technology/Project/digipose/source/DigiPOSE/Controllers/CategoriesController.cs#L139-L151) — Dòng 139:
```csharp
[HttpPost, ActionName("Delete")]                        // ← ActionName = URL vẫn là /Delete
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeleteConfirmed(int id)  // ← Tên method khác để tránh trùng
{
    var category = await _context.Categories.FindAsync(id);
    if (category != null)
    {
        _context.Categories.Remove(category);           // ← Đánh dấu XÓA
    }
    await _context.SaveChangesAsync();                  // ← Ghi vào DB (DELETE FROM...)
    return Json(new { success = true, message = "Category deleted successfully." });
}
```

> **Tại sao tên method là `DeleteConfirmed` chứ không phải `Delete`?** Vì đã có `Delete(int? id)` GET ở trên. C# không cho 2 method cùng tên cùng tham số → đặt tên khác + dùng `[ActionName("Delete")]` để URL routing vẫn map đúng.

#### 📌 TOGGLE ACTIVE — Bật/Tắt trạng thái (Soft Delete)

[CategoriesController.cs](file:///d:/Study/ASP_Web_Technology/Project/digipose/source/DigiPOSE/Controllers/CategoriesController.cs#L153-L161) — Dòng 153:
```csharp
[HttpPost, ValidateAntiForgeryToken]
public async Task<IActionResult> ToggleActive(int id)
{
    var item = await _context.Categories.FindAsync(id);
    if (item == null) return Json(new { success = false });
    
    item.IsActive = !item.IsActive;             // ← ĐẢO NGƯỢC: true→false, false→true
    await _context.SaveChangesAsync();
    
    return Json(new { 
        success = true, 
        isActive = item.IsActive, 
        message = item.IsActive ? "Activated." : "Deactivated." 
    });
}
```

> **Soft Delete vs Hard Delete**: `Remove()` = xóa khỏi DB vĩnh viễn. `IsActive = false` = ẩn đi nhưng vẫn còn trong DB → có thể khôi phục.

### 3.5. Eager Loading — `Include()` là gì?

📌 [ProductsController.cs](file:///d:/Study/ASP_Web_Technology/Project/digipose/source/DigiPOSE/Controllers/ProductsController.cs#L19-L26) — Dòng 19:
```csharp
public async Task<IActionResult> Index()
    => View(await _context.Products
        .Include(p => p.Category)           // ← JOIN với bảng Category
        .Include(p => p.ProductType)        // ← JOIN với bảng ProductType
        .Include(p => p.Unit)               // ← JOIN với bảng Unit
        .Include(p => p.Manufacturer)       // ← JOIN với bảng Manufacturer
        .Include(p => p.TaxType)            // ← JOIN với bảng TaxType
        .ToListAsync());
```

| Không có `Include()` | Có `Include()` |
|---|---|
| `item.Category` = **null** | `item.Category` = **object đầy đủ** |
| `item.Category.CategoryName` → **crash NullRef** | `item.Category.CategoryName` → "Điện thoại" ✅ |

→ `Include()` = **Eager Loading** — nạp sẵn dữ liệu bảng liên kết để View có thể hiển thị tên thay vì chỉ ID.

### 3.6. SelectList + ViewBag — Đổ dữ liệu cho Dropdown

📌 [UsersController.cs](file:///d:/Study/ASP_Web_Technology/Project/digipose/source/DigiPOSE/Controllers/UsersController.cs#L31-L36) — Dòng 31:
```csharp
public IActionResult Create()
{
    ViewBag.BranchId = new SelectList(
        _context.Branches.Where(b => b.IsActive),  // ← Nguồn dữ liệu (chỉ lấy active)
        "BranchId",                                  // ← Value (gửi lên server)
        "BranchName"                                 // ← Text (hiện trên dropdown)
    );
    ViewBag.RoleId = new SelectList(_context.Roles, "RoleId", "RoleName");
    return PartialView("_CreatePartial", new User());
}
```

> `ViewBag` là **túi dữ liệu động** — Controller bỏ dữ liệu vào, View lấy ra qua `asp-items="ViewBag.BranchId"`.

### 3.7. BCrypt — Mã hóa mật khẩu

📌 [UsersController.cs](file:///d:/Study/ASP_Web_Technology/Project/digipose/source/DigiPOSE/Controllers/UsersController.cs#L5) — Dòng 5:
```csharp
using BC = BCrypt.Net.BCrypt;   // ← Import + đặt alias "BC"
```

📌 Dòng 50-51 (Create):
```csharp
if (!string.IsNullOrWhiteSpace(model.PasswordHash))
    model.PasswordHash = BC.HashPassword(model.PasswordHash);
// Input:  "123456"
// Output: "$2a$11$KJxGh2F7jQK0..." (chuỗi hash 60 ký tự, không thể giải ngược)
```

📌 Dòng 89-92 (Edit — chỉ hash lại nếu có mật khẩu mới):
```csharp
model.PasswordHash = !string.IsNullOrWhiteSpace(NewPassword)
    ? BC.HashPassword(NewPassword)       // ← Có mật khẩu mới → hash nó
    : existing.PasswordHash;             // ← Không đổi → giữ hash cũ
```

### 3.8. Slug — URL thân thiện

📌 [SlugHelper.cs](file:///d:/Study/ASP_Web_Technology/Project/digipose/source/DigiPOSE/Helpers/SlugHelper.cs):
```
Input:  "Điện thoại Samsung"
Output: "dien-thoai-samsung"
```

Quy trình: Lowercase → Thay `đ` → Decompose Unicode (bỏ dấu) → Giữ a-z, 0-9 → Thay ký tự khác bằng `-`.

---

## PHẦN 4: AJAX MODAL ENGINE — Cơ Chế Mở/Đóng/Submit Modal

📌 [site.js](file:///d:/Study/ASP_Web_Technology/Project/digipose/source/DigiPOSE/wwwroot/js/site.js)

### 4.1. Mở Modal (GET AJAX)

Dòng 43-81 — Khi user bấm nút `.btn-show-modal`:

```
User bấm [EDIT] trên dòng ID=5
        ↓
JS đọc: data-url="/Categories/Edit/5"
        ↓
$.ajax({ url: "/Categories/Edit/5", type: "GET" })
        ↓
Server chạy: Edit(5) → FindAsync(5) → return PartialView("_EditPartial", category)
        ↓
Server trả HTML: <div class="modal-content">...<input value="Điện thoại">...</div>
        ↓
JS nhét HTML vào: $('#globalModalContainer').html(htmlContent)
        ↓
JS mở modal: bootstrap.Modal.show()
        ↓
👁️ User thấy popup Edit với dữ liệu sẵn
```

### 4.2. Submit Form trong Modal (POST AJAX)

Dòng 84-166 — Khi user bấm "SAVE" trong modal:

```
User bấm [SAVE CATEGORY]
        ↓
JS chặn submit mặc định: e.preventDefault()
        ↓
JS validate client-side: $form.valid()
        ↓
JS khóa nút (chống bấm 2 lần): lockFormSubmit($form)
        ↓
$.ajax({ url: "/Categories/Create", type: "POST", data: $form.serialize() })
        ↓
Server chạy: Create(category) → SaveChangesAsync() → return Json({success:true})
        ↓
                    ┌─── JSON {success: true} ──→ Đóng modal → Thông báo xanh → location.reload()
JS xử lý response ─┤
                    ├─── JSON {success: false} ─→ Thông báo cam (warning)
                    │
                    └─── HTML (ModelState invalid) → Nhét HTML mới vào modal (có lỗi đỏ)
```

### 4.3. Cancel / Đóng Modal

```html
<button type="button" data-bs-dismiss="modal">CANCEL</button>
```
→ `data-bs-dismiss="modal"` là **Bootstrap attribute** — tự động đóng modal khi bấm. Không cần viết JS.

### 4.4. Reload dữ liệu sau khi thao tác

Dòng 131-133 trong site.js:
```javascript
setTimeout(function () {
    location.reload();    // ← Reload toàn trang để lấy data mới từ server
}, 600);                  // ← Delay 600ms để user thấy thông báo thành công
```

### 4.5. Toggle Active (Soft Delete qua AJAX)

Dòng 169-229 — Khi user bấm badge ACTIVE/INACTIVE:

```
User bấm badge [ACTIVE] trên dòng
        ↓
JS hiện Confirm dialog (Cyber style)
        ↓
User bấm [DEACTIVATE]
        ↓
$.ajax POST → /Categories/ToggleActive/5
        ↓
Server: item.IsActive = !item.IsActive → SaveChanges
        ↓
Server trả: { success: true, isActive: false, message: "Deactivated." }
        ↓
JS thông báo → reload trang
```

---

## PHẦN 5: TỔNG KẾT LUỒNG DỮ LIỆU

```
┌──────────────┐    GET /Index     ┌──────────────────┐    ToListAsync()   ┌──────────┐
│   BROWSER    │ ──────────────→   │   CONTROLLER     │ ──────────────→    │ DATABASE │
│   (View)     │ ←──────────────   │   (Action)       │ ←──────────────    │ (SQL)    │
└──────────────┘    return View()  └──────────────────┘   List<Category>   └──────────┘

┌──────────────┐  AJAX GET /Edit/5 ┌──────────────────┐    FindAsync(5)   ┌──────────┐
│  MODAL (JS)  │ ──────────────→   │   Edit(int? id)  │ ──────────────→   │ DATABASE │
│              │ ←──────────────   │                  │ ←──────────────   │          │
└──────────────┘  PartialView HTML └──────────────────┘    Category obj   └──────────┘

┌──────────────┐  AJAX POST /Edit  ┌──────────────────┐  Update + Save   ┌──────────┐
│  MODAL FORM  │ ──────────────→   │  Edit(id, model) │ ──────────────→  │ DATABASE │
│  (submit)    │ ←──────────────   │                  │ ←──────────────  │          │
└──────────────┘  JSON {success}   └──────────────────┘      OK/Error    └──────────┘
```

---

## PHẦN 6: BẢNG TRA CỨU NHANH

| Từ khóa | Ý nghĩa | Ví dụ trong project |
|---|---|---|
| `async` | Phương thức bất đồng bộ | `public async Task<IActionResult> Index()` |
| `await` | Chờ tác vụ async hoàn thành | `await _context.Categories.ToListAsync()` |
| `Task<T>` | "Lời hứa" trả về kiểu T trong tương lai | `Task<IActionResult>` |
| `IActionResult` | Interface chung cho mọi kiểu response | View, Json, PartialView, NotFound, Redirect |
| `return View(data)` | Trả HTML trang đầy đủ | `return View(categories)` |
| `return PartialView(name, data)` | Trả HTML mảnh (cho AJAX) | `return PartialView("_CreatePartial", new Category())` |
| `return Json(obj)` | Trả JSON (cho AJAX) | `return Json(new { success = true })` |
| `return NotFound()` | Trả HTTP 404 | `if (id == null) return NotFound()` |
| `return RedirectToAction(name)` | Chuyển hướng sang Action khác | `return RedirectToAction(nameof(Index))` |
| `[HttpPost]` | Chỉ chấp nhận POST | Trên method Create, Edit, Delete |
| `[HttpGet]` | Chỉ chấp nhận GET (mặc định) | Không cần ghi, mặc định đã là GET |
| `[ValidateAntiForgeryToken]` | Kiểm tra CSRF token | Trên mọi POST action |
| `[Bind("...")]` | Whitelist các field cho phép bind | `[Bind("CategoryId,CategoryName")]` |
| `[ActionName("Delete")]` | Ghi đè tên action trong URL | `DeleteConfirmed` → URL vẫn là `/Delete` |
| `ModelState.IsValid` | Kiểm tra tất cả Data Annotations | `if (!ModelState.IsValid) return PartialView(...)` |
| `FindAsync(id)` | Tìm theo Primary Key | `_context.Categories.FindAsync(id)` |
| `FirstOrDefaultAsync(...)` | Tìm theo điều kiện | `_context.Categories.FirstOrDefaultAsync(m => m.CategoryId == id)` |
| `ToListAsync()` | Lấy tất cả records | `_context.Categories.ToListAsync()` |
| `Include(x => x.Nav)` | Eager Loading (JOIN) | `_context.Products.Include(p => p.Category)` |
| `_context.Add(obj)` | Đánh dấu INSERT | `_context.Add(category)` |
| `_context.Update(obj)` | Đánh dấu UPDATE | `_context.Update(category)` |
| `_context.Remove(obj)` | Đánh dấu DELETE | `_context.Categories.Remove(category)` |
| `SaveChangesAsync()` | Ghi tất cả thay đổi vào DB | `await _context.SaveChangesAsync()` |
| `SelectList` | Tạo datasource cho dropdown | `new SelectList(_context.Branches, "BranchId", "BranchName")` |
| `ViewBag` | Túi dữ liệu dynamic từ Controller→View | `ViewBag.BranchId = new SelectList(...)` |
| `BC.HashPassword()` | Mã hóa BCrypt 1 chiều | `BC.HashPassword("123456")` → `$2a$11$...` |
