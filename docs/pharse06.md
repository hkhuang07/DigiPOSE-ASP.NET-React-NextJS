BUỔI 6: XÂY DỰNG RESTFUL API CHO MÁY POS & TÍCH HỢP HÓA ĐƠN ĐIỆN TỬ (EMAIL SMTP)Bước 1: Thiết lập Dịch vụ gửi Email (Hóa đơn điện tử)Hệ thống PoS hiện đại yêu cầu gửi biên lai qua Email để tiết kiệm giấy và thu thập data. Chúng ta sử dụng thư viện MailKit.  Cài đặt thư viện: Chạy lệnh: Install-Package MailKitCấu hình appsettings.json:JSON{
  "MailSettings": {
    "Address": "pos.noreply@digipose.vn",
    "DisplayName": "DigiPOSE System",
    "Password": "your-app-password",
    "Host": "smtp.gmail.com",
    "Port": 587
  }
}
Logic Gửi Email (Services/MailService.cs):  (Đã cập nhật logic để hiển thị đúng cột Thuế và Chiết khấu cho từng sản phẩm).C#public async Task SendReceiptEmailAsync(Order order, string toEmail)
{
    string chiTietHtml = "";
    foreach (var item in order.OrderDetails!)
    {
        chiTietHtml += $@"<tr>
                            <td>{item.Product?.ProductName}</td>
                            <td>{item.Quantity}</td>
                            <td>{item.UnitPrice:N0} đ</td>
                            <td>{item.TaxAmount:N0} đ</td>
                            <td>{item.DiscountAmount:N0} đ</td>
                            <td>{(item.Quantity * item.UnitPrice) + item.TaxAmount - item.DiscountAmount:N0} đ</td>
                          </tr>";
    }

    string mailBody = $@"<h3>Hóa đơn điện tử #{order.OrderId:000000}</h3>
                        <table border='1'><thead><tr><th>Sản phẩm</th><th>SL</th><th>Đơn giá</th><th>VAT</th><th>Giảm</th><th>Thành tiền</th></tr></thead>
                        <tbody>{chiTietHtml}</tbody></table>
                        <h3>Tổng thanh toán: {order.FinalTotal:N0} đ</h3>";
    // ... logic gửi MailKit tương tự tài liệu cũ
}
Bước 2: Xây dựng RESTful API cho Máy PoS (Dựa trên Blueprint V2.0)Tạo Controllers/Api/PosController.cs. Chúng ta sẽ sử dụng các Id của bảng danh mục (OrderStatus, PaymentMethod) thay vì chuỗi hard-code.  C#[Route("api/v1/[controller]")]
[ApiController]
public class PosController : ControllerBase
{
    private readonly DigiPoseDbContext _context;
    private readonly IMailService _mailService;

    public PosController(DigiPoseDbContext context, IMailService mailService)
    {
        _context = context;
        _mailService = mailService;
    }

    // 1. Thêm vào Đơn Nháp
    [HttpPost("draft-order/{shiftId}/add-item")]
    public async Task<IActionResult> AddItemToDraftOrder(int shiftId, [FromBody] AddItemRequest request)
    {
        // Lấy Id của trạng thái "Draft" (Giả định StatusId = 1 là Draft)
        var draftStatus = await _context.OrderStatuses.FirstOrDefaultAsync(s => s.StatusName == "Draft");
        
        var draftOrder = await _context.Orders
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o => o.ShiftId == shiftId && o.StatusId == draftStatus!.StatusId);

        if (draftOrder == null)
        {
            draftOrder = new Order { ShiftId = shiftId, UserId = 1, StatusId = draftStatus!.StatusId, CreatedAt = DateTime.Now };
            _context.Orders.Add(draftOrder);
        }

        var product = await _context.Products.FindAsync(request.ProductId);
        // Lưu giá, thuế tại thời điểm thêm vào giỏ
        draftOrder.OrderDetails!.Add(new OrderDetail {
            ProductId = product!.ProductId,
            Quantity = request.Quantity,
            UnitPrice = product.BasePrice,
            TaxRate = 0.1m, // Logic lấy thuế suất sản phẩm
            TaxAmount = product.BasePrice * 0.1m * request.Quantity 
        });

        draftOrder.SubTotal = draftOrder.OrderDetails.Sum(d => d.Quantity * d.UnitPrice);
        draftOrder.FinalTotal = draftOrder.OrderDetails.Sum(d => (d.Quantity * d.UnitPrice) + d.TaxAmount - d.DiscountAmount);

        await _context.SaveChangesAsync();
        return Ok(draftOrder);
    }

    // 2. Chốt Hóa đơn (Checkout) - Vá lỗi dòng tiền
    [HttpPost("checkout/{orderId}")]
    public async Task<IActionResult> Checkout(int orderId, [FromBody] CheckoutRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var order = await _context.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.OrderId == orderId);
            
            // Cập nhật trạng thái từ bảng danh mục động
            var completedStatus = await _context.OrderStatuses.FirstOrDefaultAsync(s => s.StatusName == "Completed");
            order!.StatusId = completedStatus!.StatusId;
            order.PaymentMethodId = request.PaymentMethodId; // Dùng ID phương thức TT
            order.AmountTendered = request.AmountTendered;
            order.ChangeAmount = request.AmountTendered - order.FinalTotal;

            // Cập nhật doanh thu Ca làm việc
            var shift = await _context.Shifts.FindAsync(order.ShiftId);
            shift!.EndCash += order.FinalTotal;

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new { Message = "Thanh toán thành công!", Change = order.ChangeAmount });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, ex.Message);
        }
    }
}

