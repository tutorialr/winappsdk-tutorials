using Comentsys.Assets.FluentEmoji;
using Comentsys.Toolkit.WindowsAppSdk;
using Microsoft.UI;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;

namespace LuckyRacer;

public enum State
{
    Select, Ready, Started, Finished
}

public class Racer
{
    public int Index { get; set; }
    public TimeSpan Time { get; set; }

    public Racer(int index) =>
        Index = index;

    public Racer(int index, TimeSpan time) =>
        (Index, Time) = (index, time);
}

public class Library
{
    // Constants, Variables & Choose Method
    private const string title = "Lucky Racer";
    private const int image_size = 72;
    private const int size = 400;

    private readonly Random _random = new((int)DateTime.UtcNow.Ticks);

    private Dialog _dialog;
    private Grid _grid;

    private bool _finish;
    private int _count;
    private State _state;
    private Racer _winner;
    private Racer _select;
    private List<Image> _images;
    private ImageSource[] _sources;

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

    // Get Finish, Get Racer, Set Sources & Get Image

    private async Task<ImageSource> GetFinishAsync() =>
        await FlatFluentEmoji.Get(FluentEmojiType.ChequeredFlag)
        .AsImageSourceAsync();

    private async Task<ImageSource> GetRacerAsync(Color main, Color trim) =>
        await FlatFluentEmoji.Get(FluentEmojiType.RacingCar,
        new[]
        {
        Color.FromArgb(255, 248, 49, 47).AsDrawingColor(),
        Color.FromArgb(255, 202, 11, 74).AsDrawingColor()
        },
        new[]
        {
        main.AsDrawingColor(),
        trim.AsDrawingColor()
        }).AsImageSourceAsync();

    private async Task SetSourcesAsync() =>
    _sources ??= (new ImageSource[]
    {
    await GetFinishAsync(),
    await GetRacerAsync(Colors.Red, Colors.DarkRed),
    await GetRacerAsync(Colors.Blue, Colors.DarkBlue),
    await GetRacerAsync(Colors.Green, Colors.DarkGreen),
    await GetRacerAsync(Colors.Goldenrod, Colors.DarkGoldenrod)
    });

    private Image GetImage(ImageSource source) =>
        new()
        {
            Height = image_size,
            Width = image_size,
            Source = source
        };

    // Content, Move & Start
    private StackPanel Content(string text, int index)
    {
        var panel = new StackPanel()
        {
            Orientation = Orientation.Vertical,
        };
        panel.Children.Add(new TextBlock()
        {
            Text = text
        });
        panel.Children.Add(GetImage(_sources[index]));
        return panel;
    }

    private void Move(Image image, double from, double to, TimeSpan duration)
    {
        var animation = new DoubleAnimation()
        {
            To = to,
            From = from,
            Duration = duration,
            EasingFunction = new ExponentialEase()
            {
                EasingMode = EasingMode.EaseIn
            }
        };
        var storyboard = new Storyboard();
        Storyboard.SetTargetProperty(animation, "(Canvas.Left)");
        Storyboard.SetTarget(animation, image);
        storyboard.Completed += (object sender, object e) =>
            Progress(sender as Storyboard);
        storyboard.Children.Add(animation);
        storyboard.Begin();
    }

    private void Start()
    {
        _count = 0;
        _finish = false;
        _state = State.Select;
    }

    // Finish & Progress
    private async void Finish()
    {
        if (_state == State.Finished)
        {
            var message = _select.Index == _winner.Index ?
                $"You Won in {_winner.Time}!" :
                $"You Lost! Winning Car";
            var content = Content(message, _winner.Index);
            await _dialog.ConfirmAsync(content);
            if (_finish)
            {
                foreach (var image in _images)
                {
                    Move(image, 0, size - image_size,
                        TimeSpan.FromSeconds(1));
                }
                _finish = false;
            }
            Start();
        }
    }

    private void Progress(Storyboard storyboard)
    {
        if (_state == State.Started)
        {
            var duration = storyboard.GetCurrentTime();
            var racer = _images.First(w => (w.Tag as Racer)
                .Time == duration).Tag as Racer;
            _count++;
            if (_count == 1)
                _winner = new Racer(racer.Index, duration);
            if (_count == _images.Count)
            {
                _state = State.Finished;
                Finish();
            }
            _finish = true;
        }
    }

    // Race, Ready & Select
    private void Race()
    {
        if (_state == State.Ready)
        {
            var index = 0;
            var times = Choose(5, 15, _sources.Length - 1);
            foreach (var image in _images)
            {
                var racer = image.Tag as Racer;
                racer.Time = TimeSpan.FromSeconds(times[index]);
                Move(image, size - image_size, 0, racer.Time);
                index++;
            }
            _state = State.Started;
        }
    }

    private async void Ready()
    {
        if (_state == State.Ready)
        {
            var content = Content("Selected to Win", _select.Index);
            var result = await _dialog.ConfirmAsync(
                content, "Race", "Cancel");
            if (result)
                Race();
            else
                _state = State.Select;
        }
    }

    private void Select(Image image)
    {
        if (_state == State.Select)
        {
            var racer = image.Tag as Racer;
            _select = racer;
            _state = State.Ready;
        }
        Ready();
    }

    // Add Racer & Add Finish

    private void AddRacer(Grid grid, int row)
    {
        grid.RowDefinitions.Add(new RowDefinition());
        var racer = GetImage(_sources[row]);
        racer.Tag = new Racer(row);
        racer.Tapped += (object sender, TappedRoutedEventArgs e) =>
            Select(sender as Image);
        Canvas.SetLeft(racer, size - image_size);
        _images.Add(racer);
        var canvas = new Canvas()
        {
            Height = image_size,
            Width = size
        };
        canvas.Children.Add(racer);
        Grid.SetRow(canvas, row - 1);
        grid.Children.Add(canvas);
    }

    private void AddFinish(Grid grid, int row)
    {
        grid.RowDefinitions.Add(new RowDefinition());
        var finish = GetImage(_sources.First());
        Grid.SetRow(finish, row - 1);
        grid.Children.Add(finish);
    }

    // Layout & New
    private void Layout(Grid grid)
    {
        _images = new();
        grid.Children.Clear();
        var panel = new StackPanel()
        {
            Orientation = Orientation.Horizontal
        };
        _grid = new Grid()
        {
            Height = size,
            Width = size
        };
        var finish = new Grid();
        for (int row = 1; row < _sources.Length; row++)
        {
            AddRacer(_grid, row);
            AddFinish(finish, row);
        }
        panel.Children.Add(finish);
        panel.Children.Add(_grid);
        grid.Children.Add(panel);
    }

    public async void New(Grid grid)
    {
        _dialog = new Dialog(grid.XamlRoot, title);
        await SetSourcesAsync();
        Layout(grid);
        Start();
    }
}