using Comentsys.Assets.FluentEmoji;
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EmojiGame;

public enum State
{
    None,
    Correct,
    Incorrect
}

// Item Class
public class Item : ActionCommandObservableBase
{
    private State _state = State.None;

    public int Index { get; }
    public FluentEmojiType Type { get; }
    public bool Correct { get; }
    public ImageSource Source { get; }
    public State State
    {
        get => _state;
        set => SetProperty(ref _state, value);
    }

    public Item(int index, FluentEmojiType type,
        bool correct, ImageSource source, Action<int> action) :
        base(new ActionCommandHandler((param) => action(index))) =>
        (Index, Type, Correct, Source) =
            (index, type, correct, source);
}

public class Board : ObservableBase
{
    // Board Constants, Variables, Properties and GetSourceAsync Method
    private const string space = " ";
    private const int rounds = 12;
    private const int options = 2;

    private readonly Random _random = new((int)DateTime.UtcNow.Ticks);
    private Dictionary<FluentEmojiType, ImageSource> _sources;
    private ObservableCollection<Item> _items = new();
    private List<int> _selected = new();
    private List<int> _options = new();
    private List<int> _indexes = new();
    private string _question;
    private string _message;
    private int _round;
    private bool _over;

    public ObservableCollection<Item> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }

    public string Question
    {
        get => _question;
        set => SetProperty(ref _question, value);
    }

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    private async Task<ImageSource> GetSourceAsync(FluentEmojiType type) =>
        await FlatFluentEmoji.Get(type)
        .AsImageSourceAsync();

    // Board SetSourcesAsync Method
    private async Task SetSourcesAsync() =>
