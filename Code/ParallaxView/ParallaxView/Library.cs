using Microsoft.UI.Xaml.Controls;
using System;
internal class Library
{
    private class Item
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Text { get; set; } = string.Empty;
    }
    public static void Add(ListView listView, string value)
    {
        listView.Items.Add(new Item
        {
            Text = value
        });
    }
    public static void Remove(ListView listView, object sender)
    {
        Item item = (sender as AppBarButton).Tag as Item;
        listView.Items.Remove(item);
    }
}