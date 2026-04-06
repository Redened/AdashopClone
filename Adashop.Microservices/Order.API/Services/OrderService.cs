using Adashop.Shared.Results;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Order.API.Data;
using Order.API.DTOs;
using Order.API.Enums;
using Order.API.Helpers;

namespace Order.API.Services;

public class OrderService : IOrderService
{
    private readonly OrderDbContext _DB;
    private readonly ILogger<OrderService> _LOG;
    private readonly IOrderMapHelper _MAP;
    private readonly IProductClient _PRODUCT_CLIENT;
    private readonly ICartClient _CART_CLIENT;
    private readonly IValidator<CreateOrderRequest> _createOrderValidator;
    private readonly IExchangeRateHelper _exchangeRateHelper;


    public OrderService(
        OrderDbContext DB,
        ILogger<OrderService> LOG,
        IProductClient PRODUCT_CLIENT,
        ICartClient CART_CLIENT,
        IOrderMapHelper MAP,
        IValidator<CreateOrderRequest> createOrderValidator,
        IExchangeRateHelper exchangeRateHelper )
    {
        _DB = DB;
        _LOG = LOG;
        _MAP = MAP;
        _PRODUCT_CLIENT = PRODUCT_CLIENT;
        _CART_CLIENT = CART_CLIENT;
        _createOrderValidator = createOrderValidator;
        _exchangeRateHelper = exchangeRateHelper;
    }

    public async Task<Result<OrderResponse>> CreateOrder( int userId, CreateOrderRequest request )
    {
        var validationResult = await _createOrderValidator.ValidateAsync(request);
        if ( !validationResult.IsValid )
        {
            var validationErrors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Result<OrderResponse>.Error(400, "Validation failed", validationErrors);
        }

        using var transaction = await _DB.Database.BeginTransactionAsync();

        try
        {
            var cart = await _CART_CLIENT.GetUserCartAsync(userId);

            if ( cart == null || !cart.Items.Any() )
            {
                _LOG.LogWarning("Cannot create order with empty cart: {UserId}", userId);
                return Result<OrderResponse>.Error(400, "Cart is empty");
            }

            var order = new Entities.Order
            {
                UserId = userId,
                ShippingAddress = request.ShippingAddress,
                Status = OrderStatus.Pending,
                TotalPrice = 0
            };

            _DB.Orders.Add(order);
            await _DB.SaveChangesAsync();

            var orderItems = new List<Entities.OrderItem>();
            decimal totalPrice = 0;

            foreach ( var cartItem in cart.Items )
            {
                var product = await _PRODUCT_CLIENT.GetProductAsync(cartItem.ProductId);

                if ( product == null )
                {
                    await transaction.RollbackAsync();
                    _LOG.LogWarning("Product not found: {ProductId}", cartItem.ProductId);
                    return Result<OrderResponse>.Error(404, $"Product not found: {cartItem.ProductName}");
                }

                if ( product.Stock < cartItem.Quantity )
                {
                    await transaction.RollbackAsync();
                    _LOG.LogWarning("Insufficient stock for product: {ProductId}", cartItem.ProductId);
                    return Result<OrderResponse>.Error(400, $"Insufficient stock for {product.Name}");
                }

                var orderItem = new Entities.OrderItem
                {
                    OrderId = order.Id,
                    ProductId = cartItem.ProductId,
                    ProductName = cartItem.ProductName,
                    ProductPriceSnapshot = cartItem.ProductPrice,
                    Quantity = cartItem.Quantity
                };

                orderItems.Add(orderItem);
                totalPrice += cartItem.ProductPrice * cartItem.Quantity;
            }

            order.TotalPrice = totalPrice;
            order.OrderItems = orderItems;

            _DB.OrderItems.AddRange(orderItems);
            _DB.Orders.Update(order);
            await _DB.SaveChangesAsync();

            var stockReserved = true;
            foreach ( var cartItem in cart.Items )
            {
                var reserved = await _PRODUCT_CLIENT.ReserveStockAsync(cartItem.ProductId, cartItem.Quantity);
                if ( !reserved )
                {
                    stockReserved = false;
                    break;
                }
            }

            if ( !stockReserved )
            {
                await transaction.RollbackAsync();
                _LOG.LogWarning("Failed to reserve stock for order: {OrderId}", order.Id);
                return Result<OrderResponse>.Error(500, "Failed to reserve stock");
            }

            var cartCleared = await _CART_CLIENT.ClearUserCartAsync(userId);
            if ( !cartCleared )
            {
                _LOG.LogWarning("Failed to clear cart for user after order creation: {UserId}", userId);
            }

            await transaction.CommitAsync();

            var response = _MAP.MapOrderResponse(order, "GEL");
            _LOG.LogInformation("Order created: OrderId={OrderId}, UserId={UserId}, TotalPrice={TotalPrice}", order.Id, userId, totalPrice);
            return Result<OrderResponse>.Success(201, response);
        }
        catch ( Exception ex )
        {
            await transaction.RollbackAsync();
            _LOG.LogError(ex, "Error creating order: {UserId}", userId);
            return Result<OrderResponse>.Error(500, "Failed to create order");
        }
    }

