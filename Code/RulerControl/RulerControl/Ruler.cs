using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;

namespace RulerControl;

public enum Units
{
    Cm,
    Inch
};

public class Ruler : Canvas
{
    private const double default_height = 40.0;

    private double _originalHeight;

    // Static Methods
    private static double CmToDip(double cm) =>
    cm * 96.0 / 2.54;

    private static double InchToDip(double inch) =>
        inch * 96.0;

    private static Path GetLine(Brush stroke, double thickness,
        Point start, Point finish) => new()
        {
            Stroke = stroke,
            StrokeThickness = thickness,
            Data = new LineGeometry()
            {
                StartPoint = start,
                EndPoint = finish
            }
        };

    // Dependency Properties & Properties
    public static readonly DependencyProperty ForegroundProperty =
    DependencyProperty.Register(nameof(Foreground), typeof(Brush),
    typeof(Ruler), new PropertyMetadata(new SolidColorBrush(Colors.Black)));

    public static readonly DependencyProperty LengthProperty =
    DependencyProperty.Register(nameof(Length), typeof(double),
    typeof(Ruler), new PropertyMetadata(10.0));

    public static readonly DependencyProperty SegmentProperty =
    DependencyProperty.Register(nameof(Segment), typeof(double),
    typeof(Ruler), new PropertyMetadata(20.0));

    public static readonly DependencyProperty UnitProperty =
    DependencyProperty.Register(nameof(Unit), typeof(double),
    typeof(Ruler), new PropertyMetadata(Units.Cm));

    public Brush Foreground
    {
        get { return (Brush)GetValue(ForegroundProperty); }
        set { SetValue(ForegroundProperty, value); }
    }

    public double Length
    {
        get { return (double)GetValue(LengthProperty); }
        set { SetValue(LengthProperty, value); }
    }

    public double Segment
    {
        get { return (double)GetValue(SegmentProperty); }
        set { SetValue(SegmentProperty, value); }
    }

    public Units Unit
    {
        get { return (Units)GetValue(UnitProperty); }
        set { SetValue(UnitProperty, value); }
    }

    // Layout Method
    private void Layout()
    {
        Children.Clear();
        for (double value = 0.0; value <= Length; value++)
        {
            double dip;
            if (Unit == Units.Cm)
            {
                dip = CmToDip(value);
                if (value < Length)
                {
                    for (int i = 1; i <= 9; i++)
                    {
                        if (i != 5)
                        {
                            var mm = CmToDip(value + 0.1 * i);
                            Children.Add(GetLine(Foreground, 0.5, new Point(mm, Height),
                            new Point(mm, Height - Segment / 3.0)));
                        }
                    }
                    var middle = CmToDip(value + 0.5);
                    Children.Add(GetLine(Foreground, 1.0, new Point(middle, Height),
                    new Point(middle, Height - Segment * 2.0 / 3.0)));
                }
            }
            else
            {
                dip = InchToDip(value);
                if (value < Length)
                {
                    var quarter = InchToDip(value + 0.25);
                    Children.Add(GetLine(Foreground, 0.5, new Point(quarter, Height),
                    new Point(quarter, Height - Segment / 3.0)));
                    var middle = InchToDip(value + 0.5);
                    Children.Add(GetLine(Foreground, 1.0, new Point(middle, Height),
                    new Point(middle, Height - 0.5 * Segment * 2.0 / 3.0)));
                    var division = InchToDip(value + 0.75);
                    Children.Add(GetLine(Foreground, 0.5, new Point(division, Height),
                    new Point(division, Height - 0.25 * Segment / 3.0)));
                }
            }
            Children.Add(GetLine(Foreground, 1.0, new Point(dip, Height),
            new Point(dip, Height - Segment)));
        }
    }

    // Constructor & Measure Override Event Handler
    public Ruler() =>
        Loaded += (object sender, RoutedEventArgs e) => Layout();

    protected override Size MeasureOverride(Size availableSize)
    {
        Height = !double.IsNaN(Height) ? Height : default_height;
        var desiredSize = (Unit == Units.Cm) ?
            new Size(CmToDip(Length), Height) :
            new Size(InchToDip(Length), Height);
        if (Height != _originalHeight)
        {
            Layout();
            _originalHeight = Height;
        }
        return desiredSize;
    }

}