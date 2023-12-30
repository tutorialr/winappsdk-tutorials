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
using Microsoft.UI.Xaml.Shapes;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace ClockControl
{
    public sealed partial class Clock : UserControl
    {
        public Clock()
        {
            this.InitializeComponent();
        }

        private const int seconds_width = 2;
        private const int minutes_width = 4;
        private const int hours_width = 8;

        private readonly DispatcherTimer _timer = new();
        private readonly Canvas _markers = new();
        private readonly Canvas _face = new();
        private double _diameter;

        // Dependency Property & Properties
        public static readonly DependencyProperty FillProperty =
        DependencyProperty.Register(nameof(Fill), typeof(Brush),
        typeof(Clock), null);

        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        public bool IsRealTime { get; set; } = true;
        public bool ShowSeconds { get; set; } = true;
        public bool ShowMinutes { get; set; } = true;
        public bool ShowHours { get; set; } = true;
        public DateTime Time { get; set; } = DateTime.Now;

        public bool Enabled
        {
            get { return _timer.IsEnabled; }
            set
            {
                if (_timer.IsEnabled)
                {
                    _timer.Stop();
                }
                else
                {
                    _timer.Start();
                }
            }
        }

        // Transform Method
        private TransformGroup Transform(double angle, double x, double y)
        {
            var transformGroup = new TransformGroup();
            var firstTranslate = new TranslateTransform()
            {
                X = x,
                Y = y
            };
            transformGroup.Children.Add(firstTranslate);
            var rotateTransform = new RotateTransform()
            {
                Angle = angle
            };
            transformGroup.Children.Add(rotateTransform);
            var secondTranslate = new TranslateTransform()
            {
                X = _diameter / 2,
                Y = _diameter / 2
            };
            transformGroup.Children.Add(secondTranslate);
            return transformGroup;
        }

        // Markers Method
        private void Markers(Canvas canvas, double thickness)
        {
            double inner = _diameter - 15;
            _markers.Children.Clear();
            _markers.Width = inner;
            _markers.Height = inner;
            for (int i = 0; i < 60; i++)
            {
                var marker = new Rectangle
                {
                    Fill = Foreground
                };
                if ((i % 5) == 0)
                {
                    marker.Width = 3;
                    marker.Height = 8;
                    marker.RenderTransform = Transform(i * 6, -(marker.Width / 2),
                    -(marker.Height * 2 + 4.5 - thickness / 2 - inner / 2 - 6));
                }
                else
                {
                    marker.Width = 1;
                    marker.Height = 4;
                    marker.RenderTransform = Transform(i * 6, -(marker.Width / 2),
                    -(marker.Height * 2 + 12.75 - thickness / 2 - inner / 2 - 8));
                }
                _markers.Children.Add(marker);
            }
            canvas.Children.Add(_markers);
        }

        // Hand, GetHand, AddHand & RemoveHand Methods
        private Rectangle Hand(double width, double height) => new()
        {
            Width = width,
            Height = height,
            Fill = Fill,
            RadiusX = width / 2,
            RadiusY = width / 2,
            Name = width.ToString()
        };

        private Rectangle GetHand(int width) =>
            FindName(width.ToString()) as Rectangle;

        private Rectangle AddHand(int width, int height)
        {
            var hand = GetHand(width);
            if (hand == null)
            {
                hand = Hand(width, height);
                _face.Children.Add(hand);
            }
            return hand;
        }

        private void RemoveHand(int width)
        {
            var hand = GetHand(width);
            if (hand != null)
            {
                _face.Children.Remove(hand);
            }
        }

        // SecondHand, MinuteHand & HourHand Methods
        private void SecondHand(int seconds)
        {
            if (ShowSeconds)
            {
                var secondsHeight = (int)_diameter / 2 - 20;
                var secondsHand = AddHand(seconds_width, secondsHeight);
                secondsHand.RenderTransform = Transform(seconds * 6,
                    -seconds_width / 2, -secondsHeight + 4.25);
            }
            else
            {
                RemoveHand(seconds_width);
            }
        }

        private void MinuteHand(int minutes, int seconds)
        {
            if (ShowMinutes)
            {
                var minutesHeight = (int)_diameter / 2 - 40;
                var minutesHand = AddHand(minutes_width, minutesHeight);
                minutesHand.RenderTransform = Transform(6 * minutes + seconds / 10,
                    -minutes_width / 2, -minutesHeight + 4.25);
            }
            else
            {
                RemoveHand(minutes_width);
            }
        }

        private void HourHand(int hours, int minutes, int seconds)
        {
            if (ShowHours)
            {
                var hoursHeight = (int)_diameter / 2 - 60;
                var hoursHand = AddHand(hours_width, hoursHeight);
                hoursHand.RenderTransform = Transform(
                    30 * hours + minutes / 2 + seconds / 120,
                    -hours_width / 2, -hoursHeight + 4.25);
            }
            else
            {
                RemoveHand(hours_width);
            }
        }

        // Layout & Load Methods
        private void Layout(Canvas canvas)
        {
            canvas.Children.Clear();
            _diameter = canvas.Width;
            var rim = new Ellipse
            {
                Height = _diameter,
                Width = _diameter,
                Stroke = Fill,
                StrokeThickness = 20
            };
            canvas.Children.Add(rim);
            Markers(canvas, rim.StrokeThickness);
            _face.Width = _diameter;
            _face.Height = _diameter;
            canvas.Children.Add(_face);
        }

        private void Load(object sender, RoutedEventArgs e)
        {
            Layout(Display);
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (object s, object obj) =>
            {
                if (IsRealTime)
                {
                    Time = DateTime.Now;
                }
                SecondHand(Time.Second);
                MinuteHand(Time.Minute, Time.Second);
                HourHand(Time.Hour, Time.Minute, Time.Second);
            };
            _timer.Start();
        }
    }
}