    public async Task<Result<OrderResponse>> GetOrderById( int userId, int orderId, string currency = "GEL" )
    {
        try
        {
            var order = await _DB.Orders
                .Include(o => o.OrderItems)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if ( order == null )
            {
                _LOG.LogWarning("Order not found: {OrderId}", orderId);
                return Result<OrderResponse>.Error(404, "Order not found");
            }

            var response = _MAP.MapOrderResponse(order, "GEL");
            var convertedResponse = await _exchangeRateHelper.ApplyCurrencyConversion(response, currency);
            return Result<OrderResponse>.Success(200, convertedResponse);
        }
        catch ( Exception ex )
        {
            _LOG.LogError(ex, "Error retrieving order: {OrderId}", orderId);
            return Result<OrderResponse>.Error(500, "Failed to retrieve order");
        }
    }

    public async Task<Result<UserOrdersResponse>> GetUserOrders( int userId, string currency = "GEL" )
    {
        try
        {
            var orders = await _DB.Orders
                .Include(o => o.OrderItems)
                .Where(o => o.UserId == userId)
                .AsNoTracking()
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var orderResponses = orders.Select(o => _MAP.MapOrderResponse(o, "GEL")).ToList();
            var convertedResponses = await _exchangeRateHelper.ApplyCurrencyConversionToList(orderResponses, currency);
            var response = new UserOrdersResponse(convertedResponses, convertedResponses.Count, currency);

            return Result<UserOrdersResponse>.Success(200, response);
        }
        catch ( Exception ex )
        {
            _LOG.LogError(ex, "Error retrieving user orders: {UserId}", userId);
            return Result<UserOrdersResponse>.Error(500, "Failed to retrieve orders");
        }
    }

    public async Task<Result<OrderResponse>> CancelOrder( int userId, int orderId, string currency = "GEL" )
    {
        using var transaction = await _DB.Database.BeginTransactionAsync();

        try
        {
            var order = await _DB.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if ( order == null )
            {
                _LOG.LogWarning("Order not found: {OrderId}", orderId);
                return Result<OrderResponse>.Error(404, "Order not found");
            }

            if ( order.Status != OrderStatus.Pending )
            {
                _LOG.LogWarning("Cannot cancel order with status: {OrderStatus}", order.Status);
                return Result<OrderResponse>.Error(400, $"Cannot cancel order with status {order.Status}");
            }

            order.Status = OrderStatus.Cancelled;

            foreach ( var orderItem in order.OrderItems )
            {
                var released = await _PRODUCT_CLIENT.ReleaseStockAsync(orderItem.ProductId, orderItem.Quantity);
                if ( !released )
                {
                    _LOG.LogWarning("Failed to release stock for product: {ProductId}", orderItem.ProductId);
                }
            }

            _DB.Orders.Update(order);
            await _DB.SaveChangesAsync();
            await transaction.CommitAsync();

            var response = _MAP.MapOrderResponse(order, currency);
            var convertedResponse = await _exchangeRateHelper.ApplyCurrencyConversion(response, currency);
            _LOG.LogInformation("Order cancelled: OrderId={OrderId}, UserId={UserId}", orderId, userId);
            return Result<OrderResponse>.Success(200, convertedResponse);
        }
        catch ( Exception ex )
        {
            await transaction.RollbackAsync();
            _LOG.LogError(ex, "Error cancelling order: {OrderId}", orderId);
            return Result<OrderResponse>.Error(500, "Failed to cancel order");
        }
    }
}