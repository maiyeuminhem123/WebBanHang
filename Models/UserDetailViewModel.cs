// Models/UserDetailViewModel.cs
using System.Collections.Generic;

namespace Bai3_WebBanHang.Models
{
    public class UserDetailViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; } // Ngày tạo tài khoản
        public DateTime? LastLoginDate { get; set; } // Lần cuối đăng nhập
        public List<string> Roles { get; set; }
        public IEnumerable<Order> Orders { get; set; }
    }
}