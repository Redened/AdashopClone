using System.Text.Json;

namespace Cart.API.Services;

public class ProductClient : IProductClient
{
    private readonly HttpClient _HTTP;
    private readonly ILogger<ProductClient> _LOG;

    public ProductClient( HttpClient HTTP, ILogger<ProductClient> LOG )
    {
        _HTTP = HTTP;
        _LOG = LOG;
    }

    public async Task<ProductInfo?> GetProductAsync( int productId )
    {
        try
        {
            var response = await _HTTP.GetAsync($"/api/products/{productId}");

            if ( !response.IsSuccessStatusCode )
            {
                _LOG.LogWarning("Failed to fetch product {ProductId}: {StatusCode}", productId, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<ProductApiResponse>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if ( result?.Value == null ) return null;

            return new ProductInfo(
                result.Value.Id,
                result.Value.Name,
                result.Value.Price,
                result.Value.MainImageUrl,
                result.Value.Stock
            );
        }
        catch ( Exception ex )
        {
            _LOG.LogError(ex, "Error fetching product {ProductId}", productId);
            return null;
        }
    }
}

internal record ProductApiResponse( ProductData? Value );
internal record ProductData( int Id, string Name, decimal Price, string? MainImageUrl, int Stock );