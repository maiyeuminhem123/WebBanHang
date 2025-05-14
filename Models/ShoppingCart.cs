namespace Bai3_WebBanHang.Models
{
    public class ShoppingCart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        // ✅ Thêm sản phẩm vào giỏ
        public void AddItem(CartItem item)
        {
            var existingItem = Items.FirstOrDefault(i => i.ProductId == item.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                Items.Add(item);
            }
        }

        // ✅ Xóa sản phẩm khỏi giỏ
        public void RemoveItem(int productId)
        {
            Items.RemoveAll(i => i.ProductId == productId);
        }

        // ✅ Cập nhật số lượng sản phẩm trong giỏ
        public void UpdateQuantity(int productId, int quantity)
        {
            var item = Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                if (quantity > 0)
                    item.Quantity = quantity;
                else
                    Items.Remove(item); // Nếu số lượng = 0 thì xóa luôn
            }
        }

        // ✅ Tính tổng số lượng sản phẩm trong giỏ
        public int GetTotalQuantity()
        {
            return Items.Sum(i => i.Quantity);
        }

        // ✅ Tính tổng giá trị giỏ hàng
        public decimal GetTotalPrice()
        {
            return Items.Sum(i => i.Quantity * i.Price);
        }

        // ✅ Xóa toàn bộ giỏ hàng
        public void ClearCart()
        {
            Items.Clear();
        }
    }

}
