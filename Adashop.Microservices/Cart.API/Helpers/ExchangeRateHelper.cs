using Adashop.Shared.Services;
using Cart.API.DTOs;

namespace Cart.API.Helpers;

public class ExchangeRateHelper : IExchangeRateHelper
{
    private readonly IExchangeRateService _EXCHANGE_RATE;

    public ExchangeRateHelper( IExchangeRateService EXCHANGE_RATE ) => _EXCHANGE_RATE = EXCHANGE_RATE;


    public async Task<CartResponse> ApplyCurrencyConversion( CartResponse cart, string currency )
    {
        if ( currency.Equals("GEL", StringComparison.OrdinalIgnoreCase) || !currency.Equals("USD", StringComparison.OrdinalIgnoreCase) )
        {
            return cart with { Currency = "GEL" };
        }

        if ( currency.Equals("USD", StringComparison.OrdinalIgnoreCase) )
        {
            var rate = await _EXCHANGE_RATE.GetRateAsync();
            var convertedItems = cart.Items.Select(item => item with
            {
                ProductPrice = Math.Round(item.ProductPrice * rate, 2),
                SubTotal = Math.Round(item.SubTotal * rate, 2),
                Currency = "USD"
            }).ToList();

            var convertedTotalPrice = Math.Round(cart.TotalPrice * rate, 2);
            return cart with { Items = convertedItems, TotalPrice = convertedTotalPrice, Currency = "USD" };
        }

        return cart;
    }
}