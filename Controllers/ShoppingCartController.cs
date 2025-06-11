using Bai3_WebBanHang.Extensions;
using Bai3_WebBanHang.Models;
using Bai3_WebBanHang.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json; // Cần thêm thư viện này để dùng TempData
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bai3_WebBanHang.Controllers
{
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly BanHangContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShoppingCartController(
            IProductRepository productRepository,
            BanHangContext context,
            UserManager<ApplicationUser> userManager)
        {
            _productRepository = productRepository;
            _context = context;
            _userManager = userManager;
        }

        #region Cart Management
        // Hiển thị giỏ hàng
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            return View(cart);
        }

        // Thêm sản phẩm vào giỏ hàng
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null) return NotFound();

            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            cart.AddItem(product, quantity);

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Index");
        }

        // Cập nhật giỏ hàng qua AJAX
        [HttpPost]
        public IActionResult UpdateCart(int productId, string action)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            var item = cart.Items.FirstOrDefault(p => p.ProductId == productId);

            if (item != null)
            {
                switch (action)
                {
                    case "increase":
                        item.Quantity++;
                        break;
                    case "decrease":
                        if (item.Quantity > 1)
                            item.Quantity--;
                        else
                            cart.Items.Remove(item); // Xóa nếu giảm từ 1
                        break;
                    case "remove":
                        cart.Items.Remove(item);
                        break;
                }
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return Json(new
            {
                success = true,
                quantity = item?.Quantity ?? 0
            });
        }
        #endregion

        #region Checkout Flow
        // Bước 1 - GET: Hiển thị form điền thông tin giao hàng
        [HttpGet]
        public IActionResult Checkout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            if (!cart.Items.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống để có thể thanh toán.";
                return RedirectToAction("Index");
            }

            // Chuẩn bị ViewBag cho các dropdown địa chỉ
            ViewBag.Districts = GetHCMCDistricts();
            ViewBag.Wards = GetWards();

            var order = new Order
            {
                TotalPrice = cart.GetTotalPrice()
            };

            return View(order);
        }

        // Bước 1 - POST: Xử lý thông tin giao hàng và chuyển đến bước 2 (Chọn thanh toán)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(Order order)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                // Nếu có lỗi, tải lại dropdowns và hiển thị lại form với lỗi
                ViewBag.Districts = GetHCMCDistricts();
                ViewBag.Wards = GetWards();
                return View(order);
            }

            // Lưu tạm thông tin Order vào TempData để dùng ở bước tiếp theo
            order.TotalPrice = cart.GetTotalPrice();
            TempData["PendingOrder"] = JsonConvert.SerializeObject(order);

            return RedirectToAction("PaymentMethod");
        }

        // Bước 2 - GET: Hiển thị trang chọn phương thức thanh toán
        [HttpGet]
        public IActionResult PaymentMethod()
        {
            var pendingOrderJson = TempData["PendingOrder"] as string;
            if (string.IsNullOrEmpty(pendingOrderJson))
            {
                return RedirectToAction("Checkout");
            }

            var order = JsonConvert.DeserializeObject<Order>(pendingOrderJson);
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");

            ViewBag.Cart = cart; // Gửi thông tin giỏ hàng qua ViewBag để hiển thị

            // Giữ lại TempData để dùng cho bước cuối cùng
            TempData.Keep("PendingOrder");

            return View(order);
        }

        // Bước 3 - POST: Xử lý cuối cùng, lưu đơn hàng vào DB
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessOrder(string paymentMethod)
        {
            var pendingOrderJson = TempData["PendingOrder"] as string;
            if (string.IsNullOrEmpty(pendingOrderJson))
            {
                return RedirectToAction("Checkout");
            }

            var order = JsonConvert.DeserializeObject<Order>(pendingOrderJson);
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);

            // Hoàn thiện thông tin đơn hàng
            order.UserId = user.Id;
            order.OrderDate = DateTime.UtcNow;
            order.TotalPrice = cart.GetTotalPrice();
            order.PaymentMethod = paymentMethod;
            order.OrderDetails = cart.Items.Select(i => new OrderDetail
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList();

            // Lưu vào cơ sở dữ liệu
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Xóa giỏ hàng sau khi đã đặt hàng thành công
            HttpContext.Session.Remove("Cart");

            return RedirectToAction("OrderCompleted", new { orderId = order.Id });
        }

        // Bước 4 - GET: Hiển thị trang hoàn tất đơn hàng
        [HttpGet]
        public async Task<IActionResult> OrderCompleted(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }
            return View(order);
        }

        #endregion

        #region Address Helper Methods
        // HÀM LẤY DỮ LIỆU ĐỊA CHỈ - Đã xóa các hàm trùng lặp
        private List<string> GetHCMCDistricts()
        {
            return new List<string>
            {
                "Quận 1", "Quận 2", "Quận 3", "Quận 4", "Quận 5", "Quận 6", "Quận 7",
                "Quận 8", "Quận 9", "Quận 10", "Quận 11", "Quận 12", "Quận Bình Tân",
                "Quận Bình Thạnh", "Quận Gò Vấp", "Quận Phú Nhuận", "Quận Tân Bình",
                "Quận Tân Phú", "Quận Thủ Đức", "Huyện Bình Chánh", "Huyện Cần Giờ",
                "Huyện Củ Chi", "Huyện Hóc Môn", "Huyện Nhà Bè"
            };
        }

        private Dictionary<string, List<string>> GetWards()
        {
            return new Dictionary<string, List<string>>
            {
                { "Quận 1", new List<string> { "Phường Bến Nghé", "Phường Bến Thành", "Phường Cầu Kho", "Phường Cầu Ông Lãnh", "Phường Cô Giang", "Phường Đa Kao", "Phường Nguyễn Cư Trinh", "Phường Nguyễn Thái Bình", "Phường Phạm Ngũ Lão", "Phường Tân Định" } },
                { "Quận 2", new List<string> { "Phường An Khánh", "Phường An Lợi Đông", "Phường An Phú", "Phường Bình An", "Phường Bình Khánh", "Phường Bình Trưng Đông", "Phường Bình Trưng Tây", "Phường Cát Lái", "Phường Thảo Điền", "Phường Thạnh Mỹ Lợi", "Phường Thủ Thiêm" } },
                { "Quận 3", new List<string> { "Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 5", "Phường 6", "Phường 7", "Phường 8", "Phường 9", "Phường 10", "Phường 11", "Phường 12", "Phường 13", "Phường 14" } },
                { "Quận 4", new List<string> { "Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 5", "Phường 6", "Phường 8", "Phường 9", "Phường 10", "Phường 13", "Phường 14", "Phường 15", "Phường 16", "Phường 18" } },
                { "Quận 5", new List<string> { "Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 5", "Phường 6", "Phường 7", "Phường 8", "Phường 9", "Phường 10", "Phường 11", "Phường 12", "Phường 13", "Phường 14" } },
                { "Quận 6", new List<string> { "Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 5", "Phường 6", "Phường 7", "Phường 8", "Phường 9", "Phường 10", "Phường 11", "Phường 12", "Phường 13", "Phường 14" } },
                { "Quận 7", new List<string> { "Phường Bình Thuận", "Phường Phú Mỹ", "Phường Phú Thuận", "Phường Tân Hưng", "Phường Tân Kiểng", "Phường Tân Phong", "Phường Tân Phú", "Phường Tân Quy", "Phường Tân Thuận Đông", "Phường Tân Thuận Tây" } },
                { "Quận 8", new List<string> { "Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 5", "Phường 6", "Phường 7", "Phường 8", "Phường 9", "Phường 10", "Phường 11", "Phường 12", "Phường 13", "Phường 14", "Phường 15", "Phường 16" } },
                { "Quận 9", new List<string> { "Phường Hiệp Phú", "Phường Long Bình", "Phường Long Phước", "Phường Long Thạnh Mỹ", "Phường Long Trường", "Phường Phú Hữu", "Phường Phước Bình", "Phường Phước Long A", "Phường Phước Long B", "Phường Tân Phú", "Phường Tăng Nhơn Phú A", "Phường Tăng Nhơn Phú B", "Phường Trường Thạnh" } },
                { "Quận 10", new List<string> { "Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 5", "Phường 6", "Phường 7", "Phường 8", "Phường 9", "Phường 10", "Phường 11", "Phường 12", "Phường 13", "Phường 14", "Phường 15" } },
                { "Quận 11", new List<string> { "Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 5", "Phường 6", "Phường 7", "Phường 8", "Phường 9", "Phường 10", "Phường 11", "Phường 12", "Phường 13", "Phường 14", "Phường 15", "Phường 16" } },
                { "Quận 12", new List<string> { "Phường An Phú Đông", "Phường Đông Hưng Thuận", "Phường Hiệp Thành", "Phường Tân Chánh Hiệp", "Phường Tân Hưng Thuận", "Phường Tân Thới Hiệp", "Phường Tân Thới Nhất", "Phường Thạnh Lộc", "Phường Thạnh Xuân", "Phường Thới An", "Phường Trung Mỹ Tây" } },
                { "Quận Bình Tân", new List<string> { "Phường An Lạc", "Phường An Lạc A", "Phường Bình Hưng Hòa", "Phường Bình Hưng Hòa A", "Phường Bình Hưng Hòa B", "Phường Bình Trị Đông", "Phường Bình Trị Đông A", "Phường Bình Trị Đông B", "Phường Tân Tạo", "Phường Tân Tạo A" } },
                { "Quận Bình Thạnh", new List<string> { "Phường 1", "Phường 2", "Phường 3", "Phường 5", "Phường 6", "Phường 7", "Phường 11", "Phường 12", "Phường 13", "Phường 14", "Phường 15", "Phường 17", "Phường 19", "Phường 21", "Phường 22", "Phường 24", "Phường 25", "Phường 26", "Phường 27", "Phường 28" } },
                { "Quận Gò Vấp", new List<string> { "Phường 1", "Phường 3", "Phường 4", "Phường 5", "Phường 6", "Phường 7", "Phường 8", "Phường 9", "Phường 10", "Phường 11", "Phường 12", "Phường 13", "Phường 14", "Phường 15", "Phường 16", "Phường 17" } },
                { "Quận Phú Nhuận", new List<string> { "Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 5", "Phường 7", "Phường 8", "Phường 9", "Phường 10", "Phường 11", "Phường 13", "Phường 15", "Phường 17" } },
                { "Quận Tân Bình", new List<string> { "Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 5", "Phường 6", "Phường 7", "Phường 8", "Phường 9", "Phường 10", "Phường 11", "Phường 12", "Phường 13", "Phường 14", "Phường 15" } },
                { "Quận Tân Phú", new List<string> { "Phường Hiệp Tân", "Phường Hòa Thạnh", "Phường Phú Thạnh", "Phường Phú Thọ Hòa", "Phường Phú Trung", "Phường Sơn Kỳ", "Phường Tân Quý", "Phường Tân Sơn Nhì", "Phường Tân Thành", "Phường Tây Thạnh" } },
                { "Quận Thủ Đức", new List<string> { "Phường Bình Chiểu", "Phường Bình Thọ", "Phường Hiệp Bình Chánh", "Phường Hiệp Bình Phước", "Phường Linh Chiểu", "Phường Linh Đông", "Phường Linh Tây", "Phường Linh Trung", "Phường Linh Xuân", "Phường Tam Bình", "Phường Tam Phú", "Phường Trường Thọ" } },
                { "Huyện Bình Chánh", new List<string> { "Thị trấn Tân Túc", "Xã An Phú Tây", "Xã Bình Chánh", "Xã Bình Hưng", "Xã Bình Lợi", "Xã Đa Phước", "Xã Hưng Long", "Xã Lê Minh Xuân", "Xã Phạm Văn Hai", "Xã Phong Phú", "Xã Quy Đức", "Xã Tân Kiên", "Xã Tân Nhựt", "Xã Tân Quý Tây", "Xã Vĩnh Lộc A", "Xã Vĩnh Lộc B" } },
                { "Huyện Cần Giờ", new List<string> { "Thị trấn Cần Thạnh", "Xã An Thới Đông", "Xã Bình Khánh", "Xã Long Hòa", "Xã Lý Nhơn", "Xã Tam Thôn Hiệp", "Xã Thạnh An" } },
                { "Huyện Củ Chi", new List<string> { "Thị trấn Củ Chi", "Xã An Nhơn Tây", "Xã An Phú", "Xã Bình Mỹ", "Xã Hòa Phú", "Xã Nhuận Đức", "Xã Phạm Văn Cội", "Xã Phú Hòa Đông", "Xã Phú Mỹ Hưng", "Xã Phước Hiệp", "Xã Phước Thạnh", "Xã Phước Vĩnh An", "Xã Tân An Hội", "Xã Tân Phú Trung", "Xã Tân Thạnh Đông", "Xã Tân Thạnh Tây", "Xã Tân Thông Hội", "Xã Thái Mỹ", "Xã Trung An", "Xã Trung Lập Hạ", "Xã Trung Lập Thượng" } },
                { "Huyện Hóc Môn", new List<string> { "Thị trấn Hóc Môn", "Xã Bà Điểm", "Xã Đông Thạnh", "Xã Nhị Bình", "Xã Tân Hiệp", "Xã Tân Thới Nhì", "Xã Tân Xuân", "Xã Thới Tam Thôn", "Xã Trung Chánh", "Xã Xuân Thới Đông", "Xã Xuân Thới Sơn", "Xã Xuân Thới Thượng" } },
                { "Huyện Nhà Bè", new List<string> { "Thị trấn Nhà Bè", "Xã Hiệp Phước", "Xã Long Thới", "Xã Nhơn Đức", "Xã Phú Xuân", "Xã Phước Kiển", "Xã Phước Lộc" } }
            };
        }
        #endregion
    }
}
