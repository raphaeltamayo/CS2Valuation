namespace CStoValuation.Core.Models;

/// <summary>
/// The result of marking a whole inventory to market: every line valued, plus the
/// headline totals the UI puts front and centre. The gross-vs-net pair is the
/// finance highlight — what the holdings are "worth" versus what they'd realize.
/// </summary>
public sealed record InventoryValuation
{
    /// <summary>Every inventory line with its valuation, priced or not.</summary>
    public required IReadOnlyList<ValuedItem> Items { get; init; }

    /// <summary>Sum of all line gross values.</summary>
    public decimal TotalGross { get; init; }

    /// <summary>Sum of all line net values (after seller fees).</summary>
    public decimal TotalNet { get; init; }

    /// <summary>Currency the totals are expressed in.</summary>
    public required string Currency { get; init; }

    /// <summary>How many lines were successfully priced.</summary>
    public int PricedCount { get; init; }

    /// <summary>How many lines had no price (shown as "—" in the UI).</summary>
    public int UnpricedCount { get; init; }

    /// <summary>An empty valuation in the given currency — a tidy "nothing to show yet" state.</summary>
    public static InventoryValuation Empty(string currency) => new()
    {
        Items = [],
        Currency = currency,
        TotalGross = 0m,
        TotalNet = 0m,
        PricedCount = 0,
        UnpricedCount = 0,
    };
}
