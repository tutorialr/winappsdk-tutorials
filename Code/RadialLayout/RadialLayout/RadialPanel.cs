using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.Foundation;

namespace RadialLayout;

public class RadialPanel : Panel
{
    // Dependency Properties & Properties
    public static readonly DependencyProperty ItemHeightProperty =
    DependencyProperty.Register(nameof(ItemHeight),
    typeof(double), typeof(RadialPanel),
    new PropertyMetadata(double.NaN));

    public static readonly DependencyProperty ItemWidthProperty =
    DependencyProperty.Register(nameof(ItemWidth),
    typeof(double), typeof(RadialPanel),
    new PropertyMetadata(double.NaN));

    public static readonly DependencyProperty IsOrientedProperty =
    DependencyProperty.Register(nameof(IsOriented),
    typeof(bool), typeof(RadialPanel),
    new PropertyMetadata(false));

    public double ItemHeight
    {
        get { return (double)GetValue(ItemHeightProperty); }
        set { SetValue(ItemHeightProperty, value); }
    }

    public double ItemWidth
    {
        get { return (double)GetValue(ItemWidthProperty); }
        set { SetValue(ItemWidthProperty, value); }
    }

    public bool IsOriented
    {
        get { return (bool)GetValue(IsOrientedProperty); }
        set { SetValue(IsOrientedProperty, value); }
    }

    // Measure Override Method
    protected override Size MeasureOverride(Size availableSize)
    {
        double itemWidth = ItemWidth;
        double itemHeight = ItemHeight;
        bool hasFixedWidth = !double.IsNaN(itemWidth);
        bool hasFixedHeight = !double.IsNaN(itemHeight);
        var itemSize = new Size(
            hasFixedWidth ? itemWidth : availableSize.Width,
            hasFixedHeight ? itemHeight : availableSize.Height);
        foreach (var element in Children)
        {
            element.Measure(itemSize);
        }
        return itemSize;
    }

    // Arrange Override Method
    protected override Size ArrangeOverride(Size finalSize)
    {
        double itemWidth = ItemWidth;
        double itemHeight = ItemHeight;
        bool hasFixedWidth = !double.IsNaN(itemWidth);
        bool hasFixedHeight = !double.IsNaN(itemHeight);
        double radiusX = finalSize.Width * 0.5;
        double radiusY = finalSize.Height * 0.5;
        int count = Children.Count;
        double deltaAngle = 2 * Math.PI / count;
        var centre = new Point(finalSize.Width / 2,
            finalSize.Height / 2);
        for (int i = 0; i < count; i++)
        {
            var element = Children[i];
            var elementSize = new Size(
            hasFixedWidth ? itemWidth : element.DesiredSize.Width,
            hasFixedHeight ? itemHeight : element.DesiredSize.Height);
            double angle = i * deltaAngle;
            double x = centre.X + radiusX * Math.Cos(angle)
                - elementSize.Width / 2;
            double y = centre.Y + radiusY * Math.Sin(angle)
                - elementSize.Height / 2;
            if (IsOriented)
                element.RenderTransform = null;
            else
            {
                element.RenderTransformOrigin = new Point(0.5, 0.5);
                element.RenderTransform = new RotateTransform()
                {
                    Angle = angle * 180 / Math.PI
                };
            }
            element.Arrange(new Rect(x, y,
                elementSize.Width, elementSize.Height));
        }
        return finalSize;
    }
}