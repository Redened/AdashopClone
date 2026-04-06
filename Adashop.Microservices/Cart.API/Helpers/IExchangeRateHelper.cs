using Cart.API.DTOs;

namespace Cart.API.Helpers;

public interface IExchangeRateHelper
{
    Task<CartResponse> ApplyCurrencyConversion( CartResponse cart, string currency );
}