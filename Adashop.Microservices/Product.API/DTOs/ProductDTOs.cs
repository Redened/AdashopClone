namespace Product.API.DTOs;

public record ProductMinimalResponse(
    int Id,
    string Name,
    decimal Price,
    string? ThumbnailUrl,
    string Currency
);

public record ProductDetailResponse(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    int Stock,
    string? MainImageUrl,
    List<ProductImageResponse> Images,
    List<BreadcrumbResponse> Breadcrumbs,
    CategoryResponse? Category,
    string Currency
);

public record ProductImageResponse(
    int Id,
    string ImageUrl,
    bool IsMain,
    int SortOrder,
    int ProductId
);

public record CategoryResponse(
    int Id,
    string Name,
    int? ParentCategoryId
);

public record CategoryTreeResponse(
    int Id,
    string Name,
    int? ParentCategoryId,
    List<CategoryTreeResponse> Children
);

public record CategoryDetailResponse(
    int Id,
    string Name,
    CategoryResponse? ParentCategory,
    List<CategoryResponse> Children,
    int ProductCount
);

public record BreadcrumbResponse(
    int Id,
    string Name
);