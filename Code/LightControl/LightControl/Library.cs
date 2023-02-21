using Comentsys.Toolkit.WindowsAppSdk;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace LightControl;

public class Light : Grid, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    // Light Properties
    public static readonly DependencyProperty ForegroundProperty =
    DependencyProperty.Register("Foreground", typeof(Brush),
    typeof(Light), new PropertyMetadata(new SolidColorBrush(Colors.Black)));

    public static readonly DependencyProperty OffProperty =
    DependencyProperty.Register("Off", typeof(Visibility),
    typeof(Light), new PropertyMetadata(Visibility.Collapsed));

    public Brush Foreground
    {
        get { return (Brush)GetValue(ForegroundProperty); }
        set
        {
            SetValue(ForegroundProperty, value);
            OnPropertyChanged();
        }
    }

    public Visibility Off
    {
        get { return (Visibility)GetValue(OffProperty); }
        set
        {
            SetValue(OffProperty, value);
            OnPropertyChanged();
        }
    }

    public bool IsOn
    {
        get { return Off == Visibility.Collapsed; }
        set
        {
            Off = value ? Visibility.Collapsed : Visibility.Visible;
            OnPropertyChanged();
        }
    }

    // Light Constructor
    public Light()
    {
        Margin = new Thickness(5);
        Piece element = new()
        {
            Stroke = new SolidColorBrush(Colors.Black)
        };
        element.SetBinding(Piece.FillProperty, new Binding()
        {
            Path = new PropertyPath(nameof(Foreground)),
            Mode = BindingMode.TwoWay,
            Source = this,
        });
        Piece overlay = new()
        {
            Stroke = new SolidColorBrush(Colors.Black),
            Fill = new SolidColorBrush(Colors.Black),
            Opacity = 0.75
        };
        overlay.SetBinding(VisibilityProperty, new Binding()
        {
            Path = new PropertyPath(nameof(Off)),
            Mode = BindingMode.TwoWay,
            Source = this
        });
        Children.Add(element);
        Children.Add(overlay);
    }
}

public class Library
{
    // Library Members and Delay, Toggle & Load Methods
    private readonly Light _red = new()
    {
        Foreground = new SolidColorBrush(Colors.Red)
    };
    private readonly Light _orange = new()
    {
        Foreground = new SolidColorBrush(Colors.Orange)
    };
    private readonly Light _green = new()
    {
        Foreground = new SolidColorBrush(Colors.Green)
    };

    private static async Task Delay(int seconds = 2) =>
        await Task.Delay(seconds * 1000);

    private void Toggle(bool red, bool orange, bool green) =>
        (_red.IsOn, _orange.IsOn, _green.IsOn) = (red, orange, green);

    public void Load(StackPanel panel)
    {
        panel.Children.Add(_red);
        panel.Children.Add(_orange);
        panel.Children.Add(_green);
    }

    // Library Traffic & Reset Methods
    public async void Traffic()
    {
        Toggle(false, false, true);
        await Delay();
        Toggle(false, false, false);
        await Delay();
        Toggle(false, true, false);
        await Delay();
        Toggle(false, false, false);
        await Delay();
        Toggle(true, false, false);
        await Delay();
        Toggle(true, false, false);
        await Delay();
        Toggle(true, true, false);
        await Delay();
        Toggle(false, false, true);
        await Delay();
    }

    public void Reset() =>
        Toggle(true, true, true);
}
