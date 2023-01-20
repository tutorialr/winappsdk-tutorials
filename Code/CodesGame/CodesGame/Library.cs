using Comentsys.Toolkit.Binding;
using Comentsys.Toolkit.WindowsAppSdk;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace CodesGame;

public enum State
{
    None,
    Match
}

// Code Class
public class Code : ObservableBase
{
    private int _value;
    private State _state;
    private readonly int _index;
    private readonly Action<int> _action;

    public Code(int index, int value, State state, Action<int> action) =>
        (_index, Value, State, _action) = (index, value, state, action);

    public ICommand Command =>
        new ActionCommandHandler((param) => _action(_index));

    public int Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    public State State
    {
        get => _state;
        set => SetProperty(ref _state, value);
    }
}

// StateToBrushConverter Class
public class StateToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType,
        object parameter, string language)
    {
        if (value is State state)
        {
            var invert = bool.Parse(parameter as string);
            var none = state == State.None;
            var color = none ^= invert;
            return new SolidColorBrush(color ? Colors.White : Colors.Black);
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType,
        object parameter, string language) =>
        throw new NotImplementedException();
}


public class Library
{
    // Library Constants, Variables and Choose Method
    private const string title = "Codes Game";
    private const int max = 9;
    private const int total = 4;

    private readonly ObservableCollection<Code> _codes = new();
    private readonly Random _random = new((int)DateTime.UtcNow.Ticks);

    private List<int> _values = new();
    private int _turns = 0;

    private Dialog _dialog;
    private ItemsControl _items;

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

    // Library GetCode, IsMatch & Setup Method
    private Code GetCode(int index, int value) =>
        new(index, value, State.None, (int i) =>
        {
            var code = _codes[i];
            if (code.State == State.None)
                code.Value = (code.Value == max) ? 1 : code.Value + 1;
        });

    private bool IsMatch(int index, int value)
    {
        var code = _codes[index];
        return value == code.Value ?
        (code.State = State.Match) == State.Match :
        (code.State = State.None) == State.Match;
    }

    private void Setup()
    {
        _turns = 0;
        _codes.Clear();
        for (int index = 0; index < total; index++)
        {
            _codes.Add(GetCode(index, index + 1));
        }
        _values = Choose(1, max, total);
        _items.ItemsSource = _codes;
    }

    // Library Accept & New Method
    public void Accept()
    {
        int index = 0;
        int correct = 0;
        foreach (var value in _values)
        {
            if (IsMatch(index, value))
                correct++;
            index++;
        }
        _turns++;
        if (correct == total)
        {
            string code = string.Join(string.Empty, _codes.Select(s => s.Value));
            _dialog.Show($"Matched {code} in {_turns} turns");
            Setup();
        }
    }

    public void New(ItemsControl items)
    {
        _dialog = new Dialog(items.XamlRoot, title);
        _items = items;
        Setup();
    }
}