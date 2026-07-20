using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductType)
                .Include(p => p.Unit)
                .Include(p => p.Manufacturer)
                .Include(p => p.TaxType)
                .Where(p => p.IsActive)
                .ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products
                .Include(p => p.Category).Include(p => p.ProductType)
                .Include(p => p.Unit).Include(p => p.Manufacturer).Include(p => p.TaxType)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null) return NotFound();
            return PartialView("_DetailsPartial", product);
        }

        public IActionResult Create()
        {
            LoadViewBags();
            return PartialView("_CreateOrEditPartial", new Product());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product model, IFormFile? imageFile)
        {
            // Auto-generate Slug from ProductName if not provided
            if (string.IsNullOrWhiteSpace(model.Slug) && !string.IsNullOrWhiteSpace(model.ProductName))
                model.Slug = SlugHelper.GenerateSlug(model.ProductName);
            ModelState.Remove("Slug");
            ModelState.Remove("ImageUrl");
            ModelState.Remove("RowVersion");

            if (imageFile != null && imageFile.Length > 0)
            {
                model.ImageUrl = await SaveUploadedImage(imageFile);
            }

            model.IsActive = true;

            if (ModelState.IsValid)
            {
                _context.Add(model);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Product created successfully." });
            }

            LoadViewBags(model.CategoryId, model.ProductTypeId, model.UnitId, model.ManufacturerId, model.TaxTypeId);
            return PartialView("_CreateOrEditPartial", model);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            LoadViewBags(product.CategoryId, product.ProductTypeId, product.UnitId, product.ManufacturerId, product.TaxTypeId);
            return PartialView("_CreateOrEditPartial", product);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product model, IFormFile? imageFile)
        {
            if (id != model.ProductId) return Json(new { success = false, message = "Product ID mismatch." });

            if (string.IsNullOrWhiteSpace(model.Slug) && !string.IsNullOrWhiteSpace(model.ProductName))
                model.Slug = SlugHelper.GenerateSlug(model.ProductName);
            ModelState.Remove("Slug");
            ModelState.Remove("ImageUrl");
            ModelState.Remove("RowVersion");

            // If a new image is uploaded, save it and update; otherwise keep existing
            if (imageFile != null && imageFile.Length > 0)
            {
                model.ImageUrl = await SaveUploadedImage(imageFile);
            }
            else
            {
                // Preserve existing ImageUrl from hidden form field (passed via form)
                var existingImageUrl = Request.Form["existingImageUrl"].ToString();
                if (!string.IsNullOrEmpty(existingImageUrl))
                    model.ImageUrl = existingImageUrl;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Product updated successfully." });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Products.Any(e => e.ProductId == model.ProductId))
                        return Json(new { success = false, message = "Product no longer exists." });
                    throw;
                }
            }

            LoadViewBags(model.CategoryId, model.ProductTypeId, model.UnitId, model.ManufacturerId, model.TaxTypeId);
            return PartialView("_CreateOrEditPartial", model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var product = await _context.Products
                .Include(p => p.Category).Include(p => p.Unit)
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null) return NotFound();
            return PartialView("_DeletePartial", product);
        }

        [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.IsActive = false;
                _context.Update(product);
                await _context.SaveChangesAsync();
            }
            return Json(new { success = true, message = "Product deactivated successfully." });
        }

        // =============================================
        // PRIVATE: Save uploaded image to wwwroot/uploads/products/
        // Returns relative URL path like "/uploads/products/filename.ext"
        // =============================================
        private async Task<string> SaveUploadedImage(IFormFile imageFile)
        {
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "products");
            Directory.CreateDirectory(uploadsFolder);

            var ext = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            if (!allowedExtensions.Contains(ext)) ext = ".jpg";

            var uniqueFileName = $"{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await imageFile.CopyToAsync(stream);

            return $"/uploads/products/{uniqueFileName}";
        }

        private void LoadViewBags(int? catId = null, int? typeId = null, int? unitId = null,
                                   int? mfgrId = null, int? taxId = null)
        {
            ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "CategoryName", catId);
            ViewBag.ProductTypeId = new SelectList(_context.ProductTypes.Where(t => t.IsActive), "ProductTypeId", "TypeName", typeId);
            ViewBag.UnitId = new SelectList(_context.Units, "UnitId", "UnitName", unitId);
            ViewBag.ManufacturerId = new SelectList(_context.Manufacturers.Where(m => m.IsActive), "ManufacturerId", "ManufacturerName", mfgrId);
            ViewBag.TaxTypeId = new SelectList(_context.TaxTypes.Where(t => t.IsActive), "TaxTypeId", "TaxName", taxId);
        }
    }
}
