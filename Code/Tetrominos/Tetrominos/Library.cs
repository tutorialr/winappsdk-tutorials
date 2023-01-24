// Using Statements
using Comentsys.Toolkit.WindowsAppSdk;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tetrominos;

// Position Class
public class Position
{
    public int Row { get; set; }

    public int Column { get; set; }

    public Position(int row, int column) =>
        (Row, Column) = (row, column);
}

// Block Class
public abstract class Block
{
    private readonly Position[][] _tiles;
    private readonly Position _start;
    private readonly Position _offset;
    private int _rotate;

    public int Id { get; }

    public Block(int id, int row, int column, Position[][] tiles)
    {
        Id = id;
        _start = new Position(row, column);
        _offset = new Position(_start.Row, _start.Column);
        _tiles = tiles;
    }

    public IEnumerable<Position> Positions =>
        _tiles[_rotate].Select(position =>
        new Position(
            position.Row + _offset.Row,
            position.Column + _offset.Column));

    public void RotateClockwise() =>
        _rotate = (_rotate + 1) % _tiles.Length;

    public void RotateAntiClockwise() =>
        _rotate = _rotate == 0 ?
            _rotate = _tiles.Length - 1 : _rotate--;

    public void Move(int rows, int columns)
    {
        _offset.Row += rows;
        _offset.Column += columns;
    }

    public void Reset()
    {
        _rotate = 0;
        _offset.Row = _start.Row;
        _offset.Column = _start.Column;
    }

    public IEnumerable<Position> Preview =>
        _tiles[0];
}

// IBlock, JBlock & LBlock Class
public class IBlock : Block
{
    public IBlock() : base(1, -1, 3, new Position[][]
    {
        new Position[] { new(1,0), new(1,1), new(1,2), new(1,3) },
        new Position[] { new(0,2), new(1,2), new(2,2), new(3,2) },
        new Position[] { new(2,0), new(2,1), new(2,2), new(2,3) },
        new Position[] { new(0,1), new(1,1), new(2,1), new(3,1) }
    })
    { }
}

public class JBlock : Block
{
    public JBlock() : base(2, 0, 3, new Position[][]
    {
        new Position[] { new(0, 0), new(1, 0), new(1, 1), new(1, 2) },
        new Position[] { new(0, 1), new(0, 2), new(1, 1), new(2, 1) },
        new Position[] { new(1, 0), new(1, 1), new(1, 2), new(2, 2) },
        new Position[] { new(0, 1), new(1, 1), new(2, 1), new(2, 0) }
    })
    { }
}

public class LBlock : Block
{
    public LBlock() : base(3, 0, 3, new Position[][]
    {
        new Position[] { new(0,2), new(1,0), new(1,1), new(1,2) },
        new Position[] { new(0,1), new(1,1), new(2,1), new(2,2) },
        new Position[] { new(1,0), new(1,1), new(1,2), new(2,0) },
        new Position[] { new(0,0), new(0,1), new(1,1), new(2,1) }
    })
    { }
}

// OBlock, SBlock, TBlock & ZBlock Class

public class OBlock : Block
{
    public OBlock() : base(4, 0, 4, new Position[][]
    {
        new Position[] { new(0,0), new(0,1), new(1,0), new(1,1) }
    })
    { }
}

public class SBlock : Block
{
    public SBlock() : base(5, 0, 3, new Position[][]
    {
        new Position[] { new(0,1), new(0,2), new(1,0), new(1,1) },
        new Position[] { new(0,1), new(1,1), new(1,2), new(2,2) },
        new Position[] { new(1,1), new(1,2), new(2,0), new(2,1) },
        new Position[] { new(0,0), new(1,0), new(1,1), new(2,1) }
    })
    { }
}

