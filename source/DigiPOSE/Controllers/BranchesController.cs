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
            => View(await _context.Branches.ToListAsync());

        public async Task<IActionResult> Details(int? id) //Lấy thông tin chi tiết 1 sản phẩm
        {
            if (id == null) 
                return NotFound();
            var item = await _context.Branches.FirstOrDefaultAsync(m => m.BranchId == id);
            if (item == null) 
                return NotFound();
            return PartialView("_DetailsPartial", item);
        }

        public IActionResult Create()
            => PartialView("_CreateOrEditPartial", new Branch());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Branch model)
        {
            if (string.IsNullOrWhiteSpace(model.Slug))
                model.Slug = SlugHelper.GenerateSlug(model.BranchName);
            ModelState.Remove("Slug");

            if (!ModelState.IsValid)
                return PartialView("_CreateOrEditPartial", model);

            _context.Add(model);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Branch created successfully." });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.Branches.FindAsync(id);
            if (item == null) return NotFound();
            return PartialView("_CreateOrEditPartial", item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Branch model)
        {
            if (id != model.BranchId) return Json(new { success = false, message = "ID mismatch." });

            if (string.IsNullOrWhiteSpace(model.Slug))
                model.Slug = SlugHelper.GenerateSlug(model.BranchName);
            ModelState.Remove("Slug");

            if (!ModelState.IsValid)
                return PartialView("_CreateOrEditPartial", model);

            try
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Branch updated successfully." });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Json(new { success = false, message = "Concurrency error." });
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.Branches.FirstOrDefaultAsync(m => m.BranchId == id);
            if (item == null) return NotFound();
            return PartialView("_DeletePartial", item);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Branches.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Record not found." });
            _context.Branches.Remove(item);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Branch permanently deleted." });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var item = await _context.Branches.FindAsync(id);
            if (item == null) return Json(new { success = false });
            item.IsActive = !item.IsActive;
            await _context.SaveChangesAsync();
            return Json(new { success = true, isActive = item.IsActive, message = item.IsActive ? "Activated." : "Deactivated." });
        }
    }
}
