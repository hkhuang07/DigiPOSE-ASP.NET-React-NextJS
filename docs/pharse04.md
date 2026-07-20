
BUỔI 4: TỐI ƯU HIỆU NĂNG BẢNG DỮ LIỆU (SERVER-SIDE DATATABLES) VÀ TÍCH HỢP CKEDITOR (DIGIPOSE)Bước 1: Cài đặt thư viện truy vấn độngĐể DataTables.net có thể tự động ánh xạ (map) các cột cần sắp xếp (Sort) từ chuỗi string được gửi lên thông qua Ajax request, ta cần cài đặt gói thư viện truy vấn động của LINQ. Mở Package Manager Console và chạy lệnh:  PowerShellInstall-Package System.Linq.Dynamic.Core
Bước 2: Tích hợp thư viện UI của DataTables.net vào LayoutCập nhật file Views/Shared/_Layout.cshtml để bổ sung các file CSS và JS dùng cho DataTables và tính năng xuất file (Export Excel/Copy). Thêm RenderSection Styles vào trong thẻ <head>:  HTML<head>
    <!-- ... các meta tags hiện có ... -->
    <title>@ViewData["Title"] - DigiPOSE Admin</title>
    <link rel="stylesheet" href="~/lib/bootstrap/css/bootstrap.min.css" />
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
    
    <!-- Dành cho các CSS nhúng thêm từ các View con -->
    @await RenderSectionAsync("Styles", required: false)
</head>
Bước 3: Tối ưu hóa phân trang Server-side cho bảng Hàng hóa (Product)Khi dữ liệu Hàng hóa phình to, ta không thể tải toàn bộ dữ liệu lên View cùng lúc. Ta sẽ tái cấu trúc ProductsController để xử lý Ajax Request.  1. Cập nhật Controllers/ProductsController.cs:
Bổ sung hàm Index_LoadData với sự tích hợp của Manufacturer và lọc IsActive:  C#using System.Linq.Dynamic.Core; // Bắt buộc thêm thư viện này
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Web.Models;

namespace DigiPOSE.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly DigiPoseDbContext _context;

        public ProductsController(DigiPoseDbContext context) { _context = context; }

        public IActionResult Index() => View();

        [HttpPost]
        public IActionResult Index_LoadData()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();
            
            int pageSize = length != null ? int.Parse(length) : 0;
            int skip = start != null ? int.Parse(start) : 0;

            // Khởi tạo truy vấn gốc với Xóa mềm và Manufacturer
            var productQuery = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Unit)
                .Include(p => p.Manufacturer)
                .Where(p => p.IsActive) // Chỉ lấy hàng còn kinh doanh
                .AsQueryable();

            int totalRecords = productQuery.Count();

            // Lọc dữ liệu (Searching)
            if (!string.IsNullOrEmpty(searchValue))
            {
                productQuery = productQuery.Where(p => 
                    p.ProductName.Contains(searchValue) ||
                    p.SKU.Contains(searchValue) ||
                    p.Category.CategoryName.Contains(searchValue) ||
                    p.Manufacturer.ManufacturerName.Contains(searchValue));
            }
            int filterRecords = productQuery.Count();

            // Sắp xếp dữ liệu (Sorting)
            if (!string.IsNullOrEmpty(sortColumn))
            {
                productQuery = productQuery.OrderBy(sortColumn + " " + sortColumnDirection);
            }

            // Lấy dữ liệu và trả về JSON
            var dataList = productQuery.Skip(skip).Take(pageSize).Select(p => new {
                p.ProductId,
                p.SKU,
                p.ProductName,
                CategoryName = p.Category.CategoryName,
                ManufacturerName = p.Manufacturer.ManufacturerName,
                UnitName = p.Unit.UnitName,
                p.BasePrice
            }).ToList();

            return Json(new { draw, recordsFiltered = filterRecords, recordsTotal = totalRecords, data = dataList });
        }
    }
}
2. Cập nhật Views/Products/Index.cshtml để gọi Ajax:
Xóa vòng lặp cũ, cấu hình DataTables tương thích với Blueprint V2.0:  HTML@section Styles {
    <link rel="stylesheet" href="https://cdn.datatables.net/1.13.8/css/dataTables.bootstrap5.min.css" />
}

