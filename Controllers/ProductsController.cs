using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bai3_WebBanHang.Models;

namespace Bai3_WebBanHang.Controllers
{
    public class ProductsController : Controller
    {
        private readonly BanHangContext _context;

        public ProductsController(BanHangContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var products = _context.Products.Include(p => p.Category);
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(await products.ToListAsync());
        }


        // Hiển thị chi tiết sản phẩm
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }
    }
}
       
