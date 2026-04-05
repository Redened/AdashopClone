using Adashop.Common.Helpers.CategoryTree;
using Adashop.Common.Helpers.ExchangeRateAPI;
using Adashop.Common.Mappers;
using Adashop.Common.Results;
using Adashop.Data;
using Adashop.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Adashop.Services.Product;

public class ProductServices : IProductServices
{
    private readonly DataContext _DB;
    private readonly ILogger<ProductServices> _LOG;
    private readonly ICategoryTreeHelper _categoryTreeHelper;
    private readonly IExchangeRateHelper _exchangeRateHelper;
    private readonly IMapHelper _MAP;

    private const int CATEGORY_PAGE_SIZE = 20;
    private const int SEARCH_PAGE_SIZE = 20;
    private const int SEARCH_SUGGESTION_LIMIT = 5;

    public ProductServices( DataContext DB, ILogger<ProductServices> LOG, ICategoryTreeHelper categoryTreeHelper, IExchangeRateHelper exchangeRateHelper, IMapHelper MAP )
    {
        _DB = DB;
        _LOG = LOG;
        _categoryTreeHelper = categoryTreeHelper;
        _exchangeRateHelper = exchangeRateHelper;
        _MAP = MAP;
    }

    public async Task<Result<ProductDetailResponse>> GetProductById( int id, string currency = "GEL" )
    {
        try
        {
            var product = await _DB.Products
                .Include(p => p.Images)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if ( product == null )
            {
                _LOG.LogWarning("Product not found: {ProductId}", id);
                return Result<ProductDetailResponse>.Error(404, "Product not found");
            }

            var response = _MAP.MapProductDetailResponse(product);
            var breadcrumbs = await _categoryTreeHelper.GetCategoryBreadcrumbs(product.CategoryId);
            response = response with { Breadcrumbs = breadcrumbs };

            var convertedResponse = await _exchangeRateHelper.ApplyCurrencyConversion(response, currency);

            return Result<ProductDetailResponse>.Success(200, convertedResponse);
        }
        catch ( Exception ex )
        {
            _LOG.LogError(ex, "Error retrieving product: {ProductId}", id);
            return Result<ProductDetailResponse>.Error(500, "Failed to retrieve product");
        }
    }

    public async Task<Result<PagedResult<ProductMinimalResponse>>> GetProductsByCategory( int categoryId, int page, string currency = "GEL" )
    {
        try
        {
            var categoryExists = await _DB.Categories.AnyAsync(c => c.Id == categoryId);
            if ( !categoryExists )
            {
                _LOG.LogWarning("Category not found: {CategoryId}", categoryId);
                return Result<PagedResult<ProductMinimalResponse>>.Error(404, "Category not found");
            }

            var categoryIds = await _categoryTreeHelper.GetCategoryWithDescendants(categoryId);

            var query = _DB.Products
                .Include(p => p.Images)
                .Where(p => categoryIds.Contains(p.CategoryId))
                .OrderBy(p => p.Name);

            var totalCount = await query.CountAsync();

            var products = await query
                .Skip(( page - 1 ) * CATEGORY_PAGE_SIZE)
                .Take(CATEGORY_PAGE_SIZE)
                .ToListAsync();

            var items = products.Select(p => _MAP.MapProductMinimalResponse(p)).ToList();
            items = await _exchangeRateHelper.ApplyCurrencyConversionToList(items, currency);

            var result = new PagedResult<ProductMinimalResponse>(
                items,
                totalCount,
                page,
                CATEGORY_PAGE_SIZE,
                (int)Math.Ceiling(totalCount / (double)CATEGORY_PAGE_SIZE)
            );

            return Result<PagedResult<ProductMinimalResponse>>.Success(200, result);
        }
        catch ( Exception ex )
        {
            _LOG.LogError(ex, "Error retrieving products for category: {CategoryId}", categoryId);
            return Result<PagedResult<ProductMinimalResponse>>.Error(500, "Failed to retrieve products");
        }
    }

    public async Task<Result<List<ProductMinimalResponse>>> GetSearchSuggestions( string searchTerm, string currency = "GEL" )
    {
        try
        {
            if ( string.IsNullOrWhiteSpace(searchTerm) )
            {
                return Result<List<ProductMinimalResponse>>.Error(400, "Search term is required");
            }

            var products = await _DB.Products
                .Include(p => p.Images)
                .Where(p => p.Name.Contains(searchTerm))
                .OrderBy(p => p.Name)
                .Take(SEARCH_SUGGESTION_LIMIT)
                .ToListAsync();

            var items = products.Select(p => _MAP.MapProductMinimalResponse(p)).ToList();
            items = await _exchangeRateHelper.ApplyCurrencyConversionToList(items, currency);

            return Result<List<ProductMinimalResponse>>.Success(200, items);
        }
        catch ( Exception ex )
        {
            _LOG.LogError(ex, "Error getting search suggestions: {SearchTerm}", searchTerm);
            return Result<List<ProductMinimalResponse>>.Error(500, "Search failed");
        }
    }

