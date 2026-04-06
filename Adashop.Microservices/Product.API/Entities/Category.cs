using Adashop.Shared.Entities;

namespace Product.API.Entities;

public class Category : BaseEntity
{
    public required string Name { get; set; }

    public int? ParentCategoryId { get; set; }
    public Category? ParentCategory { get; set; }

    public List<Category> Children { get; set; } = [];
    public List<Product> Products { get; set; } = [];
}