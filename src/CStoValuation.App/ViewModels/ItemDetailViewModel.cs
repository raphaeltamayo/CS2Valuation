using System.Net.Http;
using CommunityToolkit.Mvvm.ComponentModel;
using CStoValuation.App.Presentation;
using CStoValuation.Core.Abstractions;
using CStoValuation.Core.Models;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace CStoValuation.App.ViewModels;

/// <summary>A selectable look-back window for the price chart.</summary>
internal sealed record HistoryRange(int Days, string Label)
{
    public override string ToString() => Label;
}

/// <summary>
/// Drives the item-detail panel: Skinport gross/net, an on-demand Steam Market second price plus
/// trade volume, and a price chart. The chart prefers Steam's real day-by-day history (when the
/// user is signed in) filtered to the selected window; otherwise it falls back to the coarse
/// trend from Skinport's trailing-window sales history.
/// </summary>
internal sealed partial class ItemDetailViewModel : ObservableObject
{
    private const string Currency = "EUR";
    private static readonly SKColor AccentColor = SKColor.Parse("#4B8BF5");
    private static readonly SKColor MutedColor = SKColor.Parse("#9AA1AD");

    private readonly ISteamMarketHistoryService _historyService;
    private readonly ISteamMarketPriceService _steamMarketService;
    private readonly TimeProvider _timeProvider;

    private IReadOnlyList<PriceHistoryPoint> _fullHistory = [];
    private ItemSalesHistory? _salesHistory;
    private bool _hasRealHistory;

    [ObservableProperty] private bool _hasSelection;
    [ObservableProperty] private string? _name;
    [ObservableProperty] private string? _imageUrl;
    [ObservableProperty] private string _skinportGrossText = MoneyFormatter.Placeholder;
    [ObservableProperty] private string _skinportNetText = MoneyFormatter.Placeholder;
    [ObservableProperty] private string _steamPriceText = MoneyFormatter.Placeholder;
    [ObservableProperty] private string _steamVolumeText = MoneyFormatter.Placeholder;
    [ObservableProperty] private bool _isLoadingSteamPrice;
    [ObservableProperty] private bool _hasHistory;
    [ObservableProperty] private ISeries[] _series = [];
    [ObservableProperty] private HistoryRange _selectedRange;

    public ItemDetailViewModel(
        ISteamMarketHistoryService historyService,
        ISteamMarketPriceService steamMarketService,
        TimeProvider? timeProvider = null)
    {
        _historyService = historyService;
        _steamMarketService = steamMarketService;
        _timeProvider = timeProvider ?? TimeProvider.System;

        _selectedRange = Ranges[1]; // default to 30 days
        XAxes = [BuildDateAxis()];
        YAxes = [BuildPriceAxis()];
    }

    public IReadOnlyList<HistoryRange> Ranges { get; } =
    [
        new(7, "7D"),
        new(30, "30D"),
        new(90, "90D"),
        new(365, "1Y"),
    ];

    public Axis[] XAxes { get; }

    public Axis[] YAxes { get; }

    /// <summary>Loads detail for the selected row, or clears the panel when nothing is selected.</summary>
    public async Task LoadAsync(ValuedItemViewModel? item, ItemSalesHistory? salesHistory)
    {
        if (item is null)
        {
            HasSelection = false;
            return;
        }

        HasSelection = true;
        Name = item.Name;
        ImageUrl = item.ImageUrl;
        SkinportGrossText = item.UnitGrossText;
        SkinportNetText = item.UnitNetText;
        _salesHistory = salesHistory;

        await LoadHistoryAsync(item.Name);
        await LoadSteamMarketAsync(item.Name);
    }

    partial void OnSelectedRangeChanged(HistoryRange value) => RebuildSeries();

    private async Task LoadHistoryAsync(string marketHashName)
    {
        try
        {
            _fullHistory = await _historyService.GetPriceHistoryAsync(marketHashName, Currency);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _fullHistory = [];
        }

        _hasRealHistory = _fullHistory.Count >= 2;
        RebuildSeries();
    }

