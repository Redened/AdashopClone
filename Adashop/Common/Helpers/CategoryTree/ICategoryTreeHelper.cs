using Adashop.DTOs;
using Adashop.Entities;

namespace Adashop.Common.Helpers.CategoryTree;

public interface ICategoryTreeHelper
{
    Task<List<BreadcrumbResponse>> GetCategoryBreadcrumbs( int categoryId );
    Task<List<int>> GetCategoryWithDescendants( int categoryId );
    CategoryTreeResponse BuildCategoryTree( Category category, List<Category> allCategories );
}