public class TBlock : Block
{
    public TBlock() : base(6, 0, 3, new Position[][]
    {
        new Position[] { new(0,1), new(1,0), new(1,1), new(1,2) },
        new Position[] { new(0,1), new(1,1), new(1,2), new(2,1) },
        new Position[] { new(1,0), new(1,1), new(1,2), new(2,1) },
        new Position[] { new(0,1), new(1,0), new(1,1), new(2,1) }
    })
    { }
}

public class ZBlock : Block
{
    public ZBlock() : base(7, 0, 3, new Position[][]
    {
        new Position[] { new(0,0), new(0,1), new(1,1), new(1,2) },
        new Position[] { new(0,2), new(1,1), new(1,2), new(2,1) },
        new Position[] { new(1,0), new(1,1), new(2,1), new(2,2) },
        new Position[] { new(0,1), new(1,0), new(1,1), new(2,0) }
    })
    { }
}

// Queue Class
public class Queue
{
    private readonly Block[] _blocks = new Block[]
    {
        new IBlock(),
        new JBlock(),
        new LBlock(),
        new OBlock(),
        new SBlock(),
        new TBlock(),
        new ZBlock()
    };

    private readonly Random _random = new((int)DateTime.UtcNow.Ticks);

    private Block Choose() =>
        _blocks[_random.Next(0, _blocks.Length)];

    public Block Next { get; private set; }

    public Queue() =>
        Next = Choose();

    public Block Get()
    {
        var block = Next;
        do
        {
            Next = Choose();
        } while (block.Id == Next.Id);
        return block;
    }
}

public class Board
{
    // Board Member, Properties & Constructor
    private readonly int[,] _board;

    public int Rows { get; }

    public int Columns { get; }

    public int this[int row, int column]
    {
        get => _board[row, column];
        set => _board[row, column] = value;
    }

    public Board(int rows, int columns) =>
        (Rows, Columns, _board) = (rows, columns, new int[rows, columns]);

    // Board ClearRow, MoveRowDown, IsInside & IsOutside Methods

    private void ClearRow(int row)
    {
        for (int column = 0; column < Columns; column++)
        {
            _board[row, column] = 0;
        }
    }

    private void MoveRowDown(int row, int rows)
    {
        for (int column = 0; column < Columns; column++)
        {
            _board[row + rows, column] = _board[row, column];
            _board[row, column] = 0;
        }
    }

    public bool IsInside(int row, int column) =>
        row >= 0 && row < Rows && column >= 0 && column < Columns;

    public bool IsEmpty(int row, int column) =>
        IsInside(row, column) && _board[row, column] == 0;

    // Board IsRowFull, IsRowEmpty & ClearFullRow Methods
    public bool IsRowFull(int row)
    {
        for (int column = 0; column < Columns; column++)
        {
            if (_board[row, column] == 0)
                return false;
        }
        return true;
    }

    public bool IsRowEmpty(int row)
    {
        for (int column = 0; column < Columns; column++)
        {
            if (_board[row, column] != 0)
                return false;
        }
        return true;
    }

    public int ClearFullRows()
    {
        var cleared = 0;
        for (int row = Rows - 1; row >= 0; row--)
        {
            if (IsRowFull(row))
            {
                ClearRow(row);
                cleared++;
            }
            else if (cleared > 0)
                MoveRowDown(row, cleared);
        }
        return cleared;
    }

}

public class State
{
    // State Members & Properties
    private readonly Board _board;
    private Block _current;

    public Board Board => _board;

    public Queue Queue { get; }

    public bool Over { get; private set; }

    public int Score { get; private set; }

    public Block Held { get; private set; }

    public bool CanHold { get; private set; }

    public Block Current
    {
        get => _current;
        private set => Update(value);
    }

    // State Private Methods
    private bool Fits()
    {
        foreach (var position in _current.Positions)
            if (!_board.IsEmpty(position.Row, position.Column))
                return false;
        return true;
    }

    private void Update(Block value)
    {
        _current = value;
        _current.Reset();
        for (int i = 0; i < 2; i++)
        {
            _current.Move(1, 0);
            if (!Fits())
                _current.Move(-1, 0);
        }
    }

