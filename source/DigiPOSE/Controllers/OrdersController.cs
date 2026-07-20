using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Models;


namespace DigiPOSE.Controllers
{
    public class OrdersController : Controller
    {
        private readonly DigiPoseDbContext _context;
        public OrdersController(DigiPoseDbContext context) { _context = context; }

        public async Task<IActionResult> Index()
        {
            var data = await _context.Orders
                .Include(x => x.Shift)
                .Include(x => x.User)
                .Include(x => x.Customer)
                .Include(x => x.PaymentMethod)
                .ToListAsync();
            return View(data);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.Orders
                .Include(x => x.Shift)
                .Include(x => x.User)
                .Include(x => x.Customer)
                .Include(x => x.PaymentMethod)
                .FirstOrDefaultAsync(m => m.OrderId == id);
            if (item == null) return NotFound();
            return PartialView("_DetailsPartial", item);
        }

        public IActionResult Create()
        {
            LoadViewBags();
            return PartialView("_CreateOrEditPartial", new Order());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Order model)
        {
            model.CreatedAt = DateTime.Now;

            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try {
                _context.Add(model);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync(); 
                } catch { 
                    await transaction.RollbackAsync(); 
                    return Json(new { success = false, message="Transaction Failed" }); 
                }
                return Json(new { success = true, message = "Created successfully." });
            }
            LoadViewBags(model.BranchId, model.ShiftId, model.UserId, model.CustomerId, model.StatusId, model.PaymentMethodId);
            return PartialView("_CreateOrEditPartial", model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.Orders.FindAsync(id);
            if (item == null) return NotFound();
            LoadViewBags(item.BranchId, item.ShiftId, item.UserId, item.CustomerId, item.StatusId, item.PaymentMethodId);
            return PartialView("_CreateOrEditPartial", item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Order model)
        {
            if (id != model.OrderId) return Json(new { success = false, message = "ID mismatch." });

            if (ModelState.IsValid)
            {
                try
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync(); 
                    } catch { 
                        await transaction.RollbackAsync(); 
                        return Json(new { success = false, message="Transaction Failed" }); 
                    }
                    return Json(new { success = true, message = "Updated successfully." });
                }
                catch (DbUpdateConcurrencyException) { }
            }
            LoadViewBags(model.BranchId, model.ShiftId, model.UserId, model.CustomerId, model.StatusId, model.PaymentMethodId);
            return PartialView("_CreateOrEditPartial", model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Orders.FindAsync(id);
            if (item != null)
            {
                _context.Remove(item);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Deleted successfully." });
            }
            return Json(new { success = false, message = "Not found." });
        }

        private void LoadViewBags(int? val_BranchId = null, int? val_ShiftId = null, int? val_UserId = null, int? val_CustomerId = null, int? val_StatusId = null, int? val_PaymentMethodId = null)
        {
            ViewBag.BranchId = new SelectList(_context.Branches, "BranchId", "BranchName", val_BranchId);
            ViewBag.ShiftId = new SelectList(_context.Shifts, "ShiftId", "ShiftId", val_ShiftId);
            ViewBag.UserId = new SelectList(_context.Users, "UserId", "UserName", val_UserId);
            ViewBag.CustomerId = new SelectList(_context.Customers, "CustomerId", "FullName", val_CustomerId);
            ViewBag.StatusId = new SelectList(_context.OrderStatuses, "StatusId", "StatusName", val_StatusId);
            ViewBag.PaymentMethodId = new SelectList(_context.PaymentMethods, "PaymentMethodId", "PaymentMethodName", val_PaymentMethodId);
        }
    }
}
