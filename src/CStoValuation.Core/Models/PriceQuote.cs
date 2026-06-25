using CStoValuation.Core.Enums;

namespace CStoValuation.Core.Models;

/// <summary>
/// A point-in-time market price for one item from one venue. This models the raw
/// <i>market data</i> only — the buyer-facing <see cref="Gross"/> price plus
/// liquidity signals. The seller's realizable (net-of-fees) figure is deliberately
/// not stored here: net depends on the chosen <see cref="FeeModel"/>, which is a
/// valuation-time decision, not a property of the market. Keeping the quote pure
/// means the same quote can be revalued under different fee assumptions.
/// </summary>
public sealed record PriceQuote
{
    /// <summary>The item this quote prices (its market hash name).</summary>
    public required string MarketHashName { get; init; }

    /// <summary>Which marketplace produced the quote.</summary>
    public PriceSource Source { get; init; }

    /// <summary>ISO currency code the prices are expressed in, e.g. "EUR".</summary>
    public required string Currency { get; init; }

    /// <summary>
    /// The lowest current asking price — what a buyer pays. Uses <see cref="decimal"/>
    /// (base-10, exact) rather than a binary float, because this is money.
    /// </summary>
    public decimal Gross { get; init; }

    /// <summary>Number of active listings, when known — a liquidity/depth signal.</summary>
    public int? Listings { get; init; }

    /// <summary>Recent trade volume, when known — the other half of liquidity.</summary>
    public int? Volume { get; init; }

    /// <summary>When this quote was observed (UTC instant).</summary>
    public DateTimeOffset AsOfUtc { get; init; }
}
