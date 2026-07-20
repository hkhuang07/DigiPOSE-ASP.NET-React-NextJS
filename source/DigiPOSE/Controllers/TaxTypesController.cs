using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Models;

namespace DigiPOSE.Controllers
{
    public class TaxTypesController : Controller
    {
        private readonly DigiPoseDbContext _context;
        public TaxTypesController(DigiPoseDbContext context) { _context = context; }

        public async Task<IActionResult> Index()
            => View(await _context.TaxTypes.ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.TaxTypes.FirstOrDefaultAsync(m => m.TaxTypeId == id);
            if (item == null) return NotFound();
            return PartialView("_DetailsPartial", item);
        }

        public IActionResult Create()
            => PartialView("_CreateOrEditPartial", new TaxType());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaxType model)
        {
            if (!ModelState.IsValid) return PartialView("_CreateOrEditPartial", model);
            _context.Add(model);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Tax type created successfully." });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.TaxTypes.FindAsync(id);
            if (item == null) return NotFound();
            return PartialView("_CreateOrEditPartial", item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaxType model)
        {
            if (id != model.TaxTypeId) return Json(new { success = false, message = "ID mismatch." });
            if (!ModelState.IsValid) return PartialView("_CreateOrEditPartial", model);
            try { _context.Update(model); await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) { return Json(new { success = false, message = "Concurrency error." }); }
            return Json(new { success = true, message = "Tax type updated successfully." });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.TaxTypes.FirstOrDefaultAsync(m => m.TaxTypeId == id);
            if (item == null) return NotFound();
            return PartialView("_DeletePartial", item);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.TaxTypes.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Record not found." });
            _context.TaxTypes.Remove(item);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Tax type permanently deleted." });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var item = await _context.TaxTypes.FindAsync(id);
            if (item == null) return Json(new { success = false });
            item.IsActive = !item.IsActive;
            await _context.SaveChangesAsync();
            return Json(new { success = true, isActive = item.IsActive, message = item.IsActive ? "Activated." : "Deactivated." });
        }
    }
}
