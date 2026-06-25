using CStoValuation.Core.Models;

namespace CStoValuation.Core.Abstractions;

/// <summary>
/// Marks an inventory to market: pairs each item with its quote, applies the fee model,
/// and rolls the lines up into headline totals. This is pure, synchronous business logic
/// — no I/O — which is exactly why it lives in Core and is the most thoroughly unit-tested
/// piece of the app.
/// </summary>
public interface IValuationService
{
    /// <summary>Produces a full valuation from an inventory, a price map, and a fee model.</summary>
    /// <param name="inventory">The items to value.</param>
    /// <param name="prices">Quotes keyed by market hash name (e.g. from Skinport).</param>
    /// <param name="feeModel">The fee model used to derive net from gross.</param>
    InventoryValuation Value(
        IReadOnlyCollection<InventoryItem> inventory,
        IReadOnlyDictionary<string, PriceQuote> prices,
        FeeModel feeModel);
}
