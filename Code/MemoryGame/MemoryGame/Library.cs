using Comentsys.Assets.FluentEmoji;
using Comentsys.Toolkit.WindowsAppSdk;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
public class Library
{
    private const string title = "Memory Game";
    private const int size = 4;
    private Dialog _dialog;
    private int _moves = 0;
    private int _row = 0;
    private int _column = 0;
    private int _clicks = 0;
    private int _firstId = 0;
    private int _secondId = 0;
    private Button _first;
    private Button _second;
    private readonly int[,] _board = new int[size, size];
    private readonly List<int> _matches = new();
    private static readonly Dictionary<int, FluentEmojiType> _options = new()
    {
        { 1, FluentEmojiType.NewMoon },
        { 2, FluentEmojiType.WaxingCrescentMoon },
        { 3, FluentEmojiType.FirstQuarterMoon },
        { 4, FluentEmojiType.WaxingGibbousMoon },
        { 5, FluentEmojiType.FullMoon },
        { 6, FluentEmojiType.WaningGibbousMoon },
        { 7, FluentEmojiType.LastQuarterMoon },
        { 8, FluentEmojiType.WaningCrescentMoon }
    };
    private readonly Random _random = new((int)DateTime.UtcNow.Ticks);

    // Choose, Asset, Match, NoMatch & Compare

    private List<int> Choose(int minimum, int maximum, int total) =>
         Enumerable.Range(minimum, maximum)
            .OrderBy(r => _random.Next(minimum, maximum))
                .Take(total).ToList();

    private Viewbox Asset(int option) => new()
    {
        Child = new Asset
        {
            AssetResource = FlatFluentEmoji
     .Get(_options[option])
        }
    };

    private void Match()
    {
        _matches.Add(_firstId);
        _matches.Add(_secondId);
        if (_matches.Count == size * size)
            _dialog.Show($"Matched in {_moves} moves!");
    }

    private void NoMatch()
    {
        if (_first != null)
            _first.Content = null;
        if (_second != null)
            _second.Content = null;
    }

    private async void Compare()
    {
        await Task.Delay(TimeSpan.FromSeconds(1.5));
        if (_firstId == _secondId)
            Match();
        else
            NoMatch();
        _first = null;
        _second = null;
        _moves++;
        _firstId = 0;
        _secondId = 0;
        _clicks = 0;
    }

    // Add
    private void Add(Grid grid, int row, int column)
    {
        Button button = new()
        {
            Width = 75,
            Height = 75
        };
        button.Click += (object sender, RoutedEventArgs e) =>
        {
            button = (Button)sender;
            var row = (int)button.GetValue(Grid.RowProperty);
            var column = (int)button.GetValue(Grid.ColumnProperty);
            int option = _board[row, column];
            if (_clicks <= 1 && _matches.IndexOf(option) < 0)
            {
                // First Choice
                if (_row == 0 && _column == 0)
                {
                    _clicks++;
                    _firstId = option;
                    _first = button;
                    _first.Content = Asset(option);
                    _row = row;
                    _column = column;
                }
                // Second Choice
                else if (!(_row == row && _column == column))
                {
                    _clicks++;
                    _secondId = option;
                    _second = button;
                    _second.Content = Asset(option);
                    Compare();
                    _row = 0;
                    _column = 0;
                }
            }
        };
        button.SetValue(Grid.ColumnProperty, column);
        button.SetValue(Grid.RowProperty, row);
        grid.Children.Add(button);
    }

    // Layout

    private void Layout(Grid grid)
    {
        grid.Children.Clear();
        grid.RowDefinitions.Clear();
        grid.ColumnDefinitions.Clear();
        // Setup Grid
        for (int index = 0; index < size; index++)
        {
            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
        }
        // Setup Board
        for (int row = 0; row < size; row++)
        {
            for (int column = 0; column < size; column++)
            {
                Add(grid, row, column);
            }
        }
    }

    // New

    public void New(Grid grid)
    {
        _dialog = new Dialog(grid.XamlRoot, title);
        _row = 0;
        _moves = 0;
        _column = 0;
        _clicks = 0;
        Layout(grid);
        int counter = 0;
        _matches.Clear();
        List<int> values = new();
        // Pairs : Random 1 - 8
        while (values.Count <= size * size)
        {
            List<int> numbers = Choose(1, size * 2, size * 2);
            for (int number = 0; number < size * 2; number++)
            {
                values.Add(numbers[number]);
            }
        }
        // Board : Random 1 - 16
        List<int> indices = Choose(1, size * size, size * size);
        // Setup Board
        for (int column = 0; column < size; column++)
        {
            for (int row = 0; row < size; row++)
            {
                _board[column, row] = values[indices[counter] - 1];
                counter++;
            }
        }
    }

}
