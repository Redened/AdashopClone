using Adashop.Shared.Entities;

namespace Order.API.Entities;

public class OrderItem : BaseEntity
{
    public required string ProductName { get; set; }
    public decimal ProductPriceSnapshot { get; set; }
    public int Quantity { get; set; }


    public int ProductId { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;
}