_sources ??= new Dictionary<FluentEmojiType, ImageSource>()
{
    { FluentEmojiType.GrinningFace,
        await GetSourceAsync(FluentEmojiType.GrinningFace) },
    { FluentEmojiType.BeamingFaceWithSmilingEyes,
        await GetSourceAsync(FluentEmojiType.BeamingFaceWithSmilingEyes) },
    { FluentEmojiType.FaceWithTearsOfJoy,
        await GetSourceAsync(FluentEmojiType.FaceWithTearsOfJoy) },
    { FluentEmojiType.GrinningSquintingFace,
        await GetSourceAsync(FluentEmojiType.GrinningSquintingFace) },
    { FluentEmojiType.WinkingFace,
        await GetSourceAsync(FluentEmojiType.WinkingFace) },
    { FluentEmojiType.FaceSavoringFood,
        await GetSourceAsync(FluentEmojiType.FaceSavoringFood) },
    { FluentEmojiType.SmilingFace,
        await GetSourceAsync(FluentEmojiType.SmilingFace) },
    { FluentEmojiType.HuggingFace,
        await GetSourceAsync(FluentEmojiType.HuggingFace) },
    { FluentEmojiType.ThinkingFace,
        await GetSourceAsync(FluentEmojiType.ThinkingFace) },
    { FluentEmojiType.FaceWithRaisedEyebrow,
        await GetSourceAsync(FluentEmojiType.FaceWithRaisedEyebrow) },
    { FluentEmojiType.NeutralFace,
        await GetSourceAsync(FluentEmojiType.NeutralFace) },
    { FluentEmojiType.ExpressionlessFace,
        await GetSourceAsync(FluentEmojiType.ExpressionlessFace) },
    { FluentEmojiType.FaceWithRollingEyes,
        await GetSourceAsync(FluentEmojiType.FaceWithRollingEyes) },
    { FluentEmojiType.PerseveringFace,
        await GetSourceAsync(FluentEmojiType.PerseveringFace) },
    { FluentEmojiType.FaceWithOpenMouth,
        await GetSourceAsync(FluentEmojiType.FaceWithOpenMouth) },
    { FluentEmojiType.HushedFace,
        await GetSourceAsync(FluentEmojiType.HushedFace) },
    { FluentEmojiType.SleepyFace,
        await GetSourceAsync(FluentEmojiType.SleepyFace) },
    { FluentEmojiType.TiredFace,
        await GetSourceAsync(FluentEmojiType.TiredFace) },
    { FluentEmojiType.SleepingFace,
        await GetSourceAsync(FluentEmojiType.SleepingFace) },
    { FluentEmojiType.RelievedFace,
        await GetSourceAsync(FluentEmojiType.RelievedFace) },
    { FluentEmojiType.UnamusedFace,
        await GetSourceAsync(FluentEmojiType.UnamusedFace) },
    { FluentEmojiType.PensiveFace,
        await GetSourceAsync(FluentEmojiType.PensiveFace) },
    { FluentEmojiType.ConfusedFace,
        await GetSourceAsync(FluentEmojiType.ConfusedFace) },
    { FluentEmojiType.AstonishedFace,
        await GetSourceAsync(FluentEmojiType.AstonishedFace) },
    { FluentEmojiType.FrowningFace,
        await GetSourceAsync(FluentEmojiType.FrowningFace) },
    { FluentEmojiType.ConfoundedFace,
        await GetSourceAsync(FluentEmojiType.ConfoundedFace) },
    { FluentEmojiType.DisappointedFace,
        await GetSourceAsync(FluentEmojiType.DisappointedFace) },
    { FluentEmojiType.WorriedFace,
        await GetSourceAsync(FluentEmojiType.WorriedFace) },
    { FluentEmojiType.FaceWithSteamFromNose,
        await GetSourceAsync(FluentEmojiType.FaceWithSteamFromNose) },
    { FluentEmojiType.AnguishedFace,
        await GetSourceAsync(FluentEmojiType.AnguishedFace) },
    { FluentEmojiType.FearfulFace,
        await GetSourceAsync(FluentEmojiType.FearfulFace) },
    { FluentEmojiType.FlushedFace,
        await GetSourceAsync(FluentEmojiType.FlushedFace) },
    { FluentEmojiType.ZanyFace,
        await GetSourceAsync(FluentEmojiType.ZanyFace) },
    { FluentEmojiType.FaceExhaling,
        await GetSourceAsync(FluentEmojiType.FaceExhaling) },
    { FluentEmojiType.AngryFace,
        await GetSourceAsync(FluentEmojiType.AngryFace) },
    { FluentEmojiType.NerdFace,
        await GetSourceAsync(FluentEmojiType.NerdFace)  }
    };

    // Board ChooseValues, ChooseUnique, Name, GetQuestion & Indexes Method
    private List<int> ChooseValues(int minimum, int maximum, int total)
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

    private List<int> ChooseUnique(int minimum, int maximum, int total) =>
        Enumerable.Range(minimum, maximum)
            .OrderBy(r => _random.Next(minimum, maximum))
                .Take(total).ToList();

    private string Name(FluentEmojiType item) =>
        Enum.GetName(typeof(FluentEmojiType), item);

    private string GetQuestion(FluentEmojiType item) =>
        string.Join(space, new Regex(@"\p{Lu}\p{Ll}*")
            .Matches(Name(item))
                    .Select(s => s.Value));

    private List<int> Indexes(IEnumerable<FluentEmojiType> items) =>
        items.Select(item => Array.IndexOf(items.ToArray(), item))
            .ToList();

    // Board Next Method
    public bool Next()
    {
        if (_round < rounds)
        {
            Items.Clear();
            var emoji = _sources.Keys.ToArray();
            var correct = emoji[_selected[_round]];
            Question = GetQuestion(correct);
            var incorrect = ChooseUnique(0, _options.Count - 1, options);
            var indexOne = _options[incorrect.First()];
            var indexTwo = _options[incorrect.Last()];
            var one = emoji[indexOne];
            var two = emoji[indexTwo];
            _options.Remove(indexOne);
            _options.Remove(indexTwo);
            var indexes = ChooseUnique(0, options + 1, options + 1);
            var items = new List<Item>()
        {
            new Item(indexes[0], correct, true, _sources[correct], Play),
            new Item(indexes[1], one, false, _sources[one], Play),
            new Item(indexes[2], two, false, _sources[two], Play)
        }.OrderBy(o => o.Index);
            foreach (var item in items)
            {
                Items.Add(item);
            }
            _round++;
            return true;
        }
        return false;
    }

    // Board SetupAsync, Correct & Play Method
    public async Task SetupAsync()
    {
        _round = 0;
        _over = false;
        Question = string.Empty;
        Message = string.Empty;
        await SetSourcesAsync();
        _indexes = Indexes(_sources.Keys);
        _selected = ChooseValues(0, _indexes.Count, rounds);
        _options = _indexes.Where(index => !_selected
            .Any(selected => selected == index)).ToList();
        Next();
    }

    public bool Correct(Item selected)
    {
        foreach (var item in Items)
        {
            item.State = item.Correct ?
                State.Correct : State.Incorrect;
        }
        return selected.Correct;
    }

    public void Play(int index)
    {
        if (!_over)
        {
            if (Correct(_items[index]))
            {
                if (!Next())
                {
                    Message = "Game Over, You Won";
                    _over = true;
                }
            }
            else
            {
                Message = "Incorrect, You Lost!";
                _over = true;
            }
        }
        else
            Message = "Game Over";
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
                State.Correct => Colors.Green,
                State.Incorrect => Colors.Red,
                _ => Colors.Transparent
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
    // Library Constants and GetBoundText Method

    private const int font = 20;
    private readonly Board _board = new();

    private TextBlock GetBoundText(string property)
    {
        var text = new TextBlock()
        {
            FontSize = font,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        var binding = new Binding()
        {
            Source = _board,
            Mode = BindingMode.OneWay,
            Path = new PropertyPath(property),
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        };
        BindingOperations.SetBinding(text, TextBlock.TextProperty, binding);
        return text;
    }

    // Library Layout & New Methods
    private void Layout(Grid grid, DataTemplate itemTemplate,
        ItemsPanelTemplate itemsPanel)
    {
        grid.Children.Clear();
        var panel = new StackPanel()
        {
            Orientation = Orientation.Vertical
        };
        var question = GetBoundText(nameof(_board.Question));
        panel.Children.Add(question);
        var items = new ItemsControl()
        {
            ItemsSource = _board.Items,
            ItemTemplate = itemTemplate,
            ItemsPanel = itemsPanel
        };
        panel.Children.Add(items);
        var message = GetBoundText(nameof(_board.Message));
        panel.Children.Add(message);
        grid.Children.Add(panel);
    }

    public async void New(Grid grid, DataTemplate itemTemplate,
        ItemsPanelTemplate itemsPanel)
    {
        await _board.SetupAsync();
        Layout(grid, itemTemplate, itemsPanel);
    }

}