using Adashop.Data;
using Adashop.DTOs;
using Adashop.Entities;
using Microsoft.EntityFrameworkCore;

namespace Adashop.Common.Services.Helpers;

public interface ICategoryHelper
{
    Task<List<BreadcrumbResponse>> GetCategoryBreadcrumbs( int categoryId );
    Task<List<int>> GetCategoryWithDescendants( int categoryId );
    CategoryTreeResponse BuildCategoryTree( Category category, List<Category> allCategories );
}

public class CategoryHelper : ICategoryHelper
{
    private readonly DataContext _DB;

    public CategoryHelper( DataContext DB ) => _DB = DB;


    public async Task<List<BreadcrumbResponse>> GetCategoryBreadcrumbs( int categoryId )
    {
        var breadcrumbs = new List<BreadcrumbResponse>();
        var currentCategoryId = (int?)categoryId;

        while ( currentCategoryId.HasValue )
        {
            var category = await _DB.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == currentCategoryId.Value);

            if ( category == null ) break;

            breadcrumbs.Insert(0, new BreadcrumbResponse(category.Id, category.Name));
            currentCategoryId = category.ParentCategoryId;
        }

        return breadcrumbs;
    }

    public async Task<List<int>> GetCategoryWithDescendants( int categoryId )
    {
        var result = new List<int> { categoryId };
        var children = await _DB.Categories
            .Where(c => c.ParentCategoryId == categoryId)
            .Select(c => c.Id)
            .ToListAsync();

        foreach ( var childId in children )
        {
            result.AddRange(await GetCategoryWithDescendants(childId));
        }

        return result;
    }

    public CategoryTreeResponse BuildCategoryTree( Category category, List<Category> allCategories )
    {
        var children = allCategories
            .Where(c => c.ParentCategoryId == category.Id)
            .Select(c => BuildCategoryTree(c, allCategories))
            .ToList();

        return new CategoryTreeResponse(
            category.Id,
            category.Name,
            category.ParentCategoryId,
            children
        );
    }
}
