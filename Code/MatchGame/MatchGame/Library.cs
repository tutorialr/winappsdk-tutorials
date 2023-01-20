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
using System.Linq;
using System.Windows.Input;

namespace MatchGame;

public enum State
{
    Wait,
    Off,
    On
}

public enum Match
{
    Memorise,
    Waiting,
    Remember,
    Complete
}

// Item Class
public class Item : ObservableBase
{
    private State _state;
    private int _index;
    private readonly Action<int> _action;

    public Item(int index, State state, Action<int> action) =>
        (_index, State, _action) = (index, state, action);

    public ICommand Command =>
        new ActionCommandHandler((param) => _action(_index));

    public int Index
    {
        get => _index;
        set => SetProperty(ref _index, value);
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
            return new SolidColorBrush(value switch
            {
                State.On => Colors.White,
                State.Off => Colors.Black,
                _ => Colors.Gray
            });
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
    private const string title = "Match Game";
    private const int interval = 1;
    private const int total = 16;
    private const int delay = 4;
    private const int size = 4;

    private readonly List<int> _hits = new();
    private readonly List<int> _miss = new();
    private readonly Dictionary<int, State> _states = new();
    private readonly ObservableCollection<Item> _items = new();
    private readonly Random _random = new((int)DateTime.UtcNow.Ticks);

    private DispatcherTimer _timer;
    private Dialog _dialog;
    private Match _match;
    private int _count;
    private int _turns;

    private List<int> Choose(int minimum, int maximum, int total) =>
        Enumerable.Range(minimum, maximum)
            .OrderBy(r => _random.Next(minimum, maximum))
                .Take(total).ToList();

    // Library Set, Change, Update & Pattern Method
    private void Set(int index, State state) =>
    _items.FirstOrDefault(w => w.Index == index)
        .State = state;

    private void Change(Match match) =>
        (_count, _match) = (delay, match);

    private void Update(State state)
    {
        foreach (var item in _items)
            item.State = state;
    }

    private void Pattern()
    {
        _hits.Clear();
        _miss.Clear();
        _states.Clear();
        Update(State.Off);
        var positions = Choose(0, total, size);
        for (int index = 0; index < total; index++)
        {
            State state = State.Off;
            if (positions.Contains(index))
            {
                state = State.On;
                _states.Add(index, state);
            }
            Set(index, state);
        }
    }

    // Library Tick Method
    private void Tick()
    {
        switch (_match)
        {
            case Match.Complete:
                _dialog.Show($"Game Over with {_turns} Matches!");
                _timer?.Stop();
                break;
            case Match.Memorise:
                if (_count == delay)
                    Pattern();
                _count--;
                if (_count == 0)
                    Change(Match.Waiting);
                break;
            case Match.Waiting:
                if (_count == delay)
                    Update(State.Wait);
                _count--;
                if (_count == 0)
                    Change(Match.Remember);
                break;
            case Match.Remember:
                if (_count == delay)
                    Update(State.Off);
                _count--;
                if (_count == 0)
                {
                    if (_hits.Count == size)
                    {
                        _turns++;
                        Change(Match.Memorise);
                    }
                    else
                        Change(Match.Complete);
                }
                break;
        }
    }

    // Library Play, Layout & New Method
    private void Play(int index)
    {
        if (_match == Match.Remember &&
            _hits.Count + _miss.Count < size)
        {
            if (_states.ContainsKey(index) &&
                !_hits.Contains(index))
                _hits.Add(index);
            else if (!_states.ContainsKey(index) &&
                !_miss.Contains(index))
                _miss.Add(index);
            Set(index, State.On);
        }
    }

    private void Layout(ItemsControl display)
    {
        for (int index = 0; index < total; index++)
        {
            _items.Add(new Item(index, State.Wait, (int i) => Play(i)));
        }
        display.ItemsSource = _items;
    }

    public void New(ItemsControl display)
    {
        _turns = 1;
        _count = delay;
        _hits.Clear();
        _miss.Clear();
        _items.Clear();
        Layout(display);
        _match = Match.Memorise;
        _dialog = new Dialog(display.XamlRoot, title);
        _timer?.Stop();
        _timer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromSeconds(interval)
        };
        _timer.Tick += (object sender, object e) =>
            Tick();
        _timer.Start();
    }
}