using Adashop.DTOs;
using Adashop.Services.Cart;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adashop.Controllers;

[Authorize]
[ApiController]
[Route("api/cart")]
public class CartController : BaseApiController
{
    private readonly ICartService _cartService;

    public CartController( ICartService cartService ) => _cartService = cartService;

    /// <summary>
    /// Retrieves the current user's shopping cart with all items and totals.
    /// </summary>
    /// <param name="currency">Currency for price conversion (GEL or USD, default: GEL)</param>
    /// <returns>CartResponse containing cart items and price totals</returns>
    [HttpGet]
    public async Task<IActionResult> GetCart( [FromQuery] string currency = "GEL" )
    {
        var userId = GetCurrentUserId();
        var result = await _cartService.GetUserCart(userId, currency);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Adds a product to the user's cart or increments quantity if already present.
    /// </summary>
    /// <param name="request">AddToCartRequest containing product ID and quantity</param>
    /// <returns>CartItemResponse containing the added/updated cart item details</returns>
    [HttpPost]
    public async Task<IActionResult> AddToCart( AddToCartRequest request )
    {
        var userId = GetCurrentUserId();
        var result = await _cartService.AddToCart(userId, request);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Updates the quantity of an item in the user's cart.
    /// </summary>
    /// <param name="cartItemId">The cart item ID to update</param>
    /// <param name="request">UpdateCartItemRequest containing new quantity</param>
    /// <returns>CartItemResponse containing the updated cart item details</returns>
    [HttpPut("{cartItemId}")]
    public async Task<IActionResult> UpdateCartItem( int cartItemId, UpdateCartItemRequest request )
    {
        var userId = GetCurrentUserId();
        var result = await _cartService.UpdateCartItem(userId, cartItemId, request);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Removes a single item from the user's cart.
    /// </summary>
    /// <param name="cartItemId">The cart item ID to remove</param>
    /// <returns>Boolean indicating successful removal</returns>
    [HttpDelete("{cartItemId}")]
    public async Task<IActionResult> RemoveFromCart( int cartItemId )
    {
        var userId = GetCurrentUserId();
        var result = await _cartService.RemoveFromCart(userId, cartItemId);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Clears all items from the user's cart.
    /// </summary>
    /// <returns>Boolean indicating successful clearing</returns>
    [HttpDelete]
    public async Task<IActionResult> ClearCart()
    {
        var userId = GetCurrentUserId();
        var result = await _cartService.ClearCart(userId);
        return StatusCode(result.Status, result);
    }
}

