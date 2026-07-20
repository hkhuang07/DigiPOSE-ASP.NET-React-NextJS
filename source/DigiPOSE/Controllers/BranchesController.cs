using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Models;
using DigiPOSE.Web.Helpers;

namespace DigiPOSE.Controllers
{
    public class BranchesController : Controller
    {
        private readonly DigiPoseDbContext _context;
        public BranchesController(DigiPoseDbContext context) { _context = context; }

        public async Task<IActionResult> Index()
        {
            var data = await _context.Branches.Where(x => x.IsActive).ToListAsync();
            return View(data);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.Branches.FirstOrDefaultAsync(m => m.BranchId == id);
            if (item == null) return NotFound();
            return PartialView("_DetailsPartial", item);
        }

        public IActionResult Create()
        {
            return PartialView("_CreateOrEditPartial", new Branch());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Branch model)
        {
            if (string.IsNullOrWhiteSpace(model.Slug) && !string.IsNullOrWhiteSpace(model.BranchName))
                model.Slug = SlugHelper.GenerateSlug(model.BranchName);
            ModelState.Remove("Slug");
            model.IsActive = true;

            if (ModelState.IsValid)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Branch created successfully." });
            }
            return PartialView("_CreateOrEditPartial", model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.Branches.FindAsync(id);
            if (item == null) return NotFound();
            return PartialView("_CreateOrEditPartial", item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Branch model)
        {
            if (id != model.BranchId) return Json(new { success = false, message = "ID mismatch." });
            if (string.IsNullOrWhiteSpace(model.Slug) && !string.IsNullOrWhiteSpace(model.BranchName))
                model.Slug = SlugHelper.GenerateSlug(model.BranchName);
            ModelState.Remove("Slug");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Branch updated successfully." });
                }
                catch (DbUpdateConcurrencyException) { }
            }
            return PartialView("_CreateOrEditPartial", model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.Branches.FirstOrDefaultAsync(m => m.BranchId == id);
            if (item == null) return NotFound();
            return PartialView("_DeletePartial", item);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Branches.FindAsync(id);
            if (item != null)
            {
                item.IsActive = false;
                _context.Update(item);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Branch deactivated successfully." });
            }
            return Json(new { success = false, message = "Branch not found." });
        }
    }
}
