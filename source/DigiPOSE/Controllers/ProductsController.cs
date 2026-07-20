using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Models;
using DigiPOSE.Web.Helpers;

namespace DigiPOSE.Controllers
{
    public class ProductsController : Controller
    {
        private readonly DigiPoseDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductsController(DigiPoseDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
            => View(await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductType)
                .Include(p => p.Unit)
                .Include(p => p.Manufacturer)
                .Include(p => p.TaxType)
                .ToListAsync());

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.Products
                .Include(p => p.Category).Include(p => p.ProductType)
                .Include(p => p.Unit).Include(p => p.Manufacturer).Include(p => p.TaxType)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (item == null) return NotFound();
            return PartialView("_DetailsPartial", item);
        }

        public IActionResult Create()
        {
            PopulateDropdowns();
            return PartialView("_CreateOrEditPartial", new Product());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product model)
        {
            if (string.IsNullOrWhiteSpace(model.Slug))
                model.Slug = SlugHelper.GenerateSlug(model.ProductName);
            ModelState.Remove("Slug");
            ModelState.Remove("RowVersion");

            if (!ModelState.IsValid)
            {
                PopulateDropdowns(model);
                return PartialView("_CreateOrEditPartial", model);
            }

            _context.Add(model);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Product created successfully." });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.Products.FindAsync(id);
            if (item == null) return NotFound();
            PopulateDropdowns(item);
            return PartialView("_CreateOrEditPartial", item);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product model)
        {
            if (id != model.ProductId) return Json(new { success = false, message = "ID mismatch." });

            if (string.IsNullOrWhiteSpace(model.Slug))
                model.Slug = SlugHelper.GenerateSlug(model.ProductName);
            ModelState.Remove("Slug");
            ModelState.Remove("RowVersion");

            if (!ModelState.IsValid)
            {
                PopulateDropdowns(model);
                return PartialView("_CreateOrEditPartial", model);
            }

            try
            {
                _context.Update(model);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Product updated successfully." });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Json(new { success = false, message = "Concurrency error — please reload and try again." });
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var item = await _context.Products
                .Include(p => p.Category).Include(p => p.Unit)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (item == null) return NotFound();
            return PartialView("_DeletePartial", item);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Products.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Record not found." });
            _context.Products.Remove(item);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Product permanently deleted." });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var item = await _context.Products.FindAsync(id);
            if (item == null) return Json(new { success = false });
            item.IsActive = !item.IsActive;
            await _context.SaveChangesAsync();
            return Json(new { success = true, isActive = item.IsActive, message = item.IsActive ? "Activated." : "Deactivated." });
        }

        private void PopulateDropdowns(Product? model = null)
        {
            ViewBag.CategoryId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Categories, "CategoryId", "CategoryName", model?.CategoryId);
            ViewBag.ProductTypeId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.ProductTypes.Where(x => x.IsActive), "ProductTypeId", "TypeName", model?.ProductTypeId);
            ViewBag.UnitId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Units, "UnitId", "UnitName", model?.UnitId);
            ViewBag.ManufacturerId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Manufacturers.Where(x => x.IsActive), "ManufacturerId", "ManufacturerName", model?.ManufacturerId);
            ViewBag.TaxTypeId = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.TaxTypes.Where(x => x.IsActive), "TaxTypeId", "TaxName", model?.TaxTypeId);
        }
    }
}
