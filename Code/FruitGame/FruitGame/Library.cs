using Comentsys.Assets.FluentEmoji;
using Comentsys.Toolkit.WindowsAppSdk;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class Library
{
    private const string title = "Fruit Game";
    private const int delay_duration = 250;
    private const int size = 3;
    private readonly Dictionary<int, FluentEmojiType> _options = new()
    {
        { 0, FluentEmojiType.SlotMachine },
        { 1, FluentEmojiType.GreenApple },
        { 2, FluentEmojiType.Grapes },
        { 3, FluentEmojiType.Lemon },
        { 4, FluentEmojiType.Cherries },
        { 5, FluentEmojiType.Banana },
        { 6, FluentEmojiType.Melon },
        { 7, FluentEmojiType.Tangerine },
        { 8, FluentEmojiType.Bell }
    };

    private readonly Random _random = new((int)DateTime.UtcNow.Ticks);

    private int _spins;
    private Dialog _dialog;
    private StackPanel _panel = new();

    // Choose, Option & Set
    private List<int> Choose(int minimum, int maximum, int total)
    {
        var choose = new List<int>();
        var values = Enumerable.Range(minimum, maximum).ToList();
        for (int index = 0; index < total; index++)
        {
            var value = _random.Next(0, values.Count);
            choose.Add(values[value]);
        }
        return choose;
    }

    private Viewbox Option(int index, int option) => new()
    {
        Child = new Asset
        {
            Name = $"{index}",
            AssetResource = FlatFluentEmoji.Get(_options[option])
        }
    };

    private void Set(int index, int option) =>
        (_panel.FindName($"{index}") as Asset)
            .AssetResource = FlatFluentEmoji.Get(_options[option]);

    // Play
    private async void Play()
    {
        var values = Choose(1, _options.Count - 1, size);
        for (int index = 0; index < size; index++)
        {
            for (int option = 1; option <= values[index]; option++)
            {
                Set(index, option);
                await Task.Delay(delay_duration);
            }
        }
        _spins++;
        if (values.All(a => a.Equals(values.First())))
        {
            var content = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };
            content.Children.Add(new TextBlock()
            {
                HorizontalTextAlignment = TextAlignment.Center,
                Text = $"Spin {_spins} matched"
            });
            var fruit = new StackPanel()
            {
                Height = 100,
                Orientation = Orientation.Horizontal
            };
            foreach (int value in values)
            {
                fruit.Children.Add(new Asset
                {
                    AssetResource = FlatFluentEmoji.Get(_options[value])
                });
            }
            content.Children.Add(fruit);
            _dialog.Show(content);
            _spins = 0;
        }
    }

    // Add, Layout & New
    private void Add(StackPanel panel, int index)
    {
        Button button = new()
        {
            Width = 150,
            Height = 150,
            Margin = new Thickness(5),
            Content = Option(index, 0)
        };
        button.Click += (object sender, RoutedEventArgs e) =>
            Play();
        panel.Children.Add(button);
    }

    private void Layout(StackPanel panel)
    {
        panel.Children.Clear();
        for (int index = 0; index < size; index++)
        {
            Add(panel, index);
        }
    }

    public void New(StackPanel panel)
    {
        _spins = 0;
        _dialog = new Dialog(panel.XamlRoot, title);
        _panel = panel;
        Layout(_panel);
    }
}