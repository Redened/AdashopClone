using Adashop.Common.Services.ExchangeRateAPI;
using Adashop.DTOs;

namespace Adashop.Common.Services.Helpers;

public interface ICurrencyHelper
{
    Task<ProductDetailResponse> ApplyCurrencyConversion( ProductDetailResponse product, string currency );
    Task<CartResponse> ApplyCurrencyConversion( CartResponse cart, string currency );
    Task<OrderResponse> ApplyCurrencyConversion( OrderResponse order, string currency );
    Task<List<ProductMinimalResponse>> ApplyCurrencyConversionToList( List<ProductMinimalResponse> products, string currency );
    Task<List<OrderResponse>> ApplyCurrencyConversionToList( List<OrderResponse> orders, string currency );
}

public class CurrencyHelper : ICurrencyHelper
{
    private readonly ICurrencyService _CURRENCY;

    public CurrencyHelper( ICurrencyService CURRENCY ) => _CURRENCY = CURRENCY;


    public async Task<ProductDetailResponse> ApplyCurrencyConversion( ProductDetailResponse product, string currency )
    {
        if ( currency.Equals("GEL", StringComparison.OrdinalIgnoreCase) || !currency.Equals("USD", StringComparison.OrdinalIgnoreCase) )
        {
            return product with { Currency = "GEL" };
        }

        if ( currency.Equals("USD", StringComparison.OrdinalIgnoreCase) )
        {
            var rate = await _CURRENCY.GetRateAsync();
            var convertedPrice = Math.Round(product.Price * rate, 2);
            return product with { Price = convertedPrice, Currency = "USD" };
        }

        return product;
    }

    public async Task<CartResponse> ApplyCurrencyConversion( CartResponse cart, string currency )
    {
        if ( currency.Equals("GEL", StringComparison.OrdinalIgnoreCase) || !currency.Equals("USD", StringComparison.OrdinalIgnoreCase) )
        {
            return cart with { Currency = "GEL" };
        }

        if ( currency.Equals("USD", StringComparison.OrdinalIgnoreCase) )
        {
            var rate = await _CURRENCY.GetRateAsync();
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

    public async Task<OrderResponse> ApplyCurrencyConversion( OrderResponse order, string currency )
    {
        if ( currency.Equals("GEL", StringComparison.OrdinalIgnoreCase) || !currency.Equals("USD", StringComparison.OrdinalIgnoreCase) )
        {
            return order with { Currency = "GEL" };
        }

        if ( currency.Equals("USD", StringComparison.OrdinalIgnoreCase) )
        {
            var rate = await _CURRENCY.GetRateAsync();
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

    public async Task<List<ProductMinimalResponse>> ApplyCurrencyConversionToList( List<ProductMinimalResponse> products, string currency )
    {
        if ( currency.Equals("GEL", StringComparison.OrdinalIgnoreCase) || !currency.Equals("USD", StringComparison.OrdinalIgnoreCase) )
        {
            return products.Select(p => p with { Currency = "GEL" }).ToList();
        }

        if ( currency.Equals("USD", StringComparison.OrdinalIgnoreCase) )
        {
            var rate = await _CURRENCY.GetRateAsync();
            return products.Select(p => p with
            {
                Price = Math.Round(p.Price * rate, 2),
                Currency = "USD"
            }).ToList();
        }

        return products;
    }

    public async Task<List<OrderResponse>> ApplyCurrencyConversionToList( List<OrderResponse> orders, string currency )
    {
        if ( currency.Equals("GEL", StringComparison.OrdinalIgnoreCase) || !currency.Equals("USD", StringComparison.OrdinalIgnoreCase) )
        {
            return orders.Select(o => o with { Currency = "GEL" }).ToList();
        }

        if ( currency.Equals("USD", StringComparison.OrdinalIgnoreCase) )
        {
            var rate = await _CURRENCY.GetRateAsync();
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
