using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Reflection.Metadata;
using Windows.Devices.Radios;

namespace StickControl;

public class Stick : Grid
{
    // Members & Event
    private bool _capture;
    private Ellipse _knob;
    private Ellipse _face;
    private double x = 0;
    private double y = 0;
    private double _m = 0;
    private double _res = 0;
    private double _width = 0;
    private double _height = 0;
    private double _alpha = 0;
    private double _alphaM = 0;
    private double _centreX = 0;
    private double _centreY = 0;
    private double _distance = 0;
    private double _oldAlphaM = -999.0;
    private double _oldDistance = -999.0;

    public delegate void ValueChangedEventHandler(
        object sender, double angle, double ratio);
    public event ValueChangedEventHandler ValueChanged;

    // Dependency Properties
    public static readonly DependencyProperty RadiusProperty =
    DependencyProperty.Register(nameof(Radius), typeof(int),
    typeof(Stick), new PropertyMetadata(100));

    public static readonly DependencyProperty KnobProperty =
    DependencyProperty.Register(nameof(Knob), typeof(Brush),
    typeof(Stick), new PropertyMetadata(new SolidColorBrush(Colors.Red)));

    public static readonly DependencyProperty FaceProperty =
    DependencyProperty.Register(nameof(Face), typeof(Brush),
    typeof(Stick), new PropertyMetadata(new SolidColorBrush(Colors.Black)));

    public static readonly DependencyProperty AngleProperty =
    DependencyProperty.Register(nameof(Angle), typeof(double),
    typeof(Stick), null);

    public static readonly DependencyProperty RatioProperty =
    DependencyProperty.Register(nameof(Ratio), typeof(double),
    typeof(Stick), null);

    public static readonly DependencyProperty SensitivityProperty =
    DependencyProperty.Register(nameof(Sensitivity), typeof(double),
    typeof(Stick), null);

    // Properties
    public int Radius
    {
        get { return (int)GetValue(RadiusProperty); }
        set { SetValue(RadiusProperty, value); Load(); }
    }

    public Brush Knob
    {
        get { return (Brush)GetValue(KnobProperty); }
        set { SetValue(KnobProperty, value); }
    }

    public Brush Face
    {
        get { return (Brush)GetValue(FaceProperty); }
        set { SetValue(FaceProperty, value); }
    }

    public double Angle
    {
        get { return (double)GetValue(AngleProperty); }
        set { SetValue(AngleProperty, value); }
    }

    public double Ratio
    {
        get { return (double)GetValue(RatioProperty); }
        set { SetValue(RatioProperty, value); }
    }

    public double Sensitivity
    {
        get { return (double)GetValue(SensitivityProperty); }
        set { SetValue(SensitivityProperty, value); }
    }

    // ToRadians, ToDegrees, SetMiddle, & GetCircle Methods
    private static double ToRadians(double angle) =>
    Math.PI * angle / 180.0;

    private static double ToDegrees(double angle) =>
        angle * (180.0 / Math.PI);

    private void SetMiddle()
    {
        _capture = false;
        Canvas.SetLeft(_knob, (Width - _width) / 2);
        Canvas.SetTop(_knob, (Height - _height) / 2);
        _centreX = Width / 2;
        _centreY = Height / 2;
    }

    private Ellipse GetCircle(double dimension, string path)
    {
        var circle = new Ellipse()
        {
            Height = dimension,
            Width = dimension
        };
        circle.SetBinding(Shape.FillProperty, new Binding()
        {
            Source = this,
            Path = new PropertyPath(path),
            Mode = BindingMode.TwoWay
        });
        return circle;
    }

    // Move Method
    private void Move(PointerRoutedEventArgs e)
    {
        x = e.GetCurrentPoint(this).Position.X;
        y = e.GetCurrentPoint(this).Position.Y;
        _res = Math.Sqrt((x - _centreX) *
            (x - _centreX) + (y - _centreY) * (y - _centreY));
        _m = (y - _centreY) / (x - _centreX);
        _alpha = ToDegrees(Math.Atan(_m) + Math.PI / 2);
        if (x < _centreX)
            _alpha += 180.0;
        else if (x == _centreX && y <= _centreY)
            _alpha = 0.0;
        else if (x == _centreX)
            _alpha = 180.0;
        if (_res > Radius)
        {
            x = _centreX + Math.Cos(ToRadians(_alpha) - Math.PI / 2) * Radius;
            y = _centreY + Math.Sin(ToRadians(_alpha) - Math.PI / 2) * Radius
                * ((_alpha % 180.0 == 0.0) ? -1.0 : 1.0);
            _res = Radius;
        }
        if (Math.Abs(_alpha - _alphaM) >= Sensitivity ||
            Math.Abs(_distance * Radius - _res) >= Sensitivity)
        {
            _alphaM = _alpha;
            _distance = _res / Radius;
        }
        if (_oldAlphaM != _alphaM ||
            _oldDistance != _distance)
        {
            Angle = _alphaM;
            Ratio = _distance;
            _oldAlphaM = _alphaM;
            _oldDistance = _distance;
            ValueChanged?.Invoke(this, Angle, Ratio);
        }
        Canvas.SetLeft(_knob, x - _width / 2);
        Canvas.SetTop(_knob, y - _height / 2);
    }

    // Layout & Load Methods and Constructor
    private void Layout()
    {
        _knob = GetCircle(Radius, "Knob");
        _face = GetCircle(Radius * 2, "Face");
        _height = _knob.ActualHeight;
        _width = _knob.ActualWidth;
        Width = _width + Radius * 2;
        Height = _height + Radius * 2;
        SetMiddle();
        PointerExited -= null;
        PointerExited += (object sender, PointerRoutedEventArgs e) =>
            SetMiddle();
        _knob.PointerReleased += (object sender, PointerRoutedEventArgs e) =>
            SetMiddle();
        _knob.PointerPressed += (object sender, PointerRoutedEventArgs e) =>
            _capture = true;
        _knob.PointerMoved += (object sender, PointerRoutedEventArgs e) =>
        {
            if (_capture)
                Move(e);
        };
        _knob.PointerExited += (object sender, PointerRoutedEventArgs e) =>
            SetMiddle();
    }

    private void Load()
    {
        Layout();
        Children.Clear();
        Children.Add(_face);
        var canvas = new Canvas()
        {
            Width = Width,
            Height = Height
        };
        canvas.Children.Add(_knob);
        Children.Add(canvas);
    }

    public Stick() => Load();
}