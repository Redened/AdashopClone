using Adashop.DTOs;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace Adashop.Common.Services.ExchangeRateAPI;

public class CurrencyService : ICurrencyService
{
    private readonly HttpClient _HTTP;
    private readonly IMemoryCache _CACHE;
    private readonly ILogger<CurrencyService> _LOG;
    private readonly string _API_KEY;
    private const string CACHE_KEY = "exchange_rate_gel_usd";
    private const string BASE_CURRENCY = "GEL";
    private const string TARGET_CURRENCY = "USD";

    public CurrencyService( HttpClient HTTP, IMemoryCache CACHE, ILogger<CurrencyService> LOG, IConfiguration config )
    {
        _HTTP = HTTP;
        _CACHE = CACHE;
        _LOG = LOG;
        _API_KEY = config["ExchangeRateAPI:Key"] ?? throw new InvalidOperationException("ExchangeRateAPI:Key is not configured");
    }

    public async Task<decimal> GetRateAsync()
    {
        if ( _CACHE.TryGetValue(CACHE_KEY, out decimal cachedRate) )
        {
            _LOG.LogInformation("Exchange rate retrieved from cache: {Rate}", cachedRate);
            return cachedRate;
        }

        try
        {
            var url = $"https://v6.exchangerate-api.com/v6/{_API_KEY}/pair/{BASE_CURRENCY}/{TARGET_CURRENCY}";
            var response = await _HTTP.GetAsync(url);

            if ( !response.IsSuccessStatusCode )
            {
                _LOG.LogError("Exchange rate API returned status code: {StatusCode}", response.StatusCode);
                return 1m;
            }

            var content = await response.Content.ReadAsStringAsync();

            _LOG.LogInformation("ExchangeRateAPI Response: {APIResponse}", content);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var apiResponse = JsonSerializer.Deserialize<ExchangeRateResponse>(content, options);

            _LOG.LogInformation("ExchangeRateAPI Response: {APIResponse}", apiResponse);

            if ( apiResponse == null )
            {
                _LOG.LogError("Failed to deserialize exchange rate response: {Response}", content);
                return 1m;
            }

            if ( !apiResponse.Result.Equals("success", StringComparison.OrdinalIgnoreCase) )
            {
                _LOG.LogError("Exchange rate API returned non-success result: {Result}", apiResponse.Result);
                return 1m;
            }

            if ( apiResponse.ConversionRate <= 0 )
            {
                _LOG.LogError("Exchange rate is invalid (not positive): {Rate}", apiResponse.ConversionRate);
                return 1m;
            }

            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            };

            _CACHE.Set(CACHE_KEY, apiResponse.ConversionRate, cacheOptions);
            _LOG.LogInformation("Exchange rate cached: {Rate}", apiResponse.ConversionRate);

            return apiResponse.ConversionRate;
        }
        catch ( JsonException ex )
        {
            _LOG.LogError(ex, "Error deserializing exchange rate response");
            return 1m;
        }
        catch ( Exception ex )
        {
            _LOG.LogError(ex, "Error fetching exchange rate from API");
            return 1m;
        }
    }
}
