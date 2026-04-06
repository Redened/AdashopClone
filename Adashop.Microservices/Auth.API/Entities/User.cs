using Adashop.Shared.Entities;
using Auth.API.Enums;

namespace Auth.API.Entities;

public class User : BaseEntity
{
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }

    public UserRole Role { get; set; } = UserRole.Customer;

    public DateTime? LastLoginAt { get; set; }

    public bool IsVerified { get; set; }
    public string? VerificationToken { get; set; }
    public DateTime? VerificationTokenExpiry { get; set; }

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }

    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }


    //public UserDetails? UserDetails { get; set; }

    //public List<CartItem> CartItems { get; set; } = [];
    //public List<Order> Orders { get; set; } = [];
}