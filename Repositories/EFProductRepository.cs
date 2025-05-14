using Microsoft.EntityFrameworkCore;
using Bai3_WebBanHang.Models;

namespace Bai3_WebBanHang.Repositories
{
    public class EFProductRepository : IProductRepository
    {
        private readonly BanHangContext _context;
        public int id { get; set; }
        public EFProductRepository(BanHangContext context)
        {
            _context = context;
            id = 0;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products
                .Include(p => p.Category)
                .ToListAsync();
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id) ?? throw new InvalidOperationException("Product not found");
        }

        public async Task AddAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product); // Code này đã được sửa
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new InvalidOperationException("Product not found");
            }
        }
    }
}