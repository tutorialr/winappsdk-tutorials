using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Foundation;

namespace UniformLayout;

public class UniformPanel : Panel
{
    // Members, Dependency Properties & Properties
    private int _columns;
    private int _rows;

    public static readonly DependencyProperty ColumnsProperty =
    DependencyProperty.Register(nameof(Columns), typeof(int),
    typeof(UniformPanel), new PropertyMetadata(0));

    public static readonly DependencyProperty FirstColumnProperty =
    DependencyProperty.Register(nameof(FirstColumn), typeof(int),
    typeof(UniformPanel), new PropertyMetadata(0));

    public static readonly DependencyProperty RowsProperty =
    DependencyProperty.Register(nameof(Rows), typeof(int),
    typeof(UniformPanel), new PropertyMetadata(0));

    public int Columns
    {
        get { return (int)GetValue(ColumnsProperty); }
        set { SetValue(ColumnsProperty, value); }
    }

    public int FirstColumn
    {
        get { return (int)GetValue(FirstColumnProperty); }
        set { SetValue(FirstColumnProperty, value); }
    }

    public int Rows
    {
        get { return (int)GetValue(RowsProperty); }
        set { SetValue(RowsProperty, value); }
    }

    // Update Computed Values Method
    private void UpdateComputedValues()
    {
        _columns = Columns;
        _rows = Rows;
        if (FirstColumn >= _columns) FirstColumn = 0;
        if ((_rows == 0) || (_columns == 0))
        {
            var row = 0;
            var column = 0;
            var count = Children.Count;
            while (column < count)
            {
                var element = Children[column];
                if (element.Visibility != Visibility.Collapsed)
                {
                    row++;
                }
                column++;
            }
            if (row == 0) row = 1;
            if (_rows == 0)
            {
                if (_columns > 0)
                {
                    _rows = (row + FirstColumn + (_columns - 1)) / _columns;
                }
                else
                {
                    _rows = (int)Math.Sqrt(row);
                    if ((_rows * _rows) < row)
                    {
                        _rows++;
                    }
                    _columns = _rows;
                }
            }
            else if (_columns == 0)
            {
                _columns = (row + (_rows - 1)) / _rows;
            }
        }
    }

    // Measure Override Method
    protected override Size MeasureOverride(Size availableSize)
    {
        UpdateComputedValues();
        var available = new Size(
            availableSize.Width / _columns,
            availableSize.Height / _rows);
        double width = 0.0;
        double height = 0.0;
        int value = 0;
        int count = Children.Count;
        while (value < count)
        {
            var element = Children[value];
            element.Measure(available);
            var desiredSize = element.DesiredSize;
            if (width < desiredSize.Width)
                width = desiredSize.Width;
            if (height < desiredSize.Height)
                height = desiredSize.Height;
            value++;
        }
        return new Size(width * _columns, height * _rows);
    }

    // Arrange Override Method
    protected override Size ArrangeOverride(Size finalSize)
    {
        var rect = new Rect(0.0, 0.0,
        finalSize.Width / _columns, finalSize.Height / _rows);
        double width = rect.Width;
        double value = finalSize.Width - 1.0;
        rect.X += rect.Width * FirstColumn;
        foreach (var element in Children)
        {
            element.Arrange(rect);
            if (element.Visibility != Visibility.Collapsed)
            {
                rect.X += width;
                if (rect.X >= value)
                {
                    rect.Y += rect.Height;
                    rect.X = 0.0;
                }
            }
        }
        return finalSize;
    }
}