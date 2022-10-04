using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Shapes;
using System;
internal class Library
{
    public static Frame Frame { get; set; }
    public static string Option { get; set; }
    public static void Navigate(Type page, object parameter = null)
    {
        NavigationTransitionInfo transitionInfo = Option switch
        {
            "Entrance" => new EntranceNavigationTransitionInfo(),
            "Drill In" => new DrillInNavigationTransitionInfo(),
            "Slide from Right" => new SlideNavigationTransitionInfo()
            {
                Effect = SlideNavigationTransitionEffect.FromRight
            },
            "Slide from Left" => new SlideNavigationTransitionInfo()
            {
                Effect = SlideNavigationTransitionEffect.FromLeft
            },
            "Supress" => new SuppressNavigationTransitionInfo(),
            _ => null,
        };
        if (Frame.BackStackDepth > 0)
        {
            Frame.BackStack.Clear();
        }
        Frame.Navigate(page, parameter, transitionInfo);
    }
    public static Brush GetFill(object parameter)
    {
        return (parameter as Rectangle).Fill;
    }
}