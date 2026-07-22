using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Models;
using System.Linq.Dynamic.Core;

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
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index_LoadData()
        {
            try
            {
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                var query = _context.OrderDetails
                    .Include(od => od.Order)
                    .Include(od => od.Product)
                    .Include(od => od.ItemNature)
                    .Include(od => od.TaxType)
                    .AsQueryable();

                int totalRecords = query.Count();

                // Searching
                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(m =>
                        m.OrderId.ToString().Contains(searchValue) ||
                        (m.ProductName != null && m.ProductName.Contains(searchValue)));
                }

                int filterRecords = query.Count();

                // Sorting
                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
                {
                    query = query.OrderBy(sortColumn + " " + sortColumnDirection);
                }

                // Paging & Mapping
                var dataList = query.Skip(skip).Take(pageSize).Select(m => new {
                    OrderDetailId = m.OrderDetailId,
                    OrderId = m.OrderId,
                    ProductName = m.ProductName,
                    Quantity = m.Quantity,
                    UnitPrice = m.UnitPrice,
                    TotalAmount = m.TotalAmount,
                    IsFree = m.IsFree
                }).ToList();

                return Json(new { draw = draw, recordsFiltered = filterRecords, recordsTotal = totalRecords, data = dataList });
            }
            catch (Exception ex)
            {
                return Json(new { error = "An error occurred while loading data. Error: " + ex.Message });
            }
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
