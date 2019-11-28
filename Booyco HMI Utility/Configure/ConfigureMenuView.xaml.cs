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
using System.Windows.Threading;

namespace Booyco_HMI_Utility
{
    /// <summary>
    /// Interaction logic for ConfigureMenu.xaml
    /// </summary>
    public partial class ConfigureMenuView : UserControl
    {
        private DispatcherTimer dispatcherTimer;
        public ConfigureMenuView()
        {
            InitializeComponent();

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(ActiveStatus);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);
           
        }

        void ActiveStatus(object sender, EventArgs e)
        {
            if(GlobalSharedData.ActiveDevice)
            {
                Button_Parameters.IsEnabled = true;
                Button_AudioFiles.IsEnabled = true;
                Button_ImagesFiles.IsEnabled = true;
            }
            else
            {
                Button_Parameters.IsEnabled = false;
                Button_AudioFiles.IsEnabled = false;
                Button_ImagesFiles.IsEnabled = false;
            }
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            ProgramFlow.ProgramWindow = ProgramFlow.SourseWindow;
        }

        private void ButtonParameters_Click(object sender, RoutedEventArgs e)
        {
          
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.ParametersView;
        }

        private void ButtonImages_Click(object sender, RoutedEventArgs e)
        {
          
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.ImageFilesView;
        }
        private void ButtonAudio_Click(object sender, RoutedEventArgs e)
        {      
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.AudioFilesView;
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(this.Visibility == Visibility.Visible)
            {
                dispatcherTimer.Start();
                if (GlobalSharedData.ActiveDevice)
                {
                    Button_Parameters.IsEnabled = true;
                    Button_AudioFiles.IsEnabled = true;
                    Button_ImagesFiles.IsEnabled = true;
                }
                else
                {
                    Button_Parameters.IsEnabled = false;
                    Button_AudioFiles.IsEnabled = false;
                    Button_ImagesFiles.IsEnabled = false;
                }
            }
            else
            {
                dispatcherTimer.Stop();
            }
        }
    }
}
