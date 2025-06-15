using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bai3_WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // Yêu cầu đăng nhập VÀ phải có vai trò "Admin"
    public class BaseAdminController : Controller
    {
        // Lớp này để trống, mục đích chính là để mang các attribute bảo vệ ở trên
    }
}