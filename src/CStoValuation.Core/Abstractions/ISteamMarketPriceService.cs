using CStoValuation.Core.Models;

namespace CStoValuation.Core.Abstractions;

/// <summary>
/// Fetches a single item's Steam Community Market price overview. Steam is heavily
/// rate-limited, so this is used on demand for the item-detail panel only — never for
/// a whole-inventory pass. It serves as a second price source and a liquidity signal
/// (trade volume) alongside Skinport.
/// </summary>
public interface ISteamMarketPriceService
{
    /// <summary>Gets the Steam Market quote for one item, or <c>null</c> if unpriced.</summary>
    /// <param name="marketHashName">The item to price.</param>
    /// <param name="currency">ISO currency code, e.g. "EUR".</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    Task<PriceQuote?> GetPriceOverviewAsync(
        string marketHashName, string currency, CancellationToken cancellationToken = default);
}
