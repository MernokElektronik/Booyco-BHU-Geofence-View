using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Booyco_HMI_Utility
{
    /// <summary>
    /// Interaction logic for ExtendedWindow.xaml
    /// </summary>
    public partial class ExtendedWindow : Window
    {
        public ExtendedWindow()
        {
            InitializeComponent();
            MapView.ButtonBack.Content = "Close";
            MapView.ButtonWindow.Visibility = Visibility.Collapsed;
        }
    }
}
