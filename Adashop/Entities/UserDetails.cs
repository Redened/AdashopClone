using Adashop.Common.Entities;

namespace Adashop.Entities;

public class UserDetails : BaseEntity
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }


    public User User { get; set; } = null!;
    public int UserId { get; set; }
}
