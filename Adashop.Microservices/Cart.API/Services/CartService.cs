using Adashop.Shared.Results;
using Cart.API.Data;
using Cart.API.DTOs;
using Cart.API.Entities;
using Cart.API.Helpers;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Cart.API.Services;

public class CartService : ICartService
{
    private readonly CartDbContext _DB;
    private readonly ILogger<CartService> _LOG;
    private readonly IValidator<AddToCartRequest> _addToCartValidator;
    private readonly IValidator<UpdateCartItemRequest> _updateCartItemValidator;
    private readonly IExchangeRateHelper _exchangeRateHelper;

    public CartService(
        CartDbContext DB,
        ILogger<CartService> LOG,
        IValidator<AddToCartRequest> addToCartValidator,
        IValidator<UpdateCartItemRequest> updateCartItemValidator,
        IExchangeRateHelper exchangeRateHelper )
    {
        _DB = DB;
        _LOG = LOG;
        _addToCartValidator = addToCartValidator;
        _updateCartItemValidator = updateCartItemValidator;
        _exchangeRateHelper = exchangeRateHelper;
    }

    public async Task<Result<CartResponse>> GetUserCart( int userId, string currency = "GEL" )
    {
        try
        {
            var cartItems = await _DB.CartItems
                .Include(c => c.Product)
                .ThenInclude(p => p.Images)
                .Where(c => c.UserId == userId)
                .AsNoTracking()
                .ToListAsync();

            var items = cartItems.Select(c => _MAP.MapCartItemResponse(c, "GEL")).ToList();
            var totalPrice = items.Sum(i => i.SubTotal);
            var itemCount = items.Sum(i => i.Quantity);

            var response = new CartResponse(items, totalPrice, itemCount, "GEL");
            var convertedResponse = await _exchangeRateHelper.ApplyCurrencyConversion(response, currency);
            return Result<CartResponse>.Success(200, convertedResponse);
        }
        catch ( Exception ex )
        {
            _LOG.LogError(ex, "Error retrieving cart for user: {UserId}", userId);
            return Result<CartResponse>.Error(500, "Failed to retrieve cart");
        }
    }

    public async Task<Result<CartItemResponse>> AddToCart( int userId, AddToCartRequest request )
    {
        var validationResult = await _addToCartValidator.ValidateAsync(request);
        if ( !validationResult.IsValid )
        {
            var validationErrors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Result<CartItemResponse>.Error(400, "Validation failed", validationErrors);
        }

        try
        {
            var product = await _DB.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == request.ProductId);

            if ( product == null )
            {
                _LOG.LogWarning("Product not found: {ProductId}", request.ProductId);
                return Result<CartItemResponse>.Error(404, "Product not found");
            }

            if ( product.Stock < request.Quantity )
            {
                _LOG.LogWarning("Insufficient stock: {ProductId} requested {Quantity}", request.ProductId, request.Quantity);
                return Result<CartItemResponse>.Error(400, "Insufficient stock");
            }

            var existingItem = await _DB.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == request.ProductId);

            if ( existingItem != null )
            {
                existingItem.Quantity += request.Quantity;
                _DB.CartItems.Update(existingItem);
            }
            else
            {
                var cartItem = new CartItem
                {
                    UserId = userId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity
                };
                _DB.CartItems.Add(cartItem);
            }

            await _DB.SaveChangesAsync();

