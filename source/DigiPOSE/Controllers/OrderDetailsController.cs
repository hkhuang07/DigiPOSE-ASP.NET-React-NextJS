using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Models;

namespace DigiPOSE.Controllers
{
    public class OrderDetailsController : Controller
    {
        private readonly DigiPoseDbContext _context;

        public OrderDetailsController(DigiPoseDbContext context)
        {
            _context = context;
        }

        private async Task PopulateSelectListsAsync()
        {
            ViewBag.OrderId = new SelectList(await _context.Orders.ToListAsync(), "OrderId", "OrderId");
            ViewBag.ProductId = new SelectList(await _context.Products.Where(p => p.IsActive).ToListAsync(), "ProductId", "ProductName");
            ViewBag.NatureId = new SelectList(await _context.ItemNatures.ToListAsync(), "NatureId", "NatureName");
            ViewBag.TaxTypeId = new SelectList(await _context.TaxTypes.ToListAsync(), "TaxTypeId", "TaxName");
        }

        // GET: OrderDetails
        public async Task<IActionResult> Index()
        {
            var details = await _context.OrderDetails
                .Include(od => od.Order)
                .Include(od => od.Product)
                .Include(od => od.ItemNature)
                .Include(od => od.TaxType)
                .ToListAsync();

            await PopulateSelectListsAsync();
            return View(details);
        }

        // GET: OrderDetails/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var detail = await _context.OrderDetails
                .Include(od => od.Order)
                .Include(od => od.Product)
                .Include(od => od.ItemNature)
                .Include(od => od.TaxType)
                .FirstOrDefaultAsync(m => m.OrderDetailId == id);
            if (detail == null) return NotFound();

            return View(detail);
        }

        // POST: OrderDetails/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OrderDetailId,OrderId,ProductId,NatureId,TaxTypeId,Quantity,ProductName,UnitName,UnitPrice,DiscountRate,DiscountAmount,TaxRate,TaxAmount,TotalAmount,Notes,IsFree")] OrderDetail orderDetail)
        {
            if (ModelState.IsValid)
            {
                orderDetail.TotalAmount = (orderDetail.Quantity * orderDetail.UnitPrice) - orderDetail.DiscountAmount + orderDetail.TaxAmount;
                _context.Add(orderDetail);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var details = await _context.OrderDetails
                .Include(od => od.Order).Include(od => od.Product).Include(od => od.ItemNature).Include(od => od.TaxType).ToListAsync();
            await PopulateSelectListsAsync();
            return View("Index", details);
        }

        // POST: OrderDetails/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OrderDetailId,OrderId,ProductId,NatureId,TaxTypeId,Quantity,ProductName,UnitName,UnitPrice,DiscountRate,DiscountAmount,TaxRate,TaxAmount,TotalAmount,Notes,IsFree")] OrderDetail orderDetail)
        {
            if (id != orderDetail.OrderDetailId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    orderDetail.TotalAmount = (orderDetail.Quantity * orderDetail.UnitPrice) - orderDetail.DiscountAmount + orderDetail.TaxAmount;
                    _context.Update(orderDetail);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderDetailExists(orderDetail.OrderDetailId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            var details = await _context.OrderDetails
                .Include(od => od.Order).Include(od => od.Product).Include(od => od.ItemNature).Include(od => od.TaxType).ToListAsync();
            await PopulateSelectListsAsync();
            return View("Index", details);
        }

        // POST: OrderDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var detail = await _context.OrderDetails.FindAsync(id);
            if (detail != null)
            {
                _context.OrderDetails.Remove(detail);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool OrderDetailExists(int id)
        {
            return _context.OrderDetails.Any(e => e.OrderDetailId == id);
        }
    }
}
