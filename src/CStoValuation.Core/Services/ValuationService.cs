using CStoValuation.Core.Abstractions;
using CStoValuation.Core.Models;

namespace CStoValuation.Core.Services;

/// <summary>
/// The reference implementation of <see cref="IValuationService"/>. Pure and
/// deterministic: given the same inventory, prices and fee model it always returns the
/// same valuation, with no I/O, clock or randomness involved.
/// </summary>
public sealed class ValuationService : IValuationService
{
    /// <summary>Fallback currency when the price map is empty and can't tell us one.</summary>
    private const string FallbackCurrency = "EUR";

    /// <inheritdoc />
    public InventoryValuation Value(
        IReadOnlyCollection<InventoryItem> inventory,
        IReadOnlyDictionary<string, PriceQuote> prices,
        FeeModel feeModel)
    {
        // Fail fast on null arguments — a valuation built from null inputs is a bug,
        // not a "zero" result we want to silently return.
        ArgumentNullException.ThrowIfNull(inventory);
        ArgumentNullException.ThrowIfNull(prices);
        ArgumentNullException.ThrowIfNull(feeModel);

        // All quotes from one source share a currency; read it once for the headline.
        var currency = ResolveCurrency(prices);

        var valuedItems = new List<ValuedItem>(inventory.Count);
        decimal totalGross = 0m;
        decimal totalNet = 0m;
        var pricedCount = 0;
        var unpricedCount = 0;

        foreach (var item in inventory)
        {
            if (!prices.TryGetValue(item.MarketHashName, out var quote))
            {
                // No price for this line: record it as unpriced, contributing nothing.
                unpricedCount++;
                valuedItems.Add(new ValuedItem { Item = item, Quote = null });
                continue;
            }

            // Value the whole stack, then take the fee once at the line level so the
            // rounding can't compound per unit.
            var lineGross = quote.Gross * item.Quantity;
            var lineNet = feeModel.NetFromGross(lineGross);

            totalGross += lineGross;
            totalNet += lineNet;
            pricedCount++;

            valuedItems.Add(new ValuedItem
            {
                Item = item,
                Quote = quote,
                LineGross = lineGross,
                LineNet = lineNet,
            });
        }

        return new InventoryValuation
        {
            Items = valuedItems,
            TotalGross = decimal.Round(totalGross, 2, MidpointRounding.AwayFromZero),
            TotalNet = decimal.Round(totalNet, 2, MidpointRounding.AwayFromZero),
            Currency = currency,
            PricedCount = pricedCount,
            UnpricedCount = unpricedCount,
        };
    }

    private static string ResolveCurrency(IReadOnlyDictionary<string, PriceQuote> prices)
    {
        foreach (var quote in prices.Values)
        {
            return quote.Currency;
        }

        return FallbackCurrency;
    }
}
