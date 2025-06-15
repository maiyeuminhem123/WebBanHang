using System.ComponentModel.DataAnnotations;

namespace Bai3_WebBanHang.Models
{
    public class Setting
    {
        [Key] // Đặt Key làm khóa chính
        public string Key { get; set; }

        public string? Value { get; set; }
    }
}