Điểm nhấn kỹ thuật của buổi này là việc ứng dụng IDbContextTransaction (Giao dịch nguyên khối ACID) để đảm bảo khi một phiếu nhập kho được lưu, thông tin chứng từ (StockVouchers), chi tiết hàng hóa (StockVoucherDetails) và số lượng tồn thực tế (ProductInventories) phải được ghi nhận thành công cùng lúc. Nếu có lỗi mạng hoặc lỗi phần cứng giữa chừng, toàn bộ thao tác sẽ bị Rollback để bảo vệ dữ liệu.

BUỔI 11: XÂY DỰNG PHÂN HỆ QUẢN TRỊ KHO BÃI VÀ GIAO DỊCH CHỨNG TỪ ACID (DIGIPOSE)
Bước 1: Quản lý Nhà cung cấp (Suppliers)
Bảng Suppliers là dữ liệu Master Data cơ bản. Sếp sử dụng chức năng Scaffolding của Visual Studio (giống như đã làm ở Buổi 1 và 2) để tự động sinh SuppliersController và các Views tương ứng trong thư mục Areas/Admin.

(Khu vực này chỉ là CRUD chuẩn nên tài liệu sẽ bỏ qua bước code tay để tập trung vào logic cốt lõi).

Bước 2: Chuẩn bị ViewModel cho Cấu trúc Master-Detail
Khi người dùng lập một Chứng từ kho (Phiếu nhập/xuất), họ sẽ nhập thông tin chung (Nhà cung cấp, Loại phiếu) và một danh sách nhiều hàng hóa bên dưới. Do đó, ta cần tạo một ViewModel (DTO) để hứng dữ liệu JSON từ Client gửi lên.

Tạo file VoucherViewModel.cs trong thư mục Areas/Admin/Models/:

C#
using System.ComponentModel.DataAnnotations;

namespace DigiPOSE.Web.Areas.Admin.Models
{
    public class CreateVoucherViewModel
    {
        [Required]
        public int BranchId { get; set; }
        
        public int? SupplierId { get; set; } // Null nếu là phiếu Xuất
        
        [Required]
        public string VoucherType { get; set; } = "Import"; // Import, Export, Return

        [Required]
        public List<VoucherDetailViewModel> Details { get; set; } = new List<VoucherDetailViewModel>();
    }

    public class VoucherDetailViewModel
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal ActualPrice { get; set; }
    }
}
Bước 3: Xử lý Backend (Controller & Transaction) cho Chứng từ
Tạo StockVouchersController.cs trong Areas/Admin/Controllers/. Đây là "bộ não" xử lý tồn kho.

C#
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Web.Models;
using DigiPOSE.Web.Areas.Admin.Models;

