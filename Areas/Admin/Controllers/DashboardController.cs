// Areas/Admin/Controllers/DashboardController.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bai3_WebBanHang.Models;

namespace Bai3_WebBanHang.Areas.Admin.Controllers
{
    public class DashboardController : BaseAdminController
    {
        private readonly BanHangContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(BanHangContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var today = DateTime.UtcNow.Date;
            var sevenDaysAgo = today.AddDays(-6);
            var viewModel = new DashboardViewModel();

            // === 1. TÍNH TOÁN DỮ LIỆU CHO CÁC THẺ KPI ===
            var allOrders = await _context.Orders.AsNoTracking().ToListAsync();
            viewModel.RevenueToday = allOrders
                .Where(o => o.OrderDate.Date == today && o.OrderStatus != "Cancelled")
                .Sum(o => o.TotalAmount);
            viewModel.OrdersToday = allOrders.Count(o => o.OrderDate.Date == today);
            viewModel.NewUsersToday = await _context.Users.AsNoTracking().CountAsync(u => u.LockoutEnd != null && u.LockoutEnd.Value.Date == today); // Dùng tạm LockoutEnd
            viewModel.PendingOrders = allOrders.Count(o => o.OrderStatus == "Pending" || o.OrderStatus == "Confirmed");

            // === 2. DỮ LIỆU BIỂU ĐỒ DOANH THU 7 NGÀY (LINE CHART) ===
            var dailyRevenue = allOrders
                .Where(o => o.OrderDate.Date >= sevenDaysAgo && o.OrderStatus != "Cancelled")
                .GroupBy(o => o.OrderDate.Date)
                .ToDictionary(g => g.Key, g => g.Sum(o => o.TotalAmount));
            for (var date = sevenDaysAgo; date <= today; date = date.AddDays(1))
            {
                viewModel.RevenueChartLabels.Add(date.ToString("dd/MM"));
                viewModel.RevenueChartData.Add(dailyRevenue.ContainsKey(date) ? dailyRevenue[date] : 0);
            }

            // === 3. DỮ LIỆU TỶ TRỌNG DANH MỤC (DOUGHNUT CHART) ===
            var categoryRevenue = await _context.OrderDetails.AsNoTracking()
                .Include(od => od.Product).ThenInclude(p => p.Category)
                .Where(od => od.Order.OrderStatus != "Cancelled")
                .GroupBy(od => od.Product.Category.Name)
                .Select(g => new { CategoryName = g.Key, Total = g.Sum(od => od.Quantity * od.Price) })
                .OrderByDescending(x => x.Total)
                .Take(5) // Lấy 5 danh mục top
                .ToListAsync();
            foreach (var item in categoryRevenue)
            {
                viewModel.CategoryPieChartLabels.Add(item.CategoryName);
                viewModel.CategoryPieChartData.Add(item.Total);
            }

            // === 4. DỮ LIỆU TOP SẢN PHẨM BÁN CHẠY (BAR CHART) ===
            var topProducts = await _context.OrderDetails.AsNoTracking()
                .GroupBy(od => new { od.ProductId, od.Product.Name })
                .Select(g => new { ProductName = g.Key.Name, TotalQuantity = g.Sum(od => od.Quantity) })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(5)
                .ToListAsync();
            foreach (var item in topProducts)
            {
                viewModel.TopProductsBarChartLabels.Add(item.ProductName);
                viewModel.TopProductsBarChartData.Add(item.TotalQuantity);
            }

            // === 5. DANH SÁCH ĐƠN HÀNG GẦN ĐÂY ===
            viewModel.RecentOrders = allOrders.OrderByDescending(o => o.OrderDate).Take(5);

            return View(viewModel);
        }
    }
}