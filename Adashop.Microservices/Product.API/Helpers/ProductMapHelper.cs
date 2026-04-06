using Product.API.DTOs;
using Product.API.Entities;

namespace Product.API.Helpers;

public class ProductMapHelper : IProductMapHelper
{
    public ProductDetailResponse MapProductDetailResponse( Entities.Product product )
    {
        var mainImage = product.Images.FirstOrDefault(i => i.IsMain);
        return new ProductDetailResponse(
            Id: product.Id,
            Name: product.Name,
            Description: product.Description,
            Price: product.Price,
            Stock: product.Stock,
            MainImageUrl: mainImage?.ImageUrl,
            Images: product.Images.Select(img => new ProductImageResponse(
                Id: img.Id,
                ImageUrl: img.ImageUrl,
                IsMain: img.IsMain,
                SortOrder: img.SortOrder,
                ProductId: img.ProductId
            )).ToList(),
            Breadcrumbs: [],
            Category: product.Category != null ? new CategoryResponse(
                Id: product.Category.Id,
                Name: product.Category.Name,
                ParentCategoryId: product.Category.ParentCategoryId
            ) : null,
            Currency: "GEL"
        );
    }

    public ProductMinimalResponse MapProductMinimalResponse( Entities.Product product )
    {
        var thumbnailUrl = product.Images.FirstOrDefault(i => i.IsMain)?.ImageUrl;
        return new ProductMinimalResponse(
            Id: product.Id,
            Name: product.Name,
            Price: product.Price,
            ThumbnailUrl: thumbnailUrl,
            Currency: "GEL"
        );
    }

    public CategoryDetailResponse MapCategoryDetailResponse( Category category )
    {
        return new CategoryDetailResponse(
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
    }
}
