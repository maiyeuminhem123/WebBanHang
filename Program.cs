    using Microsoft.AspNetCore.Identity;
    using Bai3_WebBanHang.Models;
    using Bai3_WebBanHang.Repositories;
    using Microsoft.EntityFrameworkCore;

    var builder = WebApplication.CreateBuilder(args);

    // 1. Thêm dịch vụ vào container
    builder.Services.AddControllersWithViews();
    builder.Services.AddRazorPages();

    // 2. Cấu hình DbContext
    builder.Services.AddDbContext<BanHangContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // 3. Cấu hình Identity
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
        .AddEntityFrameworkStores<BanHangContext>()
        .AddDefaultTokenProviders()
        .AddDefaultUI();

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

    // 5. Đăng ký Repository
    builder.Services.AddScoped<IProductRepository, EFProductRepository>();
    builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();

    var app = builder.Build();

    // 6. Middleware Pipeline
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseSession();
    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapRazorPages();

    // 7. Cấu hình routing cho controller

    // Route cho Area (ví dụ: /Admin/Product/Index)
    app.MapControllerRoute(
        name: "areas",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.Run();
