using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace SolRIA.SAFT.Desktop.Controls;

public sealed class DataGridTotalsFooter : Panel
{
    public static readonly StyledProperty<DataGrid> TargetGridProperty =
        AvaloniaProperty.Register<DataGridTotalsFooter, DataGrid>(nameof(TargetGrid));

    private DataGridColumnHeadersPresenter headers;
    private Rect[] cellBounds = [];
    private Rect viewport;

    public DataGridTotalsFooter()
    {
        LayoutUpdated += OnLayoutUpdated;
    }

    public DataGrid TargetGrid
    {
        get => GetValue(TargetGridProperty);
        set => SetValue(TargetGridProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TargetGridProperty)
            headers = null;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        double height = 0;
        foreach (var cell in Children)
        {
            cell.Measure(Size.Infinity);
            height = Math.Max(height, cell.DesiredSize.Height);
        }

        // The document grid determines the width, even when its columns scroll.
        return new Size(0, height + 8);
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        for (int i = 0; i < Children.Count; i++)
        {
            var bounds = i < cellBounds.Length ? cellBounds[i] : default;
            Children[i].Arrange(new Rect(bounds.X, 0, bounds.Width, finalSize.Height));
        }

        return finalSize;
    }

    private void OnLayoutUpdated(object sender, EventArgs e)
    {
        if (headers == null || !headers.IsAttachedToVisualTree())
        {
            headers = TargetGrid?.GetVisualDescendants()
                .OfType<DataGridColumnHeadersPresenter>()
                .FirstOrDefault();
        }

        if (headers?.TranslatePoint(default, this) is not Point origin)
            return;

        // Use the arranged headers so scroll offsets and display order are reflected in the footer.
        var visibleBounds = new Rect(origin.X, 0, headers.Bounds.Width, Bounds.Height);
        if (viewport != visibleBounds)
        {
            viewport = visibleBounds;
            Clip = new RectangleGeometry(viewport);
        }

        bool arrangeNeeded = cellBounds.Length != Children.Count;
        if (arrangeNeeded)
            cellBounds = new Rect[Children.Count];

        for (int i = 0; i < Children.Count; i++)
        {
            var cell = Children[i];
            var column = TargetGrid.Columns.FirstOrDefault(c => Equals(c.Header, cell.Tag));
            if (column != null)
            {
                // Auto-sized columns only measure document cells, which may be narrower than the sum.
                var minimum = Math.Max(TargetGrid.MinColumnWidth, cell.DesiredSize.Width);
                if (column.MinWidth != minimum)
                    column.MinWidth = minimum;
            }

            var header = headers.Children.OfType<DataGridColumnHeader>()
                .FirstOrDefault(h => h.IsVisible && Equals(h.Content, cell.Tag));

            var bounds = header?.TranslatePoint(default, this) is Point position
                ? new Rect(position.X, 0, header.Bounds.Width, 0)
                : default;
            if (cellBounds[i] != bounds)
            {
                cellBounds[i] = bounds;
                arrangeNeeded = true;
            }
        }

        if (arrangeNeeded)
            InvalidateArrange();
    }
}