    private bool IsOver() =>
        !(_board.IsRowEmpty(0) && _board.IsRowEmpty(1));

    private void Place()
    {
        foreach (var position in Current.Positions)
            _board[position.Row, position.Column] = Current.Id;
        Score += _board.ClearFullRows();
        if (IsOver())
            Over = true;
        else
        {
            Current = Queue.Get();
            CanHold = true;
        }
    }

    private int GetDistance(Position position)
    {
        var drop = 0;
        while (_board.IsEmpty(position.Row + drop + 1, position.Column))
            drop++;
        return drop;
    }

    // State Constructor and Hold, RotateClockwise & RotateAntiClockwise Methods
    public State()
    {
        _board = new Board(22, 10);
        Queue = new Queue();
        Current = Queue.Get();
        CanHold = true;
    }

    public void Hold()
    {
        if (!CanHold)
            return;
        if (Held == null)
        {
            Held = Current;
            Current = Queue.Get();
        }
        else
            (Held, Current) = (Current, Held);
        CanHold = false;
    }

    public void RotateClockwise()
    {
        Current.RotateClockwise();
        if (!Fits())
            Current.RotateAntiClockwise();
    }

    public void RotateAntiClockwise()
    {
        Current.RotateAntiClockwise();
        if (!Fits())
            Current.RotateClockwise();
    }

    // State Left, Right, Down, Distance & Drop Methods
    public void Left()
    {
        Current.Move(0, -1);
        if (!Fits())
            Current.Move(0, 1);
    }

    public void Right()
    {
        Current.Move(0, 1);
        if (!Fits())
            Current.Move(0, -1);
    }

    public void Down()
    {
        Current.Move(1, 0);
        if (!Fits())
        {
            Current.Move(-1, 0);
            Place();
        }
    }

    public int Distance()
    {
        var drop = _board.Rows;
        foreach (var position in Current.Positions)
        {
            drop = Math.Min(drop, GetDistance(position));
        }
        return drop;
    }

    public void Drop()
    {
        Current.Move(Distance(), 0);
        Place();
    }

}

public class Library
{
    // Constants, Variables & Enum
    private const int max_delay = 1000;
    private const int min_delay = 75;
    private const int increase = 25;
    private const int preview = 4;
    private const int size = 36;
    private readonly Brush[] _brushes = new Brush[]
    {
    new SolidColorBrush(Colors.White),
    new SolidColorBrush(Colors.Cyan),
    new SolidColorBrush(Colors.Blue),
    new SolidColorBrush(Colors.Orange),
    new SolidColorBrush(Colors.Gold),
    new SolidColorBrush(Colors.Green),
    new SolidColorBrush(Colors.Purple),
    new SolidColorBrush(Colors.Red)
    };
    private Grid _grid;
    private Grid _next;
    private Grid _hold;
    private TextBlock _text;
    private Piece[,] _pieces;
    private State _state;

    public enum Moves
    {
        Left,
        Right,
        Down,
        RotateClockwise,
        RotateAntiClockwise,
        Hold,
        Drop
    }

