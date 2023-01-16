using Comentsys.Assets.FluentEmoji;
using Comentsys.Toolkit.WindowsAppSdk;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

public class Library
{
    private const string title = "Lucky Bunny";
    private const int size = 5;
    private const int bunnies = 3;
    private const int maximum = 25;
    private const double timer = 1;
    private const bool bunny = true;
    private const bool carrot = false;

    private readonly Random _random = new((int)DateTime.UtcNow.Ticks);

    private DispatcherTimer _timer;
    private Dialog _dialog;
    private Panel _panel;
    private Grid _grid;

    private List<int> _numbers;
    private int _current;
    private int _missed;
    private bool _over;

    private List<int> Choose(int minimum, int maximum, int total) =>
        Enumerable.Range(minimum, maximum)
            .OrderBy(r => _random.Next(minimum, maximum))
                .Take(total).ToList();

    // Get Asset, Get Content, Set Content, Over & Miss
    private Asset GetAsset(bool? isBunny = null) =>
    new()
    {
        AssetResource = FlatFluentEmoji.Get(
        isBunny switch
        {
            bunny => FluentEmojiType.RabbitFace,
            carrot => FluentEmojiType.Carrot,
            _ => FluentEmojiType.None
        })
    };

    private Asset GetContent(int counter) =>
        _numbers[_current] == counter ? GetAsset(false) : GetAsset();

    private void SetContent()
    {
        foreach (var button in _grid.Children.Cast<Button>())
        {
            button.Content = GetContent((int)button.Tag);
        }
    }

    private void Over()
    {
        if (_over)
            _dialog.Show("Game Over!");
    }

    private void Miss()
    {
        if (!_over)
        {
            _panel.Children.Remove(_panel.Children.FirstOrDefault());
            _panel.Children.Add(GetAsset());
            _missed++;
            if (_missed >= bunnies)
            {
                _timer.Stop();
                _over = true;
            }
        }
        Over();
    }

    // Add
    private void Add(Grid grid, int row, int column, int index)
    {
        Button button = new()
        {
            Width = 50,
            Height = 50,
            Tag = index,
            Content = GetContent(index)
        };
        button.Click += (object sender, RoutedEventArgs e) =>
        {
            if (!_over)
            {
                if (_current < maximum - 1)
                {
                    var value = (int)button.Tag;
                    if (_numbers[_current] == value)
                    {
                        _current++;
                        SetContent();
                        _timer.Start();
                    }
                    else
                    {
                        Miss();
                    }
                }
                else
                {
                    _dialog.Show($"You Won with {_missed} missed!");
                    _timer.Stop();
                }
            }
            Over();
        };
        button.SetValue(Grid.ColumnProperty, column);
        button.SetValue(Grid.RowProperty, row);
        grid.Children.Add(button);
    }

    // Layout & New
    private void Layout(Grid grid)
    {
        int index = 0;
        grid.Children.Clear();
        var panel = new StackPanel()
        {
            Orientation = Orientation.Vertical
        };
        _grid = new Grid();
        for (int row = 0; row < size; row++)
        {
            _grid.RowDefinitions.Add(new RowDefinition());
            for (int column = 0; column < size; column++)
            {
                if (row == 0)
                    _grid.ColumnDefinitions.Add(new ColumnDefinition());
                Add(_grid, row, column, index);
                index++;
            }
        }
        panel.Children.Add(_grid);
        _panel = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        for (int counter = 0; counter < bunnies; counter++)
        {
            _panel.Children.Add(GetAsset(true));
        }
        panel.Children.Add(_panel);
        grid.Children.Add(panel);
    }

    public void New(Grid grid)
    {
        _dialog = new Dialog(grid.XamlRoot, title);
        _numbers = Choose(0, size * size, maximum);
        _timer = new()
        {
            Interval = TimeSpan.FromSeconds(timer)
        };
        _timer.Tick += (object sender, object e) =>
            Miss();
        _over = false;
        _current = 0;
        _missed = 0;
        Layout(grid);
    }
}
