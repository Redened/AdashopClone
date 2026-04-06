namespace Adashop.Shared.Services;

public interface IExchangeRateService
{
    Task<decimal> GetRateAsync();
}
