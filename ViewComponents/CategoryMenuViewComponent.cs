using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Bai3_WebBanHang.Models;

public class CategoryMenuViewComponent : ViewComponent
{
    private readonly BanHangContext _context;

    public CategoryMenuViewComponent(BanHangContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var categories = await _context.Categories.ToListAsync();
        return View(categories);
    }
}