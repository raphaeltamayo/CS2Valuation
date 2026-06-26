namespace CStoValuation.Core.Models;

/// <summary>
/// Aggregated sales statistics for one item over a trailing time window
/// (e.g. the last 7 days). Any field can be <c>null</c> when there were no sales.
/// </summary>
public sealed record SalesWindow
{
    public decimal? Min { get; init; }
    public decimal? Max { get; init; }
    public decimal? Average { get; init; }
    public decimal? Median { get; init; }
    public int Volume { get; init; }

    /// <summary>An empty window (no sales recorded).</summary>
    public static SalesWindow Empty { get; } = new();
}

/// <summary>
/// An item's recent sales history as four trailing windows. This is what makes "biggest
/// movers" and a price trend available <i>instantly</i> from a single bulk API call,
/// rather than waiting for the app to record its own snapshots over time.
/// </summary>
public sealed record ItemSalesHistory
{
    public required string MarketHashName { get; init; }
    public required string Currency { get; init; }
    public SalesWindow Last24Hours { get; init; } = SalesWindow.Empty;
    public SalesWindow Last7Days { get; init; } = SalesWindow.Empty;
    public SalesWindow Last30Days { get; init; } = SalesWindow.Empty;
    public SalesWindow Last90Days { get; init; } = SalesWindow.Empty;
}
