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

namespace SplitControl
{
    public sealed partial class Flap : UserControl
    {
        public Flap()
        {
            this.InitializeComponent();
        }

        private string _value;
        private string _from;

        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                if (_from != null)
                {
                    if (_from != value)
                    {
                        TextBlockTop.Text = TextBlockFlipBottom.Text = value;
                        TextBlockFlipTop.Text = _from;
                        FlipAnimation.Begin();
                        FlipAnimation.Completed -= (s, e) => { };
                        FlipAnimation.Completed += (s, e) =>
                            TextBlockBottom.Text = _from;
                    }
                }
                if (_from == null)
                {
                    TextBlockFlipTop.Text = TextBlockBottom.Text = value;
                }
                _from = value;
            }
        }
    }
}
