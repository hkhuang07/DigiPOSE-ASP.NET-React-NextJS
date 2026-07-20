BUỔI 7: XÂY DỰNG PHÂN HỆ QUẢN TRỊ KHÁCH HÀNG (CRM) VÀ API LỊCH SỬ GIAO DỊCH (DIGIPOSE)
Bước 1: Xây dựng Quản lý Khách hàng trong Admin CMS
Tính năng này giúp Cửa hàng trưởng quản lý tệp khách hàng, điều chỉnh phân loại và kiểm soát điểm thưởng.  
MD

1. CustomersController trong Areas/Admin/Controllers/:

  
MD

Áp dụng cơ chế Xóa mềm và Eager Loading chuẩn mực.

C#
[Area("Admin")]
[Authorize(Roles = "Admin, Branch Manager")]
public class CustomersController : Controller
{
    private readonly DigiPoseDbContext _context;
    public CustomersController(DigiPoseDbContext context) => _context = context;

    // GET: Admin/Customers
    public async Task<IActionResult> Index()
    {
        // Chỉ lấy khách hàng đang active
        var customers = _context.Customers
            .Include(c => c.CustomerType)
            .Where(c => c.IsActive);
        return View(await customers.ToListAsync());
    }

    // POST: Admin/Customers/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("CustomerId,CustomerTypeId,FullName,Phone,RewardPoints,IsActive")] Customer customer)
    {
        if (id != customer.CustomerId) return NotFound();
        if (ModelState.IsValid)
        {
            _context.Update(customer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["CustomerTypeId"] = new SelectList(_context.CustomerTypes, "CustomerTypeId", "TypeName", customer.CustomerTypeId);
        return View(customer);
    }
}
Bước 2: Xây dựng RESTful API Tra cứu Lịch sử Khách hàng cho máy POS
Khi nhân viên thu ngân nhập số điện thoại, hệ thống sẽ lấy lịch sử các hóa đơn gần nhất (dựa trên các trạng thái đơn hàng động từ bảng OrderStatus).  
MD

1. DTO (Models/DTOs/CustomerHistoryDto.cs):

  
MD

C#
namespace DigiPOSE.Web.Models.DTOs
{
    public class CustomerHistoryDto
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; } = null!;
        public string? Phone { get; set; }
        public int RewardPoints { get; set; }
        public string CustomerTypeName { get; set; } = null!;
        public List<OrderHistoryDto> RecentOrders { get; set; } = new();
    }

    public class OrderHistoryDto
    {
        public int OrderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal FinalTotal { get; set; }
        public string StatusName { get; set; } = null!; // Lấy tên từ bảng OrderStatus
        public int TotalItems { get; set; } 
    }
}
2. CustomerApiController (Controllers/Api/CustomerApiController.cs):  
MD

C#
[Route("api/v1/customers")]
[ApiController]
public class CustomerApiController : ControllerBase
{
    private readonly DigiPoseDbContext _context;
    public CustomerApiController(DigiPoseDbContext context) => _context = context;

    [HttpGet("search/{phone}")]
    public async Task<IActionResult> SearchCustomerByPhone(string phone)
    {
        var customer = await _context.Customers
            .Include(c => c.CustomerType)
            .Include(c => c.Orders!.OrderByDescending(o => o.CreatedAt).Take(5))
                .ThenInclude(o => o.OrderStatus) // Eager load bảng danh mục trạng thái
            .Include(c => c.Orders)
                .ThenInclude(o => o.OrderDetails)
            .FirstOrDefaultAsync(c => c.Phone == phone && c.IsActive);

        if (customer == null) return NotFound(new { Message = "Không tìm thấy khách hàng." });

        var result = new CustomerHistoryDto
        {
            CustomerId = customer.CustomerId,
            FullName = customer.FullName,
            Phone = customer.Phone,
            RewardPoints = customer.RewardPoints,
            CustomerTypeName = customer.CustomerType?.TypeName ?? "Khách lẻ",
            RecentOrders = customer.Orders!.Select(o => new OrderHistoryDto
            {
                OrderId = o.OrderId,
                CreatedAt = o.CreatedAt,
                FinalTotal = o.FinalTotal,
                StatusName = o.OrderStatus!.StatusName, // Lấy tên trạng thái động
                TotalItems = o.OrderDetails!.Sum(d => d.Quantity)
            }).ToList()
        };
        return Ok(result);
    }
}
Bước 3: API Tra cứu Hàng hóa nhanh (Barcode Scanner API)
Sử dụng chỉ mục SKU (Index) để truy vấn với độ phức tạp O(1).  
MD

Thêm vào Controllers/Api/PosController.cs:  
MD

