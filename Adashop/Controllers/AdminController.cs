using Adashop.DTOs;
using Adashop.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Adashop.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminServices _adminServices;

    public AdminController( IAdminServices adminServices ) => _adminServices = adminServices;

    /// <summary>
    /// Retrieves detailed information about a specific user including their cart and orders.
    /// </summary>
    /// <param name="id">The user ID</param>
    /// <returns>UserDetailResponse containing user details, cart, and order history</returns>
    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUser( int id )
    {
        var result = await _adminServices.GetUserById(id);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Retrieves a list of all users with minimal information.
    /// </summary>
    /// <returns>AllUsersResponse containing list of UserMinimalResponse and total count</returns>
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var result = await _adminServices.GetAllUsers();
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Retrieves detailed information about a specific order.
    /// </summary>
    /// <param name="orderId">The order ID</param>
    /// <returns>AdminOrderResponse containing order details and items</returns>
    [HttpGet("orders/{orderId}")]
    public async Task<IActionResult> GetOrder( int orderId )
    {
        var result = await _adminServices.GetOrderById(orderId);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Retrieves a list of all orders in the system.
    /// </summary>
    /// <returns>AllOrdersResponse containing list of AdminOrderResponse and total count</returns>
    [HttpGet("orders")]
    public async Task<IActionResult> GetAllOrders()
    {
        var result = await _adminServices.GetAllOrders();
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Creates a new product with the provided details.
    /// </summary>
    /// <param name="request">CreateProductRequest containing product information</param>
    /// <returns>ProductDetailResponse containing the created product details</returns>
    [HttpPost("products")]
    public async Task<IActionResult> CreateProduct( CreateProductRequest request )
    {
        var result = await _adminServices.CreateProduct(request);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Updates an existing product with the provided details.
    /// </summary>
    /// <param name="id">The product ID to update</param>
    /// <param name="request">UpdateProductRequest containing updated product information</param>
    /// <returns>ProductDetailResponse containing the updated product details</returns>
    [HttpPut("products/{id}")]
    public async Task<IActionResult> UpdateProduct( int id, UpdateProductRequest request )
    {
        var result = await _adminServices.UpdateProduct(id, request);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Deletes a product from the system.
    /// </summary>
    /// <param name="id">The product ID to delete</param>
    /// <returns>Boolean indicating successful deletion</returns>
    [HttpDelete("products/{id}")]
    public async Task<IActionResult> DeleteProduct( int id )
    {
        var result = await _adminServices.DeleteProduct(id);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Creates a new product category.
    /// </summary>
    /// <param name="request">CreateCategoryRequest containing category information</param>
    /// <returns>CategoryDetailResponse containing the created category details</returns>
    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory( CreateCategoryRequest request )
    {
        var result = await _adminServices.CreateCategory(request);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Updates an existing product category.
    /// </summary>
    /// <param name="id">The category ID to update</param>
    /// <param name="request">UpdateCategoryRequest containing updated category information</param>
    /// <returns>CategoryDetailResponse containing the updated category details</returns>
    [HttpPut("categories/{id}")]
    public async Task<IActionResult> UpdateCategory( int id, UpdateCategoryRequest request )
    {
        var result = await _adminServices.UpdateCategory(id, request);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Deletes a product category (only if it has no children or products).
    /// </summary>
    /// <param name="id">The category ID to delete</param>
    /// <returns>Boolean indicating successful deletion</returns>
    [HttpDelete("categories/{id}")]
    public async Task<IActionResult> DeleteCategory( int id )
    {
        var result = await _adminServices.DeleteCategory(id);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Uploads and creates a new product image.
    /// </summary>
    /// <param name="request">CreateProductImageRequest containing image URL and metadata</param>
    /// <returns>ProductImageResponse containing the created image details</returns>
    [HttpPost("product-images")]
    public async Task<IActionResult> CreateProductImage( CreateProductImageRequest request )
    {
        var result = await _adminServices.CreateProductImage(request);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Updates an existing product image metadata.
    /// </summary>
    /// <param name="id">The product image ID to update</param>
    /// <param name="request">UpdateProductImageRequest containing updated image information</param>
    /// <returns>ProductImageResponse containing the updated image details</returns>
    [HttpPut("product-images/{id}")]
    public async Task<IActionResult> UpdateProductImage( int id, UpdateProductImageRequest request )
    {
        var result = await _adminServices.UpdateProductImage(id, request);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Deletes a product image from the system.
    /// </summary>
    /// <param name="id">The product image ID to delete</param>
    /// <returns>Boolean indicating successful deletion</returns>
    [HttpDelete("product-images/{id}")]
    public async Task<IActionResult> DeleteProductImage( int id )
    {
        var result = await _adminServices.DeleteProductImage(id);
        return StatusCode(result.Status, result);
    }
}