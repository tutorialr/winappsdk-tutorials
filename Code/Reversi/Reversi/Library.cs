using Comentsys.Assets.FluentEmoji;
using Comentsys.Toolkit.Binding;
using Comentsys.Toolkit.WindowsAppSdk;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Reversi;

public enum Player
{
    None, Black, White
}

// Extensions, Position and Move Class
public static class Extensions
{
    public static Player Other(this Player player) =>
    player switch
    {
        Player.Black => Player.White,
        Player.White => Player.Black,
        _ => Player.None
    };
}

public class Position
{
    public int Row { get; set; }

    public int Column { get; set; }

    public Position(int row, int column) =>
    (Row, Column) = (row, column);

    public override bool Equals(object obj) =>
    obj is Position pos && Row == pos.Row && Column == pos.Column;

    public override int GetHashCode() =>
    Row.GetHashCode() + Column.GetHashCode();
}

public class Move
{
    public Player Player { get; set; }

    public Position Position { get; set; }

    public IEnumerable<Position> Outflanked { get; set; }

    public IEnumerable<Position> PreviousValid { get; set; }

    public Move(Player player, Position position,
    IEnumerable<Position> outflanked, IEnumerable<Position> previousValid) =>
    (Player, Position, Outflanked, PreviousValid) =
    (player, position, outflanked, previousValid);
}


public class State
{
    // State Variables, Is Inside & Outflanked
    private const int rows = 8;
    private const int columns = 8;

    public Player[,] Board { get; }
    public Dictionary<Player, int> Count { get; }
    public Player Current { get; private set; }
    public bool Over { get; private set; }
    public Player Winner { get; private set; }
    public Dictionary<Position, IEnumerable<Position>> Valid { get; private set; }

    private bool IsInside(int row, int column) =>
        row >= 0 && row < rows && column >= 0 && column < columns;
    private IEnumerable<Position> Outflanked(
     Position position, Player player, int rowOffset, int columnOffset)
    {
        List<Position> outflanked = new();
        int row = position.Row + rowOffset;
        int column = position.Column + columnOffset;
        while (IsInside(row, column) && Board[row, column] != Player.None)
        {
            if (Board[row, column] == player.Other())
            {
                outflanked.Add(new Position(row, column));
                row += rowOffset;
                column += columnOffset;
            }
            else if (Board[row, column] == player)
                return outflanked;
        }
        return Enumerable.Empty<Position>();
    }

    private IEnumerable<Position> Outflanked(Position position, Player player)
    {
        List<Position> outflanked = new();
        for (int rowOffset = -1; rowOffset <= 1; rowOffset++)
        {
            for (int columnOffset = -1; columnOffset <= 1; columnOffset++)
            {
                if (rowOffset == 0 && columnOffset == 0)
                    continue;
                outflanked.AddRange(
                Outflanked(position, player, rowOffset, columnOffset));
            }
        }
        return outflanked;
    }

    // Is Valid, Get Valid, Set Flip, Set Count & Swap
    private bool IsValid(
        Player player, Position position, out IEnumerable<Position> outflanked)
    {
        outflanked = Board[position.Row, position.Column] == Player.None ?
        Outflanked(position, player) : Enumerable.Empty<Position>();
        return outflanked.Any();
    }

    private Dictionary<Position, IEnumerable<Position>> GetValid(Player player)
    {
        Dictionary<Position, IEnumerable<Position>> valid = new();
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                var position = new Position(row, column);
                if (IsValid(player, position, out IEnumerable<Position> outflanked))
                {
                    valid[position] = outflanked;
                }
            }
        }
        return valid;
    }

    private void SetFlip(IEnumerable<Position> positions)
    {
        foreach (var position in positions)
        {
            Board[position.Row, position.Column] =
            Board[position.Row, position.Column].Other();
        }
    }

    private void SetCount(Player player, int count)
    {
        Count[player] += count + 1;
        Count[player.Other()] -= count;
    }

    private void Swap()
    {
        Current = Current.Other();
        Valid = GetValid(Current);
    }

    // Get Winner, Set Turn & Constructor
    private Player GetWinner()
    {
        if (Count[Player.Black] > Count[Player.White])
            return Player.Black;
        if (Count[Player.Black] < Count[Player.White])
            return Player.White;
        return Player.None;
    }

    private void SetTurn()
    {
        Swap();
        if (Valid.Any())
            return;
        Swap();
        if (Valid.Count == 0)
        {
            Current = Player.None;
            Over = true;
            Winner = GetWinner();
        }
    }

    public State()
    {
        Board = new Player[rows, columns];
        Board[3, 3] = Player.White;
        Board[3, 4] = Player.Black;
        Board[4, 3] = Player.Black;
        Board[4, 4] = Player.White;
        Count = new Dictionary<Player, int>()
        {
            { Player.Black, 2 },
            { Player.White, 2 }
        };
        Current = Player.Black;
        Valid = GetValid(Current);
    }


    // Move & Occupied
    public bool Move(Position position, out Move move)
    {
        if (!Valid.ContainsKey(position))
        {
            move = null;
            return false;
        }
        var player = Current;
        var previous = Valid.Keys;
        var outflanked = Valid[position];
        Board[position.Row, position.Column] = player;
        SetFlip(outflanked);
        SetCount(player, outflanked.Count());
        SetTurn();
        move = new Move(player, position, outflanked, previous);
        return true;
    }
    public IEnumerable<Position> Occupied()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                if (Board[row, column] != Player.None)
                    yield return new Position(row, column);
            }
        }
    }
}

