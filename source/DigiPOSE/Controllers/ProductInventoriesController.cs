using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Models;

namespace DigiPOSE.Controllers
{
    public class ProductInventoriesController : Controller
    {
        private readonly DigiPoseDbContext _context;
        public ProductInventoriesController(DigiPoseDbContext context) { _context = context; }

        public async Task<IActionResult> Index()
            => View(await _context.ProductInventories.Include(p => p.Branch).Include(p => p.Product).ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.ProductInventories.Include(p => p.Branch).Include(p => p.Product).FirstOrDefaultAsync(m => m.InventoryId == id);
            if (item == null) return NotFound();
            return PartialView("_DetailsPartial", item);
        }

        public IActionResult Create()
        {
            ViewBag.BranchId = new SelectList(_context.Branches.Where(b => b.IsActive), "BranchId", "BranchName");
            ViewBag.ProductId = new SelectList(_context.Products.Where(p => p.IsActive), "ProductId", "ProductName");
            return PartialView("_CreateOrEditPartial", new ProductInventory());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductInventory model)
        {
            if (ModelState.IsValid) { _context.Add(model); await _context.SaveChangesAsync(); return Json(new { success = true, message = "Inventory record created." }); }
            ViewBag.BranchId = new SelectList(_context.Branches.Where(b => b.IsActive), "BranchId", "BranchName", model.BranchId);
            ViewBag.ProductId = new SelectList(_context.Products.Where(p => p.IsActive), "ProductId", "ProductName", model.ProductId);
            return PartialView("_CreateOrEditPartial", model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.ProductInventories.FindAsync(id);
            if (item == null) return NotFound();
            ViewBag.BranchId = new SelectList(_context.Branches.Where(b => b.IsActive), "BranchId", "BranchName", item.BranchId);
            ViewBag.ProductId = new SelectList(_context.Products.Where(p => p.IsActive), "ProductId", "ProductName", item.ProductId);
            return PartialView("_CreateOrEditPartial", item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductInventory model)
        {
            if (id != model.InventoryId) return Json(new { success = false, message = "ID mismatch." });
            if (ModelState.IsValid)
            {
                try { _context.Update(model); await _context.SaveChangesAsync(); return Json(new { success = true, message = "Inventory updated." }); }
                catch (DbUpdateConcurrencyException) { }
            }
            ViewBag.BranchId = new SelectList(_context.Branches.Where(b => b.IsActive), "BranchId", "BranchName", model.BranchId);
            ViewBag.ProductId = new SelectList(_context.Products.Where(p => p.IsActive), "ProductId", "ProductName", model.ProductId);
            return PartialView("_CreateOrEditPartial", model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.ProductInventories.Include(p => p.Branch).Include(p => p.Product).FirstOrDefaultAsync(m => m.InventoryId == id);
            if (item == null) return NotFound();
            return PartialView("_DeletePartial", item);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.ProductInventories.FindAsync(id);
            if (item != null) { _context.ProductInventories.Remove(item); await _context.SaveChangesAsync(); }
            return Json(new { success = true, message = "Inventory record deleted." });
        }
    }
}
