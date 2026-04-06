namespace Order.API.DTOs;

public record CreateOrderRequest(
    string ShippingAddress
);

public record OrderItemResponse(
    int Id,
    int ProductId,
    string ProductName,
    decimal ProductPriceSnapshot,
    int Quantity,
    decimal SubTotal
);

public record OrderResponse(
    int Id,
    string Status,
    string ShippingAddress,
    decimal TotalPrice,
    List<OrderItemResponse> Items,
    string CreatedAt,
    string Currency
);

public record UserOrdersResponse(
    List<OrderResponse> Orders,
    int TotalCount,
    string Currency
);
