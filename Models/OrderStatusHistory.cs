using System;
using System.ComponentModel.DataAnnotations;

namespace Bai3_WebBanHang.Models
{
    public class OrderStatusHistory
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order? Order { get; set; } // Nullable
        public string Status { get; set; } = string.Empty; // Initialized to empty string
        public DateTime UpdatedAt { get; set; }
        public string? Notes { get; set; }
    }
}
