using Comentsys.Toolkit.WindowsAppSdk;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SlideGame;

public class Item : Piece
{
    public Item(int row, int column, int index) =>
        (Row, Column, Value) = (row, column, $"{index}");

    public int Row { get; set; }
    public int Column { get; set; }
}

public class Library
{
    private const string title = "Slide Game";
    private const int canvas_size = 400;
    private const int size = 4;

    private readonly Random _random = new((int)DateTime.UtcNow.Ticks);
    private readonly int[,] _board = new int[size, size];

    private Dialog _dialog;
    private Canvas _canvas;

    private int _moves;
    private List<int> _values;

    // Choose, IsValid & IsComplete
    private List<int> Choose(int minimum, int maximum, int total) =>
        Enumerable.Range(minimum, maximum)
            .OrderBy(r => _random.Next(minimum, maximum))
                .Take(total).ToList();

    private bool IsValid(int row, int column) =>
        row >= 0 && column >= 0 && row <= 3 &&
        column <= 3 && _board[row, column] == 0;

    private bool IsComplete()
    {
        int previous = _board[0, 0];
        for (int row = 0; row < size; row++)
        {
            for (int column = 0; column < size; column++)
            {
                if (_board[row, column] < previous)
                    return false;
                previous = _board[row, column];
            }
        }
        return true;
    }

    // Update & Move
    private void Update()
    {
        _canvas.Children.Clear();
        for (int row = 0; row < size; row++)
        {
            for (int column = 0; column < size; column++)
            {
                if (_board[row, column] > 0)
                {
                    var index = _board[row, column];
                    var piece = new Item(row, column, index)
                    {
                        Foreground = new SolidColorBrush(Colors.White),
                        Fill = new SolidColorBrush(Colors.Black),
                        Height = _canvas.Height / size,
                        Width = _canvas.Width / size,
                        IsSquare = true
                    };
                    piece.PointerReleased += (object sender,
                        PointerRoutedEventArgs e) =>
                        Play(sender as Item);
                    Canvas.SetTop(piece, row * (_canvas.Width / size));
                    Canvas.SetLeft(piece, column * (_canvas.Width / size));
                    _canvas.Children.Add(piece);
                }
            }
        }
    }

    private void Move(Item item, int row, int column)
    {
        _moves++;
        _board[row, column] = _board[item.Row, item.Column];
        _board[item.Row, item.Column] = 0;
        item.Row = row;
        item.Column = column;
        Update();
        if (IsComplete())
            _dialog.Show($"Correct in {_moves} Moves");
    }

    // Play & Setup
    private void Play(Item item)
    {
        if (IsValid(item.Row - 1, item.Column))
            Move(item, item.Row - 1, item.Column);
        else if (IsValid(item.Row, item.Column + 1))
            Move(item, item.Row, item.Column + 1);
        else if (IsValid(item.Row + 1, item.Column))
            Move(item, item.Row + 1, item.Column);
        else if (IsValid(item.Row, item.Column - 1))
            Move(item, item.Row, item.Column - 1);
    }


    public void Setup()
    {
        int index = 1;
        _values = Choose(1, _board.Length - 1, _board.Length - 1);
        _values.Insert(0, 0);
        for (int row = 0; row < size; row++)
        {
            for (int column = 0; column < size; column++)
            {
                _board[row, column] = _values[index++];
                if (index == size * size)
                    index = 0;
            }
        }
    }

    // Layout & New
    public void Layout(Grid grid)
    {
        grid.Children.Clear();
        _canvas = new Canvas()
        {
            Height = canvas_size,
            Width = canvas_size
        };
        grid.Children.Add(_canvas);
    }

    public void New(Grid grid)
    {
        _dialog = new Dialog(grid.XamlRoot, title);
        Layout(grid);
        Setup();
        Update();
    }
}
