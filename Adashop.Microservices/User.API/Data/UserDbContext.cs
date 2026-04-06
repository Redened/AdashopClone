using Microsoft.EntityFrameworkCore;
using User.API.Entities;

namespace User.API.Data;

public class UserDbContext : DbContext
{
    public DbSet<UserDetails> UserDetails { get; set; }

    public UserDbContext( DbContextOptions<UserDbContext> options ) : base(options)
    {
    }

    protected override void OnModelCreating( ModelBuilder modelBuilder )
    {
        //modelBuilder.Entity<User>()
        //    .HasOne(u => u.UserDetails)
        //    .WithOne(ud => ud.User)
        //    .HasForeignKey<UserDetails>(ud => ud.UserId)
        //    .OnDelete(DeleteBehavior.Cascade);

        //modelBuilder.Entity<User>()
        //    .HasIndex(u => u.Email)
        //    .IsUnique();
    }
}