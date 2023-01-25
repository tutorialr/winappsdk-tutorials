using Comentsys.Toolkit.Binding;
using Comentsys.Toolkit.WindowsAppSdk;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace WordsGame;

public enum State
{
    Key,
    Empty,
    Absent,
    Present,
    Correct
}

// Position Class
public class Position : ObservableBase
{
    private int _row;
    private int _column;
    private char _letter;

    public Position(int row, int column, char letter) =>
        (_column, _row, _letter) = (column, row, letter);

    public int Row
    {
        get => _row;
        set => SetProperty(ref _row, value);
    }

    public int Column
    {
        get => _column;
        set => SetProperty(ref _column, value);
    }

    public char Letter
    {
        get => _letter;
        set => SetProperty(ref _letter, value);
    }
}

// Item Class
public class Item : ActionCommandObservableBase
{
    private State _state;
    private Position _position;

    public Item(Position position, State state) : base(null) =>
        (_position, State) = (position, state);

    public Item(Position position, State state, Action<Position> action) :
        base(new ActionCommandHandler((param) => action(position))) =>
        (_position, State) = (position, state);

    public Position Position
    {
        get => _position;
        set => SetProperty(ref _position, value);
    }

    public State State
    {
        get => _state;
        set => SetProperty(ref _state, value);
    }
}

// StateToBrushConvertor Class
public class StateToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType,
        object parameter, string language)
    {
        if (value is State state)
        {
            return new SolidColorBrush(value switch
            {
                State.Empty => Colors.White,
                State.Absent => Colors.DarkGray,
                State.Present => Colors.DarkKhaki,
                State.Correct => Colors.DarkSeaGreen,
                _ => Colors.LightGray
            });
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType,
        object parameter, string language) =>
        throw new NotImplementedException();
}

// ItemTemplateSelector Class
public class ItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate SpacerItem { get; set; }
    public DataTemplate KeyItem { get; set; }

    protected override DataTemplate SelectTemplateCore
        (object value, DependencyObject container) =>
        value is Item item ? item?.Command != null ?
        KeyItem : SpacerItem : null;
}

// Words Class
public class Words
{
    private const string request = "https://raw.githubusercontent.com/tutorialr/winappsdk-tutorials/main/Code/WordsGame/words.txt";
    private readonly List<string> _results = new();
    private readonly HttpClient _client = new();

    public async Task RequestAsync()
    {
        try
        {
            _results.Clear();
            var response = await _client.GetStreamAsync(request);
            using var reader = new StreamReader(response);
            while (!reader.EndOfStream)
            {
                var word = await reader.ReadLineAsync();
                if (word != null)
                    _results.Add(word);
            }
        }
        catch { }
    }

    public List<string> Response => _results;
}

public class Library
{
    // Library Constants, Variables & GetIndexes Method
    private const string title = "Words Game";
    private const char backspace = '⌫';
    private const char empty = ' ';
    private const int count = 5;
    private const int keys = 11;
    private const int rows = 3;

    private readonly Words _words = new();
    private readonly ObservableCollection<Item> _keys = new();
    private readonly ObservableCollection<Item> _items = new();
    private readonly Random _random = new((int)DateTime.UtcNow.Ticks);
    private readonly List<char> _letters = new()
    {
        'Q', 'W', 'E', 'R', 'T', 'Y', 'U', 'I', 'O', 'P', backspace,
        empty, 'A', 'S', 'D', 'F', 'G', 'H', 'J', 'K', 'L', empty,
        empty, empty, 'Z', 'X', 'C', 'V', 'B', 'N', 'M', empty, empty
    };

    private Dialog _dialog;
    private string _word;
    private bool _winner;
    private int _column;
    private int _row;

    public static IEnumerable<int> GetIndexes(string source, char target)
    {
        int index = source.IndexOf(target);
        while (index != -1)
        {
            yield return index;
            index = source.IndexOf(target, index + 1);
        }
    }

