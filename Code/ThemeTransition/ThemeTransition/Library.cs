using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System.Collections.Generic;
using System.Linq;
using Windows.UI;
internal class Library
{
    private static readonly List<Color> _colours = new()
    {
        Colors.Black, Colors.Gray, Colors.Red, Colors.Orange, Colors.Yellow,
        Colors.Green, Colors.Cyan, Colors.Blue, Colors.Magenta, Colors.Purple
    };
    private static int _index = 0;

    public static void Add(StackPanel panel)
    {
        Rectangle previous = panel.Children.LastOrDefault() as Rectangle;
        if (previous == null || _index == _colours.Count)
        {
            _index = 0;
        }
        panel.Children.Add(new Rectangle()
        {
            Width = 50,
            Height = 50,
            Fill = new SolidColorBrush(_colours[_index])
        });
        _index++;
    }

    public static void Remove(StackPanel panel)
    {
        int count = panel.Children.Count;
        if (count > 0)
        {
            panel.Children.RemoveAt(count - 1);
        }
    }
}
