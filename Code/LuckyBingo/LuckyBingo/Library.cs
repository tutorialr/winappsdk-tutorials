using Comentsys.Toolkit.WindowsAppSdk;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
public class Library
{
    private const string title = "Lucky Bingo";
    private const int size = 22;
    private const int balls = 90;
    private const int marks = 25;
    private static readonly Dictionary<int, Color> _style = new()
    {
        { 0, Colors.DarkViolet },
        { 10, Colors.DeepSkyBlue },
        { 20, Colors.Green },
        { 30, Colors.Gold },
        { 40, Colors.DarkOrange },
        { 50, Colors.RoyalBlue },
        { 60, Colors.Crimson },
        { 70, Colors.DarkCyan },
        { 80, Colors.Purple }
    };
    private readonly Random _random = new((int)DateTime.UtcNow.Ticks);

    private int _count;
    private int _house;
    private List<int> _balls;
    private List<int> _marks;
    private bool _over = false;
    private Dialog _dialog;
    private StackPanel _panel = new();

    // Choose, Piece & Add
    private List<int> Choose(int minimum, int maximum, int total) =>
        Enumerable.Range(minimum, maximum)
            .OrderBy(r => _random.Next(minimum, maximum))
                .Take(total).ToList();

    private Piece Piece(bool isBall, int value) => new()
    {
        Width = size,
        Height = size,
        Value = $"{value}",
        IsSquare = !isBall,
        Opacity = isBall ? 0 : 1,
        Stroke = new SolidColorBrush(isBall ? _style
     .Where(w => value > w.Key)
     .Select(s => s.Value)
     .LastOrDefault() : Colors.Red),
        Name = isBall ? $"ball{value}" : $"mark{value}"
    };

    private void Add(ref Grid grid, bool isBall, int row, int column, int value)
    {
        var counter = Piece(isBall, value);
        counter.SetValue(Grid.RowProperty, row);
        counter.SetValue(Grid.ColumnProperty, column);
        grid.Children.Add(counter);
    }

    // Layout, Ball & Mark
    private Grid Layout(bool isBall, int rows, int columns, List<int> list)
    {
        int count = 0;
        Grid grid = new()
        {
            Margin = new Thickness(5),
            VerticalAlignment = VerticalAlignment.Center
        };
        // Setup Grid
        for (int row = 0; row < rows; row++)
        {
            grid.RowDefinitions.Add(new RowDefinition());
        }
        for (int column = 0; column < columns; column++)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition());
        }
        // Setup Board
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                Add(ref grid, isBall, row, column, list[count]);
                count++;
            }
        }
        return grid;
    }
    private void Ball(int value)
    {
        UIElement element = (UIElement)_panel.FindName($"ball{value}");
        if (element != null) element.Opacity = 1;
    }
    private void Mark(int value)
    {
        Piece piece = (Piece)_panel.FindName($"mark{value}");
        if (piece != null) piece.Fill = piece.Stroke;
    }

    // New & Play
    public void New(StackPanel panel)
    {
        _count = 0;
        _house = 0;
        _over = false;
        _panel = panel;
        _balls = Choose(1, balls, balls);
        _marks = Choose(1, balls, marks);
        _dialog = new Dialog(panel.XamlRoot, title);
        panel.Children.Clear();
        panel.Children.Add(Layout(true, 9, 10, _balls));
        panel.Children.Add(Layout(false, 5, 5, _marks));
    }
    public void Play(StackPanel panel)
    {
        if (!panel.Children.Any()) New(panel);
        if (_count < balls && !_over)
        {
            var ball = _balls[_count];
            Ball(ball);
            if (_marks.Contains(ball))
            {
                _house++;
                Mark(ball);
                if (_house == marks)
                {
                    _over = true;
                    _dialog.Show($"Full House in {_count} Balls!");
                }
            }
            _count++;
        }
        else
        {
            _dialog.Show($"Game Over!");
        }
    }

}
