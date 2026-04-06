using Cart.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cart.API.Data;

public class CartDbContext : DbContext
{
    public DbSet<CartItem> CartItems { get; set; }

    public CartDbContext( DbContextOptions<CartDbContext> options ) : base(options)
    {
    }

    protected override void OnModelCreating( ModelBuilder modelBuilder )
    {
        // one-to-many / User and CartItem
        //modelBuilder.Entity<CartItem>()
        //    .HasOne(ci => ci.User)
        //    .WithMany(u => u.CartItems)
        //    .HasForeignKey(ci => ci.UserId)
        //    .OnDelete(DeleteBehavior.Cascade);

        // prevent duplicates in User CartItems
        modelBuilder.Entity<CartItem>()
            .HasIndex(ci => new { ci.UserId, ci.ProductId })
            .IsUnique();

        // prevents rounding issues in CartItem price
        modelBuilder.Entity<CartItem>()
            .Property(ci => ci.ProductPrice)
            .HasPrecision(18, 2);
    }
}
