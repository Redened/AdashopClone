using Adashop.Services.Product;
using Microsoft.AspNetCore.Mvc;

namespace Adashop.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly IProductServices _productServices;

    public ProductController( IProductServices productServices ) => _productServices = productServices;

    /// <summary>
    /// Retrieves detailed information about a specific product including images, category, and breadcrumbs.
    /// </summary>
    /// <param name="id">The product ID</param>
    /// <param name="currency">Currency for price conversion (GEL or USD, default: GEL)</param>
    /// <returns>ProductDetailResponse containing complete product information</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct( int id, [FromQuery] string currency = "GEL" )
    {
        var result = await _productServices.GetProductById(id, currency);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Retrieves paginated products for a specific category and its subcategories.
    /// </summary>
    /// <param name="categoryId">The category ID</param>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="currency">Currency for price conversion (GEL or USD, default: GEL)</param>
    /// <returns>PagedResult containing list of ProductMinimalResponse and pagination info</returns>
    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetProductsByCategory( int categoryId, [FromQuery] int page = 1, [FromQuery] string currency = "GEL" )
    {
        var result = await _productServices.GetProductsByCategory(categoryId, page, currency);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Returns search suggestions (up to 5) matching the search term.
    /// </summary>
    /// <param name="q">Search query string</param>
    /// <param name="currency">Currency for price conversion (GEL or USD, default: GEL)</param>
    /// <returns>List of ProductMinimalResponse matching the search term</returns>
    [HttpGet("search/suggestions")]
    public async Task<IActionResult> GetSearchSuggestions( [FromQuery] string q, [FromQuery] string currency = "GEL" )
    {
        var result = await _productServices.GetSearchSuggestions(q, currency);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Searches for products by name or description with pagination.
    /// </summary>
    /// <param name="q">Search query string</param>
    /// <param name="page">Page number for pagination (default: 1)</param>
    /// <param name="currency">Currency for price conversion (GEL or USD, default: GEL)</param>
    /// <returns>PagedResult containing list of ProductMinimalResponse and pagination info</returns>
    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts( [FromQuery] string q, [FromQuery] int page = 1, [FromQuery] string currency = "GEL" )
    {
        var result = await _productServices.SearchProducts(q, page, currency);
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Retrieves all product categories as a flat list.
    /// </summary>
    /// <returns>List of CategoryResponse</returns>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var result = await _productServices.GetAllCategories();
        return StatusCode(result.Status, result);
    }

    /// <summary>
    /// Retrieves all product categories organized in a hierarchical tree structure.
    /// </summary>
    /// <returns>List of CategoryTreeResponse with nested children</returns>
    [HttpGet("categories/tree")]
    public async Task<IActionResult> GetCategoryTree()
    {
        var result = await _productServices.GetCategoryTree();
        return StatusCode(result.Status, result);
    }
}