using Adashop.Common.Helpers.ExchangeRateAPI;
using Adashop.Common.Mappers;
using Adashop.Common.Results;
using Adashop.Data;
using Adashop.DTOs;
using Adashop.Enums;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Adashop.Services.Order;

public class OrderService : IOrderService
{
    private readonly DataContext _DB;
    private readonly ILogger<OrderService> _LOG;
    private readonly IValidator<CreateOrderRequest> _createOrderValidator;
    private readonly IExchangeRateHelper _exchangeRateHelper;
    private readonly IMapHelper _MAP;

    public OrderService(
        DataContext DB,
        ILogger<OrderService> LOG,
        IValidator<CreateOrderRequest> createOrderValidator,
        IExchangeRateHelper exchangeRateHelper,
        IMapHelper MAP )
    {
        _DB = DB;
        _LOG = LOG;
        _createOrderValidator = createOrderValidator;
        _exchangeRateHelper = exchangeRateHelper;
        _MAP = MAP;
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
            var cartItems = await _DB.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if ( !cartItems.Any() )
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

            foreach ( var cartItem in cartItems )
            {
                if ( cartItem.Product.Stock < cartItem.Quantity )
                {
                    await transaction.RollbackAsync();
                    _LOG.LogWarning("Insufficient stock for product: {ProductId}", cartItem.ProductId);
                    return Result<OrderResponse>.Error(400, $"Insufficient stock for {cartItem.Product.Name}");
                }

                var orderItem = new Entities.OrderItem
                {
                    OrderId = order.Id,
                    ProductId = cartItem.ProductId,
                    ProductName = cartItem.Product.Name,
                    ProductPriceSnapshot = cartItem.Product.Price,
                    Quantity = cartItem.Quantity
                };

                orderItems.Add(orderItem);
                totalPrice += cartItem.Product.Price * cartItem.Quantity;
                cartItem.Product.Stock -= cartItem.Quantity;
            }

            order.TotalPrice = totalPrice;
            order.OrderItems = orderItems;

            _DB.OrderItems.AddRange(orderItems);
            _DB.Products.UpdateRange(cartItems.Select(c => c.Product));
            _DB.Orders.Update(order);
            await _DB.SaveChangesAsync();

            _DB.CartItems.RemoveRange(cartItems);
            await _DB.SaveChangesAsync();

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
                var product = await _DB.Products.FindAsync(orderItem.ProductId);
                if ( product != null )
                {
                    product.Stock += orderItem.Quantity;
                    _DB.Products.Update(product);
                }
            }

            _DB.Orders.Update(order);
            await _DB.SaveChangesAsync();
            await transaction.CommitAsync();

            var response = _MAP.MapOrderResponse(order, "GEL");
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