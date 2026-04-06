using Adashop.Shared.Services;
using Order.API.DTOs;

namespace Order.API.Helpers;

public class ExchangeRateHelper : IExchangeRateHelper
{
    private readonly IExchangeRateService _EXCHANGE_RATE;

    public ExchangeRateHelper( IExchangeRateService EXCHANGE_RATE ) => _EXCHANGE_RATE = EXCHANGE_RATE;


    public async Task<OrderResponse> ApplyCurrencyConversion( OrderResponse order, string currency )
    {
        if ( currency.Equals("GEL", StringComparison.OrdinalIgnoreCase) || !currency.Equals("USD", StringComparison.OrdinalIgnoreCase) )
        {
            return order with { Currency = "GEL" };
        }

        if ( currency.Equals("USD", StringComparison.OrdinalIgnoreCase) )
        {
            var rate = await _EXCHANGE_RATE.GetRateAsync();
            var convertedItems = order.Items.Select(item => item with
            {
                ProductPriceSnapshot = Math.Round(item.ProductPriceSnapshot * rate, 2),
                SubTotal = Math.Round(item.SubTotal * rate, 2)
            }).ToList();

            var convertedTotalPrice = Math.Round(order.TotalPrice * rate, 2);
            return order with { Items = convertedItems, TotalPrice = convertedTotalPrice, Currency = "USD" };
        }

        return order;
    }

    public async Task<List<OrderResponse>> ApplyCurrencyConversionToList( List<OrderResponse> orders, string currency )
    {
        if ( currency.Equals("GEL", StringComparison.OrdinalIgnoreCase) || !currency.Equals("USD", StringComparison.OrdinalIgnoreCase) )
        {
            return orders.Select(o => o with { Currency = "GEL" }).ToList();
        }

        if ( currency.Equals("USD", StringComparison.OrdinalIgnoreCase) )
        {
            var rate = await _EXCHANGE_RATE.GetRateAsync();
            return orders.Select(o =>
            {
                var convertedItems = o.Items.Select(item => item with
                {
                    ProductPriceSnapshot = Math.Round(item.ProductPriceSnapshot * rate, 2),
                    SubTotal = Math.Round(item.SubTotal * rate, 2)
                }).ToList();

                var convertedTotalPrice = Math.Round(o.TotalPrice * rate, 2);
                return o with { Items = convertedItems, TotalPrice = convertedTotalPrice, Currency = "USD" };
            }).ToList();
        }

        return orders;
    }
}
