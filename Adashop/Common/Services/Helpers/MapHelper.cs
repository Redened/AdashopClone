using Adashop.DTOs;
using Adashop.Entities;

namespace Adashop.Common.Services.Helpers;

public interface IMapHelper
{
    ProductDetailResponse MapProductDetailResponse( Product product );
    ProductMinimalResponse MapProductMinimalResponse( Product product );
    CategoryDetailResponse MapCategoryDetailResponse( Category category );

    UserMinimalResponse MapUserMinimalResponse( User user );
    UserDetailResponse MapUserDetailResponse( User user, CartResponse? cart, List<OrderResponse> orders );
}

public class MapHelper : IMapHelper
{
    public ProductDetailResponse MapProductDetailResponse( Product product )
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

    public ProductMinimalResponse MapProductMinimalResponse( Product product )
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

    public UserMinimalResponse MapUserMinimalResponse( User user )
    {
        return new UserMinimalResponse(
            Id: user.Id,
            Email: user.Email,
            Role: user.Role.ToString(),
            IsVerified: user.IsVerified,
            FirstName: user.UserDetails?.FirstName,
            LastName: user.UserDetails?.LastName,
            PhoneNumber: user.UserDetails?.PhoneNumber,
            Address: user.UserDetails?.Address,
            LastLoginAt: user.LastLoginAt,
            CreatedAt: user.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
        );
    }

    public UserDetailResponse MapUserDetailResponse( User user, CartResponse? cart, List<OrderResponse> orders )
    {
        return new UserDetailResponse(
            Id: user.Id,
            Email: user.Email,
            Role: user.Role.ToString(),
            IsVerified: user.IsVerified,
            FirstName: user.UserDetails?.FirstName,
            LastName: user.UserDetails?.LastName,
            PhoneNumber: user.UserDetails?.PhoneNumber,
            Address: user.UserDetails?.Address,
            LastLoginAt: user.LastLoginAt,
            CreatedAt: user.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
            Cart: cart,
            Orders: orders
        );
    }
}
