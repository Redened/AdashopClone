namespace Adashop.DTOs;

public record CreateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    int CategoryId
);

public record UpdateProductRequest(
    string? Name,
    string? Description,
    decimal? Price,
    int? Stock,
    int? CategoryId
);

public record CreateCategoryRequest(
    string Name,
    int? ParentCategoryId
);

public record UpdateCategoryRequest(
    string? Name,
    int? ParentCategoryId
);

public record CreateProductImageRequest(
    string ImageUrl,
    bool IsMain,
    int SortOrder,
    int ProductId
);

public record UpdateProductImageRequest(
    string? ImageUrl,
    bool? IsMain,
    int? SortOrder
);

public record AllUsersResponse(
    List<UserMinimalResponse> Users,
    int TotalCount
);

public record UserMinimalResponse(
    int Id,
    string Email,
    string Role,
    bool IsVerified,
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    string? Address,
    DateTime? LastLoginAt,
    string CreatedAt
);

public record UserDetailResponse(
    int Id,
    string Email,
    string Role,
    bool IsVerified,
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    string? Address,
    DateTime? LastLoginAt,
    string CreatedAt,
    CartResponse? Cart,
    List<OrderResponse> Orders
);
public record AllOrdersResponse(
    List<AdminOrderResponse> Orders,
    int TotalCount
);

public record AdminOrderResponse(
    int Id,
    string Status,
    string ShippingAddress,
    decimal TotalPrice,
    int UserId,
    string UserEmail,
    List<OrderItemResponse> Items,
    string CreatedAt
);