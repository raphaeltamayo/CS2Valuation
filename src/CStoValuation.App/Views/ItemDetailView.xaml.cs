using System.Windows.Controls;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace CStoValuation.App.Views;

/// <summary>The item-detail panel; bound to an <see cref="ViewModels.ItemDetailViewModel"/>.</summary>
public partial class ItemDetailView : UserControl
{
    public ItemDetailView()
    {
        InitializeComponent();

        // Dark-mode tooltip to match the rest of the app (set here as it needs SkiaSharp paints).
        TrendChart.TooltipBackgroundPaint = new SolidColorPaint(SKColor.Parse("#272B33"));
        TrendChart.TooltipTextPaint = new SolidColorPaint(SKColor.Parse("#E7E9EE")) { SKTypeface = SKTypeface.Default };
    }
}
