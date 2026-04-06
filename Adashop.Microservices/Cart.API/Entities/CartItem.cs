using Adashop.Shared.Entities;

namespace Cart.API.Entities;

public class CartItem : BaseEntity
{
    public int Quantity { get; set; }

    public int UserId { get; set; }
    //public User User { get; set; } = null!;

    public int ProductId { get; set; }
    //public Product Product { get; set; } = null!;

    public string ProductName { get; set; } = string.Empty;
    public decimal ProductPrice { get; set; }
    public string? ProductImageUrl { get; set; }
}