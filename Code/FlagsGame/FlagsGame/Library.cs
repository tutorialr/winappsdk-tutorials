using Comentsys.Assets.Flags;
using Comentsys.Toolkit.WindowsAppSdk;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class Library
{
    private const string space = " ";
    private const int flag_size = 72;
    private const int font = 20;
    private const int size = 3;

    private readonly Random _random = new((int)DateTime.UtcNow.Ticks);

    private Grid _grid;
    private TextBlock _text;

    private Dictionary<FlagType, ImageSource> _sources;
    private List<int> _indexes = new();
    private List<int> _choices = new();
    private int _turns;
    private bool _over;
    private string _name;

    // GetSourceAsync & SetSourcesAsync
    private async Task<ImageSource> GetSourceAsync(FlagType flagType) =>
    await Flag.Get(FlagSet.Square, flagType)
    .AsImageSourceAsync();

    private async Task SetSourcesAsync() =>
        _sources ??= new Dictionary<FlagType, ImageSource>()
        {
        { FlagType.Armenia, await GetSourceAsync(FlagType.Armenia) },
        { FlagType.Austria, await GetSourceAsync(FlagType.Austria) },
        { FlagType.Belgium, await GetSourceAsync(FlagType.Belgium) },
        { FlagType.Bulgaria, await GetSourceAsync(FlagType.Bulgaria) },
        { FlagType.Estonia, await GetSourceAsync(FlagType.Estonia) },
        { FlagType.France, await GetSourceAsync(FlagType.France) },
        { FlagType.Gabon, await GetSourceAsync(FlagType.Gabon) },
        { FlagType.Germany, await GetSourceAsync(FlagType.Germany) },
        { FlagType.Guinea, await GetSourceAsync(FlagType.Guinea) },
        { FlagType.Ireland, await GetSourceAsync(FlagType.Ireland) },
        { FlagType.Italy, await GetSourceAsync(FlagType.Italy) },
        { FlagType.Lithuania, await GetSourceAsync(FlagType.Lithuania) },
        { FlagType.Luxembourg, await GetSourceAsync(FlagType.Luxembourg) },
        { FlagType.Mali, await GetSourceAsync(FlagType.Mali) },
        { FlagType.Netherlands, await GetSourceAsync(FlagType.Netherlands) },
        { FlagType.Nigeria, await GetSourceAsync(FlagType.Nigeria) },
        { FlagType.Romania, await GetSourceAsync(FlagType.Romania) },
        { FlagType.Hungary, await GetSourceAsync(FlagType.Hungary) },
        { FlagType.SierraLeone, await GetSourceAsync(FlagType.SierraLeone) },
        { FlagType.Yemen, await GetSourceAsync(FlagType.Yemen) }
        };

    // Chose, Name, Country, Select & Set
    private List<int> Choose(int minimum, int maximum, int total) =>
    Enumerable.Range(minimum, maximum)
        .OrderBy(r => _random.Next(minimum, maximum))
            .Take(total).ToList();

    private string Name(FlagType flag) =>
        Enum.GetName(typeof(FlagType), flag);

    private string Country(FlagType flag) =>
        string.Join(space, new Regex(@"\p{Lu}\p{Ll}*")
            .Matches(Name(flag))
                .Select(s => s.Value));

    private void Select()
    {
        var choice = _choices[_turns];
        var index = _indexes[choice];
        var flag = _sources.ElementAt(index);
        _name = Name(flag.Key);
        _text.Text = Country(flag.Key);
        _turns++;
    }

    private void Set(string name, bool display) =>
        (_grid.FindName(name) as Button).Opacity = display ? 1 : 0;

    // Play & Add
    private void Play(Button button)
    {
        if (!_over)
        {
            string name = button.Name;
            if (_name == name)
            {
                Set(name, false);
                if (_turns < size * size)
                    Select();
                else
                    _text.Text = "You Won!";
            }
            else
                _over = true;
        }
        if (_over)
            _text.Text = "Game Over!";
    }

    private void Add(int row, int column, int index)
    {
        var flag = _sources.ElementAt(_indexes[index]);
        var border = new Border()
        {
            BorderBrush = new SolidColorBrush(Colors.Black),
            BorderThickness = new Thickness(2)
        };
        var image = new Image()
        {
            Height = flag_size,
            Width = flag_size,
            Source = flag.Value
        };
        border.Child = image;
        var button = new Button()
        {
            Name = Name(flag.Key),
            Content = border
        };
        button.Click += (object sender, RoutedEventArgs e) =>
            Play(sender as Button);
        button.SetValue(Grid.RowProperty, row);
        button.SetValue(Grid.ColumnProperty, column);
        _grid.Children.Add(button);
    }

    // Layout & New
    private void Layout(Grid grid)
    {
        var index = 0;
        grid.Children.Clear();
        grid.RowDefinitions.Clear();
        grid.RowDefinitions.Add(new RowDefinition()
        {
            Height = GridLength.Auto
        });
        grid.RowDefinitions.Add(new RowDefinition()
        {
            Height = new GridLength(1, GridUnitType.Star)
        });
        _text = new TextBlock()
        {
            FontSize = font,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Grid.SetRow(_text, 0);
        grid.Children.Add(_text);
        _grid = new Grid();
        for (int row = 0; row < size; row++)
        {
            _grid.RowDefinitions.Add(new RowDefinition());
            for (int column = 0; column < size; column++)
            {
                if (row == 0)
                    _grid.ColumnDefinitions.Add(new ColumnDefinition());
                Add(row, column, index);
                index++;
            }
        }
        Grid.SetRow(_grid, 1);
        grid.Children.Add(_grid);
    }

    public async void New(Grid grid)
    {
        _turns = 0;
        _over = false;
        await SetSourcesAsync();
        _indexes = Choose(0, _sources.Count, _sources.Count);
        _choices = Choose(0, size * size, size * size);
        Layout(grid);
        Select();
    }
}