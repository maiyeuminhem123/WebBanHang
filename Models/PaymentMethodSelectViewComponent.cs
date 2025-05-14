using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bai3_WebBanHang.Models;

public class PaymentMethodSelectViewComponent : ViewComponent
{
    private readonly BanHangContext _context;

    public PaymentMethodSelectViewComponent(BanHangContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var paymentMethods = await _context.PaymentMethods
            .Where(pm => pm.IsActive)
            .ToListAsync();
        return View(paymentMethods ?? new List<PaymentMethod>());
    }
}