namespace DigiPOSE.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, Branch Manager")] // Phân quyền quản lý kho
    public class StockVouchersController : Controller
    {
        private readonly DigiPoseDbContext _context;

        public StockVouchersController(DigiPoseDbContext context)
        {
            _context = context;
        }

        // GET: Admin/StockVouchers/Create
        public IActionResult Create()
        {
            ViewData["BranchId"] = new SelectList(_context.Branches, "BranchId", "BranchName");
            ViewData["SupplierId"] = new SelectList(_context.Suppliers, "SupplierId", "SupplierName");
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ProductName");
            return View();
        }

        // POST (AJAX): Admin/StockVouchers/CreateVoucher
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVoucher([FromBody] CreateVoucherViewModel model)
        {
            if (!ModelState.IsValid || model.Details.Count == 0)
            {
                return BadRequest(new { Message = "Dữ liệu chứng từ không hợp lệ hoặc rỗng." });
            }

            // Lấy ID người lập phiếu từ Token/Cookie
            var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
            int currentUserId = string.IsNullOrEmpty(userIdStr) ? 1 : int.Parse(userIdStr);

            // BẮT ĐẦU GIAO DỊCH NGUYÊN KHỐI (ACID TRANSACTION)
            // Độ phức tạp thao tác: O(N) với N là số lượng dòng chi tiết trong chứng từ
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Tạo bản ghi Master (Chứng từ tổng)
                var voucher = new StockVoucher
                {
                    BranchId = model.BranchId,
                    UserId = currentUserId,
                    SupplierId = model.VoucherType == "Import" ? model.SupplierId : null,
                    VoucherType = model.VoucherType,
                    CreatedAt = DateTime.Now,
                    TotalValue = model.Details.Sum(d => d.Quantity * d.ActualPrice)
                };
                _context.StockVouchers.Add(voucher);
                await _context.SaveChangesAsync(); // Lưu để lấy VoucherId phát sinh

                // Lấy danh sách tồn kho hiện tại của chi nhánh này (để tối ưu I/O)
                var productIds = model.Details.Select(d => d.ProductId).ToList();
                var currentInventories = await _context.ProductInventories
                    .Where(inv => inv.BranchId == model.BranchId && productIds.Contains(inv.ProductId))
                    .ToDictionaryAsync(inv => inv.ProductId);

                // 2. Lưu các dòng chi tiết (Details) và Cập nhật tồn kho
                foreach (var detail in model.Details)
                {
                    // Thêm dòng chi tiết
                    _context.StockVoucherDetails.Add(new StockVoucherDetail
                    {
                        VoucherId = voucher.VoucherId,
                        ProductId = detail.ProductId,
                        Quantity = detail.Quantity,
                        ActualPrice = detail.ActualPrice
                    });

                    // Cập nhật tồn kho (ProductInventory)
                    int qtyModifier = model.VoucherType == "Import" ? detail.Quantity : -detail.Quantity;

                    if (currentInventories.TryGetValue(detail.ProductId, out var inventory))
                    {
                        inventory.StockQuantity += qtyModifier;
                        _context.ProductInventories.Update(inventory);
                    }
                    else
                    {
                        // Nếu chi nhánh chưa từng có sản phẩm này, tạo mới row tồn kho
                        if (model.VoucherType == "Export") 
                            throw new Exception($"Không thể xuất sản phẩm ID {detail.ProductId} vì kho trống.");

                        _context.ProductInventories.Add(new ProductInventory
                        {
                            BranchId = model.BranchId,
                            ProductId = detail.ProductId,
                            StockQuantity = detail.Quantity,
                            MinStockLevel = 5 // Cảnh báo mặc định
                        });
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync(); // Xác nhận chốt giao dịch

                return Ok(new { Message = "Lưu chứng từ và cập nhật tồn kho thành công!", VoucherId = voucher.VoucherId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); // Nếu lỗi bất kỳ ở đâu, đảo ngược toàn bộ dữ liệu
                return StatusCode(500, new { Message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}
Bước 4: Xây dựng Giao diện Master-Detail bằng AJAX & jQuery
Giao diện này cho phép người dùng chọn sản phẩm, nhập số lượng và ấn "Thêm dòng" mà không cần tải lại trang. Khi hoàn tất, một mảng JSON sẽ được bắn lên API.

Tạo file Areas/Admin/Views/StockVouchers/Create.cshtml:

HTML
@{
    ViewData["Title"] = "Lập chứng từ kho";
}

<div class="card border-0 shadow-sm mb-4">
    <h5 class="card-header bg-success text-white">@ViewData["Title"]</h5>
    <div class="card-body">
        
        <!-- Khối thông tin Master (Chứng từ tổng) -->
        <h6 class="text-primary mb-3">1. Thông tin chung</h6>
        <div class="row">
            <div class="col-md-4 mb-3">
                <label class="form-label fw-bold">Chi nhánh nhập/xuất</label>
                <select id="selBranchId" class="form-select" asp-items="ViewBag.BranchId"></select>
            </div>
            <div class="col-md-4 mb-3">
                <label class="form-label fw-bold">Loại chứng từ</label>
                <select id="selVoucherType" class="form-select">
                    <option value="Import">Phiếu Nhập (Import)</option>
                    <option value="Export">Phiếu Xuất/Hủy (Export)</option>
                    <option value="Return">Trả hàng (Return)</option>
                </select>
            </div>
            <div class="col-md-4 mb-3">
                <label class="form-label fw-bold">Nhà cung cấp (Bỏ qua nếu xuất)</label>
                <select id="selSupplierId" class="form-select" asp-items="ViewBag.SupplierId">
                    <option value="">-- Không chọn --</option>
                </select>
            </div>
        </div>

        <hr />

        <!-- Khối nhập liệu Detail (Thêm hàng hóa) -->
        <h6 class="text-primary mb-3 mt-3">2. Chi tiết hàng hóa</h6>
        <div class="row align-items-end bg-light p-3 rounded mb-3">
            <div class="col-md-5">
                <label class="form-label">Sản phẩm</label>
                <select id="selProductId" class="form-select" asp-items="ViewBag.ProductId"></select>
            </div>
            <div class="col-md-2">
                <label class="form-label">Số lượng</label>
                <input type="number" id="txtQuantity" class="form-control" value="1" min="1" />
            </div>
            <div class="col-md-3">
                <label class="form-label">Đơn giá (VNĐ)</label>
                <input type="number" id="txtPrice" class="form-control" value="0" min="0" />
            </div>
            <div class="col-md-2">
                <button type="button" id="btnAddRow" class="btn btn-secondary w-100"><i class="fa fa-plus"></i> Thêm dòng</button>
            </div>
        </div>

        <!-- Bảng hiển thị danh sách Detail chờ lưu -->
        <table class="table table-bordered table-hover" id="tblVoucherDetails">
            <thead class="table-dark">
                <tr>
                    <th width="40%">Tên sản phẩm</th>
                    <th width="15%" class="text-center">Số lượng</th>
                    <th width="20%" class="text-end">Đơn giá</th>
                    <th width="20%" class="text-end">Thành tiền</th>
                    <th width="5%" class="text-center">Xóa</th>
                </tr>
            </thead>
            <tbody>
                <!-- Các dòng dữ liệu sẽ được JavaScript chèn vào đây -->
            </tbody>
            <tfoot>
                <tr>
                    <th colspan="3" class="text-end text-danger fs-5">TỔNG CỘNG:</th>
                    <th class="text-end text-danger fs-5" id="lblTotalValue">0 đ</th>
                    <th></th>
                </tr>
            </tfoot>
        </table>

        <!-- Khối Nút lệnh -->
        <div class="text-end mt-4">
            <!-- Antiforgery Token để bảo mật -->
            @Html.AntiForgeryToken() 
            <button type="button" id="btnSaveVoucher" class="btn btn-success btn-lg"><i class="fa fa-save"></i> LƯU CHỨNG TỪ</button>
        </div>
    </div>
</div>

@section Scripts {
    <script>
        var voucherDetails = []; // Mảng lưu trữ dữ liệu chi tiết

        $(document).ready(function () {
            
            // Xử lý sự kiện Thêm dòng
            $('#btnAddRow').click(function () {
                var productId = $('#selProductId').val();
                var productName = $('#selProductId option:selected').text();
                var qty = parseInt($('#txtQuantity').val());
                var price = parseFloat($('#txtPrice').val());

                if (qty <= 0 || price < 0) {
                    alert("Số lượng và Đơn giá không hợp lệ."); return;
                }

                // Kiểm tra xem sản phẩm đã có trong lưới chưa (nếu có thì gộp số lượng)
                var existingItem = voucherDetails.find(x => x.ProductId == productId);
                if (existingItem) {
                    existingItem.Quantity += qty;
                    existingItem.ActualPrice = price; // Lấy giá mới nhất
                } else {
                    voucherDetails.push({
                        ProductId: parseInt(productId),
                        ProductName: productName,
                        Quantity: qty,
                        ActualPrice: price
                    });
                }
                
                RenderTable();
            });

            // Xử lý sự kiện Xóa dòng
            $(document).on('click', '.btn-remove', function () {
                var idToRemove = $(this).data('id');
                voucherDetails = voucherDetails.filter(x => x.ProductId != idToRemove);
                RenderTable();
            });

            // Hàm vẽ lại bảng dữ liệu
            function RenderTable() {
                var tbody = $('#tblVoucherDetails tbody');
                tbody.empty();
                var total = 0;

                $.each(voucherDetails, function (index, item) {
                    var rowTotal = item.Quantity * item.ActualPrice;
                    total += rowTotal;

                    var tr = `<tr>
                        <td class='fw-bold text-primary'>${item.ProductName}</td>
                        <td class='text-center'>${item.Quantity}</td>
                        <td class='text-end'>${item.ActualPrice.toLocaleString('vi-VN')} đ</td>
                        <td class='text-end'>${rowTotal.toLocaleString('vi-VN')} đ</td>
                        <td class='text-center'><button class='btn btn-sm btn-danger btn-remove' data-id='${item.ProductId}'><i class='fa fa-trash'></i></button></td>
                    </tr>`;
                    tbody.append(tr);
                });

                $('#lblTotalValue').text(total.toLocaleString('vi-VN') + ' đ');
            }

            // Xử lý sự kiện Bấm Lưu chứng từ (Gửi AJAX lên API)
            $('#btnSaveVoucher').click(function () {
                if (voucherDetails.length === 0) {
                    alert("Chưa có sản phẩm nào trong chứng từ!"); return;
                }

                var payload = {
                    BranchId: parseInt($('#selBranchId').val()),
                    SupplierId: $('#selSupplierId').val() ? parseInt($('#selSupplierId').val()) : null,
                    VoucherType: $('#selVoucherType').val(),
                    Details: voucherDetails
                };

                // Vượt qua kiểm tra bảo mật ValidateAntiForgeryToken
                var token = $('input[name="__RequestVerificationToken"]').val();

                $.ajax({
                    url: '@Url.Action("CreateVoucher", "StockVouchers", new { Area = "Admin" })',
                    type: 'POST',
                    contentType: 'application/json',
                    headers: { "RequestVerificationToken": token },
                    data: JSON.stringify(payload),
                    success: function (res) {
                        alert(res.message);
                        window.location.reload(); // Reload lại trang để lập phiếu mới
                    },
                    error: function (err) {
                        var errorMsg = err.responseJSON ? err.responseJSON.message : "Có lỗi xảy ra";
                        alert("Lỗi: " + errorMsg);
                    }
                });
            });
        });
    </script>
}
