using Order.API.DTOs;

namespace Order.API.Helpers;

public interface IExchangeRateHelper
{
    Task<OrderResponse> ApplyCurrencyConversion( OrderResponse order, string currency );
    Task<List<OrderResponse>> ApplyCurrencyConversionToList( List<OrderResponse> orders, string currency );
}