<table id="datatable" class="table table-sm table-hover table-bordered mb-0 w-100">
    <thead class="table-light">
        <tr>
            <th>#</th><th>SKU</th><th>Tên sản phẩm</th><th>Danh mục</th><th>Hãng SX</th><th>ĐVT</th><th>Giá cơ sở</th><th>Thao tác</th>
        </tr>
    </thead>
</table>

@section Scripts {
    <script src="https://cdn.datatables.net/1.13.8/js/jquery.dataTables.min.js"></script>
    <script>
        $(document).ready(function () {
            $('#datatable').DataTable({
                serverSide: true,
                ajax: { url: '@Url.Action("Index_LoadData", "Products")', type: 'POST' },
                columns: [
                    { data: null, render: (d, t, r, m) => m.row + 1, sortable: false },
                    { data: 'sku', name: 'SKU' },
                    { data: 'productName', name: 'ProductName' },
                    { data: 'categoryName', name: 'CategoryName' },
                    { data: 'manufacturerName', name: 'ManufacturerName' },
                    { data: 'unitName', name: 'UnitName' },
                    { data: 'basePrice', name: 'BasePrice', render: $.fn.dataTable.render.number(',', '.', 0, '') },
                    { data: 'productId', render: d => `<a href="/Products/Edit/${d}" class="btn btn-sm btn-outline-primary">Sửa</a>`, sortable: false }
                ]
            });
        });
    </script>
}
Bước 4: Tích hợp CKEditor  Áp dụng cho các trường ghi chú dài như mô tả sản phẩm hoặc biên bản kho. Sếp nhúng thư viện từ wwwroot/lib/ckeditor5/ và khởi tạo như cũ trong @section Scripts của View cần sử dụng.  


# OLD VERSION
BUỔI 4: TỐI ƯU HIỆU NĂNG BẢNG DỮ LIỆU (SERVER-SIDE DATATABLES) VÀ TÍCH HỢP CKEDITOR (DIGIPOSE)
Bước 1: Cài đặt thư viện truy vấn độngĐể DataTables.net có thể tự động ánh xạ (map) các cột cần sắp xếp (Sort) từ chuỗi string được gửi lên thông qua Ajax request, ta cần cài đặt gói thư viện truy vấn động của LINQ.  Mở Package Manager Console và chạy lệnh:PowerShellInstall-Package System.Linq.Dynamic.Core
Bước 2: Tích hợp thư viện UI của DataTables.net vào LayoutCập nhật file Views/Shared/_Layout.cshtml để bổ sung các file CSS và JS dùng cho DataTables và tính năng xuất file (Export Excel/Copy).  Thêm RenderSection Styles vào trong thẻ <head>:  HTML<head>
    <!-- ... các meta tags hiện có ... -->
    <title>@ViewData["Title"] - DigiPOSE Admin</title>
    <link rel="stylesheet" href="~/lib/bootstrap/css/bootstrap.min.css" />
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
    
    <!-- Dành cho các CSS nhúng thêm từ các View con -->
    @await RenderSectionAsync("Styles", required: false)
</head>
Bước 3: Tối ưu hóa phân trang Server-side cho bảng Hàng hóa (Product)Khi dữ liệu Hàng hóa phình to, ta không thể tải toàn bộ dữ liệu lên View cùng lúc. Ta sẽ tái cấu trúc ProductsController để xử lý Ajax Request.  1. Cập nhật Controllers/ProductsController.cs:
Bổ sung hàm Index_LoadData để nhận các tham số phân trang, tìm kiếm từ DataTables.  C#using System.Linq.Dynamic.Core; // Bắt buộc thêm thư viện này
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Web.Models;

