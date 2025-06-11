using Bai3_WebBanHang.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bai3_WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly BanHangContext _context;

        public OrderController(BanHangContext context)
        {
            _context = context;
        }

        // GET: Admin/Order - Hiển thị danh sách đơn hàng
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                                       .OrderByDescending(o => o.OrderDate)
                                       .ToListAsync();
            return View(orders);
        }

        // GET: Admin/Order/Details/5 - Xem chi tiết đơn hàng
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }

        // POST: Admin/Order/UpdateStatus - Cập nhật trạng thái
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
            }

            // Có thể thêm logic kiểm tra chuyển đổi trạng thái hợp lệ ở đây
            // Ví dụ: không thể hủy đơn hàng đã giao

            order.OrderStatus = status;
            _context.Update(order);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"Đã cập nhật trạng thái đơn hàng thành '{status}'." });
        }

        // POST: Admin/Order/UpdateSelectedStatus - Cập nhật trạng thái cho nhiều đơn hàng
        [HttpPost]
        public async Task<IActionResult> UpdateSelectedStatus(int[] ids, string status)
        {
            if (ids == null || !ids.Any())
            {
                return Json(new { success = false, message = "Vui lòng chọn ít nhất một đơn hàng." });
            }

            var orders = await _context.Orders.Where(o => ids.Contains(o.Id)).ToListAsync();

            foreach (var order in orders)
            {
                // Chỉ cập nhật nếu trạng thái hợp lệ để thay đổi
                // Ví dụ: chỉ xác nhận các đơn hàng đang "Chờ xử lý"
                if (status == "Đã xác nhận" && order.OrderStatus == "Đang chờ xử lý")
                {
                    order.OrderStatus = status;
                }
                else if (status == "Đã hủy") // Có thể hủy ở nhiều trạng thái
                {
                    order.OrderStatus = status;
                }
                // Thêm các điều kiện khác nếu cần
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"Đã cập nhật trạng thái cho {orders.Count} đơn hàng được chọn." });
        }


        // POST: Admin/Order/Delete/5 - Xóa một đơn hàng
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng." });
            }

            var orderDetails = _context.OrderDetails.Where(d => d.OrderId == id);
            _context.OrderDetails.RemoveRange(orderDetails);
            _context.Orders.Remove(order);

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Đã xóa đơn hàng thành công." });
        }

        // POST: Admin/Order/DeleteSelected - Xóa nhiều đơn hàng
        [HttpPost]
        public async Task<IActionResult> DeleteSelected(int[] ids)
        {
            if (ids == null || !ids.Any())
            {
                return Json(new { success = false, message = "Vui lòng chọn ít nhất một đơn hàng để xóa." });
            }

            var orders = await _context.Orders.Where(o => ids.Contains(o.Id)).ToListAsync();
            var orderDetails = await _context.OrderDetails.Where(d => ids.Contains(d.OrderId)).ToListAsync();

            _context.OrderDetails.RemoveRange(orderDetails);
            _context.Orders.RemoveRange(orders);

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"Đã xóa {orders.Count} đơn hàng được chọn." });
        }
    }
}
