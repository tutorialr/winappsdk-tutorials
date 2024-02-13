using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Foundation;

namespace DockingLayout;

public class DockingPanel : Panel
{
    public enum Dock
    {
        Left,
        Top,
        Right,
        Bottom
    }

    // Dependency Properties, Properties, Get Dock & Set Dock Methods
    public static readonly DependencyProperty LastChildFillProperty =
    DependencyProperty.Register(nameof(LastChildFill), typeof(bool),
    typeof(DockingPanel), new PropertyMetadata(false));

    public static readonly DependencyProperty DockProperty =
    DependencyProperty.RegisterAttached(nameof(Dock), typeof(Dock),
    typeof(DockingPanel), new PropertyMetadata(Dock.Left));

    public bool LastChildFill
    {
        get { return (bool)GetValue(LastChildFillProperty); }
        set { SetValue(LastChildFillProperty, value); }
    }

    public static Dock GetDock(UIElement element)
    {
        ArgumentNullException.ThrowIfNull(element);
        return (Dock)element.GetValue(DockProperty);
    }

    public static void SetDock(UIElement element, Dock dock)
    {
        ArgumentNullException.ThrowIfNull(element);
        element.SetValue(DockProperty, dock);
    }

    // Measure Override Methods
    protected override Size MeasureOverride(Size availableSize)
    {
        double width = 0.0;
        double height = 0.0;
        double maxWidth = 0.0;
        double maxHeight = 0.0;
        foreach (var element in Children)
        {
            var remainingSize = new Size(
                Math.Max(0.0, availableSize.Width - width),
                Math.Max(0.0, availableSize.Height - height));
            element.Measure(remainingSize);
            var desiredSize = element.DesiredSize;
            switch (GetDock(element))
            {
                case Dock.Left:
                case Dock.Right:
                    maxHeight = Math.Max(maxHeight, height + desiredSize.Height);
                    width += desiredSize.Width;
                    break;
                case Dock.Top:
                case Dock.Bottom:
                    maxWidth = Math.Max(maxWidth, width + desiredSize.Width);
                    height += desiredSize.Height;
                    break;
            }
        }
        maxWidth = Math.Max(maxWidth, width);
        maxHeight = Math.Max(maxHeight, height);
        return new Size(maxWidth, maxHeight);
    }

    // Arrange Override Method
    protected override Size ArrangeOverride(Size finalSize)
    {
        double left = 0.0;
        double top = 0.0;
        double right = 0.0;
        double bottom = 0.0;
        var children = Children;
        var count = children.Count - (LastChildFill ? 1 : 0);
        var index = 0;
        foreach (var element in children)
        {
            var rect = new Rect(left, top,
                Math.Max(0.0, finalSize.Width - left - right),
                Math.Max(0.0, finalSize.Height - top - bottom));
            if (index < count)
            {
                var desiredSize = element.DesiredSize;
                switch (GetDock(element))
                {
                    case Dock.Left:
                        left += desiredSize.Width;
                        rect.Width = desiredSize.Width;
                        break;
                    case Dock.Top:
                        top += desiredSize.Height;
                        rect.Height = desiredSize.Height;
                        break;
                    case Dock.Right:
                        right += desiredSize.Width;
                        rect.X = Math.Max(0.0, finalSize.Width - right);
                        rect.Width = desiredSize.Width;
                        break;
                    case Dock.Bottom:
                        bottom += desiredSize.Height;
                        rect.Y = Math.Max(0.0, finalSize.Height - bottom);
                        rect.Height = desiredSize.Height;
                        break;
                }
            }
            element.Arrange(rect);
            index++;
        }
        return finalSize;
    }
}