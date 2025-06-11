namespace Bai3_WebBanHang.Models
{
    public class ShoppingCart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        /// <summary>
        /// Thêm sản phẩm vào giỏ hàng hoặc tăng số lượng nếu đã tồn tại.
        /// </summary>
        /// <param name="product">Sản phẩm cần thêm (lấy từ database).</param>
        /// <param name="quantity">Số lượng cần thêm.</param>
        public void AddItem(Product product, int quantity = 1)
        {
            var existingItem = Items.FirstOrDefault(i => i.ProductId == product.Id);

            if (existingItem != null)
            {
                // Nếu sản phẩm đã có trong giỏ, chỉ cần tăng số lượng
                existingItem.Quantity += quantity;
            }
            else
            {
                // Nếu sản phẩm chưa có, tạo một CartItem mới và thêm vào giỏ
                Items.Add(new CartItem
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Quantity = quantity
                    // Giả sử CartItem có thuộc tính ImageUrl, bạn có thể thêm ở đây
                    // ImageUrl = product.ImageUrl 
                });
            }
        }

        /// <summary>
        /// Xóa một sản phẩm khỏi giỏ hàng dựa trên ProductId.
        /// </summary>
        /// <param name="productId">ID của sản phẩm cần xóa.</param>
        public void RemoveItem(int productId)
        {
            Items.RemoveAll(i => i.ProductId == productId);
        }

        /// <summary>
        /// Cập nhật số lượng cho một sản phẩm cụ thể.
        /// </summary>
        /// <param name="productId">ID của sản phẩm cần cập nhật.</param>
        /// <param name="quantity">Số lượng mới.</param>
        public void UpdateQuantity(int productId, int quantity)
        {
            var item = Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                if (quantity > 0)
                {
                    item.Quantity = quantity;
                }
                else
                {
                    // Nếu số lượng mới là 0 hoặc âm, xóa sản phẩm khỏi giỏ
                    RemoveItem(productId);
                }
            }
        }

        /// <summary>
        /// Lấy tổng số lượng của tất cả các mặt hàng trong giỏ.
        /// </summary>
        public int GetTotalQuantity()
        {
            return Items.Sum(i => i.Quantity);
        }

        /// <summary>
        /// Tính tổng thành tiền của giỏ hàng.
        /// </summary>
        public decimal GetTotalPrice()
        {
            return Items.Sum(i => i.Price * i.Quantity);
        }

        /// <summary>
        /// Dọn dẹp, xóa sạch tất cả sản phẩm trong giỏ hàng.
        /// </summary>
        public void ClearCart()
        {
            Items.Clear();
        }
    }
}