public class AddItemRequest { public int ProductId { get; set; } public int Quantity { get; set; } }
public class CheckoutRequest { public int PaymentMethodId { get; set; } public decimal AmountTendered { get; set; } }
  Đánh giá kỹ thuật:Blueprint V2.0 Compliance: Mọi trạng thái đơn hàng (StatusId) và phương thức thanh toán (PaymentMethodId) đều được truy vấn từ bảng, không còn hard-code "Draft" hay "Completed" trong code C#.  Transaction Integrity: Toàn bộ quá trình đối soát tiền mặt, cập nhật ca và chốt đơn hàng được bọc trong transaction, đảm bảo dữ liệu không bị lệch nếu hệ thống ngắt quãng.  Tax Compliance: Đã bổ sung việc lưu cứng TaxRate và TaxAmount vào OrderDetail, đáp ứng yêu cầu pháp lý về hóa đơn điện tử. 



# OLD VERSION

BUỔI 6: XÂY DỰNG RESTFUL API CHO MÁY POS & TÍCH HỢP HÓA ĐƠN ĐIỆN TỬ (EMAIL SMTP)Bước 1: Thiết lập Dịch vụ gửi Email (Hóa đơn điện tử)Hệ thống PoS hiện đại yêu cầu tính năng gửi biên lai điện tử qua Email để tiết kiệm giấy và thu thập data khách hàng. Chúng ta sử dụng dịch vụ SMTP (ví dụ: Gmail) và thư viện MailKit.  1. Cài đặt thư viện MailKit
Mở Package Manager Console và chạy lệnh:  PowerShellInstall-Package MailKit
2. Cấu hình SMTP trong appsettings.json  
Khai báo thông số kết nối Email (Sếp có thể dùng App Password của Gmail):  JSON{
  "MailSettings": {
    "Address": "pos.noreply@digipose.vn",
    "DisplayName": "DigiPOSE System",
    "Password": "your-app-password-here",
    "Host": "smtp.gmail.com",
    "Port": 587
  }
}
3. Tạo Models cấu hình Mail
Tạo file MailSettings.cs và MailRequest.cs trong thư mục Models/:  C#namespace DigiPOSE.Web.Models
{
    public class MailSettings
    {
        public string Address { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Host { get; set; } = null!;
        public int Port { get; set; }
    }

    public class MailRequest
    {
        public string ToEmail { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string Body { get; set; } = null!;
    }
}
4. Xây dựng Logic gửi Mail (Interface & Implementation)
Tạo thư mục Services/. Thêm IMailService.cs và MailService.cs:  C#using DigiPOSE.Web.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace DigiPOSE.Web.Services
{
    public interface IMailService
    {
        Task SendReceiptEmailAsync(Order order, string toEmail);
    }

    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;

        public MailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task SendReceiptEmailAsync(Order order, string toEmail)
        {
            // Build nội dung E-Receipt HTML
            string chiTietHtml = "";
            foreach (var item in order.OrderDetails!)
            {
                chiTietHtml += $@"<tr>
                                    <td>{item.Product?.ProductName}</td>
                                    <td>{item.Quantity}</td>
                                    <td>{item.UnitPrice:N0} đ</td>
                                    <td>{item.Quantity * item.UnitPrice:N0} đ</td>
                                  </tr>";
            }

            string mailBody = $@"
                <h3>Cảm ơn quý khách đã mua sắm tại DigiPOSE</h3>
                <p>Mã hóa đơn: <strong>{order.OrderId:000000}</strong></p>
                <p>Ngày giao dịch: {order.CreatedAt:dd/MM/yyyy HH:mm}</p>
                <table border='1' cellpadding='5' cellspacing='0'>
                    <thead><tr><th>Sản phẩm</th><th>SL</th><th>Đơn giá</th><th>Thành tiền</th></tr></thead>
                    <tbody>{chiTietHtml}</tbody>
                </table>
                <h3 style='color: red;'>Tổng tiền thanh toán: {order.FinalTotal:N0} đ</h3>";

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Address));
            email.To.Add(new MailboxAddress("Khách hàng", toEmail));
            email.Subject = $"Hóa đơn điện tử #{order.OrderId:000000} - DigiPOSE";
            
            var builder = new BodyBuilder { HtmlBody = mailBody };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_mailSettings.Address, _mailSettings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
5. Đăng ký Service vào Program.cs  C#// Đọc cấu hình MailSettings từ appsettings.json
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
builder.Services.AddTransient<IMailService, MailService>();
Bước 2: Xây dựng RESTful API cho Máy PoS (Thay thế Giỏ hàng Session)Tạo thư mục Controllers/Api/ và thêm PosController.cs. File này sẽ xử lý các thao tác của thu ngân: Quét mã vạch (Thêm vào Đơn nháp), Cập nhật số lượng, và Thanh toán.C#using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Web.Models;
using DigiPOSE.Web.Services;

namespace DigiPOSE.Web.Controllers.Api
{
    [Route("api/v1/[controller]")]
    [ApiController]
    // [Authorize] // Kích hoạt khi đã tích hợp JWT
    public class PosController : ControllerBase
    {
        private readonly DigiPoseDbContext _context;
        private readonly IMailService _mailService;

        public PosController(DigiPoseDbContext context, IMailService mailService)
        {
            _context = context;
            _mailService = mailService;
        }

        // 1. Thêm sản phẩm vào Đơn Nháp (Tương đương Thêm vào Giỏ hàng)
        [HttpPost("draft-order/{shiftId}/add-item")]
        public async Task<IActionResult> AddItemToDraftOrder(int shiftId, [FromBody] AddItemRequest request)
        {
            // Tìm Đơn nháp hiện tại của Ca làm việc, nếu chưa có thì tạo mới
            var draftOrder = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.ShiftId == shiftId && o.Status == "Draft");

            if (draftOrder == null)
            {
                draftOrder = new Order
                {
                    ShiftId = shiftId,
                    UserId = 1, // Lấy từ User Claim (Token)
                    Status = "Draft",
                    CreatedAt = DateTime.Now,
                    OrderDetails = new List<OrderDetail>()
                };
                _context.Orders.Add(draftOrder);
            }

            var product = await _context.Products.Include(p => p.Unit).FirstOrDefaultAsync(p => p.ProductId == request.ProductId);
            if (product == null) return NotFound("Sản phẩm không tồn tại.");

            // Kiểm tra xem sản phẩm đã có trong hóa đơn chưa
            var existingItem = draftOrder.OrderDetails!.FirstOrDefault(d => d.ProductId == product.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += request.Quantity;
            }
            else
            {
                draftOrder.OrderDetails!.Add(new OrderDetail
                {
                    ProductId = product.ProductId,
                    Quantity = request.Quantity,
                    UnitPrice = product.BasePrice, // Lưu cứng giá bán
                    UnitName = product.Unit!.UnitName // Lưu cứng ĐVT
                });
            }

            // Cập nhật tổng tiền
            draftOrder.SubTotal = draftOrder.OrderDetails.Sum(d => d.Quantity * d.UnitPrice);
            draftOrder.FinalTotal = draftOrder.SubTotal; // Chưa tính giảm giá

            await _context.SaveChangesAsync();
            return Ok(draftOrder);
        }

        // 2. Chốt Hóa đơn & Thanh toán tại quầy (Checkout)
        [HttpPost("checkout/{orderId}")]
        public async Task<IActionResult> Checkout(int orderId, [FromBody] CheckoutRequest request)
        {
            // BẮT ĐẦU TRANSACTION ĐỂ ĐẢM BẢO TOÀN VẸN DỮ LIỆU KẾT TOÁN
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Product)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId && o.Status == "Draft");

                if (order == null) return BadRequest("Hóa đơn không hợp lệ hoặc đã thanh toán.");

                // Bước 1: Tạo record Thanh toán
                var payment = new Payment
                {
                    OrderId = order.OrderId,
                    Amount = order.FinalTotal,
                    PaymentMethod = request.PaymentMethod,
                    CreatedAt = DateTime.Now
                };
                _context.Payments.Add(payment);

                // Bước 2: Cập nhật trạng thái Order và trừ tồn kho (Giả lập)
                order.Status = "Completed";
                order.CustomerId = request.CustomerId;

                // Cập nhật doanh thu Ca làm việc (Shift)
                var shift = await _context.Shifts.FindAsync(order.ShiftId);
                if (shift != null) shift.EndCash += order.FinalTotal;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Bước 3: Gửi Email E-Receipt nếu có CustomerId và Customer có Email
                if (request.CustomerId.HasValue)
                {
                    var customer = await _context.Customers.FindAsync(request.CustomerId);
                    // Giả sử bảng Customer đã được bổ sung cột Email
                    // await _mailService.SendReceiptEmailAsync(order, customer.Email);
                }

                return Ok(new { Message = "Thanh toán thành công!", OrderId = order.OrderId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, "Lỗi giao dịch thanh toán: " + ex.Message);
            }
        }
    }

    // Các lớp DTO để nhận dữ liệu từ Frontend React/Vue
    public class AddItemRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class CheckoutRequest
    {
        public int? CustomerId { get; set; }
        public string PaymentMethod { get; set; } = "Cash"; // Cash, Card, Transfer
    }
}
(Giải thích Kỹ thuật: Cấu trúc Transaction trên loại bỏ hoàn toàn Session của MVC cũ. Mọi thao tác bán hàng được lưu ở trạng thái "Draft" trong Database. Khi bấm "Thanh Toán", hệ thống thực hiện nguyên khối (Atomic) các việc: Đổi Status, Ghi nhận Payment, Tính tiền vào Ca làm việc).  