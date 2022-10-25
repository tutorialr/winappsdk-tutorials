using Comentsys.Toolkit.WindowsAppSdk;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class Library
{
    private const string title = "High or Low";
    private const string higher = "Higher";
    private const string lower = "Lower";
    private const int minimum = 1;
    private const int maximum = 52;
    private const int suit = 13;
    private const int total = 4;
    private readonly Random _random = new((int)DateTime.UtcNow.Ticks);

    private StackPanel _panel = new();
    private Dialog _dialog;
    private List<int> _values = new();
    private bool _over = false;
    private int _card = 1;

    // Choose, Get Card, Set Card & Get Confirm
    private List<int> Choose(int minimum, int maximum, int total) =>
        Enumerable.Range(minimum, maximum)
            .OrderBy(r => _random.Next(minimum, maximum))
                .Take(total).ToList();
    public Card GetCard(int value, string name = "") => new()
    {
        Back = new SolidColorBrush(Colors.Red),
        Margin = new Thickness(10),
        Value = value,
        Name = name
    };

    public void SetCard(int value, string name) =>
        ((Card)_panel.FindName(name)).Value = value;

    private async Task<bool> GetConfirmAsync(int card)
    {
        var confirm = new StackPanel()
        {
            Orientation = Orientation.Vertical
        };
        confirm.Children.Add(new TextBlock()
        {
            HorizontalTextAlignment = TextAlignment.Center,
            Text = "Is Next Card?"
        });
        confirm.Children.Add(new Viewbox()
        {
            Height = 150,
            Child = GetCard(_values[card])
        });
        return await _dialog.ConfirmAsync(confirm, higher, lower);
    }

    // Play
    private async void Play(Card card)
    {
        if (card.Name == $"{_values[_card]}")
        {
            int next = _card;
            int prev = _card - 1;
            var isHigher = await GetConfirmAsync(prev);
            var source = _values[prev] % suit;
            source = source == 0 ? suit : source;
            var target = _values[next] % suit;
            target = target == 0 ? suit : target;
            if ((isHigher == true && target > source) ||
            (isHigher == false && target < source))
            {
                SetCard(_values[next], $"{_values[next]}");
                _card++;
                if (_card == _values.Count)
                {
                    _dialog.Show("Congratulations - You Win!");
                    _over = true;
                }
            }
            else
            {
                var content = new StackPanel()
                {
                    Orientation = Orientation.Vertical
                };
                content.Children.Add(new TextBlock()
                {
                    HorizontalTextAlignment = TextAlignment.Center,
                    Text = $"Incorrect - Next Card was {(!isHigher ? higher : lower)}!"
                });
                content.Children.Add(new Viewbox()
                {
                    Height = 150,
                    Child = GetCard(_values[_card])
                });
                _dialog.Show(content);
                _over = true;
            }
        }
        else
            _dialog.Show("Select Next Card!");
    }

    // Add Card, Layout & New
    public void AddCard(StackPanel panel, string name, int value)
    {
        var card = GetCard(value, name);
        card.Tapped += (object sender, TappedRoutedEventArgs e) =>
        {
            if (_over)
                _dialog.Show("Game Over!");
            else
                Play((Card)sender);
        };
        panel.Children.Add(card);
    }
    private void Layout(StackPanel panel)
    {
        panel.Children.Clear();
        for (int index = 0; index < total; index++)
        {
            AddCard(panel, $"{_values[index]}", index == 0 ? _values[index] : 0);
        }
    }
    public void New(StackPanel panel)
    {
        _card = 1;
        _over = false;
        _panel = panel;
        _dialog = new Dialog(panel.XamlRoot, title);
        _values = Choose(minimum, maximum, total);
        Layout(panel);
    }
}
