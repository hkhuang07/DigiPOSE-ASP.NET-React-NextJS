using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Models;

namespace DigiPOSE.Controllers
{
    public class ProductTypesController : Controller
    {
        private readonly DigiPoseDbContext _context;
        public ProductTypesController(DigiPoseDbContext context) { _context = context; }

        public async Task<IActionResult> Index()
            => View(await _context.ProductTypes.Where(x => x.IsActive).ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.ProductTypes.FirstOrDefaultAsync(m => m.ProductTypeId == id);
            if (item == null) return NotFound();
            return PartialView("_DetailsPartial", item);
        }

        public IActionResult Create()
        {

            return PartialView("_CreateOrEditPartial", new ProductType());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductType model)
        {
            if (ModelState.IsValid)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "ProductType created successfully." });
            }

            return PartialView("_CreateOrEditPartial", model);
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
            
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "ProductType updated successfully." });
                }
                catch (DbUpdateConcurrencyException) { }
            }

            return PartialView("_CreateOrEditPartial", model);
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
            if (item != null)
            {
                item.IsActive = false;
                _context.Update(item);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "ProductType deactivated successfully." });
            }
            return Json(new { success = false, message = "ProductType not found." });
        }
    }
}
