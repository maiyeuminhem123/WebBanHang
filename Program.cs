using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Bai3_WebBanHang.Models;
using Bai3_WebBanHang.Repositories;
using Bai3_WebBanHang.Services;
using WebBanMayTinh.Models.VNPAY;

var builder = WebApplication.CreateBuilder(args);

// 1. Thêm dịch vụ vào container
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// 2. Cấu hình DbContext
builder.Services.AddDbContext<BanHangContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3. Cấu hình Identity (Đã sửa lỗi, chỉ giữ lại một phương thức đăng ký)
// Cấu hình này hỗ trợ cả User (ApplicationUser) và Role (IdentityRole)
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<BanHangContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();

// Tùy chỉnh đường dẫn cho các trang Identity
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// 4. Thêm Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 5. Đăng ký Repository (Dependency Injection)
builder.Services.AddScoped<IProductRepository, EFProductRepository>();
builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();


var app = builder.Build();

// 6. Cấu hình Middleware Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession(); // Session phải được gọi trước Authentication và Authorization

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// 7. Cấu hình routing cho controller
// Route cho Area (ví dụ: /Admin/Product/Index)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Products}/{action=Index}/{id?}");

// Route mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
