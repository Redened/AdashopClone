using System.Text.Json.Serialization;

namespace Adashop.DTOs;

public record ExchangeRateResponse(
    [property: JsonPropertyName("result")]
    string Result,

    [property: JsonPropertyName("base_code")]
    string BaseCode,

    [property: JsonPropertyName("target_code")]
    string TargetCode,

    [property: JsonPropertyName("conversion_rate")]
    decimal ConversionRate
);