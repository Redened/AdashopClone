namespace User.API.Helpers;

public interface IUserMapHelper
{
    UserMinimalResponse MapUserMinimalResponse( User user );
    UserDetailResponse MapUserDetailResponse( User user, CartResponse? cart, List<OrderResponse> orders );
}