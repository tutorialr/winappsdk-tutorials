// Usings & Namespace
using Comentsys.Toolkit.Binding;
using Comentsys.Toolkit.WindowsAppSdk;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YatzyGame;

public enum ScoreType
{
    AcesScore, TwosScore, ThreesScore, FoursScore, FivesScore,
    SixesScore, UpperTotalScore, UpperTotalBonusScore, ThreeOfAKindScore,
    FourOfAKindScore, FullHouseScore, SmallStraightScore, LargeStraightScore,
    YahtzeeScore, ChanceScore, YahtzeeBonusScore, LowerTotalScore, TotalScore
}

// Extensions Class
public static class Extensions
{
    private const string space = " ";
    private const string score = "Score";
    private static readonly Regex regex = new(@"\p{Lu}\p{Ll}*");

    public static string Name(this ScoreType type) =>
        string.Join(space, regex
            .Matches(Enum.GetName(typeof(ScoreType), type)
                .Replace(score, string.Empty))
                    .Select(s => s.Value));

    public static string Name(this ScoreType type, int take) =>
        string.Join(space, regex
            .Matches(Enum.GetName(typeof(ScoreType), type))
                .Select(s => s.Value)
                    .Take(take));
}

// Item Class
public class Item : ActionCommandObservableBase
{
    private int _value;
    private bool _hold;

    public int Index { get; }
    public int Value { get => _value; set => SetProperty(ref _value, value); }
    public bool Hold { get => _hold; set => SetProperty(ref _hold, value); }

    public Item(int index, Action<int> action) :
        base(new ActionCommandHandler((param) =>
        action((param as Item).Index),
        (param) => (param as Item).IsEnabled)) =>
        (Index, Value) = (index, index + 1);
}


// Option Class
public class Option : ActionCommandObservableBase
{
    private int _score;

    public int Score { get => _score; set => SetProperty(ref _score, value); }
    public ScoreType Type { get; }
    public string Content => Type.Name();

    public Option(ScoreType type) : base(null) =>
        Type = type;

    public Option(ScoreType type, Action<object> action) :
        base(new ActionCommandHandler((param) =>
        action(null),
        (param) => (param as Option).IsEnabled)) =>
        Type = type;
}

public class Calculate
{
    // Calculate GetAddUp, GetOfAKind & GetFullHouse Method
    public static int GetAddUp(Item[] dice, int value)
    {
        int sum = 0;
        foreach (var item in dice.Where(w => w.Value == value))
            sum += value;
        return sum;
    }

    public static int GetOfAKind(Item[] dice, int value)
    {
        int sum = 0;
        bool result = false;
        for (int i = 1; i <= 6; i++)
        {
            int count = 0;
            for (int j = 0; j < 5; j++)
            {
                if (dice[j].Value == i)
                    count++;
                if (count > value)
                    result = true;
            }
        }
        if (result)
        {
            foreach (var item in dice)
                sum += item.Value;
        }
        return sum;
    }

    public static int GetFullHouse(Item[] dice)
    {
        int sum = 0;
        int[] item = dice.Select(s => s.Value).ToArray();
        Array.Sort(item);
        if (((item[0] == item[1]) && (item[1] == item[2]) && // Three of a Kind
            (item[3] == item[4]) && // Two of a Kind
            (item[2] != item[3])) ||
            ((item[0] == item[1]) && // Two of a Kind
            (item[2] == item[3]) && (item[3] == item[4]) && // Three of a Kind
            (item[1] != item[2])))
            sum = 25;
        return sum;
    }

