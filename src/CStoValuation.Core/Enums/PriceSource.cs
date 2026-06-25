namespace CStoValuation.Core.Enums;

/// <summary>
/// Identifies which marketplace a <see cref="Models.PriceQuote"/> came from.
/// Different venues have different fees, liquidity and spreads, so the source
/// is part of the price's meaning — not just metadata.
/// </summary>
public enum PriceSource
{
    /// <summary>Skinport — bulk-priced, used for the whole-inventory valuation.</summary>
    Skinport,

    /// <summary>Steam Community Market — fetched on demand for a single item.</summary>
    SteamMarket,
}
