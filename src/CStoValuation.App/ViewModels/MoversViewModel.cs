using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CStoValuation.Core.Models;

namespace CStoValuation.App.ViewModels;

/// <summary>
/// Ranks owned items by recent price momentum — the "biggest movers". The change is measured
/// from each item's trailing 30-day average to its trailing 7-day average (Skinport sales
/// history), so the ranking is available <i>instantly</i> on first import, with no waiting for
/// the app to accumulate its own snapshots.
/// </summary>
internal sealed partial class MoversViewModel : ObservableObject
{
    private const string Currency = "EUR";
    private const int MaxMovers = 12;

    [ObservableProperty]
    private bool _hasMovers;

    public ObservableCollection<MoverViewModel> Movers { get; } = [];

    /// <summary>Recomputes movers for the owned items from the supplied sales history.</summary>
    public void Load(IReadOnlyDictionary<string, ItemSalesHistory> salesHistory, IEnumerable<string> ownedNames)
    {
        var computed = new List<MoverViewModel>();

        foreach (var name in ownedNames.Distinct())
        {
            if (!salesHistory.TryGetValue(name, out var history))
            {
                continue;
            }

            var recent = history.Last7Days.Average;
            var baseline = history.Last30Days.Average;
            if (recent is not { } recentAvg || baseline is not { } baselineAvg || baselineAvg <= 0m)
            {
                continue; // need both windows to measure a change
            }

            var changePercent = (recentAvg - baselineAvg) / baselineAvg * 100m;
            var latest = history.Last24Hours.Average ?? recentAvg;
            computed.Add(new MoverViewModel(name, changePercent, latest, Currency));
        }

        Movers.Clear();
        foreach (var mover in computed
            .OrderByDescending(mover => Math.Abs(mover.ChangePercent))
            .Take(MaxMovers))
        {
            Movers.Add(mover);
        }

        HasMovers = Movers.Count > 0;
    }
}
