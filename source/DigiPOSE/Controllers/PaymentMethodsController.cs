using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Models;

namespace DigiPOSE.Controllers
{
    public class PaymentMethodsController : Controller
    {
        private readonly DigiPoseDbContext _context;
        public PaymentMethodsController(DigiPoseDbContext context) { _context = context; }

        public async Task<IActionResult> Index()
            => View(await _context.PaymentMethods.ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.PaymentMethods.FirstOrDefaultAsync(m => m.PaymentMethodId == id);
            if (item == null) return NotFound();
            return PartialView("_DetailsPartial", item);
        }

        public IActionResult Create()
        {

            return PartialView("_CreateOrEditPartial", new PaymentMethod());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentMethod model)
        {
            if (ModelState.IsValid)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "PaymentMethod created successfully." });
            }

            return PartialView("_CreateOrEditPartial", model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.PaymentMethods.FindAsync(id);
            if (item == null) return NotFound();

            return PartialView("_CreateOrEditPartial", item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PaymentMethod model)
        {
            if (id != model.PaymentMethodId) return Json(new { success = false, message = "ID mismatch." });
            
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "PaymentMethod updated successfully." });
                }
                catch (DbUpdateConcurrencyException) { }
            }

            return PartialView("_CreateOrEditPartial", model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.PaymentMethods.FirstOrDefaultAsync(m => m.PaymentMethodId == id);
            if (item == null) return NotFound();
            return PartialView("_DeletePartial", item);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.PaymentMethods.FindAsync(id);
            if (item != null) { _context.PaymentMethods.Remove(item); await _context.SaveChangesAsync(); }
            return Json(new { success = true, message = "PaymentMethod deleted successfully." });
        }
    }
}