    // SetPieces & Board Methods
    private static Piece[,] SetPieces(Grid grid, int rows, int columns)
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
                var piece = new Piece
                {
                    Width = size,
                    Height = size,
                    IsSquare = true,
                    Stroke = new SolidColorBrush(Colors.Black)
                };
                Grid.SetColumn(piece, column);
                Grid.SetRow(piece, row);
                grid.Children.Add(piece);
                pieces[row, column] = piece;
            }
        }
        return pieces;
    }

    private void Board(Board board)
    {
        for (int row = 0; row < board.Rows; row++)
        {
            for (int column = 0; column < board.Columns; column++)
            {
                var id = board[row, column];
                var counter = _pieces[row, column];
                counter.Opacity = 1;
                counter.Fill = _brushes[id];
            }
        }
    }

    // Block, Preview, Next & Held Methods
    private void Block(Block block)
    {
        foreach (var position in block.Positions)
        {
            var counter = _pieces[position.Row, position.Column];
            counter.Opacity = 1;
            counter.Fill = _brushes[block.Id];
        }
    }

    private void Preview(Grid grid, Block block = null)
    {
        foreach (var piece in grid.Children.Cast<Piece>())
        {
            piece.Fill = _brushes[0];
        }
        if (block != null)
        {
            foreach (var position in block.Preview)
            {
                var piece = grid.Children.Cast<Piece>()
                    .First(f => Grid.GetRow(f) == position.Row
                        && Grid.GetColumn(f) == position.Column);
                piece.Fill = _brushes[block.Id];
            }
        }
    }

    private void Next(Queue queue) =>
        Preview(_next, queue.Next);

    private void Held(Block block)
    {
        if (block == null)
            Preview(_hold);
        else
            Preview(_hold, block);
    }

    // Ghost, Score, Over & Draw Methods
    private void Ghost(Block block)
    {
        int distance = _state.Distance();
        foreach (var position in block.Positions)
        {
            var counter = _pieces[position.Row + distance, position.Column];
            counter.Opacity = 0.25;
            counter.Fill = _brushes[block.Id];
        }
    }

    private void Score(int score) =>
        _text.Text = $"Score {score}";

    private void Over(int score) =>
        _text.Text = $"Game Over! Final Score {score}";

    private void Draw(State state)
    {
        Board(state.Board);
        Ghost(state.Current);
        Block(state.Current);
        Next(state.Queue);
        Held(state.Held);
        Score(state.Score);
    }

    // Loop & Move Methods
    private async Task Loop()
    {
        Draw(_state);
        while (!_state.Over)
        {
            var delay = Math.Max(min_delay, max_delay - (_state.Score * increase));
            await Task.Delay(delay);
            _state.Down();
            Draw(_state);
        }
        Over(_state.Score);
    }

    public void Move(string value)
    {
        var move = Enum.Parse(typeof(Moves), value);
        if (_state.Over)
            return;
        switch (move)
        {
            case Moves.Left:
                _state.Left();
                break;
            case Moves.Right:
                _state.Right();
                break;
            case Moves.Down:
                _state.Down();
                break;
            case Moves.RotateClockwise:
                _state.RotateClockwise();
                break;
            case Moves.RotateAntiClockwise:
                _state.RotateAntiClockwise();
                break;
            case Moves.Hold:
                _state.Hold();
                break;
            case Moves.Drop:
                _state.Drop();
                break;
            default:
                return;
        }
        Draw(_state);
    }

    // Layout & New Methods
    private void Layout(Grid grid)
    {
        grid.Children.Clear();
        grid.RowDefinitions.Clear();
        grid.ColumnDefinitions.Clear();
        grid.ColumnDefinitions.Add(new ColumnDefinition()
        {
            Width = GridLength.Auto
        });
        grid.ColumnDefinitions.Add(new ColumnDefinition()
        {
            Width = new GridLength(1, GridUnitType.Star)
        });
        StackPanel panel = new()
        {
            Orientation = Orientation.Vertical,
        };
        _text = new TextBlock();
        panel.Children.Add(_text);
        panel.Children.Add(new TextBlock() { Text = "Next" });
        _next = new Grid();
        SetPieces(_next, preview, preview);
        panel.Children.Add(_next);
        panel.Children.Add(new TextBlock() { Text = "Hold" });
        _hold = new Grid();
        SetPieces(_hold, preview, preview);
        panel.Children.Add(_hold);
        grid.Children.Add(panel);
        _grid = new Grid();
        grid.Children.Add(_grid);
        _pieces = SetPieces(_grid, _state.Board.Rows, _state.Board.Columns);
        Grid.SetColumn(_grid, 1);
    }

    public async void New(Grid grid)
    {
        _grid = grid;
        _state = new State();
        Layout(grid);
        await Loop();
    }

}