namespace CStoValuation.App.ViewModels;

/// <summary>
/// One ranked row in the movers list. The headline <see cref="ValueText"/> is already
/// formatted for whichever metric is currently selected (percentage, per-unit, or total).
/// </summary>
internal sealed class MoverViewModel
{
    public MoverViewModel(string name, string valueText, bool isPositive, string latestPriceText)
    {
        Name = name;
        ValueText = valueText;
        IsPositive = isPositive;
        LatestPriceText = latestPriceText;
    }

    public string Name { get; }
    public string ValueText { get; }
    public bool IsPositive { get; }
    public string LatestPriceText { get; }
}
