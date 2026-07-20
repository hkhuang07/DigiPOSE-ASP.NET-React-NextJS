using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Models;

namespace DigiPOSE.Controllers
{
    public class CountersController : Controller
    {
        private readonly DigiPoseDbContext _context;
        public CountersController(DigiPoseDbContext context) { _context = context; }

        public async Task<IActionResult> Index()
            => View(await _context.Counters.Include(c => c.Branch).ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.Counters.Include(c => c.Branch).FirstOrDefaultAsync(m => m.CounterId == id);
            if (item == null) return NotFound();
            return PartialView("_DetailsPartial", item);
        }

        public IActionResult Create()
        {
            ViewBag.BranchId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Branches.Where(b => b.IsActive), "BranchId", "BranchName");
            return PartialView("_CreateOrEditPartial", new Counter());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Counter model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.BranchId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Branches.Where(b => b.IsActive), "BranchId", "BranchName", model.BranchId);
                return PartialView("_CreateOrEditPartial", model);
            }
            _context.Add(model);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Counter created successfully." });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.Counters.FindAsync(id);
            if (item == null) return NotFound();
            ViewBag.BranchId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Branches.Where(b => b.IsActive), "BranchId", "BranchName", item.BranchId);
            return PartialView("_CreateOrEditPartial", item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Counter model)
        {
            if (id != model.CounterId) return Json(new { success = false, message = "ID mismatch." });
            if (!ModelState.IsValid)
            {
                ViewBag.BranchId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Branches.Where(b => b.IsActive), "BranchId", "BranchName", model.BranchId);
                return PartialView("_CreateOrEditPartial", model);
            }
            try { _context.Update(model); await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) { return Json(new { success = false, message = "Concurrency error." }); }
            return Json(new { success = true, message = "Counter updated successfully." });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.Counters.Include(c => c.Branch).FirstOrDefaultAsync(m => m.CounterId == id);
            if (item == null) return NotFound();
            return PartialView("_DeletePartial", item);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Counters.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Record not found." });
            _context.Counters.Remove(item);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Counter permanently deleted." });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var item = await _context.Counters.FindAsync(id);
            if (item == null) return Json(new { success = false });
            item.IsActive = !item.IsActive;
            await _context.SaveChangesAsync();
            return Json(new { success = true, isActive = item.IsActive, message = item.IsActive ? "Activated." : "Deactivated." });
        }
    }
}
