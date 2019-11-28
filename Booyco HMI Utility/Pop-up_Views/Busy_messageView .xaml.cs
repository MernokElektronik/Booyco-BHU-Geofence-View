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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Booyco_HMI_Utility
{
    /// <summary>
    /// Interaction logic for Error_messageView.xaml
    /// </summary>
    public partial class Busy_messageView : UserControl
    {
        public static string BusyMessage = "";

        public Busy_messageView()
        {
            InitializeComponent();
        }

        private void BtnProgramDone_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //ErrorMessagelbl.Text = BusyMessage;
        }

        private void UserControlSpinnerLoad_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
