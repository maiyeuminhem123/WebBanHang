using System;
using System.Collections.Generic;

namespace Bai3_WebBanHang.Models;

public partial class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;
    public decimal Price { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public int? CategoryId { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
}
