using Adashop.Common.Helpers.CategoryTree;
using Adashop.Common.Mappers;
using Adashop.Common.Results;
using Adashop.Data;
using Adashop.DTOs;
using Adashop.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Adashop.Services.Admin;

public class AdminServices : IAdminServices
{
    private readonly DataContext _DB;
    private readonly ILogger<AdminServices> _LOG;
    private readonly IValidator<CreateProductRequest> _createProductValidator;
    private readonly IValidator<UpdateProductRequest> _updateProductValidator;
    private readonly IValidator<CreateCategoryRequest> _createCategoryValidator;
    private readonly IValidator<UpdateCategoryRequest> _updateCategoryValidator;
    private readonly IValidator<CreateProductImageRequest> _createProductImageValidator;
    private readonly IValidator<UpdateProductImageRequest> _updateProductImageValidator;
    private readonly ICategoryTreeHelper _categoryTreeHelper;
    private readonly IMapHelper _MAP;

    public AdminServices(
        DataContext DB,
        ILogger<AdminServices> LOG,
        IValidator<CreateProductRequest> createProductValidator,
        IValidator<UpdateProductRequest> updateProductValidator,
        IValidator<CreateCategoryRequest> createCategoryValidator,
        IValidator<UpdateCategoryRequest> updateCategoryValidator,
        IValidator<CreateProductImageRequest> createProductImageValidator,
        IValidator<UpdateProductImageRequest> updateProductImageValidator,
        IMapHelper MAP,
        ICategoryTreeHelper categoryTreeHelper )
    {
        _DB = DB;
        _LOG = LOG;
        _createProductValidator = createProductValidator;
        _updateProductValidator = updateProductValidator;
        _createCategoryValidator = createCategoryValidator;
        _updateCategoryValidator = updateCategoryValidator;
        _createProductImageValidator = createProductImageValidator;
        _updateProductImageValidator = updateProductImageValidator;
        _MAP = MAP;
        _categoryTreeHelper = categoryTreeHelper;
    }

