namespace CStoValuation.Core.Models;

/// <summary>
/// An inventory line marked to market: the item, the quote used to price it (if any),
/// and the resulting line totals. <see cref="LineGross"/>/<see cref="LineNet"/> already
/// account for <see cref="InventoryItem.Quantity"/>.
/// </summary>
public sealed record ValuedItem
{
    /// <summary>The underlying inventory line.</summary>
    public required InventoryItem Item { get; init; }

    /// <summary>The market quote applied, or <c>null</c> when no price was found.</summary>
    public PriceQuote? Quote { get; init; }

    /// <summary>Gross value of the whole line (unit gross × quantity).</summary>
    public decimal LineGross { get; init; }

    /// <summary>Net-of-fees value of the whole line.</summary>
    public decimal LineNet { get; init; }

    /// <summary>True when a price was available; unpriced items contribute zero.</summary>
    public bool IsPriced => Quote is not null;
}
