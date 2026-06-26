using CStoValuation.Core.Models;

namespace CStoValuation.Core.Abstractions;

/// <summary>
/// Provides Skinport's aggregated sales history (trailing 24h / 7d / 30d / 90d windows) for
/// the whole catalogue in one bulk call, keyed by market hash name. This is the instant
/// source for the movers ranking and the price-trend chart. Implementations cache the result
/// (Skinport is rate-limited).
/// </summary>
public interface ISkinportSalesHistoryService
{
    Task<IReadOnlyDictionary<string, ItemSalesHistory>> GetSalesHistoryAsync(
        string currency, CancellationToken cancellationToken = default);
}
