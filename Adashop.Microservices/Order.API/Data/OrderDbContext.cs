using Microsoft.EntityFrameworkCore;
using Order.API.Entities;

namespace Order.API.Data;

public class OrderDbContext : DbContext
{
    public DbSet<Entities.Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }

    public OrderDbContext( DbContextOptions<OrderDbContext> options ) : base(options)
    {
    }

    protected override void OnModelCreating( ModelBuilder modelBuilder )
    {
        // optimize queries filtering by order status and user
        modelBuilder.Entity<Entities.Order>()
            .HasIndex(o => o.Status);
        modelBuilder.Entity<Entities.Order>()
            .HasIndex(o => o.UserId);

        // prevents rounding issues in Order total price
        modelBuilder.Entity<Entities.Order>()
            .Property(o => o.TotalPrice)
            .HasPrecision(18, 2);

        // one-to-many / Order and OrderItem
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.OrderItems)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // prevents rounding issues in Order total price
        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.ProductPriceSnapshot)
            .HasPrecision(18, 2);
    }
}