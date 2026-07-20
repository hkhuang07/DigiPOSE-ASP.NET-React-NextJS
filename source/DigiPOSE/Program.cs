using DigiPOSE.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Sử dụng DbContextPooling để tái sử dụng DbContext Instance, giảm tối đa chi phí cấp phát bộ nhớ (GC Pressure) khi xử lý hàng ngàn request/giây
builder.Services.AddDbContextPool<DigiPoseDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DigiPoseConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);
        });
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
