using Comentsys.Toolkit.WindowsAppSdk;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;

public class Library
{
    private static readonly Dictionary<int, Color> _style = new()
    {
         { 0, Colors.White },
         { 10, Colors.RoyalBlue },
         { 20, Colors.HotPink },
         { 30, Colors.MediumSpringGreen },
         { 40, Colors.Gold },
         { 50, Colors.Indigo }
    };
    private readonly Random _random = new((int)DateTime.UtcNow.Ticks);
    private List<int> Choose(int minimum, int maximum, int total) =>
    Enumerable.Range(minimum, maximum)
    .OrderBy(r => _random.Next(minimum, maximum))
    .Take(total).ToList();

    // Other Methods
    private void Add(StackPanel panel, int value)
    {
        Color style = _style.Where(w => value > w.Key)
            .Select(s => s.Value).LastOrDefault();
        var piece = new Piece()
        {
            Foreground = new SolidColorBrush(Colors.Black),
            Stroke = new SolidColorBrush(style),
            Value = value.ToString()
        };
        panel.Children.Add(piece);
    }

    public void New(StackPanel panel)
    {
        panel.Children.Clear();
        panel.CornerRadius = new CornerRadius(10);
        panel.Background = new SolidColorBrush(Colors.WhiteSmoke);
        var numbers = Choose(1, 59, 6);
        numbers.Sort();
        foreach (int number in numbers)
        {
            Add(panel, number);
        }
    }

}
