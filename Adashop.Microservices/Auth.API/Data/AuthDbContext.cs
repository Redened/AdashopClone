using Auth.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.API.Data;

public class AuthDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<UserDetails> UserDetails { get; set; }

    public AuthDbContext( DbContextOptions<AuthDbContext> options ) : base(options)
    {
    }

    protected override void OnModelCreating( ModelBuilder modelBuilder )
    {
        modelBuilder.Entity<User>()
            .HasOne(u => u.UserDetails)
            .WithOne(ud => ud.User)
            .HasForeignKey<UserDetails>(ud => ud.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
}