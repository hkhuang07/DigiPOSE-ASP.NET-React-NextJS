using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Models;

namespace DigiPOSE.Controllers
{
    public class StockVoucherDetailsController : Controller
    {
        private readonly DigiPoseDbContext _context;
        public StockVoucherDetailsController(DigiPoseDbContext context) { _context = context; }

        public async Task<IActionResult> Index()
            => View(await _context.StockVoucherDetails.Include(d => d.StockVoucher).Include(d => d.Product).ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.StockVoucherDetails.Include(d => d.StockVoucher).Include(d => d.Product).FirstOrDefaultAsync(m => m.VoucherDetailId == id);
            if (item == null) return NotFound();
            return PartialView("_DetailsPartial", item);
        }

        public IActionResult Create()
        {
            ViewBag.VoucherId = new SelectList(_context.StockVouchers.OrderByDescending(v => v.CreatedAt), "VoucherId", "VoucherType");
            ViewBag.ProductId = new SelectList(_context.Products.Where(p => p.IsActive), "ProductId", "ProductName");
            return PartialView("_CreateOrEditPartial", new StockVoucherDetail());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StockVoucherDetail model)
        {
            if (ModelState.IsValid) { _context.Add(model); await _context.SaveChangesAsync(); return Json(new { success = true, message = "Line item created." }); }
            ViewBag.VoucherId = new SelectList(_context.StockVouchers, "VoucherId", "VoucherType", model.VoucherId);
            ViewBag.ProductId = new SelectList(_context.Products.Where(p => p.IsActive), "ProductId", "ProductName", model.ProductId);
            return PartialView("_CreateOrEditPartial", model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.StockVoucherDetails.FindAsync(id);
            if (item == null) return NotFound();
            ViewBag.VoucherId = new SelectList(_context.StockVouchers, "VoucherId", "VoucherType", item.VoucherId);
            ViewBag.ProductId = new SelectList(_context.Products.Where(p => p.IsActive), "ProductId", "ProductName", item.ProductId);
            return PartialView("_CreateOrEditPartial", item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StockVoucherDetail model)
        {
            if (id != model.VoucherDetailId) return Json(new { success = false, message = "ID mismatch." });
            if (ModelState.IsValid)
            {
                try { _context.Update(model); await _context.SaveChangesAsync(); return Json(new { success = true, message = "Line item updated." }); }
                catch (DbUpdateConcurrencyException) { }
            }
            ViewBag.VoucherId = new SelectList(_context.StockVouchers, "VoucherId", "VoucherType", model.VoucherId);
            ViewBag.ProductId = new SelectList(_context.Products.Where(p => p.IsActive), "ProductId", "ProductName", model.ProductId);
            return PartialView("_CreateOrEditPartial", model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.StockVoucherDetails.Include(d => d.StockVoucher).Include(d => d.Product).FirstOrDefaultAsync(m => m.VoucherDetailId == id);
            if (item == null) return NotFound();
            return PartialView("_DeletePartial", item);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.StockVoucherDetails.FindAsync(id);
            if (item != null) { _context.StockVoucherDetails.Remove(item); await _context.SaveChangesAsync(); }
            return Json(new { success = true, message = "Line item deleted." });
        }
    }
}
