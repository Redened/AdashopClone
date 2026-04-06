using Order.API.DTOs;

namespace Order.API.Helpers;

public class OrderMapHelper : IOrderMapHelper
{
    public OrderResponse MapOrderResponse( Entities.Order order, string currency )
    {
        var items = order.OrderItems.Select(oi => new OrderItemResponse(
            Id: oi.Id,
            ProductId: oi.ProductId,
            ProductName: oi.ProductName,
            ProductPriceSnapshot: oi.ProductPriceSnapshot,
            Quantity: oi.Quantity,
            SubTotal: oi.ProductPriceSnapshot * oi.Quantity
        )).ToList();

        return new OrderResponse(
            Id: order.Id,
            Status: order.Status.ToString(),
            ShippingAddress: order.ShippingAddress,
            TotalPrice: order.TotalPrice,
            Items: items,
            CreatedAt: order.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            Currency: currency
        );
    }

}
