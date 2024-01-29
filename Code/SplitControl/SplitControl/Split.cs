using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;

namespace SplitControl;

public enum Sources
{
    Value, Time, Date, TimeDate
}

public class Split : StackPanel
{
    // Constants, Members, Dependency Property & Property
    private const char space = ' ';
    private const string time = "HH mm ss";
    private const string date = "dd MM yyyy";
    private const string date_time = "HH mm ss  dd MM yyyy";
    private const string invalid_source = "Invalid argument";

    private string _value;
    private int _count;

    public static readonly DependencyProperty SourceProperty =
    DependencyProperty.Register(nameof(Source), typeof(Sources),
    typeof(Split), new PropertyMetadata(Sources.Time));

    public Sources Source
    {
        get { return (Sources)GetValue(SourceProperty); }
        set { SetValue(SourceProperty, value); }
    }

    // Set Element & Add Element Methods
    private void SetElement(string name, char glyph)
    {
        var element = Children.Cast<FrameworkElement>()
        .FirstOrDefault(f => (string)f.Tag == name);
        if (element is Flap flap)
        {
            flap.Value = glyph.ToString();
        }
    }

    private void AddElement(string name)
    {
        FrameworkElement element = name == null
            ? new Canvas
            {
                Width = 5
            }
            : new Flap()
            {
                Tag = name
            };
        Children.Add(element);
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
            foreach (int item in list)
            {
                AddElement((array[item] == space)
                ? null : item.ToString());
            }
            _count = length;
        }
        foreach (int item in list)
        {
            SetElement(item.ToString(), array[item]);
        }
    }

    public string Value
    {
        get { return _value; }
        set { _value = value; AddLayout(); }
    }

    // Constructor
    public Split()
    {
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