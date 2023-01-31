using Comentsys.Toolkit.WindowsAppSdk;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

public class Library
{
    private const string title = "Light Game";
    private const int on = 1;
    private const int off = 0;
    private const int size = 7;
    private readonly Color lightOn = Colors.Gold;
    private readonly Color lightOff = Colors.Black;
    private readonly int[,] _board = new int[size, size];

    private Grid _grid;
    private Dialog _dialog;
    private int _moves = 0;
    private bool _over = false;

    // Toggle, Set & Winner
    private void Toggle(int row, int column)
    {
        _board[row, column] = _board[row, column] == on ? off : on;
        var piece = _grid.FindName($"{row}:{column}") as Piece;
        piece.Fill = _board[row, column] == on ?
        new SolidColorBrush(lightOn) :
        new SolidColorBrush(lightOff);
    }

    private void Set(int row, int column)
    {
        Toggle(row, column);
        if (row > 0)
            Toggle(row - 1, column); // Toggle Left
        if (row < (size - 1))
            Toggle(row + 1, column); // Toggle Right
        if (column > 0)
            Toggle(row, column - 1); // Toggle Above
        if (column < (size - 1))
            Toggle(row, column + 1); // Toggle Below
    }

    private bool Winner()
    {
        for (int row = 0; row < size; row++)
        {
            for (int column = 0; column < size; column++)
            {
                if (_board[column, row] == on)
                {
                    return false;
                }
            }
        }
        return true;
    }

    // Select & Add
    private void Select(Piece piece)
    {
        if (!_over)
        {
            int row = (int)piece.GetValue(Grid.RowProperty);
            int column = (int)piece.GetValue(Grid.ColumnProperty);
            Set(row, column);
            _moves++;
            if (Winner())
            {
                _dialog.Show($"Well Done! You won in {_moves} moves!");
                _over = true;
            }
        }
        else
            _dialog.Show($"Game Over!");
    }

    private void Add(Grid grid, int row, int column)
    {
        Piece piece = new()
        {
            Width = 50,
            Height = 50,
            IsSquare = true,
            Name = $"{row}:{column}",
            Fill = new SolidColorBrush(lightOn)
        };
        piece.Tapped += (object sender, TappedRoutedEventArgs e) =>
            Select(piece);
        piece.SetValue(Grid.ColumnProperty, column);
        piece.SetValue(Grid.RowProperty, row);
        grid.Children.Add(piece);
    }

    // Layout & New
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
        for (int row = 0; row < size; row++)
        {
            for (int column = 0; column < size; column++)
            {
                Add(grid, row, column);
            }
        }
    }

    public void New(Grid grid)
    {
        _grid = grid;
        _moves = 0;
        _over = false;
        Layout(grid);
        _dialog = new Dialog(grid.XamlRoot, title);
        for (int column = 0; column < size; column++)
        {
            for (int row = 0; row < size; row++)
            {
                _board[column, row] = on;
            }
        }
    }
}