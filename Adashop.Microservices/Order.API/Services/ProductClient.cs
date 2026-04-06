using System.Text.Json;

namespace Order.API.Services;

public class ProductClient : IProductClient
{
    private readonly HttpClient _http;
    private readonly ILogger<ProductClient> _logger;

    public ProductClient(HttpClient http, ILogger<ProductClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<ProductInfo?> GetProductAsync(int productId)
    {
        try
        {
            var response = await _http.GetAsync($"/api/products/{productId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch product {ProductId}: {StatusCode}", productId, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ProductApiResponse>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result?.Value == null)
                return null;

            return new ProductInfo(
                result.Value.Id,
                result.Value.Name,
                result.Value.Price,
                result.Value.MainImageUrl,
                result.Value.Stock
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching product {ProductId}", productId);
            return null;
        }
    }

    public async Task<bool> ReserveStockAsync(int productId, int quantity)
    {
        try
        {
            var request = new { quantity };
            var content = new StringContent(
                JsonSerializer.Serialize(request),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var response = await _http.PutAsync($"/api/products/{productId}/reserve-stock", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to reserve stock for product {ProductId}: {StatusCode}", productId, response.StatusCode);
                return false;
            }

            _logger.LogInformation("Stock reserved for product {ProductId}: {Quantity}", productId, quantity);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reserving stock for product {ProductId}", productId);
            return false;
        }
    }

    public async Task<bool> ReleaseStockAsync(int productId, int quantity)
    {
        try
        {
            var request = new { quantity };
            var content = new StringContent(
                JsonSerializer.Serialize(request),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var response = await _http.PutAsync($"/api/products/{productId}/release-stock", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to release stock for product {ProductId}: {StatusCode}", productId, response.StatusCode);
                return false;
            }

            _logger.LogInformation("Stock released for product {ProductId}: {Quantity}", productId, quantity);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error releasing stock for product {ProductId}", productId);
            return false;
        }
    }
}

internal record ProductApiResponse(ProductData? Value);

internal record ProductData
{
    public required int Id { get; init; }
    public required string Name { get; init; }
    public required decimal Price { get; init; }
    public required string? MainImageUrl { get; init; }
    public required int Stock { get; init; }
}
