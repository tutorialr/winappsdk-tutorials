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
using static System.Net.WebRequestMethods;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GaugeControl
{
    public sealed partial class Gauge : UserControl
    {
        public Gauge()
        {
            this.InitializeComponent();
        }

        private Rectangle _needle;
        private double _diameter = 0;

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
            var inner = _diameter;
            var markers = new Canvas()
            {
                Width = inner,
                Height = inner
            };
            for (int i = 0; i < 51; i++)
            {
                var marker = new Rectangle()
                {
                    Fill = Foreground
                };
                if ((i % 5) == 0)
                {
                    marker.Width = 4;
                    marker.Height = 16;
                    marker.RenderTransform = Transform(i * 6, -(marker.Width / 2),
                    -(marker.Height * 2 + 4.5 - thickness / 2 - inner / 2 - 16));
                }
                else
                {
                    marker.Width = 2;
                    marker.Height = 8;
                    marker.RenderTransform = Transform(i * 6, -(marker.Width / 2),
                    -(marker.Height * 2 + 12.75 - thickness / 2 - inner / 2 - 16));
                }
                markers.Children.Add(marker);
            }
            markers.RenderTransform = new RotateTransform()
            {
                Angle = 30,
                CenterX = _diameter / 2,
                CenterY = _diameter / 2
            };
            canvas.Children.Add(markers);
        }

        // Layout, Indicator & Load Methods
        private void Layout(Canvas canvas)
        {
            canvas.Children.Clear();
            _diameter = canvas.Width;
            var face = new Ellipse()
            {
                Height = _diameter,
                Width = _diameter,
                Fill = Fill
            };
            canvas.Children.Add(face);
            Markers(canvas, face.StrokeThickness);
            _needle = new Rectangle()
            {
                Width = Needle,
                Height = (int)_diameter / 2 - 30,
                Fill = Foreground
            };
            canvas.Children.Add(_needle);
            var middle = new Ellipse()
            {
                Height = _diameter / 10,
                Width = _diameter / 10,
                Fill = Foreground
            };
            Canvas.SetLeft(middle, (_diameter - middle.ActualWidth) / 2);
            Canvas.SetTop(middle, (_diameter - middle.ActualHeight) / 2);
            canvas.Children.Add(middle);
        }

        private void Indicator(int value)
        {
            Layout(Display);
            var percentage = value / (double)Maximum * 100;
            var position = (percentage / 2) + 5;
            _needle.RenderTransform = Transform(position * 6,
            -Needle / 2, 4.25);
        }

        private void Load(object sender, RoutedEventArgs e) =>
            Indicator(Value);

        // Dependency Properties
        public static readonly DependencyProperty FillProperty =
        DependencyProperty.Register(nameof(Fill), typeof(Brush),
        typeof(Gauge), null);

        public static readonly DependencyProperty NeedleProperty =
        DependencyProperty.Register(nameof(Needle), typeof(int), typeof(Gauge),
        new PropertyMetadata(2));

        public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(Gauge),
        new PropertyMetadata(25));

        public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(int), typeof(Gauge),
        new PropertyMetadata(0));

        public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(int), typeof(Gauge),
        new PropertyMetadata(100));

        // Properties
        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        public int Needle
        {
            get { return (int)GetValue(NeedleProperty); }
            set
            {
                SetValue(NeedleProperty, value);
                Indicator(Value);
            }
        }

        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set
            {
                if (value >= Minimum && value <= Maximum)
                {
                    SetValue(ValueProperty, value);
                    Indicator(value);
                }
            }
        }

        public int Minimum
        {
            get { return (int)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public int Maximum
        {
            get { return (int)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }
    }
}
