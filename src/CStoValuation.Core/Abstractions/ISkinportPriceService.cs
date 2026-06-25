using CStoValuation.Core.Models;

namespace CStoValuation.Core.Abstractions;

/// <summary>
/// Provides Skinport prices for the whole CS2 catalogue in a single bulk call, keyed by
/// market hash name for O(1) lookup during valuation. Implementations are expected to
/// cache the result (Skinport is strictly rate-limited).
/// </summary>
public interface ISkinportPriceService
{
    /// <summary>Returns a name → quote map for every priced item in the given currency.</summary>
    /// <param name="currency">ISO currency code, e.g. "EUR".</param>
    /// <param name="cancellationToken">Cancels the download.</param>
    Task<IReadOnlyDictionary<string, PriceQuote>> GetPricesAsync(
        string currency, CancellationToken cancellationToken = default);
}