    public async Task<Result<PagedResult<ProductMinimalResponse>>> SearchProducts( string searchTerm, int page, string currency = "GEL" )
    {
        try
        {
            if ( string.IsNullOrWhiteSpace(searchTerm) )
            {
                return Result<PagedResult<ProductMinimalResponse>>.Error(400, "Search term is required");
            }

            var query = _DB.Products
                .Include(p => p.Images)
                .Where(p =>
                    p.Name.Contains(searchTerm) ||
                    ( p.Description != null && p.Description.Contains(searchTerm) ))
                .OrderBy(p => p.Name);

            var totalCount = await query.CountAsync();

            var products = await query
                .Skip(( page - 1 ) * SEARCH_PAGE_SIZE)
                .Take(SEARCH_PAGE_SIZE)
                .ToListAsync();

            var items = products.Select(p => _MAP.MapProductMinimalResponse(p)).ToList();
            items = await _exchangeRateHelper.ApplyCurrencyConversionToList(items, currency);

            var result = new PagedResult<ProductMinimalResponse>(
                items,
                totalCount,
                page,
                SEARCH_PAGE_SIZE,
                (int)Math.Ceiling(totalCount / (double)SEARCH_PAGE_SIZE)
            );

            return Result<PagedResult<ProductMinimalResponse>>.Success(200, result);
        }
        catch ( Exception ex )
        {
            _LOG.LogError(ex, "Error searching products: {SearchTerm}", searchTerm);
            return Result<PagedResult<ProductMinimalResponse>>.Error(500, "Search failed");
        }
    }

    public async Task<Result<List<CategoryResponse>>> GetAllCategories()
    {
        try
        {
            var categories = await _DB.Categories.ToListAsync();
            var items = categories.Select(c => new CategoryResponse(
                Id: c.Id,
                Name: c.Name,
                ParentCategoryId: c.ParentCategoryId
            )).ToList();

            return Result<List<CategoryResponse>>.Success(200, items);
        }
        catch ( Exception ex )
        {
            _LOG.LogError(ex, "Error retrieving categories");
            return Result<List<CategoryResponse>>.Error(500, "Failed to retrieve categories");
        }
    }

    public async Task<Result<List<CategoryTreeResponse>>> GetCategoryTree()
    {
        try
        {
            var allCategories = await _DB.Categories.ToListAsync();
            var rootCategories = allCategories.Where(c => c.ParentCategoryId == null);

            var tree = rootCategories.Select(c => _categoryTreeHelper.BuildCategoryTree(c, allCategories)).ToList();

            return Result<List<CategoryTreeResponse>>.Success(200, tree);
        }
        catch ( Exception ex )
        {
            _LOG.LogError(ex, "Error building category tree");
            return Result<List<CategoryTreeResponse>>.Error(500, "Failed to retrieve category tree");
        }
    }

    public async Task<Result<CategoryDetailResponse>> GetCategoryById( int id )
    {
        try
        {
            var category = await _DB.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.Children)
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if ( category == null )
            {
                _LOG.LogWarning("Category not found: {CategoryId}", id);
                return Result<CategoryDetailResponse>.Error(404, "Category not found");
            }

            var dto = new CategoryDetailResponse(
                Id: category.Id,
                Name: category.Name,
                ParentCategory: category.ParentCategory != null ? new CategoryResponse(
                    Id: category.ParentCategory.Id,
                    Name: category.ParentCategory.Name,
                    ParentCategoryId: category.ParentCategory.ParentCategoryId
                ) : null,
                Children: category.Children.Select(child => new CategoryResponse(
                    Id: child.Id,
                    Name: child.Name,
                    ParentCategoryId: child.ParentCategoryId
                )).ToList(),
                ProductCount: category.Products.Count
            );

            return Result<CategoryDetailResponse>.Success(200, dto);
        }
        catch ( Exception ex )
        {
            _LOG.LogError(ex, "Error retrieving category: {CategoryId}", id);
            return Result<CategoryDetailResponse>.Error(500, "Failed to retrieve category");
        }
    }
}