namespace Cart.API.DTOs;

public record AddToCartRequest(
    int ProductId,
    int Quantity
);

public record UpdateCartItemRequest(
    int Quantity
);

public record CartItemResponse(
    int Id,
    int ProductId,
    string ProductName,
    decimal ProductPrice,
    string? ProductThumbnailUrl,
    int Quantity,
    decimal SubTotal,
    string Currency
);

public record CartResponse(
    List<CartItemResponse> Items,
    decimal TotalPrice,
    int ItemCount,
    string Currency
);
