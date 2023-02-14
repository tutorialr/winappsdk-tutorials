using Comentsys.Toolkit.WindowsAppSdk;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Radios;
using Windows.UI;

namespace DonutControl;

public class Donut : Grid
{
    private const double total = 100;
    private const double circle = 360;

    private List<double> _items = new();

    // Donut GetSector & Percentages Method
    private static Sector GetSector(double size, double start,
    double finish, double radius, double hole, Color fill)
    {
        Sector sector = new()
        {
            Hole = hole,
            Start = start,
            Finish = finish,
            Radius = radius,
            Fill = new SolidColorBrush(fill),
            Stroke = new SolidColorBrush(Colors.WhiteSmoke)
        };
        Canvas.SetLeft(sector, (size - radius * 2) / 2);
        Canvas.SetTop(sector, (size - radius * 2) / 2);
        return sector;
    }

    private List<double> Percentages()
    {
        List<double> results = new();
        double total = _items.Sum();
        foreach (double item in _items)
        {
            results.Add(item / total * 100);
        }
        return results.OrderBy(o => o).ToList();
    }

    // Donut Layout Method
    internal void Layout()
    {
        double finish = 0;
        double value = circle / total;
        List<double> percentages = Percentages();
        Canvas canvas = new()
        {
            Width = Radius * 2,
            Height = Radius * 2
        };
        Children.Clear();
        for (int index = 0; index < percentages.Count; index++)
        {

            double start = finish;
            double percentage = percentages[index];
            Color colour = (index < Palette.Count) ? Palette[index] : Colors.Black;
            double sweep = value * percentage;
            finish = sweep + start;
            if (finish >= 360)
                finish = sweep;
            Sector sector = GetSector(Radius * 2, start, finish, Radius, Hole, colour);
            canvas.Children.Add(sector);
        }
        Viewbox viewbox = new()
        {
            Child = canvas
        };
        Children.Add(viewbox);
    }

    // Donut Properties
    public List<Color> Palette { get; set; } = new();

    public List<double> Items
    {
        get { return _items; }
        set { _items = value; Layout(); }
    }

    public static readonly DependencyProperty RadiusProperty =
    DependencyProperty.Register("Radius", typeof(int),
    typeof(Donut), new PropertyMetadata(100, new PropertyChangedCallback(
    (DependencyObject obj, DependencyPropertyChangedEventArgs eventArgs) =>
    {
        ((Donut)obj).Layout();
    })));

    public static readonly DependencyProperty HoleProperty =
    DependencyProperty.Register("Hole", typeof(UIElement),
    typeof(Donut), new PropertyMetadata(50.0, new PropertyChangedCallback(
    (DependencyObject obj, DependencyPropertyChangedEventArgs eventArgs) =>
    {
        ((Donut)obj).Layout();
    })));

    public int Radius
    {
        get { return (int)GetValue(RadiusProperty); }
        set { SetValue(RadiusProperty, value); Layout(); }
    }

    public double Hole
    {
        get { return (double)GetValue(HoleProperty); }
        set { SetValue(HoleProperty, value); Layout(); }
    }
}

public class Library
{
    private readonly List<Color> _colours = new()
    {
        Colors.Black,
        Colors.Gray,
        Colors.Red,
        Colors.Orange,
        Colors.Yellow,
        Colors.Green,
        Colors.Cyan,
        Colors.Blue,
        Colors.Magenta,
        Colors.Purple
    };

    // Library Methods 
    private int Fibonacci(int value) => value > 1 ?
    Fibonacci(value - 1) + Fibonacci(value - 2) : value;

    public void Load(Grid grid)
    {
        grid.Children.Clear();
        Donut donut = new()
        {
            Palette = _colours
        };
        donut.Items = Enumerable.Range(1, donut.Palette.Count)
        .Select(Fibonacci).Select(s => (double)s).ToList();
        grid.Children.Add(donut);
    }
}