    public async Task<Result<ProductDetailResponse>> CreateProduct( CreateProductRequest request )
    {
        var validationResult = await _createProductValidator.ValidateAsync(request);
        if ( !validationResult.IsValid )
        {
            var validationErrors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Result<ProductDetailResponse>.Error(400, "Validation failed", validationErrors);
        }

        using var transaction = await _DB.Database.BeginTransactionAsync();

        try
        {
            var categoryExists = await _DB.Categories.AnyAsync(c => c.Id == request.CategoryId);
            if ( !categoryExists )
            {
                _LOG.LogWarning("Category not found: {CategoryId}", request.CategoryId);
                return Result<ProductDetailResponse>.Error(400, "Category not found");
            }

            var product = new Entities.Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Stock = request.Stock,
                CategoryId = request.CategoryId
            };

            _DB.Products.Add(product);
            await _DB.SaveChangesAsync();

            var completeProduct = await _DB.Products
                .Include(p => p.Images)
                .Include(p => p.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == product.Id);

            if ( completeProduct == null )
            {
                await transaction.RollbackAsync();
                _LOG.LogError("Created product not found after save: {ProductId}", product.Id);
                return Result<ProductDetailResponse>.Error(500, "Failed to retrieve created product");
            }

            var response = _MAP.MapProductDetailResponse(completeProduct);
            var breadcrumbs = await _categoryTreeHelper.GetCategoryBreadcrumbs(completeProduct.CategoryId);
            response = response with { Breadcrumbs = breadcrumbs };

            await transaction.CommitAsync();
            _LOG.LogInformation("Product created: {ProductId} - {ProductName}", product.Id, product.Name);
            return Result<ProductDetailResponse>.Success(201, response);
        }
        catch ( Exception ex )
        {
            await transaction.RollbackAsync();
            _LOG.LogError(ex, "Error creating product: {ProductName}", request.Name);
            return Result<ProductDetailResponse>.Error(500, "Failed to create product");
        }
    }

    public async Task<Result<ProductDetailResponse>> UpdateProduct( int id, UpdateProductRequest request )
    {
        var validationResult = await _updateProductValidator.ValidateAsync(request);
        if ( !validationResult.IsValid )
        {
            var validationErrors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Result<ProductDetailResponse>.Error(400, "Validation failed", validationErrors);
        }

        using var transaction = await _DB.Database.BeginTransactionAsync();

        try
        {
            var product = await _DB.Products
                .Include(p => p.Images)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if ( product == null )
            {
                _LOG.LogWarning("Product not found for update: {ProductId}", id);
                return Result<ProductDetailResponse>.Error(404, "Product not found");
            }

            if ( request.CategoryId.HasValue )
            {
                var categoryExists = await _DB.Categories.AnyAsync(c => c.Id == request.CategoryId.Value);
                if ( !categoryExists )
                {
                    _LOG.LogWarning("Category not found: {CategoryId}", request.CategoryId.Value);
                    return Result<ProductDetailResponse>.Error(400, "Category not found");
                }
            }

            if ( request.Name != null )
                product.Name = request.Name;
            if ( request.Description != null )
                product.Description = request.Description;
            if ( request.Price.HasValue )
                product.Price = request.Price.Value;
            if ( request.Stock.HasValue )
                product.Stock = request.Stock.Value;
            if ( request.CategoryId.HasValue )
                product.CategoryId = request.CategoryId.Value;

            product.UpdatedAt = DateTime.UtcNow;

            await _DB.SaveChangesAsync();

            var updatedProduct = await _DB.Products
                .Include(p => p.Images)
                .Include(p => p.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if ( updatedProduct == null )
            {
                await transaction.RollbackAsync();
                _LOG.LogError("Updated product not found after save: {ProductId}", product.Id);
                return Result<ProductDetailResponse>.Error(500, "Failed to retrieve created product");
            }

            var response = _MAP.MapProductDetailResponse(updatedProduct);
            var breadcrumbs = await _categoryTreeHelper.GetCategoryBreadcrumbs(updatedProduct.CategoryId);
            response = response with { Breadcrumbs = breadcrumbs };

            await transaction.CommitAsync();
            _LOG.LogInformation("Product updated: {ProductId} - {ProductName}", product.Id, product.Name);
            return Result<ProductDetailResponse>.Success(200, response);
        }
        catch ( Exception ex )
        {
            await transaction.RollbackAsync();
            _LOG.LogError(ex, "Error updating product: {ProductId}", id);
            return Result<ProductDetailResponse>.Error(500, "Failed to update product");
        }
    }

    public async Task<Result<bool>> DeleteProduct( int id )
    {
        using var transaction = await _DB.Database.BeginTransactionAsync();

        try
        {
            var product = await _DB.Products.FindAsync(id);
            if ( product == null )
            {
                _LOG.LogWarning("Product not found for deletion: {ProductId}", id);
                return Result<bool>.Error(404, "Product not found");
            }

            _DB.Products.Remove(product);
            await _DB.SaveChangesAsync();

            _LOG.LogInformation("Product deleted: {ProductId}", id);

            await transaction.CommitAsync();
            return Result<bool>.Success(200, true);
        }
        catch ( Exception ex )
        {
            await transaction.RollbackAsync();
            _LOG.LogError(ex, "Error deleting product: {ProductId}", id);
            return Result<bool>.Error(500, "Failed to delete product");
        }
    }


    public async Task<Result<CategoryDetailResponse>> CreateCategory( CreateCategoryRequest request )
    {
        var validationResult = await _createCategoryValidator.ValidateAsync(request);
        if ( !validationResult.IsValid )
        {
            var validationErrors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Result<CategoryDetailResponse>.Error(400, "Validation failed", validationErrors);
        }

        using var transaction = await _DB.Database.BeginTransactionAsync();

        try
        {
            if ( request.ParentCategoryId.HasValue )
            {
                var parentExists = await _DB.Categories.AnyAsync(c => c.Id == request.ParentCategoryId.Value);
                if ( !parentExists )
                {
                    _LOG.LogWarning("Parent category not found: {ParentCategoryId}", request.ParentCategoryId.Value);
                    return Result<CategoryDetailResponse>.Error(400, "Parent category not found");
                }
            }

            var category = new Category
            {
                Name = request.Name,
                ParentCategoryId = request.ParentCategoryId
            };

            _DB.Categories.Add(category);
            await _DB.SaveChangesAsync();

            var completeCategory = await _DB.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.Children)
                .Include(c => c.Products)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == category.Id);

            if ( completeCategory == null )
            {
                await transaction.RollbackAsync();
                _LOG.LogError("Created category not found after save: {CategoryId}", category.Id);
                return Result<CategoryDetailResponse>.Error(500, "Failed to retrieve created product");
            }

            var response = _MAP.MapCategoryDetailResponse(completeCategory);

            await transaction.CommitAsync();
            _LOG.LogInformation("Category created: {CategoryId} - {CategoryName}", category.Id, category.Name);
            return Result<CategoryDetailResponse>.Success(201, response);
        }
        catch ( Exception ex )
        {
            await transaction.RollbackAsync();
            _LOG.LogError(ex, "Error creating category: {CategoryName}", request.Name);
            return Result<CategoryDetailResponse>.Error(500, "Failed to create category");
        }
    }

    public async Task<Result<CategoryDetailResponse>> UpdateCategory( int id, UpdateCategoryRequest request )
    {
        var validationResult = await _updateCategoryValidator.ValidateAsync(request);
        if ( !validationResult.IsValid )
        {
            var validationErrors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Result<CategoryDetailResponse>.Error(400, "Validation failed", validationErrors);
        }

        using var transaction = await _DB.Database.BeginTransactionAsync();

        try
        {
            var category = await _DB.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.Children)
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if ( category == null )
            {
                _LOG.LogWarning("Category not found for update: {CategoryId}", id);
                return Result<CategoryDetailResponse>.Error(404, "Category not found");
            }

            if ( request.ParentCategoryId.HasValue )
            {
                if ( request.ParentCategoryId.Value == id )
                {
                    return Result<CategoryDetailResponse>.Error(400, "Category cannot be its own parent");
                }

                var parentExists = await _DB.Categories.AnyAsync(c => c.Id == request.ParentCategoryId.Value);
                if ( !parentExists )
                {
                    _LOG.LogWarning("Parent category not found: {ParentCategoryId}", request.ParentCategoryId.Value);
                    return Result<CategoryDetailResponse>.Error(400, "Parent category not found");
                }
            }

            if ( request.Name != null )
                category.Name = request.Name;
            if ( request.ParentCategoryId != null )
                category.ParentCategoryId = request.ParentCategoryId;

            category.UpdatedAt = DateTime.UtcNow;

            await _DB.SaveChangesAsync();

            var updatedCategory = await _DB.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.Children)
                .Include(c => c.Products)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if ( updatedCategory == null )
            {
                await transaction.RollbackAsync();
                _LOG.LogError("Created category not found after save: {CategoryId}", category.Id);
                return Result<CategoryDetailResponse>.Error(500, "Failed to retrieve created product");
            }

            var response = _MAP.MapCategoryDetailResponse(updatedCategory);

            await transaction.CommitAsync();
            _LOG.LogInformation("Category updated: {CategoryId} - {CategoryName}", category.Id, category.Name);
            return Result<CategoryDetailResponse>.Success(200, response);
        }
        catch ( Exception ex )
        {
            await transaction.RollbackAsync();
            _LOG.LogError(ex, "Error updating category: {CategoryId}", id);
            return Result<CategoryDetailResponse>.Error(500, "Failed to update category");
        }
    }

    public async Task<Result<bool>> DeleteCategory( int id )
    {
        using var transaction = await _DB.Database.BeginTransactionAsync();

        try
        {
            var category = await _DB.Categories
                .Include(c => c.Children)
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if ( category == null )
            {
                _LOG.LogWarning("Category not found for deletion: {CategoryId}", id);
                return Result<bool>.Error(404, "Category not found");
            }

            if ( category.Children.Any() || category.Products.Any() )
            {
                _LOG.LogWarning("Cannot delete category with children or products: {CategoryId}", id);
                return Result<bool>.Error(400, "Cannot delete category with children or products");
            }

            _DB.Categories.Remove(category);
            await _DB.SaveChangesAsync();

            _LOG.LogInformation("Category deleted: {CategoryId}", id);

            await transaction.CommitAsync();
            return Result<bool>.Success(200, true);
        }
        catch ( Exception ex )
        {
            await transaction.RollbackAsync();
            _LOG.LogError(ex, "Error deleting category: {CategoryId}", id);
            return Result<bool>.Error(500, "Failed to delete category");
        }
    }


    public async Task<Result<ProductImageResponse>> CreateProductImage( CreateProductImageRequest request )
    {
        var validationResult = await _createProductImageValidator.ValidateAsync(request);
        if ( !validationResult.IsValid )
        {
            var validationErrors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Result<ProductImageResponse>.Error(400, "Validation failed", validationErrors);
        }

        using var transaction = await _DB.Database.BeginTransactionAsync();

        try
        {
            var productExists = await _DB.Products.AnyAsync(p => p.Id == request.ProductId);
            if ( !productExists )
            {
                _LOG.LogWarning("Product not found: {ProductId}", request.ProductId);
                return Result<ProductImageResponse>.Error(404, "Product not found");
            }

            var productImage = new ProductImage
            {
                ImageUrl = request.ImageUrl,
                IsMain = request.IsMain,
                SortOrder = request.SortOrder,
                ProductId = request.ProductId
            };

            _DB.ProductImages.Add(productImage);
            await _DB.SaveChangesAsync();

            await transaction.CommitAsync();
            _LOG.LogInformation("Product image created: {ProductImageId} for product {ProductId}", productImage.Id, request.ProductId);

            var response = new ProductImageResponse(
                Id: productImage.Id,
                ImageUrl: productImage.ImageUrl,
                IsMain: productImage.IsMain,
                SortOrder: productImage.SortOrder,
                ProductId: productImage.ProductId
            );

            return Result<ProductImageResponse>.Success(201, response);
        }
        catch ( Exception ex )
        {
            await transaction.RollbackAsync();
            _LOG.LogError(ex, "Error creating product image for product: {ProductId}", request.ProductId);
            return Result<ProductImageResponse>.Error(500, "Failed to create product image");
        }
    }

    public async Task<Result<ProductImageResponse>> UpdateProductImage( int id, UpdateProductImageRequest request )
    {
        var validationResult = await _updateProductImageValidator.ValidateAsync(request);
        if ( !validationResult.IsValid )
        {
            var validationErrors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return Result<ProductImageResponse>.Error(400, "Validation failed", validationErrors);
        }

        using var transaction = await _DB.Database.BeginTransactionAsync();

        try
        {
            var productImage = await _DB.ProductImages.FirstOrDefaultAsync(pi => pi.Id == id);

            if ( productImage == null )
            {
                _LOG.LogWarning("Product image not found: {ProductImageId}", id);
                return Result<ProductImageResponse>.Error(404, "Product image not found");
            }

            if ( request.ImageUrl != null )
                productImage.ImageUrl = request.ImageUrl;
            if ( request.IsMain.HasValue )
                productImage.IsMain = request.IsMain.Value;
            if ( request.SortOrder.HasValue )
                productImage.SortOrder = request.SortOrder.Value;

            productImage.UpdatedAt = DateTime.UtcNow;

            _DB.ProductImages.Update(productImage);
            await _DB.SaveChangesAsync();

            await transaction.CommitAsync();
            _LOG.LogInformation("Product image updated: {ProductImageId}", id);

            var response = new ProductImageResponse(
                Id: productImage.Id,
                ImageUrl: productImage.ImageUrl,
                IsMain: productImage.IsMain,
                SortOrder: productImage.SortOrder,
                ProductId: productImage.ProductId
            );

            return Result<ProductImageResponse>.Success(200, response);
        }
        catch ( Exception ex )
        {
            await transaction.RollbackAsync();
            _LOG.LogError(ex, "Error updating product image: {ProductImageId}", id);
            return Result<ProductImageResponse>.Error(500, "Failed to update product image");
        }
    }

    public async Task<Result<bool>> DeleteProductImage( int id )
    {
        using var transaction = await _DB.Database.BeginTransactionAsync();

        try
        {
            var productImage = await _DB.ProductImages.FirstOrDefaultAsync(pi => pi.Id == id);

            if ( productImage == null )
            {
                _LOG.LogWarning("Product image not found: {ProductImageId}", id);
                return Result<bool>.Error(404, "Product image not found");
            }

            _DB.ProductImages.Remove(productImage);
            await _DB.SaveChangesAsync();

            await transaction.CommitAsync();
            _LOG.LogInformation("Product image deleted: {ProductImageId}", id);

            return Result<bool>.Success(200, true);
        }
        catch ( Exception ex )
        {
            await transaction.RollbackAsync();
            _LOG.LogError(ex, "Error deleting product image: {ProductImageId}", id);
            return Result<bool>.Error(500, "Failed to delete product image");
        }
    }


    public async Task<Result<UserDetailResponse>> GetUserById( int id )
    {
        try
        {
            var user = await _DB.Users
                .Include(u => u.UserDetails)
                .Include(u => u.CartItems)
                .ThenInclude(ci => ci.Product)
                .ThenInclude(p => p.Images)
                .Include(u => u.Orders)
                .ThenInclude(o => o.OrderItems)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if ( user == null )
            {
                _LOG.LogWarning("User not found: {UserId}", id);
                return Result<UserDetailResponse>.Error(404, "User not found");
            }

            CartResponse? cartResponse = null;
            if ( user.CartItems.Any() )
            {
                var cartItems = user.CartItems.Select(ci => new CartItemResponse(
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

            var orders = user.Orders.Select(o => new OrderResponse(
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

            var response = _MAP.MapUserDetailResponse(user, cartResponse, orders);
            return Result<UserDetailResponse>.Success(200, response);
        }
        catch ( Exception ex )
        {
            _LOG.LogError(ex, "Error retrieving user: {UserId}", id);
            return Result<UserDetailResponse>.Error(500, "Failed to retrieve user");
        }
    }

    public async Task<Result<AllUsersResponse>> GetAllUsers()
    {
        try
        {
            var users = await _DB.Users
                .Include(u => u.UserDetails)
                .AsNoTracking()
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            var userResponses = users.Select(user => _MAP.MapUserMinimalResponse(user)).ToList();

            var response = new AllUsersResponse(userResponses, userResponses.Count);
            return Result<AllUsersResponse>.Success(200, response);
        }
        catch ( Exception ex )
        {
            _LOG.LogError(ex, "Error retrieving users");
            return Result<AllUsersResponse>.Error(500, "Failed to retrieve users");
        }
    }

    public async Task<Result<AdminOrderResponse>> GetOrderById( int id )
    {
        try
        {
            var order = await _DB.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.User)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);

            if ( order == null )
            {
                _LOG.LogWarning("Order not found: {OrderId}", id);
                return Result<AdminOrderResponse>.Error(404, "Order not found");
            }

            var response = new AdminOrderResponse(
                Id: order.Id,
                Status: order.Status.ToString(),
                ShippingAddress: order.ShippingAddress,
                TotalPrice: order.TotalPrice,
                UserId: order.UserId,
                UserEmail: order.User.Email,
                Items: order.OrderItems.Select(oi => new OrderItemResponse(
                    Id: oi.Id,
                    ProductId: oi.ProductId,
                    ProductName: oi.ProductName,
                    ProductPriceSnapshot: oi.ProductPriceSnapshot,
                    Quantity: oi.Quantity,
                    SubTotal: oi.ProductPriceSnapshot * oi.Quantity
                )).ToList(),
                CreatedAt: order.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            );

            return Result<AdminOrderResponse>.Success(200, response);
        }
        catch ( Exception ex )
        {
            _LOG.LogError(ex, "Error retrieving order: {OrderId}", id);
            return Result<AdminOrderResponse>.Error(500, "Failed to retrieve order");
        }
    }

    public async Task<Result<AllOrdersResponse>> GetAllOrders()
    {
        try
        {
            var orders = await _DB.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.User)
                .AsNoTracking()
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var orderResponses = orders.Select(order => new AdminOrderResponse(
                Id: order.Id,
                Status: order.Status.ToString(),
                ShippingAddress: order.ShippingAddress,
                TotalPrice: order.TotalPrice,
                UserId: order.UserId,
                UserEmail: order.User.Email,
                Items: order.OrderItems.Select(oi => new OrderItemResponse(
                    Id: oi.Id,
                    ProductId: oi.ProductId,
                    ProductName: oi.ProductName,
                    ProductPriceSnapshot: oi.ProductPriceSnapshot,
                    Quantity: oi.Quantity,
                    SubTotal: oi.ProductPriceSnapshot * oi.Quantity
                )).ToList(),
                CreatedAt: order.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            )).ToList();

            var response = new AllOrdersResponse(orderResponses, orderResponses.Count);
            return Result<AllOrdersResponse>.Success(200, response);
        }
        catch ( Exception ex )
        {
            _LOG.LogError(ex, "Error retrieving orders");
            return Result<AllOrdersResponse>.Error(500, "Failed to retrieve orders");
        }
    }
}