    // Calculate GetSmallStraight & GetLargeStraight Method
    public static int GetSmallStraight(Item[] dice)
    {
        int sort = 0;
        int[] item = dice.Select(s => s.Value).ToArray();
        Array.Sort(item);
        for (int j = 0; j < 4; j++)
        {
            int value = 0;
            if (item[j] == item[j + 1])
            {
                value = item[j];
                for (int k = j; k < 4; k++)
                    item[k] = item[k + 1];
                item[4] = value;
            }
        }
        if (((item[0] == 1) && (item[1] == 2) && (item[2] == 3) && (item[3] == 4)) ||
            ((item[0] == 2) && (item[1] == 3) && (item[2] == 4) && (item[3] == 5)) ||
            ((item[0] == 3) && (item[1] == 4) && (item[2] == 5) && (item[3] == 6)) ||
            ((item[1] == 1) && (item[2] == 2) && (item[3] == 3) && (item[4] == 4)) ||
            ((item[1] == 2) && (item[2] == 3) && (item[3] == 4) && (item[4] == 5)) ||
            ((item[1] == 3) && (item[2] == 4) && (item[3] == 5) && (item[4] == 6)))
            sort = 30;
        return sort;
    }

    public static int GetLargeStraight(Item[] dice)
    {
        int sum = 0;
        int[] i = dice.Select(s => s.Value).ToArray();
        Array.Sort(i);
        if (((i[0] == 1) && (i[1] == 2) && (i[2] == 3) && (i[3] == 4) && (i[4] == 5)) ||
            ((i[0] == 2) && (i[1] == 3) && (i[2] == 4) && (i[3] == 5) && (i[4] == 6)))
            sum = 40;
        return sum;
    }

    // Calculate Get GetYahtzee & GetChance Method
    public static int GetYahtzee(Item[] dice)
    {
        int sum = 0;
        for (int i = 1; i <= 6; i++)
        {
            int Count = 0;
            for (int j = 0; j < 5; j++)
            {
                if (dice[j].Value == i)
                    Count++;
                if (Count > 4)
                    sum = 50;
            }
        }
        return sum;
    }

    public static int GetChance(Item[] dice)
    {
        int sum = 0;
        for (int i = 0; i < 5; i++)
            sum += dice[i].Value;
        return sum;
    }

}

public class Board : ActionCommandObservableBase
{
    // Board Constants, Variables, Properties and Choose, SetScore & GetScore Method
    private const int dice = 5;
    private const int count = 14;
    private const string accept = "Accept?";

    private readonly Random _random = new((int)DateTime.UtcNow.Ticks);
    private readonly Func<string, Task<bool>> _confirm = null;

    private List<Option> _options;
    private Item[] _dice;
    private int _rolls;
    private int _count;
    private int _total;
    private int _upper;
    private int _lower;
    private int _bonus;

    public List<Option> Options { get => _options; set => SetProperty(ref _options, value); }
    public Item[] Dice { get => _dice; set => SetProperty(ref _dice, value); }

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

    private void SetScore(ScoreType type, int value)
    {
        var score = Options.FirstOrDefault(f => f.Type == type);
        if (score != null)
            score.Score = value;
    }

    private int GetScore(ScoreType type) =>
        Options.FirstOrDefault(f => f.Type == type)?.Score ?? 0;

    // Board Clear & Reset Method
    private void Clear()
    {
        _rolls = 0;
        _count = 0;
        _total = 0;
        _upper = 0;
        _lower = 0;
        _bonus = 0;
        foreach (ScoreType type in Enum.GetValues(typeof(ScoreType)))
            SetScore(type, 0);
        int value = 1;
        foreach (var dice in Dice)
        {
            dice.Hold = false;
            dice.Value = value++;
            dice.IsEnabled = false;
        }
        foreach (var option in Options)
            option.IsEnabled = false;
        IsEnabled = true;
    }

    private async Task Reset()
    {
        _rolls = 0;
        _count++;
        foreach (var dice in _dice)
            dice.Hold = false;
        SetScore(ScoreType.UpperTotalScore, _upper);
        SetScore(ScoreType.UpperTotalBonusScore, _bonus);
        SetScore(ScoreType.LowerTotalScore, _lower);
        SetScore(ScoreType.TotalScore, _total);
        if (_count == count)
        {
            int total = GetScore(ScoreType.TotalScore);
            bool result = await _confirm($"Game Over, Score {total}. Play again?");
            if (result)
                Clear();
        }
    }

    // Board SetTotal & AddUpDice Method

