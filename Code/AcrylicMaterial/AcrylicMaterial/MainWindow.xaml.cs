using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace AcrylicMaterial
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void Options_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Overlay != null && Windows.Foundation.Metadata.ApiInformation.IsTypePresent(
            "Windows.UI.Xaml.Media.XamlCompositionBrushBase"))
            {
                string value = (Options.SelectedItem as ComboBoxItem).Content as string;
                Overlay.Fill = value != "None" ?
                Application.Current.Resources[value] as AcrylicBrush : null;
            }
        }

    }
}
