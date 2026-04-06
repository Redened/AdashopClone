using Adashop.Shared.Entities;
using Order.API.Enums;

namespace Order.API.Entities;

public class Order : BaseEntity
{
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public required string ShippingAddress { get; set; }
    public decimal TotalPrice { get; set; }


    public List<OrderItem> OrderItems { get; set; } = [];

    public int UserId { get; set; }
    //public User User { get; set; } = null!;
}