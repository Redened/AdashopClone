using Adashop.Shared.Services;
using Product.API.DTOs;
using Product.API.Helpers;

namespace Adashop.Common.Helpers.ExchangeRateAPI;

public class ExchangeRateHelper : IExchangeRateHelper
{
    private readonly IExchangeRateService _EXCHANGE_RATE;

    public ExchangeRateHelper( IExchangeRateService EXCHANGE_RATE ) => _EXCHANGE_RATE = EXCHANGE_RATE;


    public async Task<ProductDetailResponse> ApplyCurrencyConversion( ProductDetailResponse product, string currency )
    {
        if ( currency.Equals("GEL", StringComparison.OrdinalIgnoreCase) || !currency.Equals("USD", StringComparison.OrdinalIgnoreCase) )
        {
            return product with { Currency = "GEL" };
        }

        if ( currency.Equals("USD", StringComparison.OrdinalIgnoreCase) )
        {
            var rate = await _EXCHANGE_RATE.GetRateAsync();
            var convertedPrice = Math.Round(product.Price * rate, 2);
            return product with { Price = convertedPrice, Currency = "USD" };
        }

        return product;
    }

    public async Task<List<ProductMinimalResponse>> ApplyCurrencyConversionToList( List<ProductMinimalResponse> products, string currency )
    {
        if ( currency.Equals("GEL", StringComparison.OrdinalIgnoreCase) || !currency.Equals("USD", StringComparison.OrdinalIgnoreCase) )
        {
            return products.Select(p => p with { Currency = "GEL" }).ToList();
        }

        if ( currency.Equals("USD", StringComparison.OrdinalIgnoreCase) )
        {
            var rate = await _EXCHANGE_RATE.GetRateAsync();
            return products.Select(p => p with
            {
                Price = Math.Round(p.Price * rate, 2),
                Currency = "USD"
            }).ToList();
        }

        return products;
    }
}
