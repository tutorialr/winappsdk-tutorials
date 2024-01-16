using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace CarouselControl
{
    public sealed partial class Carousel : UserControl
    {
        public Carousel()
        {
            this.InitializeComponent();
        }

        private const double speed = 0.0125;
        private const double perspective = 55;

        private readonly Storyboard _animation = new();
        private readonly List<BitmapImage> _list = new();
        private readonly Point _radius = new() { X = -20, Y = 200 };

        private Point _position;
        private double _distance;

        // Rotate Method
        private void Rotate()
        {
            foreach (var item in Display.Children.Cast<Image>())
            {
                double angle = (double)item.Tag;
                angle -= speed;
                item.Tag = angle;
                _position.X = Math.Cos(angle) * _radius.X;
                _position.Y = Math.Sin(angle) * _radius.Y;
                Canvas.SetLeft(item, _position.X - (item.Width - perspective));
                Canvas.SetTop(item, _position.Y);
                if (_radius.X >= 0)
                {
                    _distance = 1 * (1 - (_position.X / perspective));
                    Canvas.SetZIndex(item, -(int)_position.X);
                }
                else
                {
                    _distance = 1 / (1 - (_position.X / perspective));
                    Canvas.SetZIndex(item, (int)_position.X);
                }
                item.Opacity = ((ScaleTransform)item.RenderTransform).ScaleX =
                    ((ScaleTransform)item.RenderTransform).ScaleY = _distance;
            }
            _animation.Begin();
        }

        // Layout Method
        private void Layout(Canvas display)
        {
            display.Children.Clear();
            for (int index = 0; index < _list.Count; index++)
            {
                _distance = 1 / (1 - (_position.X / perspective));
                var item = new Image
                {
                    Width = 150,
                    Source = _list[index],
                    Tag = index * (Math.PI * 2 / _list.Count),
                    RenderTransform = new ScaleTransform()
                };
                _position.X = Math.Cos((double)item.Tag) * _radius.X;
                _position.Y = Math.Sin((double)item.Tag) * _radius.Y;
                Canvas.SetLeft(item, _position.X - (item.Width - perspective));
                Canvas.SetTop(item, _position.Y);
                item.Opacity = ((ScaleTransform)item.RenderTransform).ScaleX =
                    ((ScaleTransform)item.RenderTransform).ScaleY = _distance;
                display.Children.Add(item);
            }
        }

        // Add, Remove, New & Load Methods
        public void Add(BitmapImage image)
        {
            _list.Add(image);
            Layout(Display);
        }

        public void Remove()
        {
            if (_list.Any())
            {
                _list.Remove(_list.Last());
                Layout(Display);
            }
        }

        public void New()
        {
            _list.Clear();
            Layout(Display);
        }

        private void Load(object sender, RoutedEventArgs e)
        {
            _animation.Completed += (object s, object obj) =>
                Rotate();
            _animation.Begin();
        }
    }
}
