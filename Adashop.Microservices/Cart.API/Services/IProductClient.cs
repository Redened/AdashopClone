namespace Cart.API.Services;

public interface IProductClient
{
    Task<ProductInfo?> GetProductAsync( int productId );
}

public record ProductInfo(
    int Id,
    string Name,
    decimal Price,
    string? ImageUrl,
    int Stock
);