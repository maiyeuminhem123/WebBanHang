using Bai3_WebBanHang.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bai3_WebBanHang.Models
{
    public partial class BanHangContext : IdentityDbContext<ApplicationUser>
    {
        public BanHangContext(DbContextOptions<BanHangContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<ProductImage> ProductImages { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }
        public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Đặt tên bảng
            modelBuilder.Entity<Category>().ToTable("Categories");
            modelBuilder.Entity<Product>().ToTable("Products");
            modelBuilder.Entity<ProductImage>().ToTable("ProductImages");
            modelBuilder.Entity<Order>().ToTable("Orders");
            modelBuilder.Entity<OrderDetail>().ToTable("OrderDetails");
            modelBuilder.Entity<PaymentMethod>().ToTable("PaymentMethods");

            // Định nghĩa khóa chính cho IdentityUserLogin
            modelBuilder.Entity<IdentityUserLogin<string>>()
                .HasKey(l => new { l.LoginProvider, l.ProviderKey });

            // CATEGORY
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_Categories");
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50); // Tên danh mục ngắn gọn
                entity.HasIndex(e => e.Name).IsUnique(); // Chỉ mục duy nhất cho tìm kiếm
            });

            // PRODUCT
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_Products");
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50); // Tên sản phẩm ngắn gọn
                entity.Property(e => e.Price)
                    .IsRequired()
                    .HasColumnType("decimal(10, 2)"); // Giá nhỏ gọn
                entity.Property(e => e.Description)
                    .HasMaxLength(500); // Mô tả ngắn
                entity.Property(e => e.ImageUrl)
                    .HasMaxLength(200); // URL ảnh gọn
                entity.HasIndex(e => e.Name); // Chỉ mục cho tìm kiếm sản phẩm
                entity.HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_Products_CategoryId");
            });

            // PRODUCT IMAGE
            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_ProductImages");
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Url)
                    .IsRequired()
                    .HasMaxLength(200); // URL ảnh gọn
                entity.HasOne(pi => pi.Product)
                    .WithMany(p => p.ProductImages)
                    .HasForeignKey(pi => pi.ProductId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_ProductImages_ProductId");
            });

            // ORDER
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_Orders");
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.UserId)
                    .IsRequired()
                    .HasMaxLength(450); // Đồng bộ với AspNetUsers
                entity.Property(e => e.OrderDate)
                    .IsRequired()
                    .HasColumnType("date"); // Chỉ lưu ngày
                entity.Property(e => e.TotalPrice)
                    .IsRequired()
                    .HasColumnType("decimal(10, 2)");
                entity.Property(e => e.TotalAmount)
                    .IsRequired()
                    .HasColumnType("decimal(10, 2)");
                entity.Property(e => e.ShippingAddress)
                    .IsRequired()
                    .HasMaxLength(200); // Địa chỉ gọn
                entity.Property(e => e.CustomerName)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.PhoneNumber)
                    .IsRequired()
                    .HasMaxLength(15); // Số điện thoại ngắn
                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(50); // Email ngắn
                entity.Property(e => e.Notes)
                    .HasMaxLength(200); // Ghi chú ngắn
                entity.HasIndex(e => e.OrderDate); // Chỉ mục cho tìm kiếm theo ngày
                entity.HasIndex(e => e.UserId); // Chỉ mục cho tìm kiếm theo UserId
                entity.HasOne(o => o.ApplicationUser)
                    .WithMany()
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_Orders_UserId");
            });

            // ORDER DETAIL
            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_OrderDetails");
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Quantity)
                    .IsRequired();
                entity.Property(e => e.Price)
                    .IsRequired()
                    .HasColumnType("decimal(10, 2)");
                entity.HasOne(od => od.Order)
                    .WithMany(o => o.OrderDetails)
                    .HasForeignKey(od => od.OrderId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_OrderDetails_OrderId");
                entity.HasOne(od => od.Product)
                    .WithMany()
                    .HasForeignKey(od => od.ProductId)
                    .OnDelete(DeleteBehavior.NoAction)
                    .HasConstraintName("FK_OrderDetails_ProductId");
            });

            // PAYMENT METHOD
            modelBuilder.Entity<PaymentMethod>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK_PaymentMethods");
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(50);
                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(20); // Mã ngắn như "COD"
                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);
                entity.HasIndex(e => e.Code).IsUnique(); // Chỉ mục duy nhất cho mã
            });

            // Bỏ qua CartItem
            modelBuilder.Ignore<CartItem>();

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}