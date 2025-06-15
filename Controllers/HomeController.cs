using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Bai3_WebBanHang.Models;
using Microsoft.AspNetCore.Authorization;
using Bai3_WebBanHang.Repositories;
using System.Threading.Tasks; // Thêm using này
using System.Linq; // Thêm using này

namespace Bai3_WebBanHang.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository; // <-- Thêm repository cho danh mục

        // Cập nhật constructor để nhận thêm ICategoryRepository
        public HomeController(ILogger<HomeController> logger, IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _logger = logger;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        [AllowAnonymous]
        // Thay thế action Index cũ bằng action mới này
        public async Task<IActionResult> Index()
        {
            var allProducts = await _productRepository.GetAllAsync();
            var allCategories = await _categoryRepository.GetAllAsync();

            var viewModel = new HomeViewModel
            {
                // Lấy 4 sản phẩm làm sản phẩm nổi bật
                FeaturedProducts = allProducts.Take(4),

                FeaturedCategories = allCategories,

                // Truyền toàn bộ sản phẩm  
                AllProducts = allProducts
            };

            return View(viewModel); // Trả về View với gói dữ liệu ViewModel
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