using Bai3_WebBanHang.Models;
using Microsoft.AspNetCore.Mvc;

namespace Bai3_WebBanHang.Controllers
{
    public class CheckoutController : Controller
    {
        public IActionResult Index()
        {
            return View(); // Trả về trang Checkout.cshtml
        }

        [HttpPost]
        public IActionResult Checkout(Order order)
        {
            if (ModelState.IsValid)
            {
                
                return RedirectToAction("Success"); // Điều hướng đến trang xác nhận thành công
            }
            return View("Index", order);
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}