    private void SetTotal(int score, bool isUpper)
    {
        var isBonus = false;
        if (isUpper)
        {
            _upper += score;
            if (_upper >= 63)
                isBonus = true;
        }
        else
            _lower += score;
        _total = 0;
        _total += _upper;
        if (isBonus)
        {
            _bonus = 35;
            _total += _bonus;
        }
        _total += _lower;
    }

    private async void AddUpDice(ScoreType type, int value)
    {
        int score = GetScore(type);
        if (_rolls > 0 && score == 0)
        {
            int total = Calculate.GetAddUp(_dice, value);
            bool result = await _confirm($"Total is {total}. {accept}");
            if (result)
            {
                SetScore(type, total);
                SetTotal(total, true);
                await Reset();
            }
        }
    }

    // Board SetValueOfAKind Method
    private async void SetValueOfAKind(ScoreType type, int value)
    {
        string name = type.Name(1);
        int score = GetScore(type);
        if (_count > 0 && score == 0)
        {
            int total = Calculate.GetOfAKind(_dice, value - 1);
            if (total != 0)
            {
                bool result = await _confirm($"Total is {total}. {accept}");
                if (result)
                {
                    SetScore(type, total);
                    SetTotal(total, false);
                    await Reset();
                }
            }
            else
            {
                bool result = await _confirm($"No {name} of a Kind. {accept}");
                if (result)
                {
                    SetScore(type, 0);
                    SetTotal(total, false);
                    await Reset();
                }
            }
        }
    }

    // Board SetItemScore Method
    private async void SetItemScore(ScoreType type, int value)
    {
        string name = type.Name();
        int score = GetScore(type);
        if ((_rolls > 0) && (score == 0))
        {
            int total = type switch
            {
                ScoreType.FullHouseScore => Calculate.GetFullHouse(_dice),
                ScoreType.SmallStraightScore => Calculate.GetSmallStraight(_dice),
                ScoreType.LargeStraightScore => Calculate.GetLargeStraight(_dice),
                _ => 0,
            };
            if (total == value)
            {
                SetScore(type, total);
                SetTotal(total, false);
                await Reset();
            }
            else
            {
                bool result = await _confirm($"No {name}. {accept}");
                if (result)
                {
                    SetScore(type, 0);
                    SetTotal(total, false);
                    await Reset();
                }
            }
        }
    }

    // Board SetYahtzee Method
    private async void SetYahtzee()
    {
        int score = GetScore(ScoreType.YahtzeeScore);
        if ((_rolls > 0) && (score == 0))
        {
            int total = Calculate.GetYahtzee(_dice);
            if (total == 50)
            {
                SetScore(ScoreType.YahtzeeScore, total);
                SetTotal(total, false);
                await Reset();
            }
            else
            {
                bool result = await _confirm($"No Yahtzee. {accept}");
                if (result)
                {
                    SetScore(ScoreType.YahtzeeScore, 0);
                    SetScore(ScoreType.YahtzeeBonusScore, 0);
                    _count++;
                    SetTotal(total, true);
                    await Reset();
                }
            }
        }
    }

    // Board SetChance, SetBonus & New Method

    private async void SetChance()
    {
        int score = GetScore(ScoreType.ChanceScore);
        if ((_rolls > 0) && (score == 0))
        {
            int total = Calculate.GetChance(_dice);
            bool result = await _confirm($"Total is {total}. {accept}");
            if (result)
            {
                SetScore(ScoreType.ChanceScore, total);
                SetTotal(total, false);
                await Reset();
            }
        }
    }

    private async void SetBonus()
    {
        int score = GetScore(ScoreType.YahtzeeScore);
        int bonus = GetScore(ScoreType.YahtzeeBonusScore);
        if ((_rolls > 0) && (score == 0) && (bonus != 0))
        {
            int total = Calculate.GetYahtzee(_dice);
            if (total == 50)
            {
                SetScore(ScoreType.YahtzeeBonusScore, 100);
                SetTotal(100, false);
                await Reset();
            }
            else
            {
                SetScore(ScoreType.YahtzeeBonusScore, 0);
                SetTotal(0, true);
                await Reset();
            }
        }
    }

