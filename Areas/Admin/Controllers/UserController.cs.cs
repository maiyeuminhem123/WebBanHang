// Areas/Admin/Controllers/UserController.cs
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bai3_WebBanHang.Models;
using Bai3_WebBanHang.Repositories; // <-- Thêm using này

namespace Bai3_WebBanHang.Areas.Admin.Controllers
{
    public class UserController : BaseAdminController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOrderRepository _orderRepository; // <-- Thêm repository

        // Cập nhật constructor
        public UserController(UserManager<ApplicationUser> userManager, IOrderRepository orderRepository)
        {
            _userManager = userManager;
            _orderRepository = orderRepository; // <-- Gán giá trị
        }

        // Action Index giữ nguyên
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Roles = roles
                });
            }
            return View(userViewModels);
        }

        // ===> THÊM ACTION DETAILS MỚI <===
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var userOrders = await _orderRepository.GetByUserIdAsync(user.Id);

            var viewModel = new UserDetailViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                LockoutEnd = user.LockoutEnd, // Ngày tạo
                LastLoginDate = user.LastLoginDate, // Lần cuối đăng nhập
                Roles = userRoles.ToList(),
                Orders = userOrders.OrderByDescending(o => o.OrderDate)
            };

            return View(viewModel);
        }
    }
}