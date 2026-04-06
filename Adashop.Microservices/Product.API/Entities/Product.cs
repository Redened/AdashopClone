using Adashop.Shared.Entities;

namespace Product.API.Entities;

public class Product : BaseEntity
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public int Stock { get; set; }
    public decimal Price { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public List<ProductImage> Images { get; set; } = [];
}