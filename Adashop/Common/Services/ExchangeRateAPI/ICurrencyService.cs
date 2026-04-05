namespace Adashop.Common.Services.ExchangeRateAPI;

public interface ICurrencyService
{
    Task<decimal> GetRateAsync();
}
