using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;

namespace DipadControl;

public enum DipadDirection
{
    Up,
    Down,
    Left,
    Right
}

public class Dipad : Grid
{
    // Constants, Event, Dependency Property & Property
    private const int size = 3;
    private const string path_up = "M 0,0 40,0 40,60 20,80 0,60 0,0 z";
    private const string path_down = "M 0,20 20,0 40,20 40,80 0,80 z";
    private const string path_left = "M 0,0 60,0 80,20 60,40 0,40 z";
    private const string path_right = "M 0,20 20,0 80,0 80,40 20,40 z";

    public delegate void DirectionEvent(object sender, DipadDirection direction);
    public event DirectionEvent Direction;

    public static readonly DependencyProperty ForegroundProperty =
    DependencyProperty.Register(nameof(Foreground), typeof(Brush),
    typeof(Dipad), null);

    public Brush Foreground
    {
        get { return (Brush)GetValue(ForegroundProperty); }
        set { SetValue(ForegroundProperty, value); }
    }

    // GetPath & GetDirection Methods
    private static Path GetPath(string value) =>
    (Path)XamlReader.Load(
        @$"<Path xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'>
            <Path.Data>{value}</Path.Data>
        </Path>");

    private void GetDirection(object sender, PointerRoutedEventArgs e)
    {
        var path = (Path)sender;
        var point = e.GetCurrentPoint(this);
        bool fire = (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse) ?
            point.Properties.IsLeftButtonPressed : point.IsInContact;
        if (fire)
        {
            Direction?.Invoke(path, (DipadDirection)
                Enum.Parse(typeof(DipadDirection), path.Name));
        }
    }

    // Add Method
    private void Add(Grid grid,
    DipadDirection direction, string value,
    int row, int column,
    int? rowspan, int? columnspan,
    VerticalAlignment? vertical = null,
    HorizontalAlignment? horizontal = null)
    {
        var path = GetPath(value);
        path.Margin = new Thickness(5);
        path.Name = direction.ToString();
        if (vertical != null)
            path.VerticalAlignment = vertical.Value;
        if (horizontal != null)
            path.HorizontalAlignment = horizontal.Value;
        path.SetBinding(Shape.FillProperty, new Binding()
        {
            Path = new PropertyPath(nameof(Foreground)),
            Mode = BindingMode.TwoWay,
            Source = this
        });
        path.PointerPressed += GetDirection;
        path.PointerMoved += GetDirection;
        path.SetValue(RowProperty, row);
        path.SetValue(ColumnProperty, column);
        if (rowspan != null)
            path.SetValue(RowSpanProperty, rowspan);
        if (columnspan != null)
            path.SetValue(ColumnSpanProperty, columnspan);
        grid.Children.Add(path);
    }

    // Constructor
    public Dipad()
    {
        var grid = new Grid()
        {
            Height = 180,
            Width = 180
        };
        grid.Children.Clear();
        grid.ColumnDefinitions.Clear();
        grid.RowDefinitions.Clear();
        for (int index = 0; index < size; index++)
        {
            grid.RowDefinitions.Add(new RowDefinition()
            {
                Height = (index == 1) ? GridLength.Auto :
                    new GridLength(100, GridUnitType.Star)
            });
            grid.ColumnDefinitions.Add(new ColumnDefinition()
            {
                Width = (index == 1) ? GridLength.Auto :
                    new GridLength(100, GridUnitType.Star)
            });
        }
        Add(grid, DipadDirection.Up, path_up, 0, 1, 2, null,
            VerticalAlignment.Top, null);
        Add(grid, DipadDirection.Down, path_down, 1, 1, 2, null,
            VerticalAlignment.Bottom, null);
        Add(grid, DipadDirection.Left, path_left, 1, 0, null, 2, null,
            HorizontalAlignment.Left);
        Add(grid, DipadDirection.Right, path_right, 1, 1, null, 2, null,
            HorizontalAlignment.Right);
        var box = new Viewbox()
        {
            Child = grid
        };
        Children.Add(box);
    }
}