C#
[HttpGet("scan-barcode/{sku}")]
public async Task<IActionResult> ScanBarcode(string sku)
{
    var product = await _context.Products
        .Include(p => p.Unit)
        .Include(p => p.Manufacturer) // Bổ sung thông tin hãng sản xuất
        .AsNoTracking()
        .FirstOrDefaultAsync(p => p.SKU == sku && p.IsActive); // Kiểm tra hàng còn kinh doanh

    if (product == null) return NotFound(new { Message = "Không tìm thấy sản phẩm." });

    return Ok(new
    {
        product.ProductId,
        product.SKU,
        product.ProductName,
        product.BasePrice,
        UnitName = product.Unit?.UnitName,
        ManufacturerName = product.Manufacturer?.ManufacturerName // Trả về thông tin hãng
    });
}
  
MD
+ 3
Lưu ý: Sếp đã hoàn thành việc đồng bộ hóa dữ liệu danh mục động cho phân hệ CRM và POS. Các API hiện tại đã hoàn toàn tách biệt khỏi logic cứng (hard-code), sẵn sàng để vận hành với quy mô chuỗi siêu thị thực tế.



# OLD VERSION

BUỔI 7: XÂY DỰNG PHÂN HỆ QUẢN TRỊ KHÁCH HÀNG (CRM) VÀ API LỊCH SỬ GIAO DỊCH (DIGIPOSE)Bước 1: Xây dựng Quản lý Khách hàng trong Admin CMS(Thay thế cho luồng Đăng ký / Cập nhật hồ sơ từ trang web B2C)  Tính năng này giúp Cửa hàng trưởng (Branch Manager) quản lý tệp khách hàng, điều chỉnh phân loại (VIP, Retail) và kiểm soát điểm thưởng.1. Tạo CustomersController trong Areas/Admin/Controllers/(Sử dụng Scaffolding tương tự Buổi 2 để sinh code nhanh, sau đó tinh chỉnh lại).C#using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Web.Models;
using Microsoft.AspNetCore.Authorization;

namespace DigiPOSE.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, Branch Manager")]
    public class CustomersController : Controller
    {
        private readonly DigiPoseDbContext _context;

        public CustomersController(DigiPoseDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Customers
        public async Task<IActionResult> Index()
        {
            var customers = _context.Customers.Include(c => c.CustomerType);
            return View(await customers.ToListAsync());
        }

        // GET: Admin/Customers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();
            
            ViewData["CustomerTypeId"] = new SelectList(_context.CustomerTypes, "CustomerTypeId", "TypeName", customer.CustomerTypeId);
            return View(customer);
        }

        // POST: Admin/Customers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CustomerId,CustomerTypeId,FullName,Phone,RewardPoints")] Customer customer)
        {
            if (id != customer.CustomerId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Customers.Any(e => e.CustomerId == customer.CustomerId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CustomerTypeId"] = new SelectList(_context.CustomerTypes, "CustomerTypeId", "TypeName", customer.CustomerTypeId);
            return View(customer);
        }
    }
}
2. Giao diện Danh sách Khách hàng (Areas/Admin/Views/Customers/Index.cshtml)HTML@model IEnumerable<DigiPOSE.Web.Models.Customer>
@{
    ViewData["Title"] = "Quản lý Khách hàng";
}
<div class="card border-0 shadow-sm">
    <h5 class="card-header bg-info text-white">@ViewData["Title"]</h5>
    <div class="card-body">
        <table class="table table-hover table-bordered mb-0">
            <thead class="table-light">
                <tr>
                    <th width="5%">#</th>
                    <th>@Html.DisplayNameFor(model => model.FullName)</th>
                    <th width="15%">Số điện thoại</th>
                    <th width="15%">Phân loại</th>
                    <th width="15%" class="text-end">Điểm thưởng</th>
                    <th width="10%" class="text-center">Thao tác</th>
                </tr>
            </thead>
            <tbody>
                @{ int stt = 1; }
                @foreach (var item in Model) {
                    <tr>
                        <td>@stt</td>
                        <td class="fw-bold">@item.FullName</td>
                        <td>@item.Phone</td>
                        <td><span class="badge bg-secondary">@item.CustomerType?.TypeName</span></td>
                        <td class="text-end text-success fw-bold">@item.RewardPoints</td>
                        <td class="text-center">
                            <a asp-action="Edit" asp-route-id="@item.CustomerId" class="btn btn-sm btn-outline-primary">Cập nhật</a>
                        </td>
                    </tr>
                    stt++;
                }
            </tbody>
        </table>
    </div>
</div>
Bước 2: Xây dựng RESTful API Tra cứu Lịch sử Khách hàng cho máy POS(Thay thế trang "Đơn hàng của tôi" bằng Razor View)  Khi nhân viên thu ngân nhập số điện thoại khách hàng vào phần mềm POS, hệ thống sẽ gọi API này để lấy thông tin định danh và lịch sử các hóa đơn gần nhất.1. Tạo DTO (Data Transfer Object) để trả về JSON an toànTạo file CustomerHistoryDto.cs trong thư mục Models/DTOs/:C#namespace DigiPOSE.Web.Models.DTOs
{
    public class CustomerHistoryDto
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; } = null!;
        public string? Phone { get; set; }
        public int RewardPoints { get; set; }
        public string CustomerTypeName { get; set; } = null!;
        public List<OrderHistoryDto> RecentOrders { get; set; } = new();
    }

    public class OrderHistoryDto
    {
        public int OrderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal FinalTotal { get; set; }
        public string Status { get; set; } = null!;
        public int TotalItems { get; set; } // Tổng số lượng sản phẩm trong đơn
    }
}
2. Tạo CustomerApiController trong thư mục Controllers/Api/C#using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Web.Models;
using DigiPOSE.Web.Models.DTOs;

