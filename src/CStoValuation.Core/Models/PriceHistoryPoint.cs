namespace CStoValuation.Core.Models;

/// <summary>
/// A single persisted point on an item's price time-series — the simplified shape the
/// price chart and the "movers" calculation read from. A snapshot may capture many
/// statistics; a history point keeps just the one price we chart over time.
/// </summary>
public class PriceHistoryPoint
{
    /// <summary>Surrogate primary key (database-generated).</summary>
    public int Id { get; set; }

    /// <summary>The item this point belongs to.</summary>
    public string MarketHashName { get; set; } = string.Empty;

    /// <summary>The instant this price was recorded (UTC).</summary>
    public DateTimeOffset DateUtc { get; set; }

    /// <summary>The charted price at that instant.</summary>
    public decimal Price { get; set; }

    /// <summary>Trade volume at that instant, if known.</summary>
    public int? Volume { get; set; }
}
