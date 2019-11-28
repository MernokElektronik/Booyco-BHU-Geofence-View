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
    /// Interaction logic for FileMenuView.xaml
    /// </summary>
    public partial class FileMenuView : UserControl
    {
        public FileMenuView()
        {
            InitializeComponent();
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.Startup;
        }

        private void ButtonParameters_Click(object sender, RoutedEventArgs e)
        {

            ProgramFlow.ProgramWindow = (int)ProgramFlowE.ParameterFileView;
        }

        private void ButtonImages_Click(object sender, RoutedEventArgs e)
        {

            ProgramFlow.ProgramWindow = (int)ProgramFlowE.ImageFilesView;
        }
        private void ButtonAudio_Click(object sender, RoutedEventArgs e)
        {

            ProgramFlow.ProgramWindow = (int)ProgramFlowE.AudioFilesView;
        }

        private void ButtonDataLogs_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.DataLogFileView;
        }
    }
}
