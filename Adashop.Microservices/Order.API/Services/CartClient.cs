using System.Text.Json;

namespace Order.API.Services;

public class CartClient : ICartClient
{
    private readonly HttpClient _http;
    private readonly ILogger<CartClient> _logger;

    public CartClient(HttpClient http, ILogger<CartClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<CartInfo?> GetUserCartAsync(int userId)
    {
        try
        {
            var response = await _http.GetAsync($"/api/cart/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch cart for user {UserId}: {StatusCode}", userId, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CartApiResponse>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result?.Value == null)
                return null;

            return new CartInfo
            {
                Items = result.Value.Items.Select(item => new CartItemInfo
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    ProductPrice = item.ProductPrice,
                    Quantity = item.Quantity
                }).ToList(),
                TotalPrice = result.Value.TotalPrice
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching cart for user {UserId}", userId);
            return null;
        }
    }

    public async Task<bool> ClearUserCartAsync(int userId)
    {
        try
        {
            var response = await _http.DeleteAsync($"/api/cart/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to clear cart for user {UserId}: {StatusCode}", userId, response.StatusCode);
                return false;
            }

            _logger.LogInformation("Cart cleared for user {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cart for user {UserId}", userId);
            return false;
        }
    }
}

internal record CartApiResponse(CartData? Value);

internal record CartData
{
    public required List<CartItemData> Items { get; init; }
    public required decimal TotalPrice { get; init; }
}

internal record CartItemData
{
    public required int Id { get; init; }
    public required int ProductId { get; init; }
    public required string ProductName { get; init; }
    public required decimal ProductPrice { get; init; }
    public required int Quantity { get; init; }
}
