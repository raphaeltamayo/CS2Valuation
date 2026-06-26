using CStoValuation.Core.Models;

namespace CStoValuation.Core.Abstractions;

/// <summary>
/// Fetches an item's full day-by-day price history from the Steam Community Market. This is the
/// fine-grained time-series behind the detail chart. The endpoint requires an authenticated
/// session (see <see cref="ISteamSession"/>); without one, implementations return an empty list
/// so the caller can fall back to a coarser source.
/// </summary>
public interface ISteamMarketHistoryService
{
    Task<IReadOnlyList<PriceHistoryPoint>> GetPriceHistoryAsync(
        string marketHashName, string currency, CancellationToken cancellationToken = default);
}
