using Adashop.Shared.Results;
using Cart.API.DTOs;

namespace Cart.API.Services;

public interface ICartService
{
    Task<Result<CartResponse>> GetUserCart( int userId, string currency = "GEL" );
    Task<Result<CartItemResponse>> AddToCart( int userId, AddToCartRequest request );
    Task<Result<CartItemResponse>> UpdateCartItem( int userId, int cartItemId, UpdateCartItemRequest request );
    Task<Result<bool>> RemoveFromCart( int userId, int cartItemId );
    Task<Result<bool>> ClearCart( int userId );
}

