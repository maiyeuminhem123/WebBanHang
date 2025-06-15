using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Bai3_WebBanHang.Repositories;
using Microsoft.AspNetCore.Identity;
using Bai3_WebBanHang.Models;
using Microsoft.AspNetCore.Authorization; // << Quan trọng
using System.Security.Claims;

namespace Bai3_WebBanHang.Controllers
{
    [Authorize] // YÊU CẦU ĐĂNG NHẬP CHO TOÀN BỘ CONTROLLER NÀY
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;

        public OrderController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        // GET: /Order hoặc /Order/Index
        // Hiển thị danh sách đơn hàng của người dùng
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = await _orderRepository.GetByUserIdAsync(userId);
            return View(orders); // Trả về View Index.cshtml
        }

        // GET: /Order/Details/{id}
        // Hiển thị chi tiết một đơn hàng
        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            // Bảo mật: Đảm bảo người dùng chỉ xem được đơn hàng của chính mình
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (order.UserId != userId)
            {
                return Forbid(); // Không cho phép truy cập
            }

            return View(order); // Trả về View Details.cshtml
        }
    }
}