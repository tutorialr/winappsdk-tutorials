using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
internal class Library
{
    public List<string> Options => new()
    {
        "number", "activity", "alarm", "attention", "available", "away",
        "busy", "error", "newMessage", "paused", "playing", "unavailable"
    };

    public void SetBadge(ComboBox options, TextBox number)
    {
        var selected = options.SelectedValue as string;
        var result = selected == "number" ? number.Text : selected;
        XmlDocument badge = BadgeUpdateManager.GetTemplateContent(
        int.TryParse(result, out _) ?
        BadgeTemplateType.BadgeNumber :
        BadgeTemplateType.BadgeGlyph);
        XmlNodeList attributes = badge.GetElementsByTagName("badge");
        attributes[0].Attributes.GetNamedItem("value").NodeValue = result;
        BadgeNotification notification = new(badge);
        BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(notification);
    }

    public void ClearBadge()
    {
        BadgeUpdateManager.CreateBadgeUpdaterForApplication().Clear();
    }
}
