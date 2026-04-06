namespace Order.API.Services;

public interface ICartClient
{
    Task<CartInfo?> GetUserCartAsync(int userId);
    Task<bool> ClearUserCartAsync(int userId);
}

public record CartItemInfo
{
    public required int Id { get; init; }
    public required int ProductId { get; init; }
    public required string ProductName { get; init; }
    public required decimal ProductPrice { get; init; }
    public required int Quantity { get; init; }
}

public record CartInfo
{
    public required List<CartItemInfo> Items { get; init; }
    public required decimal TotalPrice { get; init; }
}
