using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CStoValuation.App.Presentation;
using CStoValuation.Core.Models;

namespace CStoValuation.App.ViewModels;

/// <summary>How the movers list is ranked and displayed.</summary>
internal enum MoverMetric
{
    /// <summary>Percentage change of the 7-day average vs the 30-day average.</summary>
    Percentage,

    /// <summary>Absolute change per single unit, in currency.</summary>
    UnitValue,

    /// <summary>Absolute change across the whole owned stack (per-unit × quantity).</summary>
    LineValue,
}

/// <summary>A selectable metric with its display label (for the dropdown).</summary>
internal sealed record MetricOption(MoverMetric Value, string Label)
{
    // The ComboBox shows this directly, so render as the label rather than the record default.
    public override string ToString() => Label;
}

/// <summary>
/// Ranks owned items by recent price momentum ("biggest movers"), measured from each item's
/// trailing 30-day average to its 7-day average (Skinport sales history) — available instantly,
/// no waiting for accumulated snapshots. The user chooses whether to rank by percentage, by
/// per-unit absolute change, or by total (per-unit × quantity owned) change.
/// </summary>
internal sealed partial class MoversViewModel : ObservableObject
{
    private const string Currency = "EUR";
    private const int MaxMovers = 12;

    private readonly List<MoverData> _all = [];

    [ObservableProperty]
    private bool _hasMovers;

    [ObservableProperty]
    private MoverMetric _selectedMetric = MoverMetric.Percentage;

    public ObservableCollection<MoverViewModel> Movers { get; } = [];

    public IReadOnlyList<MetricOption> Metrics { get; } =
    [
        new(MoverMetric.Percentage, "% change"),
        new(MoverMetric.UnitValue, "Per unit (€)"),
        new(MoverMetric.LineValue, "Total (€)"),
    ];

    /// <summary>Recomputes movers from the supplied sales history for the owned items.</summary>
    public void Load(IReadOnlyDictionary<string, ItemSalesHistory> salesHistory, IEnumerable<ValuedItemViewModel> ownedItems)
    {
        _all.Clear();

        foreach (var item in ownedItems)
        {
            if (!salesHistory.TryGetValue(item.Name, out var history))
            {
                continue;
            }

            var recent = history.Last7Days.Average;
            var baseline = history.Last30Days.Average;
            if (recent is not { } recentAvg || baseline is not { } baselineAvg || baselineAvg <= 0m)
            {
                continue; // need both windows to measure a change
            }

            var unitChange = recentAvg - baselineAvg;
            _all.Add(new MoverData(
                Name: item.Name,
                Percent: unitChange / baselineAvg * 100m,
                UnitChange: unitChange,
                LineChange: unitChange * item.Quantity,
                Latest: history.Last24Hours.Average ?? recentAvg));
        }

        Rebuild();
    }

    partial void OnSelectedMetricChanged(MoverMetric value) => Rebuild();

    private void Rebuild()
    {
        Movers.Clear();
        foreach (var mover in _all
            .OrderByDescending(mover => Math.Abs(SortKey(mover)))
            .Take(MaxMovers))
        {
            Movers.Add(new MoverViewModel(
                mover.Name,
                DisplayText(mover),
                SortKey(mover) >= 0m,
                MoneyFormatter.Format(mover.Latest, Currency)));
        }

        HasMovers = Movers.Count > 0;
    }

    private decimal SortKey(MoverData mover) => SelectedMetric switch
    {
        MoverMetric.UnitValue => mover.UnitChange,
        MoverMetric.LineValue => mover.LineChange,
        _ => mover.Percent,
    };

    private string DisplayText(MoverData mover) => SelectedMetric switch
    {
        MoverMetric.UnitValue => FormatSigned(mover.UnitChange),
        MoverMetric.LineValue => FormatSigned(mover.LineChange),
        _ => $"{(mover.Percent >= 0m ? "+" : string.Empty)}{mover.Percent.ToString("N1", CultureInfo.CurrentCulture)}%",
    };

    private static string FormatSigned(decimal value)
    {
        var sign = value >= 0m ? "+" : "-";
        return sign + MoneyFormatter.Format(Math.Abs(value), Currency);
    }

    private sealed record MoverData(string Name, decimal Percent, decimal UnitChange, decimal LineChange, decimal Latest);
}
