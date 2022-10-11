using Comentsys.Assets.FluentEmoji;
using Comentsys.Toolkit.WindowsAppSdk;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.UI;
public class Library
{
    private const string title = "Tic Tac Toe";
    private const string blank = " ";
    private const string nought = "O";
    private const string cross = "X";
    private const int size = 3;

    private readonly string[,] _board = new string[size, size];
    private readonly Color _red = Color.FromArgb(255, 249, 47, 96);
    private readonly Color _blue = Color.FromArgb(255, 0, 166, 237);
    private Dialog _dialog;
    private bool _won = false;
    private string _piece = string.Empty;

    // Winner & Drawn

    private bool Winner() =>
     (_board[0, 0] == _piece && _board[0, 1] ==
     _piece && _board[0, 2] == _piece) ||
     (_board[1, 0] == _piece && _board[1, 1] ==
     _piece && _board[1, 2] == _piece) ||
     (_board[2, 0] == _piece && _board[2, 1] ==
     _piece && _board[2, 2] == _piece) ||
     (_board[0, 0] == _piece && _board[1, 0] ==
     _piece && _board[2, 0] == _piece) ||
     (_board[0, 1] == _piece && _board[1, 1] ==
     _piece && _board[2, 1] == _piece) ||
     (_board[0, 2] == _piece && _board[1, 2] ==
     _piece && _board[2, 2] == _piece) ||
     (_board[0, 0] == _piece && _board[1, 1] ==
     _piece && _board[2, 2] == _piece) ||
     (_board[0, 2] == _piece && _board[1, 1] ==
     _piece && _board[2, 0] == _piece);
    private bool Drawn() =>
     _board[0, 0] != blank && _board[0, 1] !=
     blank && _board[0, 2] != blank &&
     _board[1, 0] != blank && _board[1, 1] !=
     blank && _board[1, 2] != blank &&
     _board[2, 0] != blank && _board[2, 1] !=
     blank && _board[2, 2] != blank;

    // Asset

    private Viewbox Asset() => new()
    {
        Child = new Asset
        {
            AssetResource = 
            _piece switch
            {
                nought => FlatFluentEmoji.Get(
                    FluentEmojiType.HollowRedCircle,
                    _red.AsDrawingColor(),
                    _blue.AsDrawingColor()),
                _ => FlatFluentEmoji.Get(
                    FluentEmojiType.CrossMark)
            }
        }
    };

    // Add

    private void Add(Grid grid, int row, int column)
    {
        Button button = new()
        {
            Width = 75,
            Height = 75,
            Margin = new Thickness(10)
        };
        button.Click += (object sender, RoutedEventArgs e) =>
        {
            if (!_won)
            {
                button = (Button)sender;
                if (button.Content == null)
                {
                    button.Content = Asset();
                    _board[(int)button.GetValue(Grid.RowProperty),
                    (int)button.GetValue(Grid.ColumnProperty)] = _piece;
                }
                if (Winner())
                {
                    _won = true;
                    _dialog.Show($"{_piece} wins!");
                }
                else if (Drawn())
                {
                    _dialog.Show("Draw!");
                }
                else
                {
                    // Swap Players
                    _piece = _piece == cross ? nought : cross;
                }
            }
            else
            {
                _dialog.Show("Game Over!");
            }
        };
        button.SetValue(Grid.ColumnProperty, column);
        button.SetValue(Grid.RowProperty, row);
        grid.Children.Add(button);
    }

    // Layout & New

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
                _board[row, column] = blank;
            }
        }
    }
    public async void New(Grid grid)
    {
        _won = false;
        _dialog = new Dialog(grid.XamlRoot, title);
        _piece = await _dialog.ConfirmAsync("Who goes First?", nought, cross) ?
        nought : cross;
        Layout(grid);
    }

}
