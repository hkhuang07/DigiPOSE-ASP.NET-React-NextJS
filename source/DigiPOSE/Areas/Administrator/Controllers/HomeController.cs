using DigiPOSE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace DigiPOSE.Areas.Administrator.Controllers
{
    [Area("Administrator")]
    [Authorize(Roles = "Administrator, Branch Manager")]
    public class HomeController : Controller
    {
        private readonly DigiPoseDbContext _context;

        public HomeController(DigiPoseDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel();

            // 1. System & IAM (6 Tables)
            var branchCount = await _context.Branches.CountAsync();
            var roleCount = await _context.Roles.CountAsync();
            var userCount = await _context.Users.CountAsync();
            var counterCount = await _context.Counters.CountAsync();
            var shiftCount = await _context.Shifts.CountAsync();
            var shiftStatusCount = await _context.ShiftStatuses.CountAsync();

            var mod1 = new ModuleTelemetryInfo
            {
                ModuleId = "MOD-01",
                ModuleName = "System & IAM",
                TableCount = 6,
                TotalRecordCount = branchCount + roleCount + userCount + counterCount + shiftCount + shiftStatusCount,
                Tables = new List<TableTelemetryInfo>
                {
                    new() { TableName = "Branch", ControllerName = "Branches", RecordCount = branchCount },
                    new() { TableName = "Role", ControllerName = "Roles", RecordCount = roleCount },
                    new() { TableName = "User", ControllerName = "Users", RecordCount = userCount },
                    new() { TableName = "Counter", ControllerName = "Counters", RecordCount = counterCount },
                    new() { TableName = "Shift", ControllerName = "Shifts", RecordCount = shiftCount },
                    new() { TableName = "ShiftStatus", ControllerName = "ShiftStatuses", RecordCount = shiftStatusCount }
                }
            };

            // 2. CRM & Partners (3 Tables)
            var customerTypeCount = await _context.CustomerTypes.CountAsync();
            var customerCount = await _context.Customers.CountAsync();
            var supplierCount = await _context.Suppliers.CountAsync();

            var mod2 = new ModuleTelemetryInfo
            {
                ModuleId = "MOD-02",
                ModuleName = "CRM & Partners",
                TableCount = 3,
                TotalRecordCount = customerTypeCount + customerCount + supplierCount,
                Tables = new List<TableTelemetryInfo>
                {
                    new() { TableName = "CustomerType", ControllerName = "CustomerTypes", RecordCount = customerTypeCount },
                    new() { TableName = "Customer", ControllerName = "Customers", RecordCount = customerCount },
                    new() { TableName = "Supplier", ControllerName = "Suppliers", RecordCount = supplierCount }
                }
            };

            // 3. Catalog & Inventory (10 Tables)
            var categoryCount = await _context.Categories.CountAsync();
            var unitCount = await _context.Units.CountAsync();
            var manufacturerCount = await _context.Manufacturers.CountAsync();
            var taxTypeCount = await _context.TaxTypes.CountAsync();
            var productTypeCount = await _context.ProductTypes.CountAsync();
            var itemNatureCount = await _context.ItemNatures.CountAsync();
            var productCount = await _context.Products.CountAsync();
            var inventoryCount = await _context.ProductInventories.CountAsync();
            var voucherCount = await _context.StockVouchers.CountAsync();
            var voucherDetailCount = await _context.StockVoucherDetails.CountAsync();

            var mod3 = new ModuleTelemetryInfo
            {
                ModuleId = "MOD-03",
                ModuleName = "Catalog & Inventory",
                TableCount = 10,
                TotalRecordCount = categoryCount + unitCount + manufacturerCount + taxTypeCount + productTypeCount + itemNatureCount + productCount + inventoryCount + voucherCount + voucherDetailCount,
                Tables = new List<TableTelemetryInfo>
                {
                    new() { TableName = "Category", ControllerName = "Categories", RecordCount = categoryCount },
                    new() { TableName = "Unit", ControllerName = "Units", RecordCount = unitCount },
                    new() { TableName = "Manufacturer", ControllerName = "Manufacturers", RecordCount = manufacturerCount },
                    new() { TableName = "TaxType", ControllerName = "TaxTypes", RecordCount = taxTypeCount },
                    new() { TableName = "ProductType", ControllerName = "ProductTypes", RecordCount = productTypeCount },
                    new() { TableName = "ItemNature", ControllerName = "ItemNatures", RecordCount = itemNatureCount },
                    new() { TableName = "Product", ControllerName = "Products", RecordCount = productCount },
                    new() { TableName = "ProductInventory", ControllerName = "ProductInventories", RecordCount = inventoryCount },
                    new() { TableName = "StockVoucher", ControllerName = "StockVouchers", RecordCount = voucherCount },
                    new() { TableName = "StockVoucherDetail", ControllerName = "StockVoucherDetails", RecordCount = voucherDetailCount }
                }
            };

            // 4. Sales & Billing (7 Tables)
            var orderStatusCount = await _context.OrderStatuses.CountAsync();
            var paymentMethodCount = await _context.PaymentMethods.CountAsync();
            var orderCount = await _context.Orders.CountAsync();
            var orderDetailCount = await _context.OrderDetails.CountAsync();
            var invoiceStatusCount = await _context.InvoiceStatuses.CountAsync();
            var invoiceTypeCount = await _context.InvoiceTypes.CountAsync();
            var invoiceCount = await _context.Invoices.CountAsync();

            var mod4 = new ModuleTelemetryInfo
            {
                ModuleId = "MOD-04",
                ModuleName = "Sales & Billing",
                TableCount = 7,
                TotalRecordCount = orderStatusCount + paymentMethodCount + orderCount + orderDetailCount + invoiceStatusCount + invoiceTypeCount + invoiceCount,
                Tables = new List<TableTelemetryInfo>
                {
                    new() { TableName = "OrderStatus", ControllerName = "OrderStatuses", RecordCount = orderStatusCount },
                    new() { TableName = "PaymentMethod", ControllerName = "PaymentMethods", RecordCount = paymentMethodCount },
                    new() { TableName = "Order", ControllerName = "Orders", RecordCount = orderCount },
                    new() { TableName = "OrderDetail", ControllerName = "OrderDetails", RecordCount = orderDetailCount },
                    new() { TableName = "InvoiceStatus", ControllerName = "InvoiceStatuses", RecordCount = invoiceStatusCount },
                    new() { TableName = "InvoiceType", ControllerName = "InvoiceTypes", RecordCount = invoiceTypeCount },
                    new() { TableName = "Invoice", ControllerName = "Invoices", RecordCount = invoiceCount }
                }
            };

            viewModel.Modules = new List<ModuleTelemetryInfo> { mod1, mod2, mod3, mod4 };
            viewModel.TotalSystemRecords = mod1.TotalRecordCount + mod2.TotalRecordCount + mod3.TotalRecordCount + mod4.TotalRecordCount;

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
