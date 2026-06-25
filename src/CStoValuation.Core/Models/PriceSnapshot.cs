using CStoValuation.Core.Enums;

namespace CStoValuation.Core.Models;

/// <summary>
/// A persisted, point-in-time capture of an item's full price statistics from one
/// venue. Unlike the immutable <see cref="PriceQuote"/> value object, this is a
/// database <i>entity</i>: a mutable POCO with a surrogate key. It carries no EF
/// attributes — the mapping is configured by Fluent API in the Infrastructure layer,
/// so Core stays free of any persistence dependency.
/// </summary>
public class PriceSnapshot
{
    /// <summary>Surrogate primary key (database-generated).</summary>
    public int Id { get; set; }

    /// <summary>The item these statistics describe.</summary>
    public string MarketHashName { get; set; } = string.Empty;

    /// <summary>The venue the snapshot came from.</summary>
    public PriceSource Source { get; set; }

    /// <summary>Lowest asking price at capture time, if available.</summary>
    public decimal? Min { get; set; }

    /// <summary>Median price at capture time, if available.</summary>
    public decimal? Median { get; set; }

    /// <summary>Mean price at capture time, if available.</summary>
    public decimal? Mean { get; set; }

    /// <summary>Active listing count at capture time, if available.</summary>
    public int? Listings { get; set; }

    /// <summary>Trade volume at capture time, if available.</summary>
    public int? Volume { get; set; }

    /// <summary>Currency the prices are expressed in.</summary>
    public string Currency { get; set; } = "EUR";

    /// <summary>When the snapshot was taken (UTC instant).</summary>
    public DateTimeOffset TakenUtc { get; set; }
}
