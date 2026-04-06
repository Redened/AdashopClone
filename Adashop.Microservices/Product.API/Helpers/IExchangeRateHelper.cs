using Product.API.DTOs;

namespace Product.API.Helpers;

public interface IExchangeRateHelper
{
    Task<ProductDetailResponse> ApplyCurrencyConversion( ProductDetailResponse product, string currency );
    Task<List<ProductMinimalResponse>> ApplyCurrencyConversionToList( List<ProductMinimalResponse> products, string currency );
}