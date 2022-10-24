using Comentsys.Assets.FluentEmoji;
using Comentsys.Toolkit.WindowsAppSdk;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

public class Library
{
    private const string title = "Lucky Roshambo";
    private const int size = 3;
    private const int lost = 0;
    private const int win = 1;
    private const int draw = 2;
    private static readonly int[,] _match = new int[size, size]
    {
        { draw, lost, win },
        { win, draw, lost },
        { lost, win, draw }
    };
    private static readonly FluentEmojiType[] _assets = new FluentEmojiType[]
    {
        FluentEmojiType.Rock,
        FluentEmojiType.PageWithCurl,
        FluentEmojiType.Scissors
    };
    private static readonly string[] _values = new string[]
    {
        "You Lost!",
        "You Win!",
        "You Draw!"
    };
    private readonly Random _random = new((int)DateTime.UtcNow.Ticks);
    private Dialog _dialog;

    // Asset & Play
    private Viewbox Asset(int asset) => new()
    {
        Width = 100,
        Height = 100,
        Child = new Asset
        {
            AssetResource = FlatFluentEmoji.Get(_assets[asset])
        }
    };
    private void Play(int option)
    {
        int computer = _random.Next(0, size - 1);
        var result = _match[option, computer];
        var content = new StackPanel()
        {
            Orientation = Orientation.Vertical
        };
        content.Children.Add(new TextBlock()
        {
            HorizontalTextAlignment = TextAlignment.Center,
            Text = "Computer Picked"
        });
        content.Children.Add(Asset(computer));
        content.Children.Add(new TextBlock()
        {
            HorizontalTextAlignment = TextAlignment.Center,
            Text = _values[result]
        });
        _dialog.Show(content);
    }

    // Get & New
    private Button Get(int option)
    {
        Button button = new()
        {
            Width = 150,
            Height = 150,
            Tag = option,
            Content = Asset(option),
            Margin = new Thickness(5)
        };
        button.Click += (object sender, RoutedEventArgs e) =>
        Play((int)((Button)sender).Tag);
        return button;
    }
    public void New(StackPanel panel)
    {
        _dialog = new Dialog(panel.XamlRoot, title);
        panel.Children.Clear();
        for (int index = 0; index < size; index++)
        {
            panel.Children.Add(Get(index));
        }
    }

}
