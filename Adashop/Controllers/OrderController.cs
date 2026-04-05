using Adashop.DTOs;
using Adashop.Services.Order;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adashop.Controllers;

[Authorize]
[ApiController]
[Route("api/orders")]
public class OrderController : BaseApiController
{
    private readonly IOrderService _orderService;

    public OrderController( IOrderService orderService ) => _orderService = orderService;

    /// <summary>
    /// Creates a new order from the user's cart items.
    /// </summary>
    /// <param name="request">CreateOrderRequest containing shipping address</param>
    /// <returns>OrderResponse containing the created order with items</returns>
    [HttpPost]
    public async Task<IActionResult> CreateOrder( CreateOrderRequest request )
    {
        var userId = GetCurrentUserId();
        var result = await _orderService.CreateOrder(userId, request);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Retrieves detailed information about a specific order.
    /// </summary>
    /// <param name="orderId">The order ID</param>
    /// <param name="currency">Currency for price conversion (GEL or USD, default: GEL)</param>
    /// <returns>OrderResponse containing order details and items</returns>
    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetOrder( int orderId, [FromQuery] string currency = "GEL" )
    {
        var userId = GetCurrentUserId();
        var result = await _orderService.GetOrderById(userId, orderId, currency);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Retrieves all orders for the current user.
    /// </summary>
    /// <param name="currency">Currency for price conversion (GEL or USD, default: GEL)</param>
    /// <returns>UserOrdersResponse containing list of OrderResponse and total count</returns>
    [HttpGet]
    public async Task<IActionResult> GetMyOrders( [FromQuery] string currency = "GEL" )
    {
        var userId = GetCurrentUserId();
        var result = await _orderService.GetUserOrders(userId, currency);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Cancels a pending order and restores product inventory.
    /// </summary>
    /// <param name="orderId">The order ID to cancel</param>
    /// <param name="currency">Currency for price conversion (GEL or USD, default: GEL)</param>
    /// <returns>OrderResponse containing the cancelled order details</returns>
    [HttpPost("{orderId}/cancel")]
    public async Task<IActionResult> CancelOrder( int orderId, [FromQuery] string currency = "GEL" )
    {
        var userId = GetCurrentUserId();
        var result = await _orderService.CancelOrder(userId, orderId, currency);
        return StatusCode(result.Status, result);
    }
}
