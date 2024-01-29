using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MatrixControl;

public enum Sources
{
    Value, Time, Date, TimeDate
}

public class Matrix : StackPanel
{
    private readonly byte[][] table =
    {
        // Table 0 - 4
        new byte[] {
        0,0,0,0,0,0,0,0,
        0,1,1,1,1,1,1,0,
        0,1,1,0,0,1,1,0,
        0,1,1,0,0,1,1,0,
        0,1,1,0,0,1,1,0,
        0,1,1,1,1,1,1,0,
        0,0,0,0,0,0,0,0
        }, // 0
        new byte[] {
        0,0,0,0,0,0,0,0,
        0,0,0,1,1,0,0,0,
        0,1,1,1,1,0,0,0,
        0,0,0,1,1,0,0,0,
        0,0,0,1,1,0,0,0,
        0,0,0,1,1,0,0,0,
        0,0,0,0,0,0,0,0
        }, // 1 
        new byte[] {
        0,0,0,0,0,0,0,0,
        0,1,1,1,1,1,1,0,
        0,0,0,0,0,1,1,0,
        0,1,1,1,1,1,1,0,
        0,1,1,0,0,0,0,0,
        0,1,1,1,1,1,1,0,
        0,0,0,0,0,0,0,0
        }, // 2
        new byte[] {
        0,0,0,0,0,0,0,0,
        0,1,1,1,1,1,1,0,
        0,0,0,0,0,1,1,0,
        0,1,1,1,1,1,1,0,
        0,0,0,0,0,1,1,0,
        0,1,1,1,1,1,1,0,
        0,0,0,0,0,0,0,0
        }, // 3
        new byte[] {
        0,0,0,0,0,0,0,0,
        0,1,1,0,0,1,1,0,
        0,1,1,0,0,1,1,0,
        0,1,1,1,1,1,1,0,
        0,0,0,0,0,1,1,0,
        0,0,0,0,0,1,1,0,
        0,0,0,0,0,0,0,0
        }, // 4
        // Table 5 - 9
        new byte[] {
        0,0,0,0,0,0,0,0,
        0,1,1,1,1,1,1,0,
        0,1,1,0,0,0,0,0,
        0,1,1,1,1,1,1,0,
        0,0,0,0,0,1,1,0,
        0,1,1,1,1,1,1,0,
        0,0,0,0,0,0,0,0
        }, // 5
        new byte[] {
        0,0,0,0,0,0,0,0,
        0,1,1,1,1,1,1,0,
        0,1,1,0,0,0,0,0,
        0,1,1,1,1,1,1,0,
        0,1,1,0,0,1,1,0,
        0,1,1,1,1,1,1,0,
        0,0,0,0,0,0,0,0
        }, // 6
        new byte[] {
        0,0,0,0,0,0,0,0,
        0,1,1,1,1,1,1,0,
        0,0,0,0,0,1,1,0,
        0,0,0,0,0,1,1,0,
        0,0,0,0,0,1,1,0,
        0,0,0,0,0,1,1,0,
        0,0,0,0,0,0,0,0
        }, // 7
        new byte[] {
        0,0,0,0,0,0,0,0,
        0,1,1,1,1,1,1,0,
        0,1,1,0,0,1,1,0,
        0,1,1,1,1,1,1,0,
        0,1,1,0,0,1,1,0,
        0,1,1,1,1,1,1,0,
        0,0,0,0,0,0,0,0
        }, // 8
        new byte[] {
        0,0,0,0,0,0,0,0,
        0,1,1,1,1,1,1,0,
        0,1,1,0,0,1,1,0,
        0,1,1,1,1,1,1,0,
        0,0,0,0,0,1,1,0,
        0,1,1,1,1,1,1,0,
        0,0,0,0,0,0,0,0
        }, // 9
        // Table Minus, Slash, Colon & Space
        new byte[] {
        0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,
        0,1,1,1,1,1,1,0,
        0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0
        }, // Minus
        new byte[] {
        0,0,0,0,0,0,0,0,
        0,0,0,0,0,1,1,0,
        0,0,0,0,1,1,0,0,
        0,0,0,1,1,0,0,0,
        0,0,1,1,0,0,0,0,
        0,1,1,0,0,0,0,0,
        0,0,0,0,0,0,0,0
        }, // Slash
        new byte[] {
        0,0,0,0,0,0,0,0,
        0,0,0,1,1,0,0,0,
        0,0,0,1,1,0,0,0,
        0,0,0,0,0,0,0,0,
        0,0,0,1,1,0,0,0,
        0,0,0,1,1,0,0,0,
        0,0,0,0,0,0,0,0
        }, // Colon
        new byte[] {
        0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0
        } // Space
    };

    // Constants & Members
    private readonly List<char> glyphs = new()
    {
        '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '-', '/', ':', ' '
    };

    private const string time = "HH:mm:ss";
    private const string date = "dd/MM/yyyy";
    private const string date_time = "HH:mm:ss dd/MM/yyyy";
    private const string invalid_source = "Invalid argument";
    private const int padding = 1;
    private const int columns = 8;
    private const int rows = 7;

    private string _value;
    private int _count;

    // Dependency Properties & Properties
    public static readonly DependencyProperty ForegroundProperty =
    DependencyProperty.Register(nameof(Foreground), typeof(Brush),
    typeof(Matrix), null);

    public static readonly DependencyProperty SourceProperty =
    DependencyProperty.Register(nameof(Source), typeof(Sources),
    typeof(Matrix), new PropertyMetadata(Sources.Time));

    public static readonly DependencyProperty SizeProperty =
    DependencyProperty.Register(nameof(Size), typeof(UIElement),
    typeof(Matrix), new PropertyMetadata(4));

    public Brush Foreground
    {
        get { return (Brush)GetValue(ForegroundProperty); }
        set { SetValue(ForegroundProperty, value); }
    }

    public Sources Source
    {
        get { return (Sources)GetValue(SourceProperty); }
        set { SetValue(SourceProperty, value); }
    }

    public int Size
    {
        get { return (int)GetValue(SizeProperty); }
        set { SetValue(SizeProperty, value); }
    }

    // Add Element & Add Section Methods
    private Rectangle AddElement(string name, int left, int top)
    {
        var element = new Rectangle()
        {
            Tag = name,
            Opacity = 0,
            RadiusX = 1,
            RadiusY = 1,
            Width = Size,
            Height = Size,
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

    private void AddSection(string name)
    {
        int x = 0;
        int y = 0;
        int index = 0;
        var section = new Canvas()
        {
            Tag = name,
            Height = rows * Size,
            Width = columns * Size
        };
        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                section.Children.Add(AddElement($"{name}.{index}", x, y));
                x = x + Size + padding;
                index++;
            }
            x = 0;
            y = y + Size + padding;
        }
        Children.Add(section);
    }

    // Set Layout & Add Layout Methods
    private void SetLayout(string name, char glyph)
    {
        var layout = Children.Cast<Canvas>()
            .FirstOrDefault(f => (string)f.Tag == name);
        int pos = glyphs.IndexOf(glyph);
        byte[] values = table[pos];
        for (int index = 0; index < layout.Children.Count; index++)
        {
            layout.Children.Cast<Rectangle>()
            .FirstOrDefault(f => (string)f.Tag == $"{name}.{index}")
            .Opacity = values[index];
        }
    }

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
                AddSection(item.ToString());
            }
            _count = length;
        }
        foreach (int item in list)
        {
            SetLayout(item.ToString(), array[item]);
        }
    }

    // Value Property & Constructor
    public string Value
    {
        get { return _value; }
        set { _value = value; AddLayout(); }
    }

    public Matrix()
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