using Product.API.DTOs;
using Product.API.Entities;

namespace Product.API.Helpers;

public interface IProductMapHelper
{
    ProductDetailResponse MapProductDetailResponse( Entities.Product product );
    ProductMinimalResponse MapProductMinimalResponse( Entities.Product product );
    CategoryDetailResponse MapCategoryDetailResponse( Category category );
}
