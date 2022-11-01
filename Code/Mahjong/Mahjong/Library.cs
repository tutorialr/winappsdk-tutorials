using Comentsys.Assets.Games;
using Comentsys.Toolkit.Binding;
using Comentsys.Toolkit.WindowsAppSdk;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mahjong;

// State & Result Enums and Position & Tile Class
public enum State
{
    None,
    Selected,
    Disabled,
    Hint
}
public enum Result
{
    DifferentTypes = 1,
    UnableToMove = 2,
    InvalidMove = 4,
    ValidMove = 8,
    NoMoves = 16,
    Winner = 32,
}
public class Position
{
    public int Row { get; set; }
    public int Column { get; set; }
    public int Index { get; set; }
    public Position(int row, int column, int index) =>
    (Row, Column, Index) = (row, column, index);
}
public class Tile : ObservableBase
{
    private State _state;
    public Position Position { get; }
    public MahjongTileType? Type { get; set; }
    public State State { get => _state; set => SetProperty(ref _state, value); }
    public Tile(MahjongTileType type, Position position) =>
    (Type, Position) = (type, Position = position);
    public Tile(Position position) =>
    (Type, Position) = (null, Position = position);
}

// Pair Class
public class Pair
{
    private static readonly Random _random = new((int)DateTime.UtcNow.Ticks);
    public Tile TileOne { get; set; }
    public Tile TileTwo { get; set; }
    public Pair(Tile tileOne, Tile tileTwo) =>
    (TileOne, TileTwo) = (tileOne, tileTwo);
    public static Pair Get(List<Tile> tiles)
    {
        if (tiles.Count < 2)
            throw new Exception();
        var index = _random.Next() % tiles.Count;
        var tileOne = tiles[index];
        tiles.RemoveAt(index);
        index = _random.Next() % tiles.Count;
        var tileTwo = tiles[index];
        tiles.RemoveAt(index);
        return new Pair(tileOne, tileTwo);
    }
}

public class Board
{
    // Board Constants, Variables, Event & Get Methods
    private const int rows = 8;
    private const int columns = 10;
    private const int indexes = 5;
    private static readonly byte[] _layout =
    {
        0, 1, 1, 1, 1, 1, 1, 1, 1, 0,
        1, 1, 2, 2, 2, 2, 2, 2, 1, 1,
        1, 1, 2, 3, 4, 4, 3, 2, 1, 1,
        1, 1, 2, 4, 5, 5, 4, 2, 1, 1,
        1, 1, 2, 4, 5, 5, 4, 2, 1, 1,
        1, 1, 2, 3, 4, 4, 3, 2, 1, 1,
        1, 1, 2, 2, 2, 2, 2, 2, 1, 1,
        0, 1, 1, 1, 1, 1, 1, 1, 1, 0
    };
    private static readonly Random _random = new((int)DateTime.UtcNow.Ticks);
    private static readonly List<MahjongTileType> _types =
     Enum.GetValues(typeof(MahjongTileType))
     .Cast<MahjongTileType>()
     .Where(w => w != MahjongTileType.Back)
     .ToList();
    private readonly List<Tile> _tiles;

    public delegate void RemovedEventHandler(Tile tile);
    public event RemovedEventHandler Removed;

    public IEnumerable<Tile> Get(int row, int column) =>
        _tiles.Where(w => w.Position.Row == row
        && w.Position.Column == column)
        .OrderBy(o => o.Position.Index);

    private Tile Get(int row, int column, int index) =>
        _tiles.FirstOrDefault(
        f => f.Position.Row == row
        && f.Position.Column == column
        && f.Position.Index == index);

    private Tile Get(Tile tile) =>
        Get(tile.Position.Row, tile.Position.Column, tile.Position.Index);

    // Can Move, Can Move Up, Can Move Right, Can Move Left, Can Move & Next Move
    private bool CanMove(Tile tile, int rowOffset, int columnOffset, int indexOffset)
    {
        var found = Get(
        tile.Position.Row + rowOffset,
        tile.Position.Column + columnOffset,
        tile.Position.Index + indexOffset
        );
        return found == null || tile == found;
    }
    private bool CanMoveUp(Tile tile) =>
        CanMove(tile, 0, 0, 1);

    private bool CanMoveRight(Tile tile) =>
        CanMove(tile, 1, 0, 0);

    private bool CanMoveLeft(Tile tile) =>
        CanMove(tile, -1, 0, 0);

