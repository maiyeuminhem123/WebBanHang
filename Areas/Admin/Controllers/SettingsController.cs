using Bai3_WebBanHang.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Bai3_WebBanHang.Areas.Admin.Controllers
{
    public class SettingsController : BaseAdminController
    {
        private readonly BanHangContext _context;

        public SettingsController(BanHangContext context)
        {
            _context = context;
        }

        // GET: Hiển thị trang cài đặt
        public async Task<IActionResult> Index()
        {
            var settings = await _context.Settings.ToDictionaryAsync(s => s.Key, s => s.Value);
            return View(settings);
        }

        // POST: Lưu các thay đổi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Dictionary<string, string> settings)
        {
            foreach (var (key, value) in settings)
            {
                var setting = await _context.Settings.FindAsync(key);
                if (setting != null)
                {
                    setting.Value = value;
                }
                else
                {
                    _context.Settings.Add(new Setting { Key = key, Value = value });
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã lưu cài đặt thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
