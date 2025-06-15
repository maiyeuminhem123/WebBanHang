// File: Repositories/IOrderRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using Bai3_WebBanHang.Models;

namespace Bai3_WebBanHang.Repositories
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetAllAsync();
        Task<IEnumerable<Order>> GetAllBasicAsync();
        Task<Order> GetByIdAsync(int id);
        Task<IEnumerable<Order>> GetByUserIdAsync(string userId);
        Task AddAsync(Order order);
        Task UpdateAsync(Order order);
        Task DeleteAsync(int id);
        Task DeleteAsync(Order order);
        Task DeleteRangeAsync(IEnumerable<int> ids);
        Task<Order?> FindByOrderCodeAsync(string orderCode);
    }
}