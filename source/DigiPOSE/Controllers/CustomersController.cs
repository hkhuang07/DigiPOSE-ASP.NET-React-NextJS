using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Models;

namespace DigiPOSE.Controllers
{
    public class CustomersController : Controller
    {
        private readonly DigiPoseDbContext _context;
        public CustomersController(DigiPoseDbContext context) { _context = context; }

        public async Task<IActionResult> Index()
            => View(await _context.Customers.Include(c => c.CustomeType).ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.Customers.Include(c => c.CustomeType).FirstOrDefaultAsync(m => m.CustomerId == id);
            if (item == null) return NotFound();
            return PartialView("_DetailsPartial", item);
        }

        public IActionResult Create()
        {
            ViewBag.CustomeTypeId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.CustomerTypes, "CustomeTypeId", "TypeName");
            return PartialView("_CreateOrEditPartial", new Customer());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CustomeTypeId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.CustomerTypes, "CustomeTypeId", "TypeName", model.CustomeTypeId);
                return PartialView("_CreateOrEditPartial", model);
            }
            _context.Add(model);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Customer created successfully." });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.Customers.FindAsync(id);
            if (item == null) return NotFound();
            ViewBag.CustomeTypeId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.CustomerTypes, "CustomeTypeId", "TypeName", item.CustomeTypeId);
            return PartialView("_CreateOrEditPartial", item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer model)
        {
            if (id != model.CustomerId) return Json(new { success = false, message = "ID mismatch." });
            if (!ModelState.IsValid)
            {
                ViewBag.CustomeTypeId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.CustomerTypes, "CustomeTypeId", "TypeName", model.CustomeTypeId);
                return PartialView("_CreateOrEditPartial", model);
            }
            try { _context.Update(model); await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException) { return Json(new { success = false, message = "Concurrency error." }); }
            return Json(new { success = true, message = "Customer updated successfully." });
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.Customers.Include(c => c.CustomeType).FirstOrDefaultAsync(m => m.CustomerId == id);
            if (item == null) return NotFound();
            return PartialView("_DeletePartial", item);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Customers.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Record not found." });
            _context.Customers.Remove(item);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Customer permanently deleted." });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var item = await _context.Customers.FindAsync(id);
            if (item == null) return Json(new { success = false });
            item.IsActive = !item.IsActive;
            await _context.SaveChangesAsync();
            return Json(new { success = true, isActive = item.IsActive, message = item.IsActive ? "Activated." : "Deactivated." });
        }
    }
}
