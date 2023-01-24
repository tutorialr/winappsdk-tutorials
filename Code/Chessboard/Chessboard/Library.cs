using Comentsys.Assets.Games;
using Comentsys.Toolkit.Binding;
using Comentsys.Toolkit.WindowsAppSdk;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Chessboard;

public enum ChessBackground
{
    Light,
    Dark
}

// Chess Coordinate Class & Chess Class
public class ChessCoordinate
{
    private const int size = 8;
    private static readonly string[] ranks =
        { "8", "7", "6", "5", "4", "3", "2", "1" };
    private static readonly string[] files =
        { "A", "B", "C", "D", "E", "F", "G", "H" };

    public int Id { get; set; }
    public int Row { get; set; }
    public int Column { get; set; }
    public ChessBackground Background { get; set; }
    public string Notation { get; set; }

    public ChessCoordinate(int id)
    {
        Id = id;
        Row = Id / size;
        Column = Id % size;
        Background = (Row + Column) % 2 == 0 ?
            ChessBackground.Light : ChessBackground.Dark;
        Notation = $"{files[Column]}{ranks[Row]}";
    }
}

public class Chess : ObservableBase
{
    private ChessPieceSet _set;
    private ChessPieceType _type;

    public Chess(ChessPieceSet set, ChessPieceType type) =>
        (_set, _type) = (set, type);

    public ChessPieceSet Set
    {
        get => _set;
        set => SetProperty(ref _set, value);
    }

    public ChessPieceType Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }
}

// Chess Square Class

public class ChessSquare : ObservableBase
{
    private int _id;
    private Chess _piece;
    private ChessCoordinate _coordinate;
    private bool _isSelected;

    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public Chess Piece
    {
        get => _piece;
        set => SetProperty(ref _piece, value);
    }

    public ChessCoordinate Coordinate
    {
        get => _coordinate;
        set => SetProperty(ref _coordinate, value);
    }

    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }
}

// Chess Position Class

public class ChessPosition : List<Chess>
{
    private const int size = 8;

    public ChessPosition() : base(new Chess[size * size]) { }

    public ChessPosition(string position) : this()
    {
        int i = 0;
        var black = ChessPieceSet.Black;
        var white = ChessPieceSet.White;
        var pawn = ChessPieceType.Pawn;
        var knight = ChessPieceType.Knight;
        var bishop = ChessPieceType.Bishop;
        var rook = ChessPieceType.Rook;
        var queen = ChessPieceType.Queen;
        var king = ChessPieceType.King;
        foreach (char item in position)
        {
            switch (item)
            {
                case 'p': this[i++] = new Chess(black, pawn); break;
                case 'n': this[i++] = new Chess(black, knight); break;
                case 'b': this[i++] = new Chess(black, bishop); break;
                case 'r': this[i++] = new Chess(black, rook); break;
                case 'q': this[i++] = new Chess(black, queen); break;
                case 'k': this[i++] = new Chess(black, king); break;
                case 'P': this[i++] = new Chess(white, pawn); break;
                case 'N': this[i++] = new Chess(white, knight); break;
                case 'B': this[i++] = new Chess(white, bishop); break;
                case 'R': this[i++] = new Chess(white, rook); break;
                case 'Q': this[i++] = new Chess(white, queen); break;
                case 'K': this[i++] = new Chess(white, king); break;
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8': i += int.Parse(item.ToString()); break;
                case '/':
                    if (i % size != 0)
                        throw new ArgumentException("Invalid FEN"); break;
                default:
                    throw new ArgumentException($"Invalid FEN Character: '{item}'");
            }
        }
    }
}

// Chess Board Class & Chess Square Style Selector Class


public class ChessBoard
{
    public ChessSquare[] ChessSquares { get; set; } = new ChessSquare[64];

    public ChessBoard(string fen)
    {
        ChessPosition position = new(fen);
        for (int i = 0; i < position.Count; i++)
        {
            ChessSquares[i] = new ChessSquare
            {
                Id = i,
                Piece = position[i],
                Coordinate = new ChessCoordinate(i)
            };
        }
    }
}

public class ChessSquareStyleSelector : StyleSelector
{
    private const int size = 8;

    public Style Light { get; set; }
    public Style Dark { get; set; }

