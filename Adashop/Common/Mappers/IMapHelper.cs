using Adashop.DTOs;
using Adashop.Entities;

namespace Adashop.Common.Mappers;

public interface IMapHelper
{
    ProductDetailResponse MapProductDetailResponse( Product product );
    ProductMinimalResponse MapProductMinimalResponse( Product product );
    CategoryDetailResponse MapCategoryDetailResponse( Category category );

    UserMinimalResponse MapUserMinimalResponse( User user );
    UserDetailResponse MapUserDetailResponse( User user, CartResponse? cart, List<OrderResponse> orders );

    CartItemResponse MapCartItemResponse( CartItem cartItem, string currency );

    OrderResponse MapOrderResponse( Order order, string currency );
}