namespace DigiPOSE.Web.Controllers.Api
{
    [Route("api/v1/customers")]
    [ApiController]
    // [Authorize] // Kích hoạt khi có JWT
    public class CustomerApiController : ControllerBase
    {
        private readonly DigiPoseDbContext _context;

        public CustomerApiController(DigiPoseDbContext context)
        {
            _context = context;
        }

        // Tra cứu khách hàng & Lịch sử bằng Số điện thoại
        [HttpGet("search/{phone}")]
        public async Task<IActionResult> SearchCustomerByPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) 
                return BadRequest(new { Message = "Vui lòng cung cấp số điện thoại." });

            // Truy vấn Customer và Eager Load lịch sử Orders (Giới hạn 5 đơn gần nhất)
            var customer = await _context.Customers
                .Include(c => c.CustomerType)
                .Include(c => c.Orders!.OrderByDescending(o => o.CreatedAt).Take(5))
                    .ThenInclude(o => o.OrderDetails)
                .FirstOrDefaultAsync(c => c.Phone == phone);

            if (customer == null) 
                return NotFound(new { Message = "Không tìm thấy khách hàng với số điện thoại này." });

            // Map dữ liệu Entity sang DTO để loại bỏ vòng lặp tham chiếu và ẩn dữ liệu nhạy cảm
            var result = new CustomerHistoryDto
            {
                CustomerId = customer.CustomerId,
                FullName = customer.FullName,
                Phone = customer.Phone,
                RewardPoints = customer.RewardPoints,
                CustomerTypeName = customer.CustomerType?.TypeName ?? "Khách lẻ",
                RecentOrders = customer.Orders!.Select(o => new OrderHistoryDto
                {
                    OrderId = o.OrderId,
                    CreatedAt = o.CreatedAt,
                    FinalTotal = o.FinalTotal,
                    Status = o.Status,
                    TotalItems = o.OrderDetails!.Sum(d => d.Quantity)
                }).ToList()
            };

            return Ok(result);
        }
    }
}
Bước 3: Xây dựng RESTful API Tra cứu Hàng hóa nhanh (Barcode Scanner API)(Thay thế cho trang hiển thị sản phẩm B2C)  Máy quét mã vạch (Barcode Scanner) tại quầy sẽ bắn mã SKU vào API này để load nhanh thông tin món hàng tính tiền. Đây là API cốt lõi quyết định tốc độ của máy POS.Thêm Endpoint vào Controllers/Api/PosController.cs (Tiếp nối Buổi 6):C#        // Tra cứu sản phẩm bằng SKU (Mã vạch)
        [HttpGet("scan-barcode/{sku}")]
        public async Task<IActionResult> ScanBarcode(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku)) return BadRequest();

            // Truy vấn nhanh O(1) nhờ Index đã thiết lập trên cột SKU
            var product = await _context.Products
                .Include(p => p.Unit)
                .AsNoTracking() // Tối ưu hiệu năng vì chỉ đọc dữ liệu
                .FirstOrDefaultAsync(p => p.SKU == sku);

            if (product == null) 
                return NotFound(new { Message = "Không tìm thấy sản phẩm." });

            // Trả về Payload tối giản nhất để giảm băng thông mạng
            return Ok(new
            {
                ProductId = product.ProductId,
                SKU = product.SKU,
                ProductName = product.ProductName,
                BasePrice = product.BasePrice,
                UnitName = product.Unit?.UnitName
            });
        }
