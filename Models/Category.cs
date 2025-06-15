using System.Collections.Generic;

namespace Bai3_WebBanHang.Models;

public partial class Category
{
    public int Id { get; set; } // Sửa lại: Khóa chính không nên nullable

    public string Name { get; set; } = null!;

    public string? ImageUrl { get; set; } // <-- THÊM DÒNG NÀY VÀO

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}