            var updatedItem = await _DB.CartItems
                .Include(c => c.Product)
                .ThenInclude(p => p.Images)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == request.ProductId);

            if ( updatedItem == null )
            {
                _LOG.LogError("Cart item not found after save: ProductId={ProductId}", request.ProductId);
                return Result<CartItemResponse>.Error(500, "Failed to retrieve cart item");
            }

            var response = _MAP.MapCartItemResponse(updatedItem, "GEL");
            _LOG.LogInformation("Product added to cart: UserId={UserId}, ProductId={ProductId}, Quantity={Quantity}", userId, request.ProductId, request.Quantity);
            return Result<CartItemResponse>.Success(200, response);
        }
        catch ( Exception ex )
        {
            _LOG.LogError(ex, "Error adding to cart: UserId={UserId}, ProductId={ProductId}", userId, request.ProductId);
            return Result<CartItemResponse>.Error(500, "Failed to add to cart");
        }
    }

    public async Task<Result<CartItemResponse>> UpdateCartItem( int userId, int cartItemId, UpdateCartItemRequest request )
    {
        var validationResult = await _updateCartItemValidator.ValidateAsync(request);
        if ( !validationResult.IsValid )
        {
            var validationErrors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Result<CartItemResponse>.Error(400, "Validation failed", validationErrors);
        }

        try
        {
            var cartItem = await _DB.CartItems
                .Include(c => c.Product)
                .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);

            if ( cartItem == null )
            {
                _LOG.LogWarning("Cart item not found: {CartItemId}", cartItemId);
                return Result<CartItemResponse>.Error(404, "Cart item not found");
            }

            if ( cartItem.Product.Stock < request.Quantity )
            {
                _LOG.LogWarning("Insufficient stock: {ProductId} requested {Quantity}", cartItem.ProductId, request.Quantity);
                return Result<CartItemResponse>.Error(400, "Insufficient stock");
            }

            cartItem.Quantity = request.Quantity;
            _DB.CartItems.Update(cartItem);
            await _DB.SaveChangesAsync();

            var updatedItem = await _DB.CartItems
                .Include(c => c.Product)
                .ThenInclude(p => p.Images)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == cartItemId);

            if ( updatedItem == null )
            {
                _LOG.LogError("Created cart item not found after save: {ProductId}", cartItem.Id);
                return Result<CartItemResponse>.Error(500, "Failed to retrieve created product");
            }

            var response = _MAP.MapCartItemResponse(updatedItem, "GEL");
            _LOG.LogInformation("Cart item updated: CartItemId={CartItemId}, Quantity={Quantity}", cartItemId, request.Quantity);
            return Result<CartItemResponse>.Success(200, response);
        }
        catch ( Exception ex )
        {
            _LOG.LogError(ex, "Error updating cart item: {CartItemId}", cartItemId);
            return Result<CartItemResponse>.Error(500, "Failed to update cart item");
        }
    }

    public async Task<Result<bool>> RemoveFromCart( int userId, int cartItemId )
    {
        try
        {
            var cartItem = await _DB.CartItems.FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);

            if ( cartItem == null )
            {
                _LOG.LogWarning("Cart item not found: {CartItemId}", cartItemId);
                return Result<bool>.Error(404, "Cart item not found");
            }

            _DB.CartItems.Remove(cartItem);
            await _DB.SaveChangesAsync();

            _LOG.LogInformation("Item removed from cart: CartItemId={CartItemId}", cartItemId);
            return Result<bool>.Success(200, true);
        }
        catch ( Exception ex )
        {
            _LOG.LogError(ex, "Error removing from cart: {CartItemId}", cartItemId);
            return Result<bool>.Error(500, "Failed to remove from cart");
        }
    }

    public async Task<Result<bool>> ClearCart( int userId )
    {
        try
        {
            var cartItems = await _DB.CartItems
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if ( !cartItems.Any() )
            {
                _LOG.LogWarning("Cart already empty: {UserId}", userId);
                return Result<bool>.Error(400, "Cart is already empty");
            }

            _DB.CartItems.RemoveRange(cartItems);
            await _DB.SaveChangesAsync();

            _LOG.LogInformation("Cart cleared: UserId={UserId}", userId);
            return Result<bool>.Success(200, true);
        }
        catch ( Exception ex )
        {
            _LOG.LogError(ex, "Error clearing cart: {UserId}", userId);
            return Result<bool>.Error(500, "Failed to clear cart");
        }
    }


    private CartItemResponse MapCartItemResponse( CartItem cartItem, string currency )
    {
        return new CartItemResponse(
            Id: cartItem.Id,
            ProductId: cartItem.ProductId,
            ProductName: cartItem.Product.Name,
            ProductPrice: cartItem.Product.Price,
            ProductThumbnailUrl: cartItem.Product.Images.FirstOrDefault(i => i.IsMain)?.ImageUrl,
            Quantity: cartItem.Quantity,
            SubTotal: cartItem.Product.Price * cartItem.Quantity,
            Currency: currency
        );
    }
}