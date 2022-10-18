using Comentsys.Toolkit.WindowsAppSdk;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI;
public class Library
{
    private const string title = "Lucky Roulette";
    private const int size = 400;
    private const int rim = 50;

    private static readonly int[] _wheel =
    {
        0, 32, 15, 19, 4, 21, 2, 25, 17,
        34, 6, 27, 13, 36, 11, 30, 8, 23,
        10, 5, 24, 16, 33, 1, 20, 14, 31,
        9, 22, 18, 29, 7, 28, 12, 35, 3, 26
    };
    private readonly List<int> _values =
    Enumerable.Range(0, _wheel.Length).ToList();
    private readonly Random _random = new((int)DateTime.UtcNow.Ticks);
    private int _spins = 0;
    private int _spinValue = 0;
    private int _pickValue = 0;
    private Dialog _dialog;
    private StackPanel _panel = new();

    private bool IsOdd(int value) =>
        value % 2 != 0;

    // Style & Pocket
    private Color Style(int value) =>
     value switch
     {
         >= 1 and <= 10 or >= 19 and <= 28 => IsOdd(value) ?
         Colors.Black : Colors.DarkRed,
         >= 11 and <= 18 or >= 29 and <= 36 => IsOdd(value) ?
         Colors.DarkRed : Colors.Black,
         0 => Colors.DarkGreen,
         _ => Colors.Transparent,
     };
    private Grid Pocket(int value)
    {
        Color fill = Style(value);
        Grid grid = new()
        {
            Width = size,
            Height = size
        };
        Grid pocket = new()
        {
            Width = 26,
            Height = rim,
            CornerRadius = new CornerRadius(4),
            Background = new SolidColorBrush(fill),
            VerticalAlignment = VerticalAlignment.Top
        };
        TextBlock text = new()
        {
            FontSize = 20,
            Text = $"{value}",
            VerticalAlignment = VerticalAlignment.Top,
            Foreground = new SolidColorBrush(Colors.Gold),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Ellipse ball = new()
        {
            Width = 20,
            Height = 20,
            Opacity = 0,
            Name = $"ball{value}",
            Margin = new Thickness(0, 0, 0, 4),
            Fill = new SolidColorBrush(Colors.Snow),
            VerticalAlignment = VerticalAlignment.Bottom
        };
        pocket.Children.Add(text);
        pocket.Children.Add(ball);
        grid.Children.Add(pocket);
        return grid;
    }

    // Wheel
    private Canvas Wheel()
    {
        Canvas canvas = new()
        {
            Width = size,
            Height = size,
            Margin = new Thickness(5)
        };
        Ellipse ellipse = new()
        {
            Width = size,
            Height = size,
            StrokeThickness = rim,
            Stroke = new SolidColorBrush(Colors.Peru)
        };
        canvas.Children.Add(ellipse);
        int index = 0;
        double radiusX = canvas.Width * 0.5;
        double radiusY = canvas.Height * 0.5;
        double delta = 2 * Math.PI / _wheel.Length;
        Point centre = new(canvas.Width / 2, canvas.Height / 2);
        foreach (int value in _wheel)
        {
            Grid pocket = Pocket(value);
            Size size = new(pocket.DesiredSize.Width, pocket.DesiredSize.Height);
            double angle = index * delta;
            double x = centre.X + radiusX *
            Math.Cos(angle) - size.Width / 2;
            double y = centre.Y + radiusY *
            Math.Sin(angle) - size.Height / 2;
            pocket.RenderTransformOrigin = new Point(0.5, 0.5);
            pocket.RenderTransform = new RotateTransform()
            {
                Angle = angle * 180 / Math.PI
            };
            pocket.Arrange(new Rect(x, y, size.Width, size.Height));
            canvas.Children.Add(pocket);
            index++;
        }
        return canvas;
    }

    // Ball, Pick & Square
    private void Ball(int value, int opacity)
    {
        UIElement element = (UIElement)_panel.FindName($"ball{value}");
        if (element != null) element.Opacity = opacity;
    }
    private void Pick(int value, Color fill)
    {
        Piece piece = (Piece)_panel.FindName($"pick{value}");
        if (piece != null) piece.Stroke = new SolidColorBrush(fill);
    }
    private void Square(ref Grid grid, int row, int column, int value)
    {
        var piece = new Piece()
        {
            IsSquare = true,
            Value = $"{value}",
            Name = $"pick{value}",
            Fill = new SolidColorBrush(Style(value)),
            Stroke = new SolidColorBrush(Style(value)),
            Foreground = new SolidColorBrush(Colors.Gold)
        };
        piece.Tapped += (object sender, TappedRoutedEventArgs e) =>
        {
            Ball(_spinValue, 0);
            Pick(_pickValue, Style(_pickValue));
            _spins++;
            _spinValue = _values[_random.Next(0, _values.Count)];
            _pickValue = int.Parse(((Piece)sender).Value);
            Ball(_spinValue, 1);
            Pick(_pickValue, Colors.Peru);
            // Check Win
            if (_spinValue == _pickValue)
            {
                _spins = 0;
                _dialog.Show($"Won {_spins} with {_spinValue}");
            }
            else
            {
                _dialog.Show($"Lost {_spins} with {_pickValue} was {_spinValue}");
            }
        };
        piece.SetValue(Grid.RowProperty, row);
        piece.SetValue(Grid.ColumnProperty, column);
        grid.Children.Add(piece);
    }

    // Layout & New
    private Grid Layout()
    {
        int count = 1;
        int rows = 13;
        int columns = 3;
        Grid grid = new()
        {
            Height = size,
            Margin = new Thickness(5)
        };
        for (int index = 0; index < rows; index++)
        {
            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
        }
        var numbers = _wheel.OrderBy(o => o).ToArray();
        Square(ref grid, 0, 1, numbers[0]);
        for (int row = 1; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                Square(ref grid, row, column, numbers[count]);
                count++;
            }
        }
        return grid;
    }
    public void New(StackPanel panel)
    {
        _spins = 0;
        _panel = panel;
        _panel.Children.Clear();
        _panel.Children.Add(Wheel());
        _panel.Children.Add(Layout());
        _dialog = new Dialog(panel.XamlRoot, title);
    }


}
