// Models/HomeViewModel.cs
namespace Bai3_WebBanHang.Models
{
    public class HomeViewModel
    {
        public IEnumerable<Product> FeaturedProducts { get; set; }
        public IEnumerable<Product> AllProducts { get; set; }
        public IEnumerable<Category> FeaturedCategories { get; set; }
    }
}