    public bool CanMove(Tile tile)
    {
        bool up = CanMoveUp(tile);
        bool upLeft = up && CanMoveLeft(tile);
        bool upRight = up && CanMoveRight(tile);
        return upLeft || upRight;
    }

    private bool NextMove()
    {
        var removable = new List<Tile>();
        foreach (var tile in _tiles)
            if (CanMove(tile))
                removable.Add(tile);
        for (int i = 0; i < removable.Count; i++)
            for (int j = 0; j < removable.Count; j++)
                if (j != i && removable[i].Type == removable[j].Type)
                    return true;
        return false;
    }

    // Add, Remove, Removable & Structure
    private void Add(Tile tile) =>
        _tiles.Add(tile);

    private void Remove(Tile tile)
    {
        if (tile == Get(tile))
        {
            _tiles.Remove(tile);
            Removed?.Invoke(tile);
        }
    }

    private List<Tile> Removable()
    {
        List<Tile> removable = new();
        foreach (var tile in _tiles)
            if (CanMove(tile))
                removable.Add(tile);
        foreach (Tile tile in removable)
            Remove(tile);
        return removable;
    }

    private void Structure()
    {
        for (int index = 0; index < indexes; index++)
        {
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    var current = _layout[row * columns + column];
                    if (current > 0 && index < current)
                        Add(new Tile(new Position(row, column, index)));
                }
            }
        }
    }

    // Get Hint & Scramble
    private Pair GetHint()
    {
        var tiles = new List<Tile>();
        foreach (var tile in _tiles)
            if (CanMove(tile))
                tiles.Add(tile);
        for (int i = 0; i < tiles.Count; i++)
        {
            for (int j = 0; j < tiles.Count; j++)
            {
                if (i == j)
                    continue;
                if (tiles[i].Type == tiles[j].Type)
                    return new Pair(tiles[i], tiles[j]);
            }
        }
        return null;
    }

    public void Scramble()
    {
        List<Pair> reversed = new();
        while (_tiles.Count > 0)
        {
            List<Tile> removable = new();
            removable.AddRange(Removable());
            while (removable.Count > 1)
                reversed.Add(Pair.Get(removable));
            foreach (var tile in removable)
                Add(tile);
        }
        for (int i = reversed.Count - 1; i >= 0; i--)
        {
            int index = _random.Next() % _types.Count;
            reversed[i].TileOne.Type = _types[index];
            reversed[i].TileTwo.Type = _types[index];
            Add(reversed[i].TileOne);
            Add(reversed[i].TileTwo);
        }
    }

    // Constructor, Play, Set Hint & Set Disabled
    public Board()
    {
        _tiles = new List<Tile>();
        Structure();
        Scramble();
    }
    public Result Play(Tile tileOne, Tile tileTwo)
    {
        if (tileOne == tileTwo)
            return Result.InvalidMove;
        if (tileOne.Type != tileTwo.Type)
            return Result.DifferentTypes;
        if (!CanMove(tileOne) || !CanMove(tileTwo))
            return Result.UnableToMove;
        Remove(tileOne);
        Remove(tileTwo);
        if (_tiles.Count == 0)
            return Result.Winner;
        var result = Result.ValidMove;
        if (!NextMove())
            result |= Result.NoMoves;
        return result;
    }
    public void SetHint()
    {
        if (_tiles.Count > 0)
        {
            var hint = GetHint();
            if (hint != null)
            {
                hint.TileOne.State = State.Hint;
                hint.TileTwo.State = State.Hint;
            }
        }
    }
    public void SetDisabled()
    {
        if (_tiles.Count > 0)
            foreach (var tile in _tiles)
                tile.State = CanMove(tile) ?
                State.None : State.Disabled;
    }
}

// State to Brush Converter
public class StateToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType,
    object parameter, string language) =>
    new SolidColorBrush((State)value switch
    {
        State.None => Colors.Transparent,
        State.Selected => Colors.ForestGreen,
        State.Disabled => Colors.DarkSlateGray,
        State.Hint => Colors.CornflowerBlue,
        _ => Colors.Transparent
    });
    public object ConvertBack(object value, Type targetType,
    object parameter, string language) =>
    throw new NotImplementedException();
}

public class Library
{
    // Constants, Variables, Get Source, Set Sources, Get Tile & Shuffle
    private const string title = "Mahjong";
    private const int rows = 8;
    private const int columns = 10;
    private const int tile_width = 74;
    private const int tile_height = 95;
    private const int square_height = 120;
    private const int square_width = 90;
    private readonly Dictionary<MahjongTileType, ImageSource> _sources = new();
    private Board _board = new();
    private Dialog _dialog;
    private Grid _grid;
    private Tile _selected;
    private bool _gameOver;