namespace DigiPOSE.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly DigiPoseDbContext _context;
        // ... (các hàm hiện có giữ nguyên) ...

        // GET: Products
        public IActionResult Index()
        {
            // Không truyền dữ liệu ở đây nữa, View sẽ tự gọi Ajax
            return View();
        }

        // POST: Products/Index_LoadData
        [HttpPost]
        public IActionResult Index_LoadData()
        {
            try
            {
                // Nhận tham số từ DataTables gửi lên
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                
                int pageSize = length != null ? int.Parse(length) : 0;
                int skip = start != null ? int.Parse(start) : 0;
                int totalRecords = 0;
                int filterRecords = 0;

                // Khởi tạo truy vấn gốc
                var productQuery = _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Unit)
                    .AsQueryable();

                totalRecords = productQuery.Count();

                // Lọc dữ liệu (Searching)
                if (!string.IsNullOrEmpty(searchValue) && !string.IsNullOrWhiteSpace(searchValue))
                {
                    productQuery = productQuery.Where(p => 
                        p.ProductName.Contains(searchValue) ||
                        p.SKU.Contains(searchValue) ||
                        p.Category.CategoryName.Contains(searchValue) ||
                        p.BasePrice.ToString().Contains(searchValue));
                }
                filterRecords = productQuery.Count();

                // Sắp xếp dữ liệu (Sorting)
                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
                {
                    productQuery = productQuery.OrderBy(sortColumn + " " + sortColumnDirection);
                }

                // Phân trang (Pagination) và lấy dữ liệu
                var dataList = productQuery.Skip(skip).Take(pageSize).Select(p => new {
                    ProductId = p.ProductId,
                    SKU = p.SKU,
                    ProductName = p.ProductName,
                    CategoryName = p.Category.CategoryName,
                    UnitName = p.Unit.UnitName,
                    BasePrice = p.BasePrice
                }).ToList();

                // Trả về JSON theo chuẩn của DataTables
                var jsonData = new
                {
                    draw = draw,
                    recordsFiltered = filterRecords,
                    recordsTotal = totalRecords,
                    data = dataList
                };

                return Json(jsonData);
            }
            catch (Exception ex)
            {
                // Có thể tích hợp NLog hoặc Serilog ở đây để log lỗi
                throw new Exception("Lỗi truy xuất dữ liệu: " + ex.Message);
            }
        }
    }
}
2. Cập nhật Views/Products/Index.cshtml để gọi Ajax:
Xóa vòng lặp @foreach cũ, cấu hình thẻ <table> chỉ với <thead> và nhúng mã JavaScript để kích hoạt Server-side.  HTML@{
    ViewData["Title"] = "Hàng hóa";
}

@section Styles {
    <!-- CDN DataTables CSS -->
    <link rel="stylesheet" href="https://cdn.datatables.net/1.13.8/css/dataTables.bootstrap5.min.css" />
    <link rel="stylesheet" href="https://cdn.datatables.net/buttons/2.4.2/css/buttons.bootstrap5.min.css" />
}

<div class="card border-0 shadow-sm">
    <h5 class="card-header bg-success text-white">@ViewData["Title"]</h5>
    <div class="card-body">
        <div class="row mb-3">
            <div class="col-md-6">
                <a asp-action="Create" class="btn btn-success"><i class="fa fa-plus"></i> Thêm hàng hóa</a>
            </div>
            <div class="col-md-6 text-end">
                <div id="export-buttons"></div> <!-- Khu vực render nút Xuất Excel -->
            </div>
        </div>
        
        <table id="datatable" class="table table-sm table-hover table-bordered mb-0 w-100">
            <thead class="table-light">
                <tr>
                    <th width="5%">#</th>
                    <th width="15%">Mã SKU</th>
                    <th>Tên sản phẩm</th>
                    <th width="15%">Danh mục</th>
                    <th width="10%">ĐVT</th>
                    <th width="15%" class="text-end">Giá cơ sở</th>
                    <th width="10%" class="text-center">Thao tác</th>
                </tr>
            </thead>
            <tbody>
                <!-- Dữ liệu sẽ được render bằng Ajax qua DataTables -->
            </tbody>
        </table>
    </div>
</div>

