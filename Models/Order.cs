using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bai3_WebBanHang.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty; // Khởi tạo giá trị mặc định

        public DateTime OrderDate { get; set; }

        public decimal TotalPrice { get; set; }

        [Required(ErrorMessage = "Địa chỉ giao hàng là bắt buộc")]
        public string ShippingAddress { get; set; } = string.Empty; // Khởi tạo giá trị mặc định

        public string? Notes { get; set; } // Đánh dấu là nullable vì ghi chú không bắt buộc

        [Required(ErrorMessage = "Tên khách hàng là bắt buộc")]
        public string CustomerName { get; set; } = string.Empty; // Thêm thuộc tính

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string PhoneNumber { get; set; } = string.Empty; // Thêm thuộc tính

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty; // Thêm thuộc tính

        [ForeignKey("UserId")]
        [ValidateNever]
        public ApplicationUser? ApplicationUser { get; set; } // Đánh dấu là nullable
        public decimal TotalAmount { get; set; }
        public string? OrderStatus { get; set; } // Thêm dòng này vào
        public string? PaymentMethod { get; set; }
        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>(); // Khởi tạo danh sách
    }
}