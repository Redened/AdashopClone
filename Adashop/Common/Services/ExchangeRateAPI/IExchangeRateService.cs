namespace Adashop.Common.Services.ExchangeRateAPI;

public interface IExchangeRateService
{
    Task<decimal> GetRateAsync();
}