    // Library ListCurrent, GetCurrent, Set & Check Method
    private IEnumerable<Item> ListCurrent() =>
    _items.Where(f => f.Position.Row == _row);

    private Item GetCurrent() =>
        _items.FirstOrDefault(
        f => f.Position.Row == _row
        && f.Position.Column == _column);

    private void Set(Position position, State state)
    {
        var key = _keys.FirstOrDefault(
            f => f.Position.Letter == position.Letter);
        if (key != null)
            key.State = state;
        var item = _items.FirstOrDefault(
            f => f.Position.Row == _row
            && f.Position.Column == position.Column
            && f.Position.Letter == position.Letter);
        if (item != null)
        {
            item.Position.Letter = position.Letter;
            item.State = state;
        }
    }

    private bool Check()
    {
        var current = ListCurrent();
        foreach (var item in current)
        {
            var state = State.Absent;
            var indexes = GetIndexes(_word, item.Position.Letter);
            if (indexes?.Any() == true)
            {
                foreach (var index in indexes)
                {
                    state = item.Position.Column == index ?
                        State.Correct : State.Present;
                }
            }
            Set(item.Position, state);
        }
        var word = string.Join(string.Empty, current.Select(s => s.Position.Letter));
        _winner = _word.Equals(word, StringComparison.InvariantCultureIgnoreCase);
        return _winner;
    }

    // Library Over & Select Method
    private bool Over()
    {
        if (_row == count)
        {
            _dialog.Show($"Game Over! You did not get the word {_word}!");
            return true;
        }
        else if (_winner)
        {
            _dialog.Show($"Game Over! You got the word {_word} correct!");
            return true;
        }
        return false;
    }

    private void Select(Position position)
    {
        if (!Over())
        {
            if (position.Letter == backspace)
            {
                if (_column > 0)
                {
                    _column--;
                    var current = GetCurrent();
                    if (current != null)
                    {
                        current.State = State.Empty;
                        current.Position.Letter = empty;
                    }
                }
            }
            else
            {
                if (_column < count)
                {
                    var current = GetCurrent();
                    if (current != null)
                    {
                        current.State = State.Key;
                        current.Position.Letter = position.Letter;
                        _column++;
                    }
                }
            }
        }
    }

    // Layout Method
    private void Layout(ItemsControl display, ItemsControl keyboard)
    {
        int index = 0;
        _keys.Clear();
        _items.Clear();
        for (int row = 0; row < count; row++)
        {
            for (int column = 0; column < count; column++)
            {
                _items.Add(new Item(
                new Position(column, row, empty),
                State.Empty));
            }
        }
        display.ItemsSource = _items;
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < keys; column++)
            {
                var letter = _letters[index];
                var position = new Position(row, column, letter);
                if (letter == empty)
                    _keys.Add(new Item(position,
                    State.Empty));
                else
                    _keys.Add(new Item(position,
                    State.Key, (Position p) => Select(p)));
                index++;
            }
        }
        keyboard.ItemsSource = _keys;
    }

    // Setup, Load, Accept & New Methods
    private void Setup()
    {
        _row = 0;
        _column = 0;
        _winner = false;
        var total = _words.Response.Count;
        if (total > 0)
        {
            var choice = _random.Next(0, total - 1);
            _word = _words.Response[choice];
            foreach (var key in _keys)
                key.State = State.Key;
            foreach (var item in _items)
            {
                item.State = State.Empty;
                item.Position.Letter = empty;
            }
        }
        else
            _dialog.Show("Failed to load Word List!");
    }

    public async void Load(ItemsControl display, ItemsControl keyboard)
    {
        _dialog = new Dialog(display.XamlRoot, title);
        await _words.RequestAsync();
        Layout(display, keyboard);
        Setup();
    }

    public void Accept()
    {
        if (_row < count)
        {
            if (_column == count)
            {
                if (!Check())
                {
                    _column = 0;
                    _row++;
                }
            }
            else
                _dialog.Show("Not enough letters");
        }
        Over();
    }

    public void New() =>
        Setup();
}