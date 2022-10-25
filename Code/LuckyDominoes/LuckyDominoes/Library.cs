using Comentsys.Toolkit.WindowsAppSdk;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;

public class Library
{
    private const string set_one = "one";
    private const string set_two = "two";
    private const string name_upper = "upper";
    private const string name_lower = "lower";
    private static readonly string[] _tiles =
    {
         "0,0",
         "0,1", "1,1",
         "0,2", "1,2", "2,2",
         "0,3", "1,3", "2,3", "3,3",
         "0,4", "1,4", "2,4", "3,4", "4,4",
         "0,5", "1,5", "2,5", "3,5", "4,5", "5,5",
         "0,6", "1,6", "2,6", "3,6", "4,6", "5,6", "6,6"
     };
    private readonly Random _random = new((int)DateTime.UtcNow.Ticks);

    private int _turns = 0;
    private List<int> _one = new();
    private List<int> _two = new();
    private StackPanel _panel = new();

    private List<int> Choose(int minimum, int maximum) =>
        Enumerable.Range(minimum, maximum)
            .OrderBy(r => _random.Next(minimum, maximum))
                .ToList();

    // Background, Get Portion & Set Portion
    private Brush Background() =>
        new LinearGradientBrush(new GradientStopCollection()
        {
            new GradientStop()
            {
                Color = Colors.DarkSlateGray,
                Offset = 0.0
            },
            new GradientStop()
            {
                Color = Colors.Black,
                Offset = 1.0
            }
        }, 90);

    private Dice GetPortion(string name) => new()
    {
        Name = name,
        Background = Background(),
        Foreground = new SolidColorBrush(Colors.WhiteSmoke)
    };

    private void SetPortion(string name, int value) =>
        ((Dice)_panel.FindName(name)).Value = value;

    // Set Domino, Get Domino & New
    private void SetDomino(string name, string tile)
    {
        string[] pair = tile.Split(',');
        SetPortion($"{name}.{name_upper}", int.Parse(pair[0]));
        SetPortion($"{name}.{name_lower}", int.Parse(pair[1]));
    }
    private StackPanel GetDomino(string name)
    {
        StackPanel domino = new()
        {
            Margin = new Thickness(25),
            Orientation = Orientation.Vertical
        };
        domino.Tapped += (object sender, TappedRoutedEventArgs e) =>
        {
            if (_turns > 0)
            {
                SetDomino(set_one, _tiles[_one[_turns]]);
                SetDomino(set_two, _tiles[_two[_turns]]);
                _turns--;
            }
            else
                New(_panel);
        };
        domino.Children.Add(GetPortion($"{name}.{name_upper}"));
        domino.Children.Add(GetPortion($"{name}.{name_lower}"));
        return domino;
    }
    public void New(StackPanel panel)
    {
        _panel = panel;
        _panel.Children.Clear();
        _panel.Children.Add(GetDomino(set_one));
        _panel.Children.Add(GetDomino(set_two));
        _turns = _tiles.Length - 1;
        _one = Choose(0, _tiles.Length);
        _two = Choose(0, _tiles.Length);
    }

}
