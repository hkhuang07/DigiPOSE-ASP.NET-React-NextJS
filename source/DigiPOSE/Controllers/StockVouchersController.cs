using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Models;

namespace DigiPOSE.Controllers
{
    public class StockVouchersController : Controller
    {
        private readonly DigiPoseDbContext _context;
        public StockVouchersController(DigiPoseDbContext context) { _context = context; }

        public async Task<IActionResult> Index()
            => View(await _context.StockVouchers
                .Include(v => v.Branch).Include(v => v.User).Include(v => v.Supplier)
                .OrderByDescending(v => v.CreatedAt).ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.StockVouchers.Include(v => v.Branch).Include(v => v.User).Include(v => v.Supplier)
                .FirstOrDefaultAsync(m => m.VoucherId == id);
            if (item == null) return NotFound();
            return PartialView("_DetailsPartial", item);
        }

        public IActionResult Create() { LoadViewBags(); return PartialView("_CreateOrEditPartial", new StockVoucher { CreatedAt = DateTime.Now }); }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StockVoucher model)
        {
            if (ModelState.IsValid) { _context.Add(model); await _context.SaveChangesAsync(); return Json(new { success = true, message = "Stock voucher created." }); }
            LoadViewBags(model.BranchId, model.UserId, model.SupplierId);
            return PartialView("_CreateOrEditPartial", model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.StockVouchers.FindAsync(id);
            if (item == null) return NotFound();
            LoadViewBags(item.BranchId, item.UserId, item.SupplierId);
            return PartialView("_CreateOrEditPartial", item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StockVoucher model)
        {
            if (id != model.VoucherId) return Json(new { success = false, message = "ID mismatch." });
            if (ModelState.IsValid)
            {
                try { _context.Update(model); await _context.SaveChangesAsync(); return Json(new { success = true, message = "Stock voucher updated." }); }
                catch (DbUpdateConcurrencyException) { }
            }
            LoadViewBags(model.BranchId, model.UserId, model.SupplierId);
            return PartialView("_CreateOrEditPartial", model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.StockVouchers.Include(v => v.Branch).Include(v => v.User).Include(v => v.Supplier)
                .FirstOrDefaultAsync(m => m.VoucherId == id);
            if (item == null) return NotFound();
            return PartialView("_DeletePartial", item);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.StockVouchers.FindAsync(id);
            if (item != null) { _context.StockVouchers.Remove(item); await _context.SaveChangesAsync(); }
            return Json(new { success = true, message = "Stock voucher deleted." });
        }

        private void LoadViewBags(int? branchId = null, int? userId = null, int? supplierId = null)
        {
            ViewBag.BranchId = new SelectList(_context.Branches.Where(b => b.IsActive), "BranchId", "BranchName", branchId);
            ViewBag.UserId = new SelectList(_context.Users.Where(u => u.IsActive), "UserId", "UserName", userId);
            ViewBag.SupplierId = new SelectList(_context.Suppliers.Where(s => s.IsActive), "SupplierId", "SupplierName", supplierId);
        }
    }
}
