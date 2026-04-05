using Adashop.Common.Entities;

namespace Adashop.Entities;

public class CartItem : BaseEntity
{
    public int Quantity { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}