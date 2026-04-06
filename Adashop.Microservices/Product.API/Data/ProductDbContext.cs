using Microsoft.EntityFrameworkCore;
using Product.API.Entities;

namespace Product.API.Data;

public class ProductDbContext : DbContext
{
    public DbSet<Entities.Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }

    public ProductDbContext( DbContextOptions<ProductDbContext> options ) : base(options)
    {
    }

    protected override void OnModelCreating( ModelBuilder modelBuilder )
    {
        // one-to-many / Category and Product
        modelBuilder.Entity<Entities.Product>()
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
        modelBuilder.Entity<Entities.Product>()
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
    }
}