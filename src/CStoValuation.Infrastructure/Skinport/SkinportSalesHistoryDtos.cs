using System.Text.Json.Serialization;

namespace CStoValuation.Infrastructure.Skinport;

/// <summary>Wire shape of an entry in Skinport's <c>/v1/sales/history</c> response.</summary>
internal sealed record SkinportSalesHistoryDto
{
    [JsonPropertyName("market_hash_name")]
    public string? MarketHashName { get; init; }

    [JsonPropertyName("currency")]
    public string? Currency { get; init; }

    [JsonPropertyName("last_24_hours")]
    public SkinportSalesWindowDto? Last24Hours { get; init; }

    [JsonPropertyName("last_7_days")]
    public SkinportSalesWindowDto? Last7Days { get; init; }

    [JsonPropertyName("last_30_days")]
    public SkinportSalesWindowDto? Last30Days { get; init; }

    [JsonPropertyName("last_90_days")]
    public SkinportSalesWindowDto? Last90Days { get; init; }
}

/// <summary>Aggregated stats for one trailing window.</summary>
internal sealed record SkinportSalesWindowDto
{
    [JsonPropertyName("min")]
    public decimal? Min { get; init; }

    [JsonPropertyName("max")]
    public decimal? Max { get; init; }

    [JsonPropertyName("avg")]
    public decimal? Avg { get; init; }

    [JsonPropertyName("median")]
    public decimal? Median { get; init; }

    [JsonPropertyName("volume")]
    public int Volume { get; init; }
}
