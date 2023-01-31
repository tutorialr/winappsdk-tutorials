using Comentsys.Toolkit.Binding;
using Comentsys.Toolkit.WindowsAppSdk;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TilesGame;

public enum State
{
    White,
    Black,
    Start,
    Finish,
    Correct,
    Incorrect
}

// Item Class
public class Item
{
    public int Row { get; set; }

    public int Column { get; set; }

    public State State { get; set; }

    public Item(int row, int column, State state) =>
        (Row, Column, State) = (row, column, state);
}

public class Board : ObservableBase
{
    // Board Constants, Members & Properties
    private const int bound = 1;
    private const int timer = 100;
    private readonly State[,] _board;
    private readonly Random _random = new((int)DateTime.UtcNow.Ticks);
    private readonly int _rows;
    private readonly int _columns;
    private readonly int _levels;
    private readonly int _start;
    private readonly int _finish;

    private int _index;
    private int _offset;
    private bool _over;
    private bool _started;
    private TimeSpan _time;
    private TimeSpan _best;
    private DateTime _when;
    private string _message;
    private DispatcherTimer _timer;

    public TimeSpan Time { get => _time; set => SetProperty(ref _time, value); }
    public TimeSpan Best { get => _best; set => SetProperty(ref _best, value); }
    public string Message { get => _message; set => SetProperty(ref _message, value); }

    // Board Choose, Set & Start Methods
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

    private void Set(int columns, int levels)
    {
        var values = Choose(0, columns, levels - bound);
        for (int level = 0; level < levels; level++)
        {
            for (int column = 0; column < columns; column++)
            {
                State tile;
                if (level < _finish)
                    tile = State.Finish;
                else if (level >= _start)
                    tile = State.Start;
                else
                    tile = values[level] == column ? State.Black : State.White;
                _board[level, column] = tile;
            }
        }
    }

    private void Start()
    {
        _when = DateTime.UtcNow;
        if (_timer != null)
            _timer.Stop();
        _timer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromMilliseconds(timer)
        };
        _timer.Tick += (object sender, object e) =>
        {
            if (_started && !_over)
                Time = DateTime.UtcNow - _when;
            else
                _timer.Stop();
        };
        _started = true;
        _timer.Start();
    }

    // Board Play Method
    public void Play(Item item)
    {
        if (!_over)
        {
            if (item.State == State.Black)
            {
                if (item.Row == _index)
                {
                    if (!_started)
                        Start();
                    _board[item.Row, item.Column] = State.Correct;
                    _index--;
                    if (_offset > 0)
                        _offset--;
                    if (_index < _finish)
                    {
                        _started = false;
                        Time = DateTime.UtcNow - _when;
                        if (Best == TimeSpan.Zero || Time < Best)
                            Best = Time;
                        Message = $"Completed in {Time:ss\\.fff}!";
                    }
                }
                else
                {
                    _board[item.Row, item.Column] = State.Incorrect;
                    Message = $"Game Over, Hit Wrong Tile!";
                    _over = true;
                }
            }
            else if (item.State == State.White)
            {
                _board[item.Row, item.Column] = State.Incorrect;
                Message = $"Game Over, Hit White Tile!";
                _over = true;
            }
        }
        else
            Message = $"Game Over!";
    }

    // Board Get & New Methods and Constructor
    public Item Get(int row, int column) =>
        new(row + _offset, column, _board[row + _offset, column]);

    public void New()
    {
        _over = false;
        _started = false;
        Time = TimeSpan.Zero;
        Set(_columns, _levels);
        _offset = _levels - _rows;
        _index = _levels - (bound * 2);
        Message = "Don't Hit White Tiles!";
    }

    public Board(int rows, int columns, int levels)
    {
        _rows = rows;
        _columns = columns;
        _levels = levels;
        _start = levels - bound;
        _finish = rows - bound;
        _board = new State[levels, columns];
        New();
    }
}

public class Library
{
    // Library Constants, Variables & Play Method
    private const int rows = 6;
    private const int columns = 4;
    private const int levels = 32;
    private const int size = 36;
    private const int font = 10;

    private readonly Dictionary<State, SolidColorBrush> _brushes = new()
{
    { State.White, new SolidColorBrush(Colors.White) },
    { State.Black, new SolidColorBrush(Colors.Black) },
    { State.Start, new SolidColorBrush(Colors.Gold) },
    { State.Finish, new SolidColorBrush(Colors.ForestGreen) },
    { State.Correct, new SolidColorBrush(Colors.Gray) },
    { State.Incorrect, new SolidColorBrush(Colors.IndianRed) }
};
    private readonly Board _board = new(rows, columns, levels);

    private Piece[,] _pieces;
    private Grid _grid;

    private void Play(Item selected)
    {
        _board.Play(selected);
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                var item = _board.Get(row, column);
                var piece = _pieces[row, column];
                piece.Tag = item;
                piece.Fill = _brushes[item.State];
            }
        }
    }

    // Library SetPieces Method
    private Piece[,] SetPieces(Grid grid)
    {
        grid.Children.Clear();
        var pieces = new Piece[rows, columns];
        for (int row = 0; row < rows; row++)
        {
            grid.RowDefinitions.Add(new RowDefinition());
            for (int column = 0; column < columns; column++)
            {
                if (row == 0)
                    grid.ColumnDefinitions.Add(new ColumnDefinition());
                var item = _board.Get(row, column);
                var piece = new Piece()
                {
                    Tag = item,
                    Width = size,
                    Height = size,
                    IsSquare = true,
                    Fill = _brushes[item.State],
                    Stroke = new SolidColorBrush(Colors.WhiteSmoke)
                };
                piece.Tapped += (object sender, TappedRoutedEventArgs e) =>
                    Play((Item)(sender as Piece).Tag);
                Grid.SetColumn(piece, column);
                Grid.SetRow(piece, row);
                grid.Children.Add(piece);
                pieces[row, column] = piece;
            }
        }
        return pieces;
    }

    // Library GetBoundText Method
    private TextBlock GetBoundText(string property, string format = null)
    {
        var text = new TextBlock()
        {
            FontSize = font,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        var binding = new Binding()
        {
            Source = _board,
            Mode = BindingMode.OneWay,
            Path = new PropertyPath(property),
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
            Converter = new StringFormatConverter(),
            ConverterParameter = format
        };
        BindingOperations.SetBinding(text, TextBlock.TextProperty, binding);
        return text;
    }

    // Library Layout & New Methods
    private void Layout(Grid grid)
    {
        grid.Children.Clear();
        var panel = new StackPanel()
        {
            Orientation = Orientation.Horizontal
        };
        var time = GetBoundText(nameof(_board.Time), "Time: {0:ss\\.fff}");
        panel.Children.Add(time);
        var inner = new StackPanel()
        {
            Orientation = Orientation.Vertical
        };
        var message = GetBoundText(nameof(_board.Message));
        inner.Children.Add(message);
        _grid = new()
        {
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _pieces = SetPieces(_grid);
        inner.Children.Add(_grid);
        panel.Children.Add(inner);
        var best = GetBoundText(nameof(_board.Best), "Best: {0:ss\\.fff}");
        panel.Children.Add(best);
        grid.Children.Add(panel);
    }

    public void New(Grid grid)
    {
        _board.New();
        Layout(grid);
    }
}