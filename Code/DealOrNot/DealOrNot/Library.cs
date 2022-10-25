using Comentsys.Toolkit.WindowsAppSdk;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Windows.UI;

public class Library
{
    private const string title = "Deal or Not";
    private const int rate = 5;
    private static readonly double[] amounts =
    {
        0.01, 0.10, 0.50, 1, 5, 10, 50, 100, 250, 500, 750,
        1000, 3000, 5000, 10000, 15000, 20000, 35000, 50000, 75000, 100000, 250000
    };
    private static readonly string[] colors =
    {
        "0026ff", "0039ff", "004dff", "0060ff", "0073ff", "0086ff",
        "0099ff", "0099ff", "0099ff", "00acff", "00bfff",
        "ff5900", "ff4d00", "ff4000", "ff3300", "ff2600", "ff2600",
        "ff2600", "ff2600", "ff1a00", "ff1c00", "ff0d00",
    };
    private static readonly string[] names = {
        "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k",
        "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v"
    };
    private readonly Random _random = new((int)DateTime.UtcNow.Ticks);
    private readonly List<double> _values = new();

    private int _turn;
    private bool _over;
    private bool _dealt;
    private double _amount;
    private Dialog _dialog;

    // Choose, Get Color, Get Background & Get Amount
    private List<int> Choose(int minimum, int maximum, int total) =>
        Enumerable.Range(minimum, maximum)
            .OrderBy(r => _random.Next(minimum, maximum))
                .Take(total).ToList();
    private Color GetColor(string hex)
    {
        byte r = byte.Parse(hex[0..^4], NumberStyles.HexNumber);
        byte g = byte.Parse(hex[2..^2], NumberStyles.HexNumber);
        byte b = byte.Parse(hex[4..^0], NumberStyles.HexNumber);
        return Color.FromArgb(255, r, g, b);
    }
    private Color GetBackground(double amount)
    {
        var position = Array.FindIndex(amounts, a => a.Equals(amount));
        return GetColor(colors[position]);
    }
    private Grid GetAmount(double value, Color background)
    {
        Grid grid = new()
        {
            Background = new SolidColorBrush(background)
        };
        TextBlock text = new()
        {
            Text = string.Format(new CultureInfo("en-GB"), "{0:c}", value),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = new SolidColorBrush(Colors.White),
            Margin = new Thickness(10),
            FontSize = 33
        };
        grid.Children.Add(text);
        return grid;
    }

    // Get Offer & Select Box
    private double GetOffer()
    {
        int count = 0;
        double total = 0.0;
        foreach (double value in _values)
        {
            total += value;
            count++;
        }
        double average = total / count;
        double offer = average * _turn / 10;
        return Math.Round(offer, 0);
    }
    private async void SelectBox(Button button, string name)
    {
        if (!_over)
        {
            if (_turn < names.Length)
            {
                button.Opacity = 0;
                _amount = _values[Array.IndexOf(names, name)];
                bool response = await _dialog.ConfirmAsync(
                GetAmount(_amount, GetBackground(_amount)));
                if (response)
                {
                    if (!_dealt && _turn % rate == 0 && _turn > 1)
                    {
                        double offer = GetOffer();
                        bool accept = await _dialog.ConfirmAsync(
                        GetAmount(offer, Colors.Black), "Deal", "Not");
                        if (accept)
                        {
                            _amount = offer;
                            _dealt = true;
                        }
                    }
                    _turn++;
                }
            }
            if (_turn == names.Length || _dealt)
                _over = true;
        }
        if (_over)
        {
            object content = _dealt ?
            GetAmount(_amount, Colors.Black) :
            GetAmount(_amount, GetBackground(_amount));
            await _dialog.ConfirmAsync(content, "Game Over", null);
        }
    }

    // Add Box
    private void AddBox(StackPanel panel, string name, int value)
    {
        Button button = new()
        {
            Name = $"box.{name}",
            Margin = new Thickness(5)
        };
        button.Click += (object sender, RoutedEventArgs e) =>
        SelectBox((Button)sender, name);
        StackPanel box = new()
        {
            Width = 100
        };
        Rectangle lid = new()
        {
            Height = 10,
            Fill = new SolidColorBrush(Colors.DarkRed)
        };
        Grid front = new()
        {
            Height = 75,
            Background = new SolidColorBrush(Colors.Red)
        };
        Grid label = new()
        {
            Width = 50,
            Background = new SolidColorBrush(Colors.White),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        TextBlock text = new()
        {
            TextAlignment = TextAlignment.Center,
            FontWeight = FontWeights.Bold,
            Foreground = new SolidColorBrush(Colors.Black),
            FontSize = 32,
            Text = value.ToString()
        };
        label.Children.Add(text);
        front.Children.Add(label);
        box.Children.Add(lid);
        box.Children.Add(front);
        button.Content = box;
        panel.Children.Add(button);
    }

    // Add Row, Layout & New
    private StackPanel AddRow()
    {
        int count = 0;
        StackPanel panel = new();
        int[] rows = { 5, 6, 6, 5 };
        for (int r = 0; r < 4; r++)
        {
            StackPanel places = new()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            for (int column = 0; column < rows[r]; column++)
            {
                AddBox(places, names[count], count + 1);
                count++;
            }
            panel.Children.Add(places);
        }
        return panel;
    }
    private void Layout(Grid grid)
    {
        grid.Children.Clear();
        Viewbox view = new()
        {
            Child = AddRow()
        };
        grid.Children.Add(view);
    }
    public void New(Grid grid)
    {
        _turn = 0;
        _amount = 0;
        _over = false;
        _dealt = false;
        _dialog = new Dialog(grid.XamlRoot, title);
        var positions = Choose(0, names.Length, names.Length);
        foreach (var position in positions)
        {
            _values.Add(amounts[position]);
        }
        Layout(grid);
    }

}
