namespace Bai3_WebBanHang.Models
{
    public class PaymentMethod
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Tên phương thức thanh toán (ví dụ: "Thanh toán khi nhận hàng")
        public string Code { get; set; } = string.Empty; // Mã phương thức (ví dụ: "COD", "EWallet")
        public bool IsActive { get; set; } = true; // Trạng thái hoạt động
    }
}