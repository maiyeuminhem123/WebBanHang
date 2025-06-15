// Models/DashboardViewModel.cs
namespace Bai3_WebBanHang.Models;

public class DashboardViewModel
{
    // Dữ liệu cho các thẻ KPI
    public decimal RevenueToday { get; set; }
    public int OrdersToday { get; set; }
    public int NewUsersToday { get; set; }
    public int PendingOrders { get; set; }

    // Dữ liệu cho biểu đồ Line Chart (Doanh thu theo ngày)
    public List<string> RevenueChartLabels { get; set; } = new();
    public List<decimal> RevenueChartData { get; set; } = new();

    // Dữ liệu cho biểu đồ Doughnut Chart (Doanh thu theo danh mục)
    public List<string> CategoryPieChartLabels { get; set; } = new();
    public List<decimal> CategoryPieChartData { get; set; } = new();

    // Dữ liệu cho biểu đồ Bar Chart (Top sản phẩm)
    public List<string> TopProductsBarChartLabels { get; set; } = new();
    public List<int> TopProductsBarChartData { get; set; } = new();

    // Dữ liệu cho các danh sách
    public IEnumerable<Order> RecentOrders { get; set; } = new List<Order>();
}