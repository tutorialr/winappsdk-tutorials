using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Linq;

namespace SegmentControl;

public enum Sources
{
    Value, Time, Date, TimeDate
}

public class Segment : StackPanel
{
    // Constants & Members
    private readonly byte[][] table =
    {
        // a, b, c, d, e, f, g
        new byte[] { 1, 1, 1, 1, 1, 1, 0 }, // 0
        new byte[] { 0, 1, 1, 0, 0, 0, 0 }, // 1
        new byte[] { 1, 1, 0, 1, 1, 0, 1 }, // 2
        new byte[] { 1, 1, 1, 1, 0, 0, 1 }, // 3
        new byte[] { 0, 1, 1, 0, 0, 1, 1 }, // 4
        new byte[] { 1, 0, 1, 1, 0, 1, 1 }, // 5
        new byte[] { 1, 0, 1, 1, 1, 1, 1 }, // 6
        new byte[] { 1, 1, 1, 0, 0, 0, 0 }, // 7
        new byte[] { 1, 1, 1, 1, 1, 1, 1 }, // 8
        new byte[] { 1, 1, 1, 0, 0, 1, 1 }, // 9
        new byte[] { 0, 0, 0, 0, 0, 0, 1 }, // Minus
        new byte[] { 0, 0, 0, 0, 0, 0, 0 }, // Colon
        new byte[] { 0, 0, 0, 0, 0, 0, 0 } // Space
    };
    private const int width = 5;
    private const int height = 25;
    private const int spacing = 10;
    private const int minus_pos = 10;
    private const int colon_pos = 11;
    private const int space_pos = 12;
    private const string minus = "-";
    private const string colon = ":";
    private const string space = " ";
    private const string time = "HH:mm:ss";
    private const string date = "dd-MM-yyyy";
    private const string date_time = "HH:mm:ss  dd-MM-yyyy";
    private const string invalid_source = "Invalid argument";

    private int _count;
    private string _value;

    // Dependency Properties & Properties
    public static readonly DependencyProperty SourceProperty =
    DependencyProperty.Register(nameof(Source), typeof(Sources),
    typeof(Segment), new PropertyMetadata(Sources.Time));

    public static readonly DependencyProperty ForegroundProperty =
    DependencyProperty.Register(nameof(Foreground), typeof(Brush),
    typeof(Segment), new PropertyMetadata(new SolidColorBrush(Colors.Black)));

    public Sources Source
    {
        get { return (Sources)GetValue(SourceProperty); }
        set { SetValue(SourceProperty, value); }
    }

    public Brush Foreground
    {
        get { return (Brush)GetValue(ForegroundProperty); }
        set { SetValue(ForegroundProperty, value); }
    }

    // Get Element & Add Element Methods
    private static Rectangle GetElement(Canvas segment, string name) =>
        segment.Children.Cast<Rectangle>()
        .FirstOrDefault(f => (string)f.Tag == name);

    private Rectangle AddElement(string name,
        int left, int top, int width, int height)
    {
        var element = new Rectangle()
        {
            Tag = name,
            Opacity = 0,
            RadiusX = 2,
            RadiusY = 2,
            Width = width,
            Height = height,
            Margin = new Thickness(2)
        };
        element.SetBinding(Shape.FillProperty, new Binding()
        {
            Path = new PropertyPath(nameof(Foreground)),
            Mode = BindingMode.TwoWay,
            Source = this
        });
        Canvas.SetLeft(element, left);
        Canvas.SetTop(element, top);
        return element;
    }

    // Set Segment & Add Segment Methods
    private void SetSegment(string name, int digit)
    {
        var segment = Children.Cast<Canvas>()
            .FirstOrDefault(f => (string)f.Tag == name);
        byte[] values = table[digit];
        GetElement(segment, $"{name}.a").Opacity = values[0];
        GetElement(segment, $"{name}.b").Opacity = values[1];
        GetElement(segment, $"{name}.c").Opacity = values[2];
        GetElement(segment, $"{name}.d").Opacity = values[3];
        GetElement(segment, $"{name}.e").Opacity = values[4];
        GetElement(segment, $"{name}.f").Opacity = values[5];
        GetElement(segment, $"{name}.g").Opacity = values[6];
        GetElement(segment, $"{name}.h").Opacity = digit == colon_pos ? 1 : 0;
        GetElement(segment, $"{name}.i").Opacity = digit == colon_pos ? 1 : 0;
    }

    private void AddSegment(string name)
    {
        var segment = new Canvas()
        {
            Tag = name,
            Width = 25,
            Height = 50,
            Margin = new Thickness(2)
        };
        segment.Children.Add(AddElement($"{name}.a",
            width, 0, height, width));
        segment.Children.Add(AddElement($"{name}.h",
            width * 3, width * 3, width, width));
        segment.Children.Add(AddElement($"{name}.f",
            0, width, width, height));
        segment.Children.Add(AddElement($"{name}.b",
            height + width, width, width, height));
        segment.Children.Add(AddElement($"{name}.g",
            width, height + width, height, width));
        segment.Children.Add(AddElement($"{name}.e",
            0, height + (width * 2), width, height));
        segment.Children.Add(AddElement($"{name}.c",
            height + width, height + width + width, width, height));
        segment.Children.Add(AddElement($"{name}.i",
            width * 3, height + (width * 4), width, width));
        segment.Children.Add(AddElement($"{name}.d",
            width, (height * 2) + (width * 2), height, width));
        Children.Add(segment);
    }

    // Add Layout Method & Value Property
    private void AddLayout()
    {
        var array = _value.ToCharArray();
        var length = array.Length;
        var list = Enumerable.Range(0, length);
        if (_count != length)
        {
            Children.Clear();
            foreach (var item in list)
            {
                AddSegment(item.ToString());
            }
            _count = length;
        }
        foreach (int item in list)
        {
            var value = array[item].ToString();
            var digit = value switch
            {
                minus => minus_pos,
                colon => colon_pos,
                space => space_pos,
                _ => int.Parse(value),
            };
            SetSegment(item.ToString(), digit);
        }
    }

    public string Value
    {
        get { return _value; }
        set { _value = value; AddLayout(); }
    }

    // Constructor
    public Segment()
    {
        Spacing = spacing;
        Orientation = Orientation.Horizontal;
        var timer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromMilliseconds(250)
        };
        timer.Tick += (object s, object args) =>
        {
            if (Source != Sources.Value)
            {
                var format = Source switch
                {
                    Sources.Time => time,
                    Sources.Date => date,
                    Sources.TimeDate => date_time,
                    _ => throw new ArgumentException(invalid_source)
                };
                Value = DateTime.Now.ToString(format);
            }
        };
        timer.Start();
    }
}