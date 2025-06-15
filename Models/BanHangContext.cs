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
        public virtual DbSet<CartItem> CartItems { get; set; } // Bỏ qua trong OnModelCreating
        public virtual DbSet<OrderStatusHistory> OrderStatusHistories { get; set; } // Thêm bảng lịch sử trạng thái đơn hàng
        public virtual DbSet<Setting> Settings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Định nghĩa khóa chính cho các bảng Identity (giữ nguyên)
            modelBuilder.Entity<IdentityUserLogin<string>>().HasKey(l => new { l.LoginProvider, l.ProviderKey });

            // CATEGORY
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");
                entity.HasKey(e => e.Id).HasName("PK_Categories");
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            // PRODUCT
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(e => e.Id).HasName("PK_Products");
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Price).IsRequired().HasColumnType("decimal(18, 2)");
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.ImageUrl).HasMaxLength(200);

                entity.HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // PRODUCT IMAGE
            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.ToTable("ProductImages");
                entity.HasKey(e => e.Id).HasName("PK_ProductImages");
                entity.Property(e => e.Url).IsRequired().HasMaxLength(200);

                entity.HasOne(pi => pi.Product)
                    .WithMany(p => p.ProductImages)
                    .HasForeignKey(pi => pi.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ORDER
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Orders");
                entity.HasKey(e => e.Id).HasName("PK_Orders");
                entity.Property(e => e.TotalAmount).IsRequired().HasColumnType("decimal(18, 2)");
                entity.Property(e => e.ShippingAddress).IsRequired().HasMaxLength(200);
                entity.Property(e => e.CustomerName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(15);
                entity.Property(e => e.Email).HasMaxLength(50);
                entity.Property(e => e.Notes).HasMaxLength(200);
                entity.Property(e => e.OrderStatus).IsRequired().HasMaxLength(50).HasDefaultValue("Pending");

                entity.HasOne(o => o.ApplicationUser)
                    .WithMany()
                    .HasForeignKey(o => o.UserId)
                    .OnDelete(DeleteBehavior.Restrict); // Không cho xóa User nếu còn Order

                // <== CẤU HÌNH CASCADE DELETE ĐƯỢC GOM VÀO ĐÂY ==>
                // Khi xóa Order, tự động xóa các OrderDetail liên quan
                entity.HasMany(o => o.OrderDetails)
                      .WithOne(od => od.Order)
                      .HasForeignKey(od => od.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Khi xóa Order, tự động xóa các OrderStatusHistory liên quan
                entity.HasMany(o => o.StatusHistory)
                      .WithOne(sh => sh.Order)
                      .HasForeignKey(sh => sh.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ORDER DETAIL
            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.ToTable("OrderDetails");
                entity.HasKey(e => e.Id).HasName("PK_OrderDetails");
                entity.Property(e => e.Price).IsRequired().HasColumnType("decimal(18, 2)");

                // Không cho xóa Product nếu nó đang nằm trong một OrderDetail
                entity.HasOne(od => od.Product)
                    .WithMany()
                    .HasForeignKey(od => od.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ORDER STATUS HISTORY
            modelBuilder.Entity<OrderStatusHistory>(entity =>
            {
                entity.ToTable("OrderStatusHistories");
                entity.HasKey(h => h.Id);
            });

            // PAYMENT METHOD
            modelBuilder.Entity<PaymentMethod>(entity =>
            {
                entity.ToTable("PaymentMethods");
                entity.HasKey(e => e.Id).HasName("PK_PaymentMethods");
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Code).IsRequired().HasMaxLength(20);
                entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
                entity.HasIndex(e => e.Code).IsUnique();
            });

            // Bỏ qua CartItem trong DB
            modelBuilder.Ignore<CartItem>();
        }
    }
    }