@section Scripts {
    <!-- CDN DataTables JS -->
    <script src="https://cdn.datatables.net/1.13.8/js/jquery.dataTables.min.js"></script>
    <script src="https://cdn.datatables.net/1.13.8/js/dataTables.bootstrap5.min.js"></script>
    <script src="https://cdn.datatables.net/buttons/2.4.2/js/dataTables.buttons.min.js"></script>
    <script src="https://cdn.datatables.net/buttons/2.4.2/js/buttons.bootstrap5.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/jszip/3.10.1/jszip.min.js"></script>
    <script src="https://cdn.datatables.net/buttons/2.4.2/js/buttons.html5.min.js"></script>

    <script>
        $(document).ready(function () {
            var table = $('#datatable').DataTable({
                language: {
                    processing: 'Đang xử lý...',
                    lengthMenu: 'Hiển thị _MENU_ dòng',
                    zeroRecords: 'Không tìm thấy dòng nào phù hợp',
                    info: 'Đang xem _START_ đến _END_ trong tổng số _TOTAL_ dòng',
                    infoEmpty: 'Đang xem 0 đến 0 trong tổng số 0 dòng',
                    emptyTable: 'Không có dữ liệu',
                    infoFiltered: '(được lọc từ _MAX_ dòng)',
                    search: 'Tìm kiếm:',
                    paginate: {
                        first: 'Đầu',
                        last: 'Cuối',
                        next: 'Sau',
                        previous: 'Trước'
                    }
                },
                buttons: [
                    {
                        extend: 'excelHtml5',
                        text: 'Xuất Excel',
                        className: 'btn btn-info btn-sm',
                        title: 'DanhSachHangHoa_DigiPOSE'
                    }
                ],
                processing: true,
                serverSide: true, // Kích hoạt Server-side
                filter: true, // Bật thanh Tìm kiếm
                orderMulti: false,
                ajax: {
                    url: '@Url.Action("Index_LoadData", "Products")',
                    type: 'POST',
                    datatype: 'json'
                },
                columnDefs: [
                    { targets: [0, 6], sortable: false }, // Không sort cột STT và Thao tác
                    { targets: [1, 6], className: 'text-center' },
                    { targets: [5], className: 'text-end text-danger fw-bold' }
                ],
                columns: [
                    { 
                        data: null, 
                        render: function (data, type, row, meta) {
                            return meta.row + meta.settings._iDisplayStart + 1; // Tạo số STT ảo
                        }
                    },
                    { data: 'SKU', name: 'SKU' },
                    { data: 'ProductName', name: 'ProductName' },
                    { data: 'CategoryName', name: 'CategoryName' },
                    { data: 'UnitName', name: 'UnitName' },
                    { 
                        data: 'BasePrice', 
                        name: 'BasePrice',
                        render: $.fn.dataTable.render.number(',', '.', 0, '') // Format VNĐ
                    },
                    { 
                        data: null,
                        render: function (data, type, row, meta) {
                            var editUrl = '/Products/Edit/' + row.ProductId;
                            return '<a href="' + editUrl + '" class="btn btn-sm btn-outline-primary">Sửa</a>';
                        }
                    }
                ],
                initComplete: function () {
                    // Dời nút Export lên góc trên
                    table.buttons().container().appendTo('#export-buttons');
                    $('#datatable').wrap('<div class="table-responsive"></div>');
                }
            });
        });
    </script>
}
Bước 4: Tích hợp CKEditor (Mở rộng cho các ghi chú dài)Mặc dù bảng Product của PoS thường không cần mô tả rườm rà, Sếp có thể tạo thêm trường Description hoặc áp dụng chức năng này cho bảng StockVoucher (Chứng từ kho) để lưu lại lý do/biên bản kiểm kê nếu cần.  Cách áp dụng vào file View (ví dụ: Create/Edit View):  Tải bộ nguồn CKEditor5 và bỏ vào thư mục wwwroot/lib/ckeditor5/.  Bổ sung script vào khối @section Scripts ở cuối file View cần áp dụng:  HTML@section Scripts {
    @{ await Html.RenderPartialAsync("_ValidationScriptsPartial"); }
    
    <!-- Nhúng script thư viện CKEditor -->
    <script src="~/lib/ckeditor5/ckeditor.js"></script>
    <script>
        // Target vào ID của thẻ textarea muốn biến thành CKEditor
        ClassicEditor.create(document.querySelector('#Mota_Hoac_GhiChu'), {
            licenseKey: ''
        }).then(editor => {
            window.editor = editor;
        }).catch(error => {
            console.error(error);
        });
    </script>
}