    private static async Task<ImageSource> GetSourceAsync(MahjongTileType type) =>
     await MahjongTile.Get(type)
     .AsImageSourceAsync();
    private async Task SetSourcesAsync()
    {
        if (_sources.Count == 0)
            foreach (var mahjongTileType in Enum.GetValues<MahjongTileType>())
                _sources.Add(mahjongTileType, await GetSourceAsync(mahjongTileType));
    }

    private ImageSource GetTile(MahjongTileType? type) =>
     type == null ? null : _sources[type.Value];

    private void Shuffle()
    {
        _board.Scramble();
        _grid.Children.Clear();
        for (int column = 0; column < columns; column++)
            for (int row = 0; row < rows; row++)
                Add(row, column);
    }

    // Play
    private async void Play(Tile tile)
    {
        if (!_gameOver)
        {
            if (!_board.CanMove(tile))
                return;
            if (_selected == null || tile == _selected)
            {
                if (_selected == tile)
                {
                    tile.State = State.None;
                    _selected = null;
                }
                else
                {
                    tile.State = State.Selected;
                    _selected = tile;
                }
            }
            else
            {
                var state = _board.Play(_selected, tile);
                _board.SetDisabled();
                if (state == Result.Winner)
                    _gameOver = true;
                else if ((state & Result.NoMoves) != 0)
                {
                    if (await _dialog.ConfirmAsync(
                    "No further moves. Shuffle?", "Yes", "No"))
                        Shuffle();
                }
                _selected = null;
            }
        }
        if (_gameOver)
            _dialog.Show("You Won, Game Over!");
    }

    // Add
    private void Add(int row, int column)
    {
        Canvas square = new()
        {
            Width = square_width,
            Height = square_height
        };
        var tiles = _board.Get(row, column);
        foreach (var tile in tiles)
        {
            Canvas canvas = new()
            {
                Tag = tile,
                Width = tile_width,
                Height = tile_height,
            };
            Image image = new()
            {
                Tag = tile,
                Width = tile_width,
                Height = tile_height,
                Source = GetTile(tile.Type)
            };
            image.Tapped += (object sender, TappedRoutedEventArgs e) =>
            Play((sender as Image).Tag as Tile);
            canvas.Children.Add(image);
            var rectangle = new Rectangle()
            {
                Tag = tile,
                Opacity = 0.25,
                Width = tile_width,
                Height = tile_height,
                IsHitTestVisible = false
            };
            var binding = new Binding()
            {
                Source = tile,
                Mode = BindingMode.OneWay,
                Converter = new StateToBrushConverter(),
                Path = new PropertyPath(nameof(tile.State)),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            BindingOperations.SetBinding(rectangle, Shape.FillProperty, binding);
            if (!_board.CanMove(tile))
                tile.State = State.Disabled;
            canvas.Children.Add(rectangle);
            Canvas.SetTop(canvas, -(tile.Position.Index * 5));
            square.Children.Add(canvas);
        }
        square.SetValue(Grid.RowProperty, row);
        square.SetValue(Grid.ColumnProperty, column);
        _grid.Children.Add(square);
    }

    // Layout, Remove, New & Hint
    private void Layout(Grid grid)
    {
        grid.Children.Clear();
        _grid = new Grid();
        for (int column = 0; column < columns; column++)
        {
            _grid.RowDefinitions.Add(new RowDefinition());
            for (int row = 0; row < rows; row++)
            {
                if (row == 0)
                    _grid.ColumnDefinitions.Add(new ColumnDefinition());
                Add(row, column);
            }
        }
        grid.Children.Add(_grid);
    }

    private void Remove(Tile tile)
    {
        foreach (var item in _grid.Children.Cast<Canvas>()
        .Where(w => w.Children.Any()))
        {
            var canvas = item.Children
            .Cast<Canvas>()
            .FirstOrDefault(w => w.Tag as Tile == tile);
            if (canvas != null)
                canvas.Children.Clear();
        }
    }

    public async void New(Grid grid)
    {
        _gameOver = false;
        _board = new Board();
        _board.Removed += (Tile tile) =>
        Remove(tile);
        await SetSourcesAsync();
        Layout(grid);
        _dialog = new Dialog(grid.XamlRoot, title);
    }
    public void Hint() =>
        _board.SetHint();
}
