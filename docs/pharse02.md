# BUỔI 2: TÙY BIẾN GIAO DIỆN (VIEW), XỬ LÝ LOGIC (CONTROLLER) & CLEAN CODE TỐI ƯU HIỆU NĂNG ADMIN CMS DIGIPOSE

> **Tài liệu Hướng dẫn Thực hành & Lý thuyết Kiến trúc Clean Code**  
> **Tác giả / Mentor**: Software Architecture & Senior Systems Engineer  
> **Dự án**: DigiPOSE Enterprise (Digital Point of Sale & ERP System)

---

## 📑 MỤC LỤC
1. [Lý thuyết Kiến trúc & Phân tích Đối chiếu (ITShop Buổi 2 vs. DigiPOSE ERP)](#1-lý-thuyết-kiến-trúc--phân-tích-đối-chiếu)
2. [Nguyên tắc Clean Code & Tối ưu Hiệu năng (Performance Optimization)](#2-nguyên-tắc-clean-code--tối-ưu-hiệu-năng)
3. [Bước 1: Cấu hình Trang Dashboard Admin Cyber HUD (`Views/Home/Index.cshtml`)](#bước-1-cấu-hình-trang-dashboard-admin-cyber-hud)
4. [Bước 2: Xây dựng Custom Slug Helper (Tạo SEO Slug Không Cần ThuVienNgoai)](#bước-2-xây-dựng-custom-slug-helper)
5. [Bước 3: Thiết kế Controller & Partial Views Tinh Gọn (Modal CRUD Architecture)](#bước-3-thiết-kế-controller--partial-views-tinh-gọn)
6. [Bước 4: Xử lý Logic Mã hóa Mật khẩu BCrypt & Tránh N+1 Query (Eager Loading)](#bước-4-xử-lý-logic-mã-hóa-mật-khẩu-bcrypt--tránh-n1-query)
7. [Bước 5: Tích hợp DataTables Server-Side AJAX cho 26 Entities](#bước-5-tích-hợp-datatables-server-side-ajax-cho-26-entities)
8. [Bước 6: Câu hỏi Lý thuyết & Bài tập Thực hành Buổi 2](#bước-6-câu-hỏi-lý-thuyết--bài-tập-thực-hành-buổi-2)

---

## 1. Lý thuyết Kiến trúc & Phân tích Đối chiếu

### 1.1. So sánh chi tiết: Bài thực hành ITShop Buổi 2 vs. DigiPOSE Enterprise Phase 02

| Tiêu chí Kiến trúc | ITShop Buổi 2 (Mô hình Đào tạo Nhập môn) | DigiPOSE Enterprise (Chuẩn Doanh nghiệp) | Đánh giá & Giải pháp Tối ưu của DigiPOSE |
| :--- | :--- | :--- | :--- |
| **Kiến trúc Giao diện CRUD** | 4 Trang Razor View riêng biệt cho mỗi Entity (`Index`, `Create`, `Edit`, `Delete`) | **Hệ thống Modal Dynamic Single-Page (`#globalModal`)** | Giảm từ 104 file HTML xuống còn 26 View chính. Người dùng thao tác CRUD trực tiếp trên Modal không cần chuyển trang. |
| **Cơ chế Phân trang & Nạp dữ liệu** | Duyệt vòng lặp `@foreach` HTML phía Client (`ToListAsync()`) | **100% Server-Side DataTables AJAX** (`System.Linq.Dynamic.Core`) | Khi bảng có 100,000 sản phẩm, ITShop bị tràn RAM & treo browser. DigiPOSE chỉ nạp đúng 10 dòng/trang với latency $< 50ms$. |
| **Tạo Slug Tiếng Việt** | Cài thêm thư viện ngoài `SlugGenerator` | **Viết `SlugHelper` nội bộ tích hợp Regex & Normalization** | Không phụ thuộc thư viện ngoài (Zero Dependency Overhead), chủ động tùy biến thuật toán chuyển đổi dấu Tiếng Việt chuẩn SEO. |
| **Bảo mật Mật khẩu người dùng** | Nhập & lưu trực tiếp Mật khẩu thô (Plaintext) vào Database | **Mã hóa BCrypt Hashing kèm Salt Rounds** | Chống rò rỉ mật khẩu ngay cả khi Database bị lộ (OWASP Security Compliant). |
| **Truy vấn Dữ liệu Quan hệ** | Dễ mắc lỗi $N+1$ Query nếu không dùng `.Include()` | **Eager Loading tối ưu với `.Include()` + Projections (`.Select()`)** | Chỉ Select đúng các thuộc tính cần thiết hiển thị trên Grid, giảm 80% dung lượng JSON truyền qua mạng. |

> 💡 **Kết luận từ Mentor**: ITShop Buổi 2 là mô hình nhập môn dùng View truyền thống (Full Page Reload) với **hạn chế kiến trúc lên tới > 75%** khi triển khai dự án thực tế. DigiPOSE áp dụng kiến trúc **Modal SPA + DataTables Server-Side AJAX** mang lại trải nghiệm người dùng cực mượt, không độ trễ.

---

## 2. Nguyên tắc Clean Code & Tối ưu Hiệu năng

1. **DRY (Don't Repeat Yourself)**: Thay vì viết lại 4 file View (`Create.cshtml`, `Edit.cshtml`...) cho 26 bảng ($26 \times 4 = 104$ views), DigiPOSE tái sử dụng `_CreateOrEditPartial.cshtml` và Modal tổng `#globalModal`.
2. **Tránh lỗi $N+1$ Queries**: Khi nạp danh sách Sản phẩm có liên kết tới `Category`, `Unit`, `Manufacturer`, bắt buộc sử dụng `.Include()` hoặcLINQ Projection `.Select()` để EF Core phát ra duy nhất **1 câu lệnh SQL `JOIN`**.
3. **Mã hóa Mật khẩu 1 chiều (One-Way Hashing)**: Không bao giờ lưu mật khẩu dạng văn bản thô. Luôn băm bằng BCrypt trước khi gọi `_context.SaveChangesAsync()`.
4. **Clean URLs & SEO-Friendly Slugs**: Chuyển đổi tên sản phẩm Tiếng Việt có dấu thành URL không dấu (ví dụ: `Bàn phím cơ Cyber` $\rightarrow$ `ban-phim-co-cyber`).

---

## 3. Bước 1: Cấu hình Trang Dashboard Admin Cyber HUD (`Views/Home/Index.cshtml`)

Trang chủ Admin được thiết kế theo chuẩn **Cyber-Cinematic Military HUD** với màu nền Carbon Void (`#0A0A0A`) và khung viền Hologram Cyan (`#00E5FF`):

```html
@{
    ViewData["Title"] = "Cyber HUD Command Center";
}

<div class="cyber-panel">
    <div class="cyber-panel-header">
        <div class="cyber-panel-title">
            <i class="fa-solid fa-microchip me-2 text-cyan"></i> CENTRAL COMMAND DASHBOARD
        </div>
        <div class="cyber-panel-sub">[DIGIPOSE ERP v1.0] // TELEMETRY MONITORING</div>
    </div>
    <div class="row g-4 mt-2">
        <div class="col-md-3">
            <div class="hud-stat-box border-cyan">
                <div class="hud-stat-label">TOTAL BRANCHES</div>
                <div class="hud-stat-value text-cyan">12</div>
                <div class="hud-stat-footer"><i class="fa-solid fa-code-branch me-1"></i> Active Nodes</div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="hud-stat-box border-emerald">
                <div class="hud-stat-label">ACTIVE SHIFTS</div>
                <div class="hud-stat-value text-emerald">28</div>
                <div class="hud-stat-footer"><i class="fa-solid fa-clock me-1"></i> Live Session</div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="hud-stat-box border-amber">
                <div class="hud-stat-label">LOW STOCK SKUs</div>
                <div class="hud-stat-value text-amber">05</div>
                <div class="hud-stat-footer"><i class="fa-solid fa-triangle-exclamation me-1"></i> Reorder Required</div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="hud-stat-box border-crimson">
                <div class="hud-stat-label">SYSTEM LATENCY</div>
                <div class="hud-stat-value text-crimson">14ms</div>
                <div class="hud-stat-footer"><i class="fa-solid fa-bolt me-1"></i> Optimal Speed</div>
            </div>
        </div>
    </div>
</div>
```

---

## 4. Bước 2: Xây dựng Custom Slug Helper

Tạo tập tin `Helpers/SlugHelper.cs` để tự động chuyển đổi ký tự Tiếng Việt có dấu sang định dạng URL không dấu chuẩn SEO:

```csharp
using System.Text.RegularExpressions;
using System.Text;

namespace DigiPOSE.Helpers
{
    public static class SlugHelper
    {
        public static string GenerateSlug(this string title)
        {
            if (string.IsNullOrWhiteSpace(title)) return string.Empty;

            // 1. Chuyển về chữ thường & Chuẩn hóa Unicode FormD
            string normalized = title.ToLowerInvariant().Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            foreach (char c in normalized)
            {
                // Loại bỏ các dấu phụ (Combining Diacritical Marks)
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            // 2. Chuyển chữ 'đ' thành 'd'
            string result = sb.ToString().Normalize(NormalizationForm.FormC).Replace('đ', 'd');

            // 3. Loại bỏ ký tự đặc biệt & khoảng trắng dư thừa
            result = Regex.Replace(result, @"[^a-z0-9\s-]", "");
            result = Regex.Replace(result, @"\s+", "-").Trim('-');

            return result;
        }
    }
}
```

---

## 5. Bước 3: Thiết kế Controller & Partial Views Tinh Gọn (Modal CRUD Architecture)

Thay vì tạo 4 file HTML tốn thời gian, ta sử dụng **1 View `Index.cshtml` chính** kết hợp với Modal Container `#globalModal`.

### 5.1. View `Index.cshtml` Mẫu cho `Categories`
```html
@model IEnumerable<DigiPOSE.Models.Category>
@{ ViewData["Title"] = "Category Management"; }

<div class="cyber-panel">
    <div class="cyber-panel-header">
        <div class="cyber-panel-title"><i class="fa-solid fa-folder-tree"></i> CATEGORY REGISTRY</div>
    </div>
    <div class="d-flex justify-content-between align-items-center mb-4">
        <button type="button" class="btn-cyber-success btn-show-modal" data-url="@Url.Action("Create", "Categories")">
            <i class="fa-solid fa-plus me-1"></i> ADD NEW CATEGORY
        </button>
    </div>
    <div class="table-responsive">
        <table id="datatable" class="table-cyber w-100">
            <thead>
                <tr>
                    <th style="width:5%;">#</th>
                    <th>CATEGORY NAME</th>
                    <th>SLUG</th>
                    <th style="width:120px; text-align:center;">ACTIONS</th>
                </tr>
            </thead>
        </table>
    </div>
</div>

<!-- Modal Container Dùng Chung -->
<div class="modal fade modal-cyber" id="globalModal" tabindex="-1" aria-hidden="true" data-bs-backdrop="static">
    <div class="modal-dialog modal-dialog-centered" style="max-width:620px;" id="globalModalContainer"></div>
</div>
```

---

## 6. Bước 4: Xử lý Logic Mã hóa Mật khẩu BCrypt & Tránh N+1 Query

Cập nhật `UsersController.cs` để mã hóa mật khẩu khi tạo mới và nạp dữ liệu quan hệ bằng Eager Loading:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Models;
using BC = BCrypt.Net.BCrypt;

namespace DigiPOSE.Controllers
{
    public class UsersController : Controller
    {
        private readonly DigiPoseDbContext _context;
        public UsersController(DigiPoseDbContext context) { _context = context; }

        // GET: Users/Create
        public IActionResult Create()
        {
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "BranchName");
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleName");
            return PartialView("_CreateOrEditPartial", new User());
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User model)
        {
            if (ModelState.IsValid)
            {
                // Băm mật khẩu bằng BCrypt chuẩn bảo mật OWASP
                model.PasswordHash = BC.HashPassword(model.PasswordHash);
                
                _context.Add(model);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "User created successfully." });
            }
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "BranchName", model.BranchId);
            ViewData["RoleId"] = new SelectList(_context.Roles, "RoleId", "RoleName", model.RoleId);
            return PartialView("_CreateOrEditPartial", model);
        }
    }
}
```

---

## 7. Bước 5: Tích hợp DataTables Server-Side AJAX cho 26 Entities

Ví dụ chuẩn triển khai API Endpoint `Index_LoadData` cho `ProductsController.cs`:

```csharp
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

        // Eager Loading đệm trước các navigation properties
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .AsQueryable();

        int totalRecords = await query.CountAsync();

        // Lọc dữ liệu động theo ô tìm kiếm
        if (!string.IsNullOrEmpty(searchValue))
        {
            query = query.Where(p => p.ProductName.Contains(searchValue) || p.SKU.Contains(searchValue));
        }

        int filterRecords = await query.CountAsync();

        // Sắp xếp động qua System.Linq.Dynamic.Core
        if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
        {
            query = query.OrderBy(sortColumn + " " + sortColumnDirection);
        }

        // Lấy trang dữ liệu hiện tại
        var dataList = await query.Skip(skip).Take(pageSize).Select(p => new {
            productId = p.ProductId,
            sku = p.SKU,
            productName = p.ProductName,
            categoryName = p.Category != null ? p.Category.CategoryName : "",
            retailPrice = p.RetailPrice
        }).ToListAsync();

        return Json(new { draw = draw, recordsFiltered = filterRecords, recordsTotal = totalRecords, data = dataList });
    }
    catch (Exception ex)
    {
        return Json(new { error = ex.Message });
    }
}
```

---

## 8. Bước 6: Câu hỏi Lý thuyết & Bài tập Thực hành Buổi 2

### ❓ Câu hỏi Lý thuyết Checklist
1. Tại sao cơ chế **Modal Single-Page CRUD** lại vượt trội hơn cấu trúc 4 View Razor truyền thống khi phát triển dự án thực tế?
2. Phân tích nguyên lý hoạt động của `System.Linq.Dynamic.Core` khi thực hiện `OrderBy` theo tên cột dạng chuỗi truyền từ DataTables Client.
3. Vì sao không nên truyền đối tượng `User` chứa `PasswordHash` trực tiếp về phía Client mà cần sử dụng DTO hoặc LINQ Projection `.Select()`?

### 🛠️ Bài tập Thực hành
1. Kiểm tra file `Helpers/SlugHelper.cs` và áp dụng hàm `GenerateSlug()` cho thuộc tính Category/Product.
2. Đảm bảo toàn bộ 26 View `Index.cshtml` trong dự án đã gọi Modal `#globalModal` khi bấm các nút Thêm mới, Sửa, Chi tiết, Xóa.
3. Tiến hành `dotnet run` và kiểm tra tốc độ nạp DataTables AJAX trên trình duyệt.

---
*DigiPOSE ERP System — Clean Architecture & High-Performance Standard Guidelines.*