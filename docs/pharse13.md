Đòn Tấn công Red Team (Kiểm định Hiệu năng):Nút thắt cổ chai (Bottleneck) tử huyệt của các hệ thống PoS khi làm báo cáo là Kỹ sư thường gọi hàm .ToList() để kéo toàn bộ dữ liệu Hóa đơn (Orders) về RAM server rồi mới dùng vòng lặp foreach để tính toán. Với 100,000 hóa đơn, hành động này sẽ gây tràn bộ nhớ (OutOfMemory) và sập hệ thống.Giải pháp Kiến trúc: Chúng ta phải ép Entity Framework Core dịch các biểu thức LINQ thành các câu lệnh SUM(), COUNT(), GROUP BY trực tiếp dưới tầng SQL Server. Khi đó, độ phức tạp không gian (Space Complexity) trả về cho Server chỉ là $\mathcal{O}(1)$ thay vì $\mathcal{O}(N)$.

BUỔI 13: XÂY DỰNG DASHBOARD THỐNG KÊ VÀ CẢNH BÁO TỒN KHO TỐI ƯU HIỆU NĂNG (DIGIPOSE)
Bước 1: Khởi tạo ViewModels (DTOs) chứa dữ liệu thống kê
Dashboard cần nhiều luồng dữ liệu khác nhau (Doanh thu, Top sản phẩm, Cảnh báo kho). Ta gom tất cả vào một file ViewModel để code gọn gàng.

Tạo file DashboardViewModel.cs trong thư mục Areas/Admin/Models/:

C#
namespace DigiPOSE.Web.Areas.Admin.Models
{
    public class DashboardViewModel
    {
        public decimal TodayRevenue { get; set; }
        public decimal MonthRevenue { get; set; }
        public int TotalOrdersToday { get; set; }
        
        // Danh sách sản phẩm sắp hết hàng cần nhập gấp
        public List<LowStockAlertViewModel> LowStockAlerts { get; set; } = new();
    }

    public class LowStockAlertViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public string BranchName { get; set; } = null!;
        public int StockQuantity { get; set; }
        public int MinStockLevel { get; set; }
    }

    // Dùng để trả về JSON cho Biểu đồ Chart.js
    public class RevenueChartDto
    {
        public string DateLabel { get; set; } = null!;
        public decimal Revenue { get; set; }
    }
}
Bước 2: Viết truy vấn LINQ tối ưu trong HomeController của Admin CMS
Chúng ta sẽ biến trang chủ Admin (tạo ở Buổi 5) thành Bảng điều khiển (Dashboard) trung tâm.

Mở file Areas/Admin/Controllers/HomeController.cs và tái cấu trúc lại:

C#
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Web.Models;
using DigiPOSE.Web.Areas.Admin.Models;

namespace DigiPOSE.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin, Branch Manager")]
    public class HomeController : Controller
    {
        private readonly DigiPoseDbContext _context;

        public HomeController(DigiPoseDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);
            
            var userBranchIdStr = User.Claims.FirstOrDefault(c => c.Type == "BranchId")?.Value;
            int branchId = string.IsNullOrEmpty(userBranchIdStr) ? 0 : int.Parse(userBranchIdStr);

            var model = new DashboardViewModel();

            // 1. Truy vấn Doanh thu hôm nay (Ép chạy SUM dưới SQL Server)
            model.TodayRevenue = await _context.Orders
                .Where(o => o.Status == "Completed" && o.CreatedAt.Date == today)
                .SumAsync(o => o.FinalTotal);

            // 2. Truy vấn Doanh thu tháng này
            model.MonthRevenue = await _context.Orders
                .Where(o => o.Status == "Completed" && o.CreatedAt >= startOfMonth)
                .SumAsync(o => o.FinalTotal);

            // 3. Số lượng đơn hàng hôm nay
            model.TotalOrdersToday = await _context.Orders
                .CountAsync(o => o.Status == "Completed" && o.CreatedAt.Date == today);

            // 4. Cảnh báo tồn kho (Lọc những SP có Tồn thực tế <= Mức cảnh báo)
            // Lọc theo Chi nhánh của Quản lý đang đăng nhập
            model.LowStockAlerts = await _context.ProductInventories
                .Include(pi => p.Product)
                .Include(pi => p.Branch)
                .Where(pi => pi.BranchId == branchId && pi.StockQuantity <= pi.MinStockLevel)
                .Select(pi => new LowStockAlertViewModel
                {
                    ProductId = pi.ProductId,
                    ProductName = pi.Product!.ProductName,
                    BranchName = pi.Branch!.BranchName,
                    StockQuantity = pi.StockQuantity,
                    MinStockLevel = pi.MinStockLevel
                })
                .Take(10) // Chỉ lấy 10 sản phẩm khẩn cấp nhất
                .ToListAsync();

            return View(model);
        }

        // API nội bộ cung cấp dữ liệu vẽ biểu đồ Doanh thu 7 ngày gần nhất
        [HttpGet]
        public async Task<IActionResult> GetChartData()
        {
            var sevenDaysAgo = DateTime.Today.AddDays(-6); // Bao gồm hôm nay là 7 ngày

            // Sử dụng GroupBy để gom nhóm doanh thu theo ngày trực tiếp ở DB
            var chartData = await _context.Orders
                .Where(o => o.Status == "Completed" && o.CreatedAt.Date >= sevenDaysAgo)
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new RevenueChartDto
                {
                    DateLabel = g.Key.ToString("dd/MM"),
                    Revenue = g.Sum(o => o.FinalTotal)
                })
                .OrderBy(d => d.DateLabel)
                .ToListAsync();

            return Json(chartData);
        }
    }
}
Bước 3: Tích hợp Thư viện Chart.js
Mở Terminal/Console và tải file thư viện của Chart.js về thư mục wwwroot (Hoặc có thể dùng CDN trực tiếp cho nhanh). Ở đây ta dùng CDN cho tiện lợi.

Bước 4: Xây dựng Giao diện Bảng điều khiển (Dashboard)
Mở file Areas/Admin/Views/Home/Index.cshtml và thay thế nội dung cũ bằng bản thiết kế Dashboard chuyên nghiệp dưới đây: