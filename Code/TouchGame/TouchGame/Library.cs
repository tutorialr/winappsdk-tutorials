using Comentsys.Toolkit.WindowsAppSdk;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;

public class Library
{
    private const string title = "Touch Game";
    private const int size = 2;
    private const int level = 10;
    private const int delay_duration = 250;
    private const int timer_duration = 500;
    private static readonly Dictionary<int, Color> _options = new()
    {
        { 0, Colors.Red },
        { 1, Colors.Blue },
        { 2, Colors.Green },
        { 3, Colors.Gold }
    };
    private readonly Random _random = new((int)DateTime.UtcNow.Ticks);

    private Grid _grid;
    private int _score;
    private bool _over;
    private int _index;
    private bool _playing = false;

    private Dialog _dialog;
    private DispatcherTimer _timer;
    private List<int> _values = new();

    // Choose, Option & Set
    private List<int> Choose(int minimum, int maximum, int total)
    {
        var choose = new List<int>();
        var values = Enumerable.Range(minimum, maximum).ToList();
        for (int index = 0; index < total; index++)
        {
            var value = _random.Next(0, values.Count);
            choose.Add(values[value]);
        }
        return choose;
    }

    private Viewbox Option(int option) => new()
    {
        Child = new Piece()
        {
            IsSquare = true,
            Name = $"{option}",
            Stroke = new SolidColorBrush(_options[option])
        }
    };

    private async void Set(int option)
    {
        var piece = _grid.FindName($"{option}") as Piece;
        piece.Fill = piece.Stroke;
        await Task.Delay(delay_duration);
        piece.Fill = null;
    }

    // Play
    private void Play(int option)
    {
        if (!_playing)
        {
            if (!_over)
            {
                var correct = _values[_index] == option;
                if (correct)
                {
                    if (_index < _score)
                        _index++;
                    else
                    {
                        _score++;
                        if (_score < level)
                        {
                            _index = 0;
                            _timer.Start();
                        }
                        else
                            _over = true;
                    }
                }
                else
                    _over = true;
            }
            if (_over)
                _dialog.Show($"Game Over! You scored {_score} out of {level}!");
        }
    }

    // Add & Layout
    private void Add(Grid grid, int row, int column, int option)
    {
        Button button = new()
        {
            Width = 100,
            Height = 100,
            Tag = option,
            Content = Option(option),
            Margin = new Thickness(5)
        };
        button.Click += (object sender, RoutedEventArgs e) =>
            Play((int)((Button)sender).Tag);
        button.SetValue(Grid.ColumnProperty, column);
        button.SetValue(Grid.RowProperty, row);
        grid.Children.Add(button);
    }

    private void Layout(Grid grid)
    {
        grid.Children.Clear();
        grid.RowDefinitions.Clear();
        grid.ColumnDefinitions.Clear();
        for (int index = 0; index < size; index++)
        {
            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
        }
        int count = 0;
        for (int column = 0; column < size; column++)
        {
            for (int row = 0; row < size; row++)
            {
                Add(grid, row, column, count);
                count++;
            }
        }
    }

    // Tick & New
    private void Tick()
    {
        if (_index <= _score)
        {
            _playing = true;
            Set(_values[_index]);
            _index++;
        }
        else
        {
            _index = 0;
            _timer.Stop();
            _playing = false;
        }
    }

    public void New(Grid grid)
    {
        _index = 0;
        _score = 0;
        _grid = grid;
        _over = false;
        Layout(grid);
        _dialog = new(grid.XamlRoot, title);
        _values = Choose(0, 3, level);
        _timer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromMilliseconds(timer_duration)
        };
        _timer.Tick += (object sender, object e) =>
            Tick();
        _timer.Start();
    }
}