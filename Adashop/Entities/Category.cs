using Adashop.Common.Entities;

namespace Adashop.Entities;

public class Category : BaseEntity
{
    public required string Name { get; set; }

    public int? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }

    public List<Category> Children { get; set; } = [];
    public List<Product> Products { get; set; } = [];
}