    protected override Style SelectStyleCore(
        object item, DependencyObject container) =>
        item is ChessSquare square
            ? (square.Id / size + square.Id % size) % 2 == 0 ? Light : Dark
            : base.SelectStyleCore(item, container);
}

// Chess Piece to Image Source Converter Class
public class ChessPieceToImageSourceConverter : IValueConverter
{
    private static readonly Dictionary<string, ImageSource> _sources = new();

    public static async Task SetSourcesAsync()
    {
        if (_sources.Count == 0)
            foreach (var set in Enum.GetValues<ChessPieceSet>())
                foreach (var type in Enum.GetValues<ChessPieceType>())
                    _sources.Add($"{set}{type}",
                    await ChessPiece.Get(set, type).AsImageSourceAsync());
    }

    public object Convert(object value, Type targetType,
        object parameter, string language) =>
        value is Chess piece ? _sources[$"{piece.Set}{piece.Type}"] : null;

    public object ConvertBack(object value, Type targetType,
        object parameter, string language) =>
        throw new NotImplementedException();
}

// Binder Class 
public class Binder
{
    public static readonly DependencyProperty GridColumnBindingPathProperty =
    DependencyProperty.RegisterAttached("GridColumnBindingPath",
    typeof(string), typeof(Binder),
    new PropertyMetadata(null, GridBindingPathPropertyChanged));

    public static readonly DependencyProperty GridRowBindingPathProperty =
    DependencyProperty.RegisterAttached("GridRowBindingPath",
    typeof(string), typeof(Binder),
    new PropertyMetadata(null, GridBindingPathPropertyChanged));

    public static string GetGridColumnBindingPath(DependencyObject obj) =>
        (string)obj.GetValue(GridColumnBindingPathProperty);

    public static void SetGridColumnBindingPath(
        DependencyObject obj, string value) =>
        obj.SetValue(GridColumnBindingPathProperty, value);

    public static string GetGridRowBindingPath(DependencyObject obj) =>
        (string)obj.GetValue(GridRowBindingPathProperty);

    public static void SetGridRowBindingPath(
        DependencyObject obj, string value) =>
        obj.SetValue(GridRowBindingPathProperty, value);

    private static void GridBindingPathPropertyChanged(
        DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
        if (e.NewValue is string path)
        {
            DependencyProperty property = null;
            if (e.Property == GridColumnBindingPathProperty)
                property = Grid.ColumnProperty;
            else if (e.Property == GridRowBindingPathProperty)
                property = Grid.RowProperty;

            BindingOperations.SetBinding(obj, property,
            new Binding { Path = new PropertyPath(path) });
        }
    }
}

public class Library
{
    // Constants, Variables, Property & Template Method
    private const int size = 8;
    private const string start = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";
    private ChessSquare _square;

    public ChessBoard Board { get; set; } = new ChessBoard(start);

    private static ItemsPanelTemplate Template()
    {
        StringBuilder rows = new();
        StringBuilder columns = new();
        for (int i = 0; i < size; i++)
        {
            rows.Append("<RowDefinition Height=\"*\"/>");
            columns.Append("<ColumnDefinition Width=\"*\"/>");
        }
        return (ItemsPanelTemplate)
        XamlReader.Load($@"<ItemsPanelTemplate 
        xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' 
        xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
        <Grid>
            <Grid.RowDefinitions>{rows}</Grid.RowDefinitions>
            <Grid.ColumnDefinitions>{columns}</Grid.ColumnDefinitions>
        </Grid>
    </ItemsPanelTemplate>");
    }

    // Tapped & New
    public void Tapped(ItemsControl display, ContentPresenter container)
    {
        ChessSquare square = (ChessSquare)display.ItemFromContainer(container);
        if (_square == null && square.Piece != null)
        {
            square.IsSelected = true;
            _square = square;
        }
        else if (square == _square)
        {
            square.IsSelected = false;
            _square = null;
        }
        else if (_square?.Piece != null && _square.Piece.Set != square?.Piece?.Set)
        {
            square.Piece = _square.Piece;
            _square.IsSelected = false;
            _square.Piece = null;
            _square = null;
        }
    }

    public async void New(ItemsControl display)
    {
        await ChessPieceToImageSourceConverter.SetSourcesAsync();
        display.ItemsSource = Board.ChessSquares;
        display.ItemsPanel = Template();
        Board = new ChessBoard(start);
    }
}