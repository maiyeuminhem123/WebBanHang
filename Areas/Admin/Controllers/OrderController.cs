// File: Areas/Admin/Controllers/OrderController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bai3_WebBanHang.Models;
using Bai3_WebBanHang.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bai3_WebBanHang.Areas.Admin.Controllers
{
    public class OrderController : BaseAdminController
    {
        private readonly IOrderRepository _orderRepository;

        public static readonly Dictionary<string, string> StatusDisplayNames = new Dictionary<string, string>
        {
            { "Pending", "Đang chờ thanh toán" },
            { "Confirmed", "Đã xác nhận" },
            { "Preparing", "Chuẩn bị hàng" },
            { "Shipping", "Đang giao hàng" },
            { "Delivered", "Đã giao thành công" },
            { "Cancelled", "Đã hủy" }
        };

        public OrderController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            var orders = await _orderRepository.GetAllBasicAsync();
            var totalCount = orders.Count();
            var pagedOrders = orders
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.StatusDisplayNames = StatusDisplayNames;
            return View(pagedOrders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) return NotFound();
            ViewBag.StatusDisplayNames = StatusDisplayNames;
            return View(order);
        }

        // Trong file Areas/Admin/Controllers/OrderController.cs

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
            }

            // BƯỚC DỊCH NGƯỢC ĐÃ ĐƯỢC BỎ ĐI - GIỜ 'status' CHÍNH LÀ MÃ TIẾNG ANH

            // Kiểm tra xem trạng thái mới có hợp lệ không
            if (!StatusDisplayNames.ContainsKey(status))
            {
                return Json(new { success = false, message = $"Trạng thái '{status}' không hợp lệ." });
            }

            // Kiểm tra logic chuyển đổi trạng thái
            if (!IsTransitionAllowed(order.OrderStatus, status))
            {
                return Json(new { success = false, message = $"Không thể chuyển trạng thái từ '{StatusDisplayNames.GetValueOrDefault(order.OrderStatus ?? "")}' sang '{StatusDisplayNames.GetValueOrDefault(status)}'." });
            }

            // Cập nhật trạng thái và lịch sử
            order.OrderStatus = status;
            order.StatusHistory.Add(new OrderStatusHistory
            {
                Status = status,
                Notes = "Admin đã cập nhật.",
                UpdatedAt = DateTime.Now
            });

            await _orderRepository.UpdateAsync(order);
            return Json(new { success = true, message = "Cập nhật trạng thái thành công.", newStatus = status });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
            }
            await _orderRepository.DeleteAsync(id);
            return Json(new { success = true, message = "Đã xóa đơn hàng thành công." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSelected(int[] ids)
        {
            if (ids == null || !ids.Any())
            {
                return Json(new { success = false, message = "Vui lòng chọn ít nhất một đơn hàng để xóa." });
            }
            await _orderRepository.DeleteRangeAsync(ids);
            return Json(new { success = true, message = $"Đã xóa {ids.Length} đơn hàng được chọn." });
        }

        private bool IsTransitionAllowed(string? fromStatus, string toStatus)
        {
            if (string.IsNullOrEmpty(fromStatus)) return false;
            return (fromStatus, toStatus) switch
            {
                ("Pending", "Confirmed") => true,
                ("Confirmed", "Preparing") => true,  
                ("Preparing", "Shipping") => true,    
                ("Shipping", "Delivered") => true,
                ("Pending", "Cancelled") => true,
                ("Confirmed", "Cancelled") => true,
                _ => false
            };
        }
    }
}