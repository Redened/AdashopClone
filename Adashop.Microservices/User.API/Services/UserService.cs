using FluentValidation;
using User.API.Data;
using User.API.DTOs;

namespace User.API.Services;

public class UserService : IUserService
{
    private readonly UserDbContext _DB;
    private readonly ILogger<UserService> _LOG;
    private readonly IValidator<ChangeUserDetailsRequest> _changeUserDetailsValidator;
    private readonly IMapHelper _mapHelper;

    public UserServices(
        DataContext DB,
        ILogger<UserServices> LOG,
        IValidator<ChangeUserDetailsRequest> changeUserDetailsValidator,
        IMapHelper mapHelper )
    {
        _DB = DB;
        _LOG = LOG;
        _changeUserDetailsValidator = changeUserDetailsValidator;
        _mapHelper = mapHelper;
    }

    public async Task<Result<UserDetailResponse>> ChangeUserDetails( int id, ChangeUserDetailsRequest request )
    {
        var validationResult = await _changeUserDetailsValidator.ValidateAsync(request);
        if ( !validationResult.IsValid )
        {
            var validationErrors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Result<UserDetailResponse>.Error(400, "Validation failed", validationErrors);
        }

        using var transaction = await _DB.Database.BeginTransactionAsync();

        try
        {
            var user = await _DB.Users
                .Include(u => u.UserDetails)
                .Include(u => u.CartItems)
                .ThenInclude(ci => ci.Product)
                .ThenInclude(p => p.Images)
                .Include(u => u.Orders)
                .ThenInclude(o => o.OrderItems)
                .FirstOrDefaultAsync(u => u.Id == id);

            if ( user == null )
            {
                _LOG.LogWarning("User not found for details update: {UserId}", id);
                return Result<UserDetailResponse>.Error(404, "User not found");
            }

            if ( user.UserDetails == null )
            {
                user.UserDetails = new UserDetails { UserId = user.Id };
                _DB.UserDetails.Add(user.UserDetails);
            }

            if ( request.FirstName != null )
                user.UserDetails.FirstName = request.FirstName;
            if ( request.LastName != null )
                user.UserDetails.LastName = request.LastName;
            if ( request.PhoneNumber != null )
                user.UserDetails.PhoneNumber = request.PhoneNumber;
            if ( request.Address != null )
                user.UserDetails.Address = request.Address;

            user.UserDetails.UpdatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            await _DB.SaveChangesAsync();

            var updatedUser = await _DB.Users
                .Include(u => u.UserDetails)
                .Include(u => u.CartItems)
                .ThenInclude(ci => ci.Product)
                .ThenInclude(p => p.Images)
                .Include(u => u.Orders)
                .ThenInclude(o => o.OrderItems)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if ( updatedUser == null )
            {
                await transaction.RollbackAsync();
                _LOG.LogError("Updated user not found after save: {UserId}", id);
                return Result<UserDetailResponse>.Error(500, "Failed to retrieve updated user");
            }

            CartResponse? cartResponse = null;
            if ( updatedUser.CartItems.Any() )
            {
                var cartItems = updatedUser.CartItems.Select(ci => new CartItemResponse(
                    Id: ci.Id,
                    ProductId: ci.ProductId,
                    ProductName: ci.Product.Name,
                    ProductPrice: ci.Product.Price,
                    ProductThumbnailUrl: ci.Product.Images.FirstOrDefault(i => i.IsMain)?.ImageUrl,
                    Quantity: ci.Quantity,
                    SubTotal: ci.Product.Price * ci.Quantity,
                    Currency: "GEL"
                )).ToList();

                var totalPrice = cartItems.Sum(i => i.SubTotal);
                var itemCount = cartItems.Sum(i => i.Quantity);
                cartResponse = new CartResponse(cartItems, totalPrice, itemCount, "GEL");
            }

            var orders = updatedUser.Orders.Select(o => new OrderResponse(
                Id: o.Id,
                Status: o.Status.ToString(),
                ShippingAddress: o.ShippingAddress,
                TotalPrice: o.TotalPrice,
                Items: o.OrderItems.Select(oi => new OrderItemResponse(
                    Id: oi.Id,
                    ProductId: oi.ProductId,
                    ProductName: oi.ProductName,
                    ProductPriceSnapshot: oi.ProductPriceSnapshot,
                    Quantity: oi.Quantity,
                    SubTotal: oi.ProductPriceSnapshot * oi.Quantity
                )).ToList(),
                CreatedAt: o.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                Currency: "GEL"
            )).ToList();

            var response = _mapHelper.MapUserDetailResponse(updatedUser, cartResponse, orders);

            await transaction.CommitAsync();
            _LOG.LogInformation("User details updated: {UserId}", id);
            return Result<UserDetailResponse>.Success(200, response);
        }
        catch ( Exception ex )
        {
            await transaction.RollbackAsync();
            _LOG.LogError(ex, "Error updating user details: {UserId}", id);
            return Result<UserDetailResponse>.Error(500, "Failed to update user details");
        }
    }
}
