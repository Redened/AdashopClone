using Product.API.DTOs;
using Product.API.Entities;

namespace Product.API.Helpers;

public interface ICategoryTreeHelper
{
    Task<List<BreadcrumbResponse>> GetCategoryBreadcrumbs( int categoryId );
    Task<List<int>> GetCategoryWithDescendants( int categoryId );
    CategoryTreeResponse BuildCategoryTree( Category category, List<Category> allCategories );
}