using Adashop.Common.Results;
using Adashop.DTOs;

namespace Adashop.Services.Admin;

public interface IAdminServices
{
    Task<Result<UserDetailResponse>> GetUserById( int id );
    Task<Result<AllUsersResponse>> GetAllUsers();
    Task<Result<AdminOrderResponse>> GetOrderById( int orderId );
    Task<Result<AllOrdersResponse>> GetAllOrders();

    Task<Result<ProductDetailResponse>> CreateProduct( CreateProductRequest request );
    Task<Result<ProductDetailResponse>> UpdateProduct( int id, UpdateProductRequest request );
    Task<Result<bool>> DeleteProduct( int id );

    Task<Result<CategoryDetailResponse>> CreateCategory( CreateCategoryRequest request );
    Task<Result<CategoryDetailResponse>> UpdateCategory( int id, UpdateCategoryRequest request );
    Task<Result<bool>> DeleteCategory( int id );

    Task<Result<ProductImageResponse>> CreateProductImage( CreateProductImageRequest request );
    Task<Result<ProductImageResponse>> UpdateProductImage( int id, UpdateProductImageRequest request );
    Task<Result<bool>> DeleteProductImage( int id );
}