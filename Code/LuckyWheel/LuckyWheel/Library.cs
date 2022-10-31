using Comentsys.Toolkit.WindowsAppSdk;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.UI;

public class Library
{
    private const string title = "Lucky Wheel";
    private const int size = 400, hole = 60, radius = 200, circle = 360;
    private const int border = 4, marker = 30, duration = 5;
    private static readonly List<(string Value, Color Fill)> wedges = new()
    {
        ("1000", Colors.WhiteSmoke), ("600", Colors.LightGreen),
        ("500", Colors.Yellow), ("300", Colors.Red),
        ("500", Colors.Azure), ("800", Colors.Orange),
        ("550", Colors.Violet), ("400", Colors.Yellow),
        ("300", Colors.Pink), ("900", Colors.Red),
        ("500", Colors.Azure), ("300", Colors.LightGreen),
        ("900", Colors.Pink), ("LOSE", Colors.Black),
        ("600", Colors.Violet), ("400", Colors.Yellow),
        ("300", Colors.Azure), ("LOSE", Colors.Black),
        ("800", Colors.Red), ("350", Colors.Violet),
        ("450", Colors.Pink), ("700", Colors.LightGreen),
        ("300", Colors.Orange), ("600", Colors.Violet),
    };
    private static readonly double section = circle / wedges.Count;
    private readonly Random _random = new((int)DateTime.UtcNow.Ticks);

    // Variables, Get Ellipse, Add Circle, Get Sector & Add Section
    private int _total;
    private bool _over;
    private bool _spin;
    private double _position;
    private double _selected;
    private Canvas _canvas;
    private Dialog _dialog;
    private Storyboard _storyboard;

    private Ellipse GetEllipse(double diameter, Color fill) => new()
    {
        Width = diameter,
        Height = diameter,
        StrokeThickness = border,
        Fill = new SolidColorBrush(fill),
        Stroke = new SolidColorBrush(Colors.Gold)
    };
    private void AddCircle(Canvas canvas, double diameter)
    {
        var circle = GetEllipse(diameter, Colors.Green);
        Canvas.SetLeft(circle, (size - diameter) / 2);
        Canvas.SetTop(circle, (size - diameter) / 2);
        canvas.Children.Add(circle);
    }

    private Sector GetSector(double start, double finish, double radius, Color fill)
    {
        Sector sector = new()
        {
            Hole = hole,
            Start = start,
            Finish = finish,
            Radius = radius,
            Fill = new SolidColorBrush(fill)
        };
        Canvas.SetLeft(sector, (size - radius * 2) / 2);
        Canvas.SetTop(sector, (size - radius * 2) / 2);
        return sector;
    }

    private void AddSection(Canvas canvas, int index, double start)
    {
        var finish = section;
        var sector = GetSector(start, finish, radius, wedges[index].Fill);
        canvas.Children.Add(sector);
    }

    // Get Text & Add Text
    private TextBlock GetText(string value, Color foreground)
    {
        TextBlock text = new()
        {
            FontSize = 20,
            Margin = new Thickness(2),
            FontWeight = FontWeights.SemiBold,
            TextAlignment = TextAlignment.Center,
            Foreground = new SolidColorBrush(foreground)
        };
        for (int index = 0; index < value.Length; index++)
        {
            text.Inlines.Add(new Run()
            {
                Text = value[index] + Environment.NewLine
            });
        }
        text.Measure(new Size(
        double.PositiveInfinity,
        double.PositiveInfinity));
        return text;
    }
    private void AddText(Canvas canvas, int index, double start)
    {
        double top = 0;
        var (value, fill) = wedges[index];
        var foreground = fill == Colors.Black ? Colors.White : Colors.Black;
        var text = GetText(value, foreground);
        double middle = text.DesiredSize.Width / 2;
        double left = (size / 2) - middle;
        Grid grid = new()
        {
            Height = radius,
            RenderTransform = new RotateTransform()
            {
                Angle = start,
                CenterX = middle,
                CenterY = radius
            }
        };
        grid.Children.Add(text);
        Canvas.SetLeft(grid, left);
        Canvas.SetTop(grid, top);
        canvas.Children.Add(grid);
    }

    // Get Marker & Add Rotate
    private Polygon GetMarker() => new()
    {
        Width = marker,
        Height = marker / 2,
        Fill = new SolidColorBrush(Colors.Gold),
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Center,
        Points =
        {
            new Point(0, 0),
            new Point(marker, 0),
            new Point(marker / 2, marker / 2)
        }
    };

    private void AddRotate()
    {
        DoubleAnimation animation = new()
        {
            From = _position,
            To = circle * 2,
            EasingFunction = new QuadraticEase(),
            RepeatBehavior = new RepeatBehavior(1),
            Duration = new Duration(TimeSpan.FromSeconds(duration))
        };
        Storyboard.SetTargetProperty(animation,
        "(Canvas.RenderTransform).(RotateTransform.Angle)");
        Storyboard.SetTarget(animation, _canvas);
        _storyboard = new Storyboard();
        _storyboard.Completed += (object sender, object e) =>
        {
            _spin = false;
            var angle = circle - _selected - (section / 2);
            var index = (int)Math.Ceiling(angle / section);
            var (value, _) = wedges[index];
            if (int.TryParse(value, out int result) && !_over)
            {
                _total += result;
                _dialog.Show($"You Won {result}, Total is {_total}");
            }
            else
            {
                _over = true;
                _dialog.Show($"You Lose, Total was {_total}!");
            }
        };
        _storyboard.Children.Add(animation);
    }

    // Set Rotate, Play & Add Wheel
    private void SetRotate(double angle)
    {
        var animation = _storyboard.Children.First() as DoubleAnimation;
        animation.From = _position;
        animation.To = circle * 2 + angle;
        _storyboard.Begin();
    }
    private void Play()
    {
        if (!_spin)
        {
            _spin = true;
            if (_over)
            {
                _dialog.Show($"You Lost, Total was {_total}, Starting New Game");
                Reset();
            }
            else
            {
                _position = _selected;
                _selected = _random.Next(1, circle);
                SetRotate(_selected);
            }
        }
    }
    private void AddWheel(Canvas canvas, double diameter)
    {
        var wheel = GetEllipse(diameter, Colors.Transparent);
        wheel.Tapped += (object sender, TappedRoutedEventArgs e) =>
        Play();
        Canvas.SetLeft(wheel, (size - diameter) / 2);
        Canvas.SetTop(wheel, (size - diameter) / 2);
        canvas.Children.Add(wheel);
    }

    // Layout, Reset & New
    private void Layout(Grid grid)
    {
        grid.Children.Clear();
        StackPanel panel = new();
        _canvas = new Canvas()
        {
            Width = size,
            Height = size,
            RenderTransform = new RotateTransform()
            {
                Angle = 0,
                CenterX = radius,
                CenterY = radius
            }
        };
        var start = -(section / 2);
        for (int index = 0; index < wedges.Count; index++)
        {
            AddSection(_canvas, index, start);
            AddText(_canvas, index, start + (section / 2));
            start += section;
        }
        AddCircle(_canvas, hole * 2);
        AddWheel(_canvas, size + border);
        AddRotate();
        panel.Children.Add(GetMarker());
        panel.Children.Add(_canvas);
        grid.Children.Add(panel);
    }
    private void Reset()
    {
        _total = 0;
        _spin = false;
        _over = false;
        _selected = 0;
    }
    public void New(Grid grid)
    {
        Reset();
        Layout(grid);
        _dialog = new Dialog(grid.XamlRoot, title);
    }
}