    private void RebuildSeries()
    {
        var points = _hasRealHistory ? PointsForWindow() : BuildTrendPoints(_salesHistory);
        HasHistory = points.Length >= 2;

        Series =
        [
            new LineSeries<DateTimePoint>
            {
                Values = points,
                LineSmoothness = 0.3,
                GeometrySize = points.Length <= 8 ? 5 : 0,
                Stroke = new SolidColorPaint(AccentColor) { StrokeThickness = 2 },
                Fill = new SolidColorPaint(AccentColor.WithAlpha(36)),
                GeometryStroke = new SolidColorPaint(AccentColor) { StrokeThickness = 2 },
                GeometryFill = new SolidColorPaint(AccentColor),
                YToolTipLabelFormatter = point =>
                {
                    var date = new DateTime((long)point.Coordinate.SecondaryValue);
                    return $"{MoneyFormatter.Format((decimal)point.Coordinate.PrimaryValue, Currency)}  ·  {date:MMM dd, yyyy}";
                },
            },
        ];
    }

    private DateTimePoint[] PointsForWindow()
    {
        var since = _timeProvider.GetUtcNow() - TimeSpan.FromDays(SelectedRange.Days);
        var windowed = _fullHistory.Where(point => point.DateUtc >= since).ToList();

        // If the chosen window is too sparse, show whatever history we have rather than a blank.
        if (windowed.Count < 2)
        {
            windowed = [.. _fullHistory];
        }

        return windowed
            .Select(point => new DateTimePoint(point.DateUtc.UtcDateTime, (double)point.Price))
            .ToArray();
    }

    /// <summary>
    /// Fallback when no signed-in history is available: a coarse 4-point trend (≈90d, 30d, 7d, now)
    /// from Skinport's trailing-window averages (median as a fallback). Points with no sales drop out.
    /// </summary>
    private DateTimePoint[] BuildTrendPoints(ItemSalesHistory? salesHistory)
    {
        if (salesHistory is null)
        {
            return [];
        }

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var candidates = new (DateTime At, decimal? Value)[]
        {
            (now.AddDays(-90), salesHistory.Last90Days.Average ?? salesHistory.Last90Days.Median),
            (now.AddDays(-30), salesHistory.Last30Days.Average ?? salesHistory.Last30Days.Median),
            (now.AddDays(-7), salesHistory.Last7Days.Average ?? salesHistory.Last7Days.Median),
            (now, salesHistory.Last24Hours.Average ?? salesHistory.Last24Hours.Median),
        };

        return candidates
            .Where(point => point.Value is not null)
            .Select(point => new DateTimePoint(point.At, (double)point.Value!.Value))
            .ToArray();
    }

    private async Task LoadSteamMarketAsync(string marketHashName)
    {
        IsLoadingSteamPrice = true;
        SteamPriceText = "…";
        SteamVolumeText = "…";
        try
        {
            var quote = await _steamMarketService.GetPriceOverviewAsync(marketHashName, Currency);
            if (quote is null)
            {
                SteamPriceText = MoneyFormatter.Placeholder;
                SteamVolumeText = MoneyFormatter.Placeholder;
                return;
            }

            SteamPriceText = MoneyFormatter.Format(quote.Gross, Currency);
            SteamVolumeText = quote.Volume?.ToString("N0") ?? MoneyFormatter.Placeholder;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            SteamPriceText = "unavailable";
            SteamVolumeText = MoneyFormatter.Placeholder;
        }
        finally
        {
            IsLoadingSteamPrice = false;
        }
    }

    private static Axis BuildDateAxis() => new()
    {
        Labeler = value => new DateTime((long)value).ToString("MMM dd"),
        UnitWidth = TimeSpan.FromDays(1).Ticks,
        LabelsPaint = new SolidColorPaint(MutedColor),
        TextSize = 11,
        SeparatorsPaint = null, // no vertical gridlines
    };

    private static Axis BuildPriceAxis() => new()
    {
        Labeler = value => MoneyFormatter.Format((decimal)value, Currency),
        LabelsPaint = new SolidColorPaint(MutedColor),
        TextSize = 11,
        SeparatorsPaint = new SolidColorPaint(SKColor.Parse("#2A2E36")) { StrokeThickness = 1 },
    };
}
