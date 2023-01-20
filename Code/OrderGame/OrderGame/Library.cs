using Comentsys.Toolkit.WindowsAppSdk;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

public class Library
{
    private const string title = "Order Game";
    private const int size = 6;

    private readonly Random _random = new((int)DateTime.UtcNow.Ticks);
    private readonly ObservableCollection<int> _values = new();

    private Dialog _dialog;
    private GridView _view;
    private DateTime _start;

    private List<int> Choose(int minimum, int maximum, int total) =>
        Enumerable.Range(minimum, maximum)
            .OrderBy(r => _random.Next(minimum, maximum))
                .Take(total).ToList();

    private void Completed()
    {
        if (_values.OrderBy(o => o).SequenceEqual(_values))
        {
            TimeSpan duration = (DateTime.UtcNow - _start).Duration();
            _dialog.Show($"Completed in {duration:hh\\:mm\\:ss}", title);
            _view.IsEnabled = false;
        }
    }

    // Layout & New
    private void Layout(Grid grid)
    {
        grid.Children.Clear();
        _view = new()
        {
            ItemsPanel = grid.Resources[nameof(ItemsPanelTemplate)]
                as ItemsPanelTemplate,
            ItemTemplate = grid.Resources[nameof(DataTemplate)]
                as DataTemplate,
            SelectionMode = ListViewSelectionMode.Single,
            CanReorderItems = true,
            ItemsSource = _values,
            CanDragItems = true,
            AllowDrop = true,
            IsEnabled = true,
            CanDrag = true,
        };
        _view.DragItemsCompleted += (ListViewBase sender,
            DragItemsCompletedEventArgs args) =>
            Completed();
        grid.Children.Add(_view);
    }

    public void New(Grid grid)
    {
        _dialog = new Dialog(grid.XamlRoot, title);
        _values.Clear();
        _start = DateTime.UtcNow;
        var values = Choose(1, size * size, size * size);
        foreach (var value in values)
        {
            _values.Add(value);
        }
        Layout(grid);
    }
}