    public void New() =>
        Clear();

    // Board Roll Method
    internal void Roll()
    {
        if (_rolls < 3)
        {
            if (_rolls == 0)
            {
                foreach (var dice in Dice)
                {
                    dice.IsEnabled = true;
                    dice.Hold = false;
                }
                foreach (var option in Options)
                    option.IsEnabled = true;
            }
            var values = Choose(1, 6, dice);
            for (int i = 0; i < Dice.Length; i++)
            {
                if (!Dice[i].Hold)
                    Dice[i].Value = values[i];
            }
            _rolls++;
            if (_rolls == 3)
            {
                foreach (var dice in Dice)
                {
                    dice.IsEnabled = false;
                    dice.Hold = true;
                }
                IsEnabled = false;
            }
        }
    }

    // Board Constructor
    public Board(Func<string, Task<bool>> confirm) : base(
    new ActionCommandHandler((param) => (param as Board).Roll(),
    (param) => (param as Board).IsEnabled))
    {
        IsEnabled = true;
        _confirm = confirm;
        _dice = new Item[dice];
        for (int i = 0; i < _dice.Length; i++)
            _dice[i] = new Item(i, (int i) =>
            _dice[i].Hold = !_dice[i].Hold);
        Options = new()
        {
            new Option(ScoreType.AcesScore,
                (p) => AddUpDice(ScoreType.AcesScore, 1)),
            new Option(ScoreType.TwosScore,
                (p) => AddUpDice(ScoreType.TwosScore, 2)),
            new Option(ScoreType.ThreesScore,
                (p) => AddUpDice(ScoreType.ThreesScore, 3)),
            new Option(ScoreType.FoursScore,
                (p) => AddUpDice(ScoreType.FoursScore, 4)),
            new Option(ScoreType.FivesScore,
                (p) => AddUpDice(ScoreType.FivesScore, 5)),
            new Option(ScoreType.SixesScore,
                (p) => AddUpDice(ScoreType.SixesScore, 6)),
            new Option(ScoreType.UpperTotalScore),
            new Option(ScoreType.UpperTotalBonusScore),
            new Option(ScoreType.ThreeOfAKindScore,
                (p) => SetValueOfAKind(ScoreType.ThreeOfAKindScore, 3)),
            new Option(ScoreType.FourOfAKindScore,
                (p) => SetValueOfAKind(ScoreType.FourOfAKindScore, 4)),
            new Option(ScoreType.FullHouseScore,
                (p) => SetItemScore(ScoreType.FullHouseScore, 25)),
            new Option(ScoreType.SmallStraightScore,
                (p) => SetItemScore(ScoreType.SmallStraightScore, 30)),
            new Option(ScoreType.LargeStraightScore,
                (p) => SetItemScore(ScoreType.FullHouseScore, 25)),
            new Option(ScoreType.YahtzeeScore,
                (p) => SetYahtzee()),
            new Option(ScoreType.ChanceScore,
                (p) => SetChance()),
            new Option(ScoreType.YahtzeeBonusScore,
                (p) => SetBonus()),
            new Option(ScoreType.LowerTotalScore),
            new Option(ScoreType.TotalScore)
        };
    }
}

// OptionTemplateSelector Class
public class OptionTemplateSelector : DataTemplateSelector
{
    public DataTemplate ScoreItem { get; set; }
    public DataTemplate TotalItem { get; set; }

    protected override DataTemplate SelectTemplateCore(
        object value, DependencyObject container) =>
        value is Option item ? item.Command != null ?
            ScoreItem : TotalItem : null;
}

// Library Class   
public class Library
{
    private const string title = "Yatzy Game";
    private Board _board;
    private Dialog _dialog;

    public void Load(StackPanel display)
    {
        _dialog = new Dialog(display.XamlRoot, title);
        display.DataContext = _board = new Board(
            (content) => _dialog.ConfirmAsync(content, "Yes", "No"));
    }

    public async void New()
    {
        var result = await _dialog.ConfirmAsync("Start a New Game?", "Yes", "No");
        if (result)
            _board.New();
    }
}