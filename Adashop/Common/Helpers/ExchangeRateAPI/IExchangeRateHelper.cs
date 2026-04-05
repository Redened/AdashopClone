using Adashop.DTOs;

namespace Adashop.Common.Helpers.ExchangeRateAPI;

public interface IExchangeRateHelper
{
    Task<ProductDetailResponse> ApplyCurrencyConversion( ProductDetailResponse product, string currency );
    Task<CartResponse> ApplyCurrencyConversion( CartResponse cart, string currency );
    Task<OrderResponse> ApplyCurrencyConversion( OrderResponse order, string currency );
    Task<List<ProductMinimalResponse>> ApplyCurrencyConversionToList( List<ProductMinimalResponse> products, string currency );
    Task<List<OrderResponse>> ApplyCurrencyConversionToList( List<OrderResponse> orders, string currency );
}