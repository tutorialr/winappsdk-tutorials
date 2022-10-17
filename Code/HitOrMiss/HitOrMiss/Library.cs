using Comentsys.Assets.FluentEmoji;
using Comentsys.Toolkit.WindowsAppSdk;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
public class Library
{
    private const string title = "Hit or Miss";
    private const int score = 18;
    private const int size = 6;
    private const string hit = "X";
    private const string miss = "O";
    private readonly string[,] _board = new string[size, size];
    private readonly Random _random = new((int)DateTime.UtcNow.Ticks);

    private int _go = 0;
    private int _hits = 0;
    private int _misses = 0;
    private bool _won = false;
    private Dialog _dialog;

    private List<int> Choose(int minimum, int maximum, int total) =>
        Enumerable.Range(minimum, maximum)
            .OrderBy(r => _random.Next(minimum, maximum))
                .Take(total).ToList();
    private Viewbox Asset(string value) => new()
    {
        Child = new Asset()
        {
            AssetResource = FlatFluentEmoji.Get(
            value == hit ? FluentEmojiType.Collision :
            FluentEmojiType.Hole)
        }
    };

    // Add
    private void Add(ref Grid grid, int row, int column)
    {
        Button button = new()
        {
            Width = 64,
            Height = 64
        };
        button.Click += (object sender, RoutedEventArgs e) =>
        {
            if (!_won)
            {
                button = (Button)sender;
                string selected = _board[
                (int)button.GetValue(Grid.RowProperty),
                (int)button.GetValue(Grid.ColumnProperty)
                ];
                if (button.Content == null)
                {
                    button.Content = Asset(selected);
                    if (selected == hit)
                        _hits++;
                    else if (selected == miss)
                        _misses++;
                    _go++;
                }
                if (_go < (size * size) && _misses < score)
                {
                    if (_hits == score)
                    {
                        _dialog.Show(
                       $"You Won! With {_hits} hits and {_misses} misses");
                        _won = true;
                    }
                }
                else
                {
                    _dialog.Show($"You Lost! With {_hits} hits and {_misses} misses");
                    _won = true;
                }
            }
        };
        button.SetValue(Grid.ColumnProperty, column);
        button.SetValue(Grid.RowProperty, row);
        grid.Children.Add(button);
    }

    // Layout & New
    private void Layout(Grid grid)
    {
        _go = 0;
        _hits = 0;
        _misses = 0;
        grid.Children.Clear();
        grid.RowDefinitions.Clear();
        grid.ColumnDefinitions.Clear();
        // Setup Grid
        for (int index = 0; index < size; index++)
        {
            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
        }
        for (int row = 0; row < size; row++)
        {
            for (int column = 0; column < size; column++)
            {
                Add(ref grid, row, column);
            }
        }
    }

    public void New(Grid grid)
    {
        Layout(grid);
        _won = false;
        int index = 0;
        _dialog = new Dialog(grid.XamlRoot, title);
        // Setup Values
        List<string> values = new();
        while (values.Count < (size * size))
        {
            values.Add(hit);
            values.Add(miss);
        }
        List<int> indices = Choose(1, size * size, size * size);
        // Setup Board
        for (int column = 0; column < size; column++)
        {
            for (int row = 0; row < size; row++)
            {
                _board[column, row] = values[indices[index] - 1];
                index++;
            }
        }
    }
}
