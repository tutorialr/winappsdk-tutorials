using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

internal class Library
{
    public List<string> Options =>
    Enum.GetValues(typeof(ToastTemplateType))
    .Cast<ToastTemplateType>()
    .Select(s => s.ToString())
    .ToList();

    public void SetToast(ComboBox options, TextBox value)
    {
        var selected = options.SelectedValue as string;
        ToastTemplateType template = Enum.Parse<ToastTemplateType>(selected);
        XmlDocument toast = ToastNotificationManager.GetTemplateContent(template);
        XmlNodeList text = toast.GetElementsByTagName("text");
        if (text.Length > 0)
        {
            text[0].AppendChild(toast.CreateTextNode(value.Text));
        }
        XmlNodeList image = toast.GetElementsByTagName("image");
        if (image.Length > 0)
        {
            image[0].Attributes.GetNamedItem("src").NodeValue =
            "Assets/Square44x44Logo.scale-200.png";
        }
        ToastNotification notification = new(toast);
        ToastNotificationManager.CreateToastNotifier().Show(notification);
    }
}
