using System.Collections.Immutable;
using CStoValuation.Core.Models;

namespace CStoValuation.Core.Services;

public static class PortfolioInsights
{
    /// <summary>
    /// Finds the single most valuable holding in a valuation, judged by its net-of-fees line
    /// value (<see cref="ValuedItem.LineNet"/>).
    /// </summary>
    /// <param name="items">All valued lines from an <see cref="InventoryValuation"/>.</param>
    /// <returns>The most valuable priced line, or <c>null</c> when none are priced.</returns>
    public static ValuedItem? MostValuable(IReadOnlyList<ValuedItem> items) => items.Where(static item => item.IsPriced).OrderByDescending(static item => item.LineNet).FirstOrDefault();
}
