using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Models;

namespace DigiPOSE.Controllers
{
    public class ShiftsController : Controller
    {
        private readonly DigiPoseDbContext _context;
        public ShiftsController(DigiPoseDbContext context) { _context = context; }

        public async Task<IActionResult> Index()
            => View(await _context.Shifts
                .Include(s => s.User)
                .Include(s => s.Counter)
                .Include(s => s.Status)
                .OrderByDescending(s => s.StartTime)
                .ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.Shifts.Include(s => s.User).Include(s => s.Counter).Include(s => s.Status).FirstOrDefaultAsync(m => m.ShiftId == id);
            if (item == null) return NotFound();
            return PartialView("_DetailsPartial", item);
        }

        public IActionResult Create()
        {
            LoadViewBags();
            return PartialView("_CreateOrEditPartial", new Shift());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Shift model)
        {
            if (ModelState.IsValid) { _context.Add(model); await _context.SaveChangesAsync(); return Json(new { success = true, message = "Shift created." }); }
            LoadViewBags(model.UserId, model.CounterId, model.StatusId);
            return PartialView("_CreateOrEditPartial", model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.Shifts.FindAsync(id);
            if (item == null) return NotFound();
            LoadViewBags(item.UserId, item.CounterId, item.StatusId);
            return PartialView("_CreateOrEditPartial", item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Shift model)
        {
            if (id != model.ShiftId) return Json(new { success = false, message = "ID mismatch." });
            if (ModelState.IsValid)
            {
                try { _context.Update(model); await _context.SaveChangesAsync(); return Json(new { success = true, message = "Shift updated." }); }
                catch (DbUpdateConcurrencyException) { }
            }
            LoadViewBags(model.UserId, model.CounterId, model.StatusId);
            return PartialView("_CreateOrEditPartial", model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.Shifts.Include(s => s.User).Include(s => s.Counter).Include(s => s.Status).FirstOrDefaultAsync(m => m.ShiftId == id);
            if (item == null) return NotFound();
            return PartialView("_DeletePartial", item);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Shifts.FindAsync(id);
            if (item != null) { _context.Shifts.Remove(item); await _context.SaveChangesAsync(); }
            return Json(new { success = true, message = "Shift deleted." });
        }

        private void LoadViewBags(int? userId = null, int? counterId = null, int? statusId = null)
        {
            ViewBag.UserId = new SelectList(_context.Users.Where(u => u.IsActive), "UserId", "UserName", userId);
            ViewBag.CounterId = new SelectList(_context.Counters.Where(c => c.IsActive), "CounterId", "CounterName", counterId);
            ViewBag.StatusId = new SelectList(_context.ShiftStatuses, "StatusId", "StatusName", statusId);
        }
    }
}
