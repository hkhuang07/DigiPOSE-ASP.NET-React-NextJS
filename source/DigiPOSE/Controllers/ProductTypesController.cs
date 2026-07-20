using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Models;

namespace DigiPOSE.Controllers
{
    public class ProductTypesController : Controller
    {
        private readonly DigiPoseDbContext _context;
        public ProductTypesController(DigiPoseDbContext context) { _context = context; }

        public async Task<IActionResult> Index()
            => View(await _context.ProductTypes.ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.ProductTypes.FirstOrDefaultAsync(m => m.ProductTypeId == id);
            if (item == null) return NotFound();
            return PartialView("_DetailsPartial", item);
        }

        public IActionResult Create()
            => PartialView("_CreateOrEditPartial", new ProductType());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductType model)
        {
            if (!ModelState.IsValid) return PartialView("_CreateOrEditPartial", model);
            _context.Add(model);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Product type created successfully." });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.ProductTypes.FindAsync(id);
            if (item == null) return NotFound();
            return PartialView("_CreateOrEditPartial", item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductType model)
        {
            if (id != model.ProductTypeId) return Json(new { success = false, message = "ID mismatch." });
            if (!ModelState.IsValid) return PartialView("_CreateOrEditPartial", model);
            try { _context.Update(model); await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) { return Json(new { success = false, message = "Concurrency error." }); }
            return Json(new { success = true, message = "Product type updated successfully." });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.ProductTypes.FirstOrDefaultAsync(m => m.ProductTypeId == id);
            if (item == null) return NotFound();
            return PartialView("_DeletePartial", item);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.ProductTypes.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Record not found." });
            _context.ProductTypes.Remove(item);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Product type permanently deleted." });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var item = await _context.ProductTypes.FindAsync(id);
            if (item == null) return Json(new { success = false });
            item.IsActive = !item.IsActive;
            await _context.SaveChangesAsync();
            return Json(new { success = true, isActive = item.IsActive, message = item.IsActive ? "Activated." : "Deactivated." });
        }
    }
}
