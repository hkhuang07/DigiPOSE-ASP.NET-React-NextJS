BUỔI 8: XÂY DỰNG MODULE TRUYỀN THÔNG NỘI BỘ (INTERNAL ANNOUNCEMENTS)Bước 1: Bổ sung Entities cho hệ thống Thông báo nội bộTạo 2 bảng mới để phân loại và lưu trữ nội dung thông báo.1. Models/AnnouncementCategory.cs (Tương đương bảng ChuDe)  C#using System.ComponentModel.DataAnnotations;

namespace DigiPOSE.Web.Models
{
    public class AnnouncementCategory
    {
        [Key] public int CategoryId { get; set; }
        
        [Display(Name = "Loại thông báo")]
        [Required(ErrorMessage = "Tên loại thông báo không được bỏ trống.")]
        [StringLength(100)]
        public string Name { get; set; } = null!;

        public ICollection<Announcement>? Announcements { get; set; }
    }
}
2. Models/Announcement.cs (Tương đương bảng BaiViet)  C#using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigiPOSE.Web.Models
{
    public class Announcement
    {
        [Key] public int AnnouncementId { get; set; }
        
        [Display(Name = "Phân loại")]
        [Required(ErrorMessage = "Vui lòng chọn loại thông báo.")]
        public int CategoryId { get; set; }
        
        [Display(Name = "Người đăng")]
        public int UserId { get; set; } // Liên kết tới bảng User (Admin/HR)
        
        [Display(Name = "Tiêu đề")]
        [Required(ErrorMessage = "Tiêu đề không được bỏ trống.")]
        [StringLength(255)]
        public string Title { get; set; } = null!;
        
        [Display(Name = "Nội dung")]
        [Required(ErrorMessage = "Nội dung không được bỏ trống.")]
        [Column(TypeName = "ntext")]
        public string Content { get; set; } = null!;
        
        [Display(Name = "Mức độ quan trọng")]
        public bool IsUrgent { get; set; } = false; // Gắn cờ Đỏ cho máy PoS

        [Display(Name = "Ngày đăng")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Trạng thái hiển thị")]
        public bool IsActive { get; set; } = true;

        public AnnouncementCategory? Category { get; set; }
        public User? User { get; set; }
    }
}
Bước 2: Cập nhật DbContext và chạy MigrationMở file Models/DigiPoseDbContext.cs và bổ sung 2 DbSet mới:  C#public DbSet<AnnouncementCategory> AnnouncementCategories { get; set; }
public DbSet<Announcement> Announcements { get; set; }
Mở Package Manager Console và thực thi:  PowerShellAdd-Migration Add_Internal_Announcements
Update-Database
Bước 3: Xây dựng Admin CMS cho Thông báo (Tích hợp CKEditor)Dùng Scaffolding sinh tự động AnnouncementsController và các Views trong Area Admin. Sau đó, ta tinh chỉnh lại form Thêm/Sửa để tích hợp bộ soạn thảo văn bản Rich-Text.  1. Cập nhật Areas/Admin/Controllers/AnnouncementsController.cs  Bắt tự động UserId từ Token/Cookie của người đang đăng nhập thay vì để người dùng tự chọn.C#// POST: Admin/Announcements/Create
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create([Bind("CategoryId,Title,Content,IsUrgent,IsActive")] Announcement announcement)
{
    if (ModelState.IsValid)
    {
        // Lấy UserId từ Claims của Admin đang đăng nhập
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
        announcement.UserId = Convert.ToInt32(userIdClaim);
        announcement.CreatedAt = DateTime.Now;

        _context.Add(announcement);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    ViewData["CategoryId"] = new SelectList(_context.AnnouncementCategories, "CategoryId", "Name", announcement.CategoryId);
    return View(announcement);
}
2. Nhúng CKEditor vào View Create/Edit (Areas/Admin/Views/Announcements/Create.cshtml)
Đảm bảo đã copy thư viện ckeditor5 vào thư mục wwwroot/lib/ như đã làm ở các buổi trước.  HTML@model DigiPOSE.Web.Models.Announcement
@{
    ViewData["Title"] = "Tạo thông báo nội bộ";
}
<div class="card border-0 shadow-sm">
    <h5 class="card-header bg-primary text-white">@ViewData["Title"]</h5>
    <div class="card-body">
        <form asp-action="Create">
            <div class="row">
                <div class="col-md-4 mb-3">
                    <label asp-for="CategoryId" class="control-label"></label>
                    <select asp-for="CategoryId" class="form-select" asp-items="ViewBag.CategoryId"></select>
                </div>
                <div class="col-md-8 mb-3">
                    <label asp-for="Title" class="control-label"></label>
                    <input asp-for="Title" class="form-control" />
                    <span asp-validation-for="Title" class="text-danger"></span>
                </div>
            </div>

            <div class="mb-3">
                <label asp-for="Content" class="control-label"></label>
                <!-- Thẻ textarea sẽ được CKEditor thay thế -->
                <textarea asp-for="Content" id="editorContent" class="form-control"></textarea>
                <span asp-validation-for="Content" class="text-danger"></span>
            </div>

            <div class="d-flex mb-4">
                <div class="form-check me-4">
                    <input class="form-check-input" asp-for="IsUrgent" />
                    <label class="form-check-label text-danger fw-bold" asp-for="IsUrgent">Đánh dấu Quan trọng (Urgent)</label>
                </div>
                <div class="form-check">
                    <input class="form-check-input" asp-for="IsActive" checked />
                    <label class="form-check-label" asp-for="IsActive">Hiển thị ngay</label>
                </div>
            </div>

            <div class="mb-0">
                <button type="submit" class="btn btn-primary"><i class="fa fa-paper-plane"></i> Phát hành thông báo</button>
            </div>
        </form>
    </div>
</div>

@section Scripts {
    @{ await Html.RenderPartialAsync("_ValidationScriptsPartial"); }
    
    <!-- Khởi tạo CKEditor -->
    <script src="~/lib/ckeditor5/ckeditor.js"></script>
    <script>
        ClassicEditor.create(document.querySelector('#editorContent'), {
            toolbar: [ 'heading', '|', 'bold', 'italic', 'link', 'bulletedList', 'numberedList', 'blockQuote' ]
        }).catch(error => {
            console.error(error);
        });
    </script>
}
Bước 4: Xây dựng RESTful API cấp phát Thông báo cho máy PoSThay vì render HTML Frontend như tài liệu gốc, chúng ta viết API để các thiết bị PoS tại các chi nhánh có thể "kéo" (fetch) thông báo về.  Tạo AnnouncementsApiController.cs trong Controllers/Api/:C#using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DigiPOSE.Web.Models;

namespace DigiPOSE.Web.Controllers.Api
{
    [Route("api/v1/announcements")]
    [ApiController]
    // [Authorize] // Đảm bảo chỉ máy PoS đã đăng nhập mới gọi được
    public class AnnouncementsApiController : ControllerBase
    {
        private readonly DigiPoseDbContext _context;

        public AnnouncementsApiController(DigiPoseDbContext context)
        {
            _context = context;
        }

        // Lấy 5 thông báo mới nhất hiển thị lên Dashboard PoS
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestAnnouncements()
        {
            var announcements = await _context.Announcements
                .Include(a => a.Category)
                .Include(a => a.User)
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .Select(a => new 
                {
                    Id = a.AnnouncementId,
                    Category = a.Category!.Name,
                    Title = a.Title,
                    Content = a.Content, // Frontend React/Vue sẽ dùng dangerouslySetInnerHTML để render HTML này
                    IsUrgent = a.IsUrgent,
                    Author = a.User!.Username,
                    Date = a.CreatedAt.ToString("dd/MM/yyyy HH:mm")
                })
                .ToListAsync();

            return Ok(announcements);
        }
    }
}
