// Services/InactiveUserCleanupService.cs
using Microsoft.AspNetCore.Identity;
using Bai3_WebBanHang.Models;

namespace Bai3_WebBanHang.Services;

public class InactiveUserCleanupService : BackgroundService
{
    private readonly ILogger<InactiveUserCleanupService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public InactiveUserCleanupService(ILogger<InactiveUserCleanupService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Inactive User Cleanup Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Cleanup task is running.");

            await CleanupInactiveUsers();

            // Chờ 1 ngày trước khi chạy lại
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }

    private async Task CleanupInactiveUsers()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Ngưỡng thời gian: 300 ngày trước
            var threshold = DateTime.UtcNow.AddDays(-300);

            // Tìm các user không hoạt động (lần cuối đăng nhập cũ hơn ngưỡng)
            // và chưa bao giờ đăng nhập (LastLoginDate = null) nhưng ngày tạo đã cũ
            var inactiveUsers = userManager.Users
                .Where(u => u.LastLoginDate < threshold || (u.LastLoginDate == null && u.LockoutEnd < threshold))
                .ToList();

            if (!inactiveUsers.Any())
            {
                _logger.LogInformation("No inactive users found to delete.");
                return;
            }

            foreach (var user in inactiveUsers)
            {
                // QUAN TRỌNG: Không bao giờ xóa tài khoản Admin
                var isSuperAdmin = await userManager.IsInRoleAsync(user, "Admin");
                if (isSuperAdmin)
                {
                    _logger.LogWarning($"Skipping deletion of admin user: {user.UserName}");
                    continue;
                }

                _logger.LogInformation($"Deleting inactive user: {user.UserName}");
                var result = await userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    _logger.LogError($"Failed to delete user {user.UserName}. Errors: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}