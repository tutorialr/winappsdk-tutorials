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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DialControl
{
    public sealed partial class Dial : UserControl
    {
        public Dial()
        {
            this.InitializeComponent();
        }

        private bool _hasCapture = false;

        // Dependancy Properties
        public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double),
        typeof(Dial), null);

        public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(double),
        typeof(Dial), null);

        public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double),
        typeof(Dial), null);

        public static readonly DependencyProperty KnobProperty =
        DependencyProperty.Register(nameof(Knob), typeof(UIElement),
        typeof(Dial), null);

        public static readonly DependencyProperty FaceProperty =
        DependencyProperty.Register(nameof(Face), typeof(UIElement),
        typeof(Dial), null);

        // Properties
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public double Minimum
        {
            get { return (double)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public double Maximum
        {
            get { return (double)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public UIElement Knob
        {
            get { return (UIElement)GetValue(KnobProperty); }
            set { SetValue(KnobProperty, value); }
        }

        public UIElement Face
        {
            get { return (UIElement)GetValue(FaceProperty); }
            set { SetValue(FaceProperty, value); }
        }

        // GetRotation, GetAngle & SetPosition Methods
        private double GetRotation(double width, double height, Point point)
        {
            double radius = width / 2;
            Point centre = new(radius, height / 2);
            double triangleTop = Math.Sqrt(Math.Pow(point.X - centre.X, 2)
                + Math.Pow(centre.Y - point.Y, 2));
            double triangleHeight = (point.Y > centre.Y) ?
                point.Y - centre.Y : centre.Y - point.Y;
            return triangleHeight * Math.Sin(90) / triangleTop * 100;
        }

        private double GetAngle(Point point)
        {
            double diameter = DialGrid.ActualWidth;
            double height = DialGrid.ActualHeight;
            double radius = diameter / 2;
            double rotation = GetRotation(diameter, height, point);
            if ((point.X > radius) && (point.Y <= radius))
            {
                rotation = 90.0 + (90.0 - rotation);
            }
            else if ((point.X > radius) && (point.Y > radius))
            {
                rotation = 180.0 + rotation;
            }
            else if ((point.X < radius) && (point.Y > radius))
            {
                rotation = 270.0 + (90.0 - rotation);
            }
            return rotation;
        }

        private void SetPosition(double rotation)
        {
            if (Minimum >= 0 && Maximum > 0 && Minimum < 360 && Maximum <= 360)
            {
                if (rotation < Minimum) { rotation = Minimum; }
                if (rotation > Maximum) { rotation = Maximum; }
            }
            DialValue.Angle = rotation;
            Value = rotation;
        }

        // Load Method
        private void Load(object sender, RoutedEventArgs e)
        {
            if (Minimum > 0 && Minimum < 360)
                SetPosition(Minimum);
            DialGrid.PointerReleased += (object sender, PointerRoutedEventArgs e) =>
                _hasCapture = false;
            DialGrid.PointerPressed += (object sender, PointerRoutedEventArgs e) =>
            {
                _hasCapture = true;
                SetPosition(GetAngle(e.GetCurrentPoint(DialGrid).Position));
            };
            DialGrid.PointerMoved += (object sender, PointerRoutedEventArgs e) =>
            {
                if (_hasCapture)
                    SetPosition(GetAngle(e.GetCurrentPoint(DialGrid).Position));
            };
            DialGrid.PointerExited += (object sender, PointerRoutedEventArgs e) =>
                _hasCapture = false;
        }
    }
}
