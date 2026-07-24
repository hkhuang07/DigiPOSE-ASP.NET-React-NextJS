using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Models;
using Microsoft.AspNetCore.Authorization;

namespace DigiPOSE.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class POSController : ControllerBase
    {
        private readonly DigiPoseDbContext _context;
        public POSController(DigiPoseDbContext context)
        {
            _context = context;
        }

        [HttpPost("retail-draft/create")]
        public async Task<IActionResult> CreateDraftOrder([FromBody] CreateDraftRequest request)
        {
            var order = new Order{
                BranchId = request.BranchId,
                ShiftId = request.ShiftId,
                UserId = request.UserId, //Cashier 
                StatusId = 4, //OrderStatusId = 4 , OrderStatusName = Draft
                CreateAt = DataTime.now
            };
            
            _context.Order.Add(order);
            await _context.SaveChangesAsync();
            return Ok(new{OrderId = order.OrderId, Status = "Draft Created"});
        }

        [HttpPost("retail-draft/add-item")]
        public async Task<IActionResult> AddOrIncrementItem([FromBody] AddItemRequest request)
        {
            // Logic: Kiểm tra xem OrderId đã có ProductId này chưa.
            // Nếu có -> Cộng dồn Quantity.
            // Nếu chưa -> Insert mới vào bảng OrderDetails.
            // Sau đó tính toán lại TotalAmount của bảng Order.
            return Ok(new { Message = "Sẽ được tự động hóa bằng AI" });
        }

        [HttpPost("retail-draft/remove-item")]
        public async Task<IActionResult> RemoveItem([FromBody] RemoveItemRequest request)
        {
            return Ok(new { Message = "Sẽ được tự động hóa bằng AI" });
        }


        [HttpPost("checkout/paid")]
        public async Task<IActionResult> CheckoutPaid([FromBody] CheckoutRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od=>od.Product)
                    .ThenInclude(i=>i.ItemNature)
                    .ThenInclude...............
                    .FirstOrDefaultAsync(o => o.OrderId == request.OrderId && o.StatusId == 4);
                
                if(order == null || order.StatusId != 4)
                {
                    return BadRequest(new {Error = "Draft order not found or already invoiced"});
                }

                order.StatusID = 1 //Completed
                order.PaymentMethodId = request.PaymentMethodId;
                order.CustomerId = request.CustomerId;
                order. .... = request.....          

                var shift = await _context.Shifts.FirstOrDefaultAsync(s=>s.ShiftId == order.ShiftId);
                if(shift != null)
                {
                    shift.TotalSales += order.TotalAmount;
                    _context.Shifts.Update(shift);
                }
                shift!.EndCash += order.TotalAmount;

                foreach(var detail in order.OrderDetails!)
                {
                    var product = detail.Product;
                    if (product.ItemNatureId == 1) //physical
                    {
                        //giảm số lượng product inventory
                    }
                    elst if (product.ItemNatureId == 2 && request.CustomerId != null) //digital
                    {
                        //chuẩn production cộng dồn gia hạn
                        var existingSub = await _context.Subscriptions.FirstOrDefaultAsync(
                            s => s.CustomerId == request.CustomerId && 
                            s.ProductId == product.ProductId
                            );
                        int durationDays = 365;
                        if (existingSub != null && existingSub.EndDate > DateTime.Now)
                        {
                            existingSub.EndDate = existingSub.EndDate.AddDays(durationDays);
                            existingSub.UpdateAt = DateTime.Now;
                            existingSub.OrderId = order.OrderId;   
                            ... = ...                         
                        }
                        else{
                            var newSub = new Subscription{
                                CustomerId = request.CustomerId.Value,
                                ProductId = product.ProductId,
                                OrderId = order.OrderId,
                                StartDate = DateTime.Now,
                                EndDate = DateTime.Now.AddDays(365),
                                Status = "ACTIVE"
                                LicenseKey = Guid.NewGuid().ToString().ToUpper();
                                ... = ...
                            };
                            _context.Subscriptions.Add(newSub);
                        }
                    }
                }
                    // 5. Lưu xuống DB và Commit
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // 6. Gửi Hóa Đơn Điện Tử (Email) bất đồng bộ
                    // await _mailService.SendReceiptEmailAsync(order, customer.Email);
                    return Ok(new {Message = "Checkout successfull", OrderId = order.OrderId});
            }
            catch
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new {Error = "Checkout failed"});
            }
        }
    }
}

