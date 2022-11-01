using Comentsys.Toolkit.WindowsAppSdk;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using Windows.Foundation;
using Windows.UI;

public class Library
{
    private const string title = "Lucky Darts";
    private const int radius = 200;
    private const int circle = 360;
    private const int triple = 250;
    private const int offset = 40;
    private const int chance = 5;
    private const int size = 500;
    private const int dart = 10;
    private const int bull = 20;
    private const int ring = 10;
    private const int font = 25;
    private const int line = 2;
    private static readonly int[] numbers =
    {
        20, 1, 18, 4, 13, 6, 10, 15, 2, 17,
        3, 19, 7, 16, 8, 11, 14, 9, 12, 5
    };
    private static readonly double section =
        circle / numbers.Length;
    private readonly Random _random = new((int)DateTime.UtcNow.Ticks);

    private int _score = 0;
    private Dialog _dialog;
    private Canvas _canvas;
    private Piece _dart;

    // Is Odd, Get Ellipse, Add Circle & Get Sector
    private bool IsOdd(int value) =>
        value % 2 != 0;
    private Ellipse GetEllipse(double diameter, Color fill) => new()
    {
        Width = diameter,
        Height = diameter,
        StrokeThickness = line,
        Fill = new SolidColorBrush(fill),
        Stroke = new SolidColorBrush(Colors.WhiteSmoke)
    };
    private void AddCircle(Canvas canvas, double diameter, Color fill)
    {
        var circle = GetEllipse(diameter, fill);
        Canvas.SetLeft(circle, (size - diameter) / 2);
        Canvas.SetTop(circle, (size - diameter) / 2);
        canvas.Children.Add(circle);
    }
    private Sector GetSector(double start, double finish,
        double radius, double hole, Color fill)
    {
        Sector sector = new()
        {
            Hole = hole,
            Start = start,
            Finish = finish,
            Radius = radius,
            StrokeThickness = line,
            Fill = new SolidColorBrush(fill),
            Stroke = new SolidColorBrush(Colors.WhiteSmoke)
        };
        Canvas.SetLeft(sector, (size - radius * 2) / 2);
        Canvas.SetTop(sector, (size - radius * 2) / 2);
        return sector;
    }

    // Add Section, Get Text & Add Text
    private void AddSection(Canvas canvas, int index, double start)
    {
        var finish = section;
        var sector = GetSector(start, finish, radius, bull,
        IsOdd(index) ? Colors.Black : Colors.MintCream);
        var doubleRing = GetSector(start, finish, radius, radius - ring,
        IsOdd(index) ? Colors.MediumSeaGreen : Colors.OrangeRed);
        var tripleRing = GetSector(start, finish, triple / 2, triple / 2 - ring,
        IsOdd(index) ? Colors.MediumSeaGreen : Colors.OrangeRed);
        canvas.Children.Add(sector);
        canvas.Children.Add(doubleRing);
        canvas.Children.Add(tripleRing);
    }
    private TextBlock GetText(string value)
    {
        var text = new TextBlock()
        {
            Foreground = new SolidColorBrush(Colors.WhiteSmoke),
            TextAlignment = TextAlignment.Center,
            FontSize = font,
            Text = value
        };
        text.Measure(new Size(
        double.PositiveInfinity,
        double.PositiveInfinity));
        return text;
    }
    private void AddText(Canvas canvas, int index, double start)
    {
        var text = GetText($"{numbers[index]}");
        double angle = start * Math.PI / (circle / 2);
        double width = canvas.ActualWidth / 2;
        double height = canvas.ActualHeight / 2;
        double left = width + (width - font) * Math.Cos(angle)
        - text.DesiredSize.Width / 2;
        double top = height + (height - font) * Math.Sin(angle)
        - text.DesiredSize.Height / 2;
        Canvas.SetLeft(text, left);
        Canvas.SetTop(text, top);
        canvas.Children.Add(text);
    }

    // Add Dart & Get Number
    private void AddDart(Point point)
    {
        _canvas.Children.Remove(_dart);
        _dart = new Piece()
        {
            Width = dart,
            Height = dart,
            Name = nameof(_dart),
            Fill = new SolidColorBrush(Colors.SlateGray),
            Stroke = new SolidColorBrush(Colors.DarkGray)
        };
        Canvas.SetLeft(_dart, point.X - dart / 2);
        Canvas.SetTop(_dart, point.Y - dart / 2);
        _canvas.Children.Add(_dart);
    }
    private int GetNumber(double degrees) =>
    degrees switch
    {
        >= 351 => 6,
        >= 333 => 10,
        >= 315 => 15,
        >= 297 => 2,
        >= 279 => 17,
        >= 261 => 3,
        >= 243 => 19,
        >= 225 => 7,
        >= 207 => 16,
        >= 189 => 8,
        >= 171 => 11,
        >= 153 => 14,
        >= 135 => 9,
        >= 117 => 12,
        >= 99 => 5,
        >= 81 => 20,
        >= 63 => 1,
        >= 45 => 18,
        >= 27 => 4,
        >= 9 => 13,
        _ => 6
    };

    // Get Score, Play & Add Board
    private int GetScore(Point point)
    {
        double x = point.X - size / 2;
        double y = size / 2 - point.Y;
        double radians = Math.Atan2(y, x);
        int degrees = (int)(radians * (circle / 2) / Math.PI);
        degrees = degrees < 0 ? circle + degrees : degrees;
        int number = GetNumber(degrees);
        var length = (int)Math.Floor(Math.Sqrt(x * x + y * y));
        return length switch
        {
            >= radius => 0,
            >= radius - ring and <= radius => number * 2,
            >= triple / 2 - ring and <= triple / 2 => number * 3,
            >= bull - ring and <= bull => 25,
            <= bull / 2 => 50,
            _ => number
        };
    }
    private void Play(Point point)
    {
        var x = _random.Next((int)point.X - offset, (int)point.X + offset);
        var y = _random.Next((int)point.Y - offset, (int)point.Y + offset);
        var hit = IsOdd(_random.Next(0, chance)) ? point : new Point(x, y);
        var score = GetScore(hit);
        _score += score;
        AddDart(hit);
        _dialog.Show($"Scored {score}, Total {_score}");
    }
    private void AddBoard(Canvas canvas, double diameter)
    {
        var board = GetEllipse(diameter, Colors.Transparent);
        board.Tapped += (object sender, TappedRoutedEventArgs e) =>
        Play(e.GetPosition(canvas));
        Canvas.SetLeft(board, (size - diameter) / 2);
        Canvas.SetTop(board, (size - diameter) / 2);
        canvas.Children.Add(board);
    }

    // Layout & New
    private void Layout(Grid grid)
    {
        grid.Children.Clear();
        _canvas = new Canvas()
        {
            Width = size,
            Height = size
        };
        var start = -(section / 2);
        AddCircle(_canvas, size, Colors.Black);
        AddCircle(_canvas, (radius * 2) + (line * 2), Colors.WhiteSmoke);
        for (int index = 0; index < numbers.Length; index++)
        {
            AddSection(_canvas, index, start);
            AddText(_canvas, index, start + (section / 2) - (circle / 4));
            start += section;
        }
        AddCircle(_canvas, bull * 2, Colors.MediumSeaGreen);
        AddCircle(_canvas, bull, Colors.OrangeRed);
        AddBoard(_canvas, size);
        grid.Children.Add(_canvas);
    }
    public void New(Grid grid)
    {
        _score = 0;
        _dialog = new Dialog(grid.XamlRoot, title);
        Layout(grid);
    }
}