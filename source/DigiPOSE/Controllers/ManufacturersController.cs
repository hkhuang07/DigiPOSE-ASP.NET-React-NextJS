using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Models;
using DigiPOSE.Web.Helpers;

namespace DigiPOSE.Controllers
{
    public class ManufacturersController : Controller
    {
        private readonly DigiPoseDbContext _context;
        public ManufacturersController(DigiPoseDbContext context) { _context = context; }

        public async Task<IActionResult> Index()
            => View(await _context.Manufacturers.ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.Manufacturers.FirstOrDefaultAsync(m => m.ManufacturerId == id);
            if (item == null) return NotFound();
            return PartialView("_DetailsPartial", item);
        }

        public IActionResult Create()
            => PartialView("_CreateOrEditPartial", new Manufacturer());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Manufacturer model)
        {
            if (string.IsNullOrWhiteSpace(model.Slug))
                model.Slug = SlugHelper.GenerateSlug(model.ManufacturerName);
            ModelState.Remove("Slug");

            if (!ModelState.IsValid)
                return PartialView("_CreateOrEditPartial", model);

            _context.Add(model);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Manufacturer created successfully." });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.Manufacturers.FindAsync(id);
            if (item == null) return NotFound();
            return PartialView("_CreateOrEditPartial", item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Manufacturer model)
        {
            if (id != model.ManufacturerId) return Json(new { success = false, message = "ID mismatch." });

            if (string.IsNullOrWhiteSpace(model.Slug))
                model.Slug = SlugHelper.GenerateSlug(model.ManufacturerName);
            ModelState.Remove("Slug");

            if (!ModelState.IsValid)
                return PartialView("_CreateOrEditPartial", model);

            try
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Manufacturer updated successfully." });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Json(new { success = false, message = "Concurrency error." });
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.Manufacturers.FirstOrDefaultAsync(m => m.ManufacturerId == id);
            if (item == null) return NotFound();
            return PartialView("_DeletePartial", item);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Manufacturers.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Record not found." });
            _context.Manufacturers.Remove(item);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Manufacturer permanently deleted." });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var item = await _context.Manufacturers.FindAsync(id);
            if (item == null) return Json(new { success = false });
            item.IsActive = !item.IsActive;
            await _context.SaveChangesAsync();
            return Json(new { success = true, isActive = item.IsActive, message = item.IsActive ? "Activated." : "Deactivated." });
        }
    }
}
