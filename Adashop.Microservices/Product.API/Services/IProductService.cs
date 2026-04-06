using Adashop.Shared.Results;
using Product.API.DTOs;

namespace Product.API.Services;

public interface IProductService
{
    Task<Result<ProductDetailResponse>> GetProductById( int id, string currency = "GEL" );
    Task<Result<PagedResult<ProductMinimalResponse>>> GetProductsByCategory( int categoryId, int page, string currency = "GEL" );
    Task<Result<List<ProductMinimalResponse>>> GetSearchSuggestions( string searchTerm, string currency = "GEL" );
    Task<Result<PagedResult<ProductMinimalResponse>>> SearchProducts( string searchTerm, int page, string currency = "GEL" );
    Task<Result<List<CategoryResponse>>> GetAllCategories();
    Task<Result<List<CategoryTreeResponse>>> GetCategoryTree();
    Task<Result<CategoryDetailResponse>> GetCategoryById( int id );
}

