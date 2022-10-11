using Comentsys.Toolkit.WindowsAppSdk;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.UI;
public class Library
{
    private readonly Random _random = new((int)DateTime.UtcNow.Ticks);
    public Dice Get(Color foreground, Color background)
    {
        Dice dice = new()
        {
            Margin = new Thickness(25),
            CornerRadius = new CornerRadius(10),
            Foreground = new SolidColorBrush(foreground),
            Background = new SolidColorBrush(background)
        };
        dice.Tapped += (object sender, TappedRoutedEventArgs e) =>
        ((Dice)sender).Value = _random.Next(1, 7);
        return dice;
    }
    public void New(StackPanel panel)
    {
        panel.Children.Clear();
        panel.Children.Add(Get(Colors.Red, Colors.WhiteSmoke));
        panel.Children.Add(Get(Colors.Blue, Colors.WhiteSmoke));
    }
}