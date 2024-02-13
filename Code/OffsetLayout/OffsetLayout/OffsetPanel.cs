using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Foundation;

namespace OffsetLayout;

public class OffsetPanel : Panel
{
    // Dependency Properties & Properties
    public static readonly DependencyProperty MaximumColumnsProperty =
    DependencyProperty.Register(nameof(MaximumColumns),
    typeof(int), typeof(OffsetPanel), new PropertyMetadata(2));

    public static readonly DependencyProperty ColumnOffsetProperty =
    DependencyProperty.Register(nameof(ColumnOffset),
    typeof(double), typeof(OffsetPanel), new PropertyMetadata(10.0));

    public static readonly DependencyProperty RowOffsetProperty =
    DependencyProperty.Register(nameof(RowOffset),
    typeof(double), typeof(OffsetPanel), new PropertyMetadata(10.0));

    public static readonly DependencyProperty SpacingYProperty =
    DependencyProperty.Register(nameof(SpacingY),
    typeof(double), typeof(OffsetPanel), new PropertyMetadata(10.0));

    public static readonly DependencyProperty SpacingXProperty =
    DependencyProperty.Register(nameof(SpacingX),
    typeof(double), typeof(OffsetPanel), new PropertyMetadata(10.0));

    public int MaximumColumns
    {
        get { return (int)GetValue(MaximumColumnsProperty); }
        set { SetValue(MaximumColumnsProperty, value); }
    }

    public double ColumnOffset
    {
        get { return (double)GetValue(ColumnOffsetProperty); }
        set { SetValue(ColumnOffsetProperty, value); }
    }

    public double RowOffset
    {
        get { return (double)GetValue(RowOffsetProperty); }
        set { SetValue(RowOffsetProperty, value); }
    }

    public double SpacingX
    {
        get { return (double)GetValue(SpacingXProperty); }
        set { SetValue(SpacingXProperty, value); }
    }

    public double SpacingY
    {
        get { return (double)GetValue(SpacingYProperty); }
        set { SetValue(SpacingYProperty, value); }
    }

    // Measure Override Method
    protected override Size MeasureOverride(Size availableSize)
    {
        double x = 0;
        double y = 0;
        double itemWidth = 0.0;
        double itemHeight = 0.0;
        for (int i = 0; i < Children.Count; i++)
        {
            var element = Children[i];
            element.Measure(availableSize);
            double width = element.DesiredSize.Width + x;
            double height = element.DesiredSize.Height + y;
            if (width > itemWidth) itemWidth = width;
            if (height > itemHeight) itemHeight = height;
            y += SpacingY;
            if ((i + 1) % MaximumColumns == 0)
            {
                x -= SpacingX * (MaximumColumns - 1);
                x += RowOffset;
                y += ColumnOffset;
            }
            else
                x += SpacingX;
        }
        return new Size(itemWidth, itemHeight);
    }

    // Arrange Override Method
    protected override Size ArrangeOverride(Size finalSize)
    {
        double x = 0;
        double y = 0;
        for (int i = 0; i < Children.Count; i++)
        {
            var element = Children[i];
            element.Arrange(new Rect(new Point(x, y),
                element.DesiredSize));
            y += SpacingY;
            if ((i + 1) % MaximumColumns == 0)
            {
                x -= SpacingX * (MaximumColumns - 1);
                x += RowOffset;
                y += ColumnOffset;
            }
            else
                x += SpacingX;
        }
        return finalSize;
    }
}