using Bai3_WebBanHang.Extensions;
using Bai3_WebBanHang.Models;
using Bai3_WebBanHang.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
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
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        }

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

            var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = productId,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = quantity
                });
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Index");
        }

        // Xóa sản phẩm khỏi giỏ hàng
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart != null)
            {
                cart.RemoveItem(productId);
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }
            return RedirectToAction("Index");
        }

        // Tăng số lượng sản phẩm
        public IActionResult IncreaseQuantity(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (cartItem != null)
            {
                cartItem.Quantity++;
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return Json(new
            {
                Quantity = cartItem?.Quantity ?? 0,
                TotalAmount = cart.Items.Sum(i => i.Price * i.Quantity)
            });
        }

        // Giảm số lượng sản phẩm
        public IActionResult DecreaseQuantity(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);

            if (cartItem != null)
            {
                if (cartItem.Quantity > 1)
                {
                    cartItem.Quantity--;
                }
                else
                {
                    cart.Items.Remove(cartItem);
                }
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return Json(new
            {
                success = true,
                Quantity = cartItem?.Quantity ?? 0,
                TotalAmount = cart.Items.Sum(i => i.Price * i.Quantity)
            });
        }

        // Cập nhật giỏ hàng
        [HttpPost]
        public IActionResult UpdateCart(int productId, string action)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            var item = cart.Items.FirstOrDefault(p => p.ProductId == productId);

            if (item != null)
            {
                if (action == "increase")
                {
                    item.Quantity++;
                }
                else if (action == "decrease" && item.Quantity > 1)
                {
                    item.Quantity--;
                }
                else if (action == "remove" || (action == "decrease" && item.Quantity == 1))
                {
                    cart.Items.Remove(item);
                }
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return Json(new
            {
                success = true,
                quantity = item?.Quantity ?? 0,
                total = cart.Items.Sum(i => i.Price * i.Quantity).ToString("#,##0") + " VND"
            });
        }

        // Bước 1: Hiển thị form điền thông tin giao hàng
        [HttpGet]
        public IActionResult Checkout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            if (!cart.Items.Any())
            {
                TempData["Error"] = "Giỏ hàng đang trống.";
                return RedirectToAction("Index");
            }

            var order = new Order
            {
                TotalAmount = cart.Items.Sum(i => i.Price * i.Quantity)
            };

            ViewBag.Districts = GetHCMCDistricts();
            ViewBag.Wards = GetWards();

            return View(order);
        }

        // Bước 1: Xử lý thông tin giao hàng và chuyển sang chọn phương thức thanh toán
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(Order order, string province, string district, string ward, string street)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || !cart.Items.Any())
            {
                TempData["Error"] = "Giỏ hàng đang trống.";
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Districts = GetHCMCDistricts();
                ViewBag.Wards = GetWards();
                return View(order);
            }

            // Kết hợp địa chỉ từ các trường
            order.ShippingAddress = $"{street}, {ward}, {district}, {province}";
            order.TotalAmount = cart.Items.Sum(i => i.Price * i.Quantity);

            // Lưu tạm thông tin order vào TempData
            TempData["PendingOrder"] = JsonConvert.SerializeObject(order);

            return RedirectToAction("PaymentMethod");
        }

        // Bước 2: Hiển thị chọn phương thức thanh toán
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
            ViewBag.Cart = cart;
            ViewBag.PaymentMethods = GetActivePaymentMethods();

            TempData.Keep("PendingOrder");
            return View(order);
        }

        // Bước 2: Nhận phương thức thanh toán và chuyển sang xác nhận đơn hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PaymentMethod(string paymentMethod)
        {
            var pendingOrderJson = TempData["PendingOrder"] as string;
            if (string.IsNullOrEmpty(pendingOrderJson))
            {
                return RedirectToAction("Checkout");
            }

            var order = JsonConvert.DeserializeObject<Order>(pendingOrderJson);
            order.PaymentMethod = paymentMethod;
            TempData["PendingOrder"] = JsonConvert.SerializeObject(order);

            return RedirectToAction("ConfirmOrder");
        }

        // Bước 3: Hiển thị xác nhận đơn hàng
        [HttpGet]
        public IActionResult ConfirmOrder()
        {
            var pendingOrderJson = TempData["PendingOrder"] as string;
            if (string.IsNullOrEmpty(pendingOrderJson))
            {
                return RedirectToAction("Checkout");
            }

            var order = JsonConvert.DeserializeObject<Order>(pendingOrderJson);
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            ViewBag.Cart = cart;

            TempData.Keep("PendingOrder");
            return View(order);
        }

        // Bước 3: Lưu đơn hàng vào DB
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessOrder()
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
            if (user == null) return Unauthorized();

            order.UserId = user.Id;
            order.OrderDate = DateTime.UtcNow;
            order.TotalAmount = cart.Items.Sum(i => i.Price * i.Quantity);
            order.OrderDetails = cart.Items.Select(i => new OrderDetail
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                Price = i.Price
            }).ToList();
            order.OrderCode = "DH" + DateTime.Now.ToString("yyMMddHHmmss") + new Random().Next(10, 99);
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            HttpContext.Session.Remove("Cart");

            return RedirectToAction("OrderCompleted", new { orderId = order.Id });
        }

        // Bước 4: Hiển thị trang hoàn tất đơn hàng
        [HttpGet]
        public async Task<IActionResult> OrderCompleted(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng.";
                return RedirectToAction("Index");
            }

            return View(order);
        }

        // Lấy danh sách phương thức thanh toán đang hoạt động
        private List<PaymentMethod> GetActivePaymentMethods()
        {
            return _context.PaymentMethods.Where(pm => pm.IsActive).ToList();
        }

        // Lấy danh sách Quận/Huyện của TP.HCM
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

        // Lấy danh sách Phường/Xã theo Quận/Huyện
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
                { "Quận Tân Phú", new List<string> { "Phường Hiệp Tân", "Phường Hòa Thạnh", "Phường Phú Thạnh", "Phường Phú Thọ Hòa", "Phường Phú Trung", "Phường Sơn Kỳ", "Phường Tân Quý", "Phường Tân Sơn Nhì", "Phường Tân Thành", "Phường Tây Thạnh", "Phường Trung Mỹ Tây" } },
                { "Quận Thủ Đức", new List<string> { "Phường Bình Chiểu", "Phường Bình Thọ", "Phường Hiệp Bình Chánh", "Phường Hiệp Bình Phước", "Phường Linh Chiểu", "Phường Linh Đông", "Phường Linh Tây", "Phường Linh Trung", "Phường Linh Xuân", "Phường Tam Bình", "Phường Tam Phú", "Phường Trường Thọ" } },
                { "Huyện Bình Chánh", new List<string> { "Xã An Phú Tây", "Xã Bình Chánh", "Xã Bình Hưng", "Xã Bình Lợi", "Xã Đa Phước", "Xã Hưng Long", "Xã Lê Minh Xuân", "Xã Phong Phú", "Xã Quy Đức", "Xã Tân Kiên", "Xã Tân Nhựt", "Xã Tân Quý Tây", "Xã Vĩnh Lộc A", "Xã Vĩnh Lộc B" } },
                { "Huyện Cần Giờ", new List<string> { "Thị trấn Cần Thạnh", "Xã An Thới Đông", "Xã Bình Khánh", "Xã Long Hòa", "Xã Lý Nhơn", "Xã Tam Thôn Hiệp", "Xã Thạnh An" } },
                { "Huyện Củ Chi", new List<string> { "Thị trấn Củ Chi", "Xã An Nhơn Tây", "Xã An Phú", "Xã Bình Mỹ", "Xã Hòa Phú", "Xã Nhuận Đức", "Xã Phạm Văn Cội", "Xã Phú Hòa Đông", "Xã Phú Mỹ Hưng", "Xã Phước Hiệp", "Xã Phước Thạnh", "Xã Phước Vĩnh An", "Xã Tân An Hội", "Xã Tân Phú Trung", "Xã Tân Thạnh Đông", "Xã Tân Thạnh Tây", "Xã Tân Thông Hội", "Xã Thái Mỹ", "Xã Trung An", "Xã Trung Lập Hạ", "Xã Trung Lập Thượng" } },
                { "Huyện Hóc Môn", new List<string> { "Thị trấn Hóc Môn", "Xã Bà Điểm", "Xã Đông Thạnh", "Xã Nhị Bình", "Xã Tân Hiệp", "Xã Tân Thới Nhì", "Xã Tân Xuân", "Xã Thới Tam Thôn", "Xã Trung Chánh", "Xã Xuân Thới Đông", "Xã Xuân Thới Sơn", "Xã Xuân Thới Thượng" } },
                { "Huyện Nhà Bè", new List<string> { "Thị trấn Nhà Bè", "Xã Hiệp Phước", "Xã Long Thới", "Xã Nhơn Đức", "Xã Phú Xuân", "Xã Phước Kiển", "Xã Phước Lộc" } }
            };
        }
    }
}