public class Library
{
    // Constants, Variables, Get Source, Set Source, Set Valid & Get Valid
    private const string title = "Reversi";
    private const int square_size = 100;
    private const int disc_size = 72;
    private const int font = 24;
    private const int size = 8;

    private ImageSource[] _sources;
    private State _state;

    private TextBlock _text;
    private Dialog _dialog;
    private Grid _grid;

    private async Task<ImageSource> GetSourceAsync(FluentEmojiType type) =>
        await FlatFluentEmoji.Get(type).AsImageSourceAsync();

    private async Task SetSourceAsync() =>
    _sources ??= (new ImageSource[]
    {
        await GetSourceAsync(FluentEmojiType.GreenCircle),
        await GetSourceAsync(FluentEmojiType.BlackCircle),
        await GetSourceAsync(FluentEmojiType.WhiteCircle)
    });

    private void SetSource(Position position, ImageSource source) =>
        _grid.Children.Cast<Grid>()
        .First(f => Grid.GetRow(f) == position.Row
        && Grid.GetColumn(f) == position.Column)
        .Children.Cast<Image>().First().Source = source;

    private void SetValid(IEnumerable<Position> positions, ImageSource source)
    {
        foreach (var position in positions)
        {
            var square = _state.Board[position.Row, position.Column];
            if (square == Player.None)
                SetSource(position, source);
        }
    }

    private ImageSource GetValid(int row, int column) =>
        _state.Valid.ContainsKey(new Position(row, column)) ? _sources[0] : null;

    // Player Source, Get Player, Get Score, Set Text, Set Flip & Set
    private ImageSource PlayerSource(int row, int column)
    {
        var player = _state.Board[row, column];
        return player != Player.None ? _sources[(int)player] : GetValid(row, column);
    }

    private string GetPlayer(Player player) =>
        Enum.GetName(typeof(Player), player);

    private string GetScore() =>
        $"Score: {GetPlayer(Player.Black)}: {_state.Count[Player.Black]} {GetPlayer(Player.White)}: {_state.Count[Player.White]}";

    private void SetText() =>
        _text.Text = $"Current: {GetPlayer(_state.Current)} - {GetScore()}";

    private void SetFlip(Move move)
    {
        foreach (var position in move.Outflanked)
            SetSource(position, _sources[(int)move.Player]);
    }

    private void Set(Position position, Move move)
    {
        SetValid(move.PreviousValid, null);
        var player = _state.Board[position.Row, position.Column];
        if (player != Player.None)
            SetSource(position, _sources[(int)player]);
        SetFlip(move);
        SetValid(_state.Valid.Keys, _sources[0]);
        SetText();
    }

    // Add & Play
    private void Play(Position position)
    {
        if (!_state.Over)
        {
            if (_state.Move(position, out Move move))
                Set(position, move);
        }
        else
        {
            _dialog.Show(
                    $"Game Over! Winner: {GetPlayer(_state.Winner)} - {GetScore()}");
        }
    }

    private void Add(int row, int column)
    {
        Grid square = new()
        {
            Width = square_size,
            Height = square_size,
            BorderThickness = new Thickness(1),
            BorderBrush = new SolidColorBrush(Colors.Black),
            Background = new SolidColorBrush(Colors.ForestGreen)
        };
        Image image = new()
        {
            Width = disc_size,
            Height = disc_size,
            Source = PlayerSource(row, column)
        };
        square.Children.Add(image);
        square.SetValue(Grid.RowProperty, row);
        square.SetValue(Grid.ColumnProperty, column);
        square.Tapped += (object sender, TappedRoutedEventArgs e) =>
            Play(new Position((int)((Grid)sender).GetValue(Grid.RowProperty),
                (int)((Grid)sender).GetValue(Grid.ColumnProperty)));
        _grid.Children.Add(square);
    }

    // Layout & New
    private void Layout(Grid grid)
    {
        grid.Children.Clear();
        StackPanel panel = new()
        {
            Orientation = Orientation.Vertical
        };
        _text = new TextBlock()
        {
            FontSize = font,
            Margin = new Thickness(2),
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center
        };
        SetText();
        panel.Children.Add(_text);
        _grid = new Grid();
        for (int row = 0; row < size; row++)
        {
            _grid.RowDefinitions.Add(new RowDefinition());
            for (int column = 0; column < size; column++)
            {
                if (row == 0)
                    _grid.ColumnDefinitions.Add(new ColumnDefinition());
                Add(row, column);
            }
        }
        panel.Children.Add(_grid);
        grid.Children.Add(panel);
    }

    public async void New(Grid grid)
    {
        _grid = grid;
        _state = new State();
        await SetSourceAsync();
        _dialog = new Dialog(grid.XamlRoot, title);
        Layout(grid);
    }
}
