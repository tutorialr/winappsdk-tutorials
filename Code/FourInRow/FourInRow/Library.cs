using Comentsys.Assets.FluentEmoji;
using Comentsys.Toolkit.WindowsAppSdk;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Linq;

public class Library
{
    private const string title = "Four In Row";
    private const int total = 3;
    private const int size = 7;
    private readonly string[] _players = { string.Empty, "Yellow", "Red" };
    private readonly int[,] _board = new int[size, size];

    private int _value = 0;
    private int _amend = 0;
    private int _player = 0;
    private bool _won = false;
    private Dialog _dialog;

    // Check Vertical & Check Horizontal
    private bool CheckVertical(int row, int column)
    {
        _value = 0;
        do
        {
            _value++;
        }
        while (row + _value < size &&
        _board[column, row + _value] == _player);
        return _value > total;
    }
    private bool CheckHorizontal(int row, int column)
    {
        _value = 0;
        _amend = 0;
        // From Left
        do
        {
            _value++;
        }
        while (column - _value >= 0 &&
        _board[column - _value, row] == _player);
        if (_value > total)
            return true;
        // Deduct Middle - Prevent double count
        _value -= 1;
        // Then Right
        do
        {
            _value++;
            _amend++;
        }
        while (column + _amend < size &&
        _board[column + _amend, row] == _player);
        return _value > total;
    }

    // Check Diagonal Top Left & Check Diagonal Top Right
    private bool CheckDiagonalTopLeft(int row, int column)
    {
        _value = 0;
        _amend = 0;
        // From Top Left
        do
        {
            _value++;
        }
        while (column - _value >= 0 && row - _value >= 0 &&
        _board[column - _value, row - _value] == _player);
        if (_value > total)
            return true;
        _value -= 1; // Deduct Middle - Prevent double count
                     // To Bottom Right
        do
        {
            _value++;
            _amend++;
        }
        while (column + _amend < size && row + _amend < size &&
        _board[column + _amend, row + _amend] == _player);
        return _value > total;
    }
    private bool CheckDiagonalTopRight(int row, int column)
    {
        _value = 0;
        _amend = 0;
        // From Top Right
        do
        {
            _value++;
        }
        while (column + _value < size && row - _value >= 0 &&
        _board[column + _value, row - _value] == _player);
        if (_value > total)
            return true;
        _value -= 1; // Deduct Middle - Prevent double count
                     // To Bottom Left
        do
        {
            _value++;
            _amend++;
        }
        while (column - _amend >= 0 &&
        row + _amend < size &&
        _board[column - _amend,
        row + _amend] == _player);
        return _value > total;
    }

    // Winner, Full & Asset
    private bool Winner(int row, int column)
    {
        bool vertical = CheckVertical(row, column);
        bool horizontal = CheckHorizontal(row, column);
        bool diagonalTopLeft = CheckDiagonalTopLeft(row, column);
        bool diagonalTopRight = CheckDiagonalTopRight(row, column);
        return vertical || horizontal ||
        diagonalTopLeft || diagonalTopRight;
    }
    private bool Full()
    {
        for (int row = 0; row < size; row++)
        {
            for (int column = 0; column < size; column++)
            {
                if (_board[column, row] == 0)
                {
                    return false;
                }
            }
        }
        return true;
    }
    private Viewbox Asset(int player) => new()
    {
        Child = new Asset()
        {
            AssetResource = FlatFluentEmoji.Get(
            player == 1 ? FluentEmojiType.YellowCircle :
            FluentEmojiType.RedCircle)
        }
    };

    // Set & Add
    private void Set(Grid grid, int row, int column)
    {
        for (int i = size - 1; i > -1; i--)
        {
            if (_board[column, i] == 0)
            {
                _board[column, i] = _player;
                Button button = (Button)grid.Children.Single(
                w => Grid.GetRow((Button)w) == i
                && Grid.GetColumn((Button)w) == column);
                button.Content = Asset(_player);
                row = i;
                break;
            }
        }
        if (Winner(row, column))
        {
            _won = true;
            _dialog.Show($"{_players[_player]} has won!");
        }
        else if (Full())
            _dialog.Show("Board Full!");
        _player = _player == 1 ? 2 : 1; // Set Player
    }
    private void Add(Grid grid, int row, int column)
    {
        Button button = new()
        {
            Width = 100,
            Height = 100,
            Name = $"{row}:{column}"
        };
        button.Click += (object sender, RoutedEventArgs e) =>
        {
            if (!_won)
            {
                button = (Button)sender;
                row = (int)button.GetValue(Grid.RowProperty);
                column = (int)button.GetValue(Grid.ColumnProperty);
                if (_board[column, 0] == 0) // Check Free Row
                    Set(grid, row, column);
            }
            else
                _dialog.Show("Game Over!");
        };
        button.SetValue(Grid.ColumnProperty, column);
        button.SetValue(Grid.RowProperty, row);
        grid.Children.Add(button);
    }

    // Layout & New
    private void Layout(Grid grid)
    {
        grid.Children.Clear();
        grid.ColumnDefinitions.Clear();
        grid.RowDefinitions.Clear();
        // Setup Grid
        for (int index = 0; index < size; index++)
        {
            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
        }
        // Setup Board
        for (int column = 0; column < size; column++)
        {
            for (int row = 0; row < size; row++)
            {
                Add(grid, row, column);
                _board[row, column] = 0;
            }
        }
    }
    public async void New(Grid grid)
    {
        _won = false;
        _dialog = new Dialog(grid.XamlRoot, title);
        _player = await _dialog.ConfirmAsync("Who goes First?",
        _players[1], _players[2]) ? 1 : 2;
        Layout(grid);
    }
}