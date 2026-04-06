using Order.API.DTOs;

namespace Order.API.Helpers;

public interface IOrderMapHelper
{
    OrderResponse MapOrderResponse( Entities.Order order, string currency );
}