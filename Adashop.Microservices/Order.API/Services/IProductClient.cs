namespace Order.API.Services;

public interface IProductClient
{
    Task<ProductInfo?> GetProductAsync(int productId);
    Task<bool> ReserveStockAsync(int productId, int quantity);
    Task<bool> ReleaseStockAsync(int productId, int quantity);
}

public record ProductInfo(
    int Id,
    string Name,
    decimal Price,
    string? ImageUrl,
    int Stock
);
