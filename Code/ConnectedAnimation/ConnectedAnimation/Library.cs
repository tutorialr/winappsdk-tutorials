using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Linq;
internal static class Library
{
    private const string animate_back = "AnimateBack";
    private const string animate_next = "AnimateNext";
    public static Frame Frame { get; set; }
    public static string Current { get; set; }

    public static void Back(ListView listview)
    {
        var rectangle = listview.Items
        .SingleOrDefault(f => (f as Rectangle)
        .Tag.Equals(Current)) as Rectangle;
        var animation = ConnectedAnimationService
        .GetForCurrentView()
        .GetAnimation(animate_back);
        animation?.TryStart(rectangle);
    }

    public static Brush Next(object selected)
    {
        var rectangle = selected as Rectangle;
        Current = rectangle.Tag as string;
        ConnectedAnimationService
        .GetForCurrentView()
        .PrepareToAnimate(animate_next, rectangle);
        return rectangle.Fill;
    }

    // Other Methods
    public static void From(Rectangle from)
    {
        ConnectedAnimationService
        .GetForCurrentView()
        .PrepareToAnimate(animate_back, from);
    }
    public static void Loaded(Rectangle rectangle)
    {
        var animation = ConnectedAnimationService
        .GetForCurrentView()
        .GetAnimation(animate_next);
        rectangle.Opacity = 1;
        animation?.TryStart(rectangle);
    }
    public static void Navigate(Type page, object parameter)
    {
        Frame.Navigate(page, Next(parameter));
    }
    public static Brush GetBrush(object parameter)
    {
        return parameter as SolidColorBrush;
    }

}
