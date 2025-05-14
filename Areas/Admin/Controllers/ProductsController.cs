using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Bai3_WebBanHang.Models;
using Microsoft.AspNetCore.Authorization;
using Bai3_WebBanHang.Repositories;

namespace Bai3_WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly BanHangContext _context;

        public ProductsController(BanHangContext context)
        {
            _context = context;
        }

        // Index: Cho phép tất cả người dùng (bao gồm chưa đăng nhập)
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var banHangContext = _context.Products.Include(p => p.Category);
            ViewBag.Categories = await _context.Categories.ToListAsync();
            return View(await banHangContext.ToListAsync());
        }

        // Details: Cho phép tất cả người dùng (bao gồm chưa đăng nhập)
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // Create: Cho phép Admin và Employee
        [Authorize(Roles = "Admin,Employee")]
        public IActionResult Create()
        {
            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        // Create POST: Cho phép Admin và Employee
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Create([Bind("Name,Price,Description")] Product product, string CategoryName, IFormFile MainImage, IFormFile[] DetailImages)
        {
            if (ModelState.IsValid)
            {
                var category = await _context.Categories.FirstOrDefaultAsync(c => c.Name == CategoryName);
                if (category == null)
                {
                    category = new Category { Name = CategoryName };
                    _context.Categories.Add(category);
                    await _context.SaveChangesAsync();
                }
                product.CategoryId = category.Id;

                if (MainImage != null && MainImage.Length > 0)
                {
                    product.ImageUrl = await SaveImage(MainImage);
                }

                _context.Add(product);
                await _context.SaveChangesAsync();

                if (DetailImages != null && DetailImages.Length > 0)
                {
                    foreach (var image in DetailImages)
                    {
                        if (image != null && image.Length > 0 && image.ContentType.StartsWith("image/"))
                        {
                            var imageUrl = await SaveImage(image);
                            _context.ProductImages.Add(new ProductImage { ProductId = product.Id, Url = imageUrl });
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name");
            return View(product);
        }

        // Edit: Cho phép Admin và Employee
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // Edit POST: Cho phép Admin và Employee
        // Edit POST: Cho phép Admin và Employee
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price,Description,CategoryId")] Product product, IFormFile ProductImage, IFormFile[] DetailImages, int[] DeleteImageIds)
        {
            if (id != product.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy sản phẩm hiện tại từ database để giữ lại ImageUrl nếu không upload ảnh mới
                    var existingProduct = await _context.Products
                        .Include(p => p.ProductImages)
                        .FirstOrDefaultAsync(p => p.Id == id);

                    if (existingProduct == null) return NotFound();

                    // Cập nhật thông tin cơ bản của sản phẩm
                    existingProduct.Name = product.Name;
                    existingProduct.Price = product.Price;
                    existingProduct.Description = product.Description;
                    existingProduct.CategoryId = product.CategoryId;

                    // Cập nhật ảnh chính nếu có
                    if (ProductImage != null && ProductImage.Length > 0)
                    {
                        existingProduct.ImageUrl = await SaveImage(ProductImage);
                    }

                    // Xóa ảnh chi tiết được chọn
                    if (DeleteImageIds != null && DeleteImageIds.Any())
                    {
                        var imagesToDelete = existingProduct.ProductImages
                            .Where(img => DeleteImageIds.Contains(img.Id))
                            .ToList();

                        foreach (var image in imagesToDelete)
                        {
                            _context.ProductImages.Remove(image);
                            // (Tùy chọn) Xóa file ảnh trên server nếu cần
                            // var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.Url.TrimStart('/'));
                            // if (System.IO.File.Exists(filePath)) System.IO.File.Delete(filePath);
                        }
                    }

                    // Thêm ảnh chi tiết mới
                    if (DetailImages != null && DetailImages.Length > 0)
                    {
                        foreach (var image in DetailImages)
                        {
                            if (image != null && image.Length > 0 && image.ContentType.StartsWith("image/"))
                            {
                                var imageUrl = await SaveImage(image);
                                _context.ProductImages.Add(new ProductImage { ProductId = product.Id, Url = imageUrl });
                            }
                        }
                    }

                    _context.Update(existingProduct);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // Delete: Chỉ cho phép Admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null) return NotFound();

            return View(product);
        }

        // Delete POST: Chỉ cho phép Admin
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            return "/images/" + fileName;
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}