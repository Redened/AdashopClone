using Adashop.Entities;
using Microsoft.EntityFrameworkCore;

namespace Adashop.Data;

public class DataContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<UserDetails> UserDetails { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<CartItem> CartItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    public DataContext( DbContextOptions<DataContext> options ) : base(options)
    {
    }

    protected override void OnModelCreating( ModelBuilder modelBuilder )
    {
        // one-to-one / User and UserDetails
        modelBuilder.Entity<User>()
            .HasOne(u => u.UserDetails)
            .WithOne(ud => ud.User)
            .HasForeignKey<UserDetails>(ud => ud.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // prevent duplicate email in Users
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // one-to-many / Category and Product
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // one-to-many / Product and ProductImage
        modelBuilder.Entity<ProductImage>()
            .HasOne(pi => pi.Product)
            .WithMany(p => p.Images)
            .HasForeignKey(pi => pi.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        // prevents rounding issues in Product price
        modelBuilder.Entity<Product>()
            .Property(p => p.Price)
            .HasPrecision(18, 2);

        // prevent duplicate sort order in Products
        modelBuilder.Entity<ProductImage>()
            .HasIndex(pi => new { pi.ProductId, pi.SortOrder });

        // establishes hierarchical relationship for categories (self-referencing)
        modelBuilder.Entity<Category>()
            .HasOne(c => c.ParentCategory)
            .WithMany(c => c.Children)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // one-to-many / User and CartItem
        modelBuilder.Entity<CartItem>()
            .HasOne(ci => ci.User)
            .WithMany(u => u.CartItems)
            .HasForeignKey(ci => ci.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // prevent duplicates in User CartItems
        modelBuilder.Entity<CartItem>()
            .HasIndex(ci => new { ci.UserId, ci.ProductId })
            .IsUnique();

        // one-to-many / User and Order
        modelBuilder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // optimize queries filtering by order status and user
        modelBuilder.Entity<Order>()
            .HasIndex(o => o.Status);
        modelBuilder.Entity<Order>()
            .HasIndex(o => o.UserId);

        // prevents rounding issues in Order total price
        modelBuilder.Entity<Order>()
            .Property(o => o.TotalPrice)
            .HasPrecision(18, 2);

        // one-to-many / Order and OrderItem
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // one-to-many / Product and OrderItem // restrict deletion of products that are part of existing orders
        modelBuilder.Entity<OrderItem>()
            .HasOne<Product>()
            .WithMany()
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // prevents rounding issues in Order total price
        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.ProductPriceSnapshot)
            .HasPrecision(18, 2);
    }
}