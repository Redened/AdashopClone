using Adashop.Shared.Results;
using Order.API.DTOs;

namespace Order.API.Services;

public interface IOrderService
{
    Task<Result<OrderResponse>> CreateOrder( int userId, CreateOrderRequest request );
    Task<Result<OrderResponse>> GetOrderById( int userId, int orderId, string currency = "GEL" );
    Task<Result<UserOrdersResponse>> GetUserOrders( int userId, string currency = "GEL" );
    Task<Result<OrderResponse>> CancelOrder( int userId, int orderId, string currency = "GEL" );
}
