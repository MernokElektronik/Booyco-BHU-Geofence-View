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
    /// Interaction logic for HMIDisplayView.xaml
    /// </summary>
    public partial class HMIDisplayView : UserControl
    {
        // === Public Variables ===
        public bool CloseRequest = false;

        // === Private Variables ===
        private DispatcherTimer dispatcherPlayTimer;
        private bool DisplpayPlay = false;
        private double scaleFactor = 0.07;

        /// <summary>
        /// HMIDisplayViewView: The constructor function
        /// Setup required variables 
        /// </summary>
        public HMIDisplayView()
        {
            InitializeComponent();
            dispatcherPlayTimer = new DispatcherTimer();
            dispatcherPlayTimer.Tick += new EventHandler(PlayTimerTick);
            dispatcherPlayTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
   
        }

        /// <summary>
        /// ButtonBack_Click: Back Button Click Event
        /// Open previous View
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.DataLogView;
            CloseRequest = true;
        }
        
        /// <summary>
        /// ButtonNewWindow_Click: New Window Button Click Event
        /// Opens a new HMIDisplayView
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonNewWindow_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 1; i < 25 ; i++)
            {
          
                Rectangle _rectangle = (Rectangle)this.FindName("Presence" + i);
                _rectangle.Opacity += 0.2;
            }
        }

        /// <summary>
        /// PlayTimerTick: PlayTimer handler
        /// Increment slider value with the timer's timeout;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayTimerTick(object sender, EventArgs e)
        {
            Slider_DateTime.Value += 5000000;
        }

    
        void ClearClusters()
        {
            for (int i = 1; i < 25; i++)
            {

                Rectangle _rectangle = (Rectangle)this.FindName("Presence" + i);
                _rectangle.Opacity = 0.1;
            }
            for (int i = 1; i < 25; i++)
            {

                Rectangle _rectangle = (Rectangle)this.FindName("Warning" + i);
                _rectangle.Opacity = 0.1;
            }
            for (int i = 1; i < 25; i++)
            {

                Rectangle _rectangle = (Rectangle)this.FindName("Critical" + i);
                _rectangle.Opacity = 0.1;
            }
        }

        void PDSThreatOpacity(int _zone, int sector, int width, double opacity)
        {
            string _rectangleName ="";
            switch(_zone)
            {
                case 3:
                    _rectangleName = "Critical";
                    break;
                case 2:
                    _rectangleName = "Warning";
                    break;
                case 1:
                    _rectangleName = "Presence";
                    break;
            }

            ClusterOpacity(sector, width, opacity, _rectangleName);


        }

        void ClusterOpacity(int sector, int width, double opacity , string _rectangleName)
        {
            if(width > 360)
            {
                width = 360;
            }
            if(sector > 12)
           {
                sector = 12;
            }

            int numberSegements = width / 15;

            int startPos = sector * 2 - numberSegements / 2 + 1;

            if(startPos<0)
            {
                startPos = 24 + startPos;
            }

            for(int i = 0; i <numberSegements; i++)
            {
                int segement;
                if (startPos+i>24)
                {
                    segement = startPos + i - 24;
                }
                else
                {
                    segement = startPos + i;
                }

                Rectangle _rectangle = (Rectangle)this.FindName(_rectangleName + segement);
                _rectangle.Opacity += opacity;

            }

        }

        private void Slider_DateTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ClearClusters();
            DateTime _dateTime = new DateTime((long)Slider_DateTime.Value);
            DateTime _enddateTime = new DateTime();
            TextBlock_SelectDateTime.Text = _dateTime.ToString("MM/dd/yyyy hh:mm:ss.fff tt");

            int count = 0;
            foreach (HMIDisplayEntry item in GlobalSharedData.HMIDisplayList)
            {
                if (Convert.ToUInt32(item.ThreatBID) > 0)
                {

                    int firstindex = item.PDSThreat.FindLastIndex(p => p.DateTime < _dateTime);

                    // int lastindex = item.PDSThreat.FindLastIndex(p => p.DateTime < _dateTime);

                    if (firstindex == -1)
                    {
                        firstindex = item.PDSThreat.Count - 1;
                    }

                    //// if (firstindex == lastindex)
                    //  {
                    if (_dateTime <= item.EndDateTime && _dateTime >= item.StartDateTime)
                    {
                        TextBlock_Date.Text = item.PDSThreat.ElementAt(firstindex).DateTime.ToString("MM/dd/yyyy");
                        TextBlock_Time.Text = item.PDSThreat.ElementAt(firstindex).DateTime.ToString("hh:mm:ss");
                        count++;
                        PDSThreatOpacity(Convert.ToInt32(item.PDSThreat.ElementAt(firstindex).ThreatZone), Convert.ToInt32(item.PDSThreat.ElementAt(firstindex).ThreatSector), Convert.ToInt32(item.PDSThreat.ElementAt(firstindex).ThreatWidth), 0.8);
                        //  }
                    }

                    else if(_dateTime >= item.EndDateTime)
                    {
                        _enddateTime = item.EndDateTime;
                    }
                   
                }
            }

            if (count > 0)
            {
                TextBlock_Title.Foreground = new SolidColorBrush(Color.FromRgb(255, 165, 0)); 
                TextBlock_Title.Text = "PDS(1/" + count.ToString() + ") - Proximity Detection 01";

            }
            else
            {
                TextBlock_Time.Text = _enddateTime.ToString("hh:mm:ss");
                TextBlock_Title.Text = "";
            }
        }

        private void Button_Play_Click(object sender, RoutedEventArgs e)
        {
            if (DisplpayPlay)
            {
                dispatcherPlayTimer.Stop();
                Button_Play.Content = "Play";
             
                DisplpayPlay = false;
            }
            else
            {
                dispatcherPlayTimer.Start();
                Button_Play.Content = "Pause";
                DisplpayPlay = true;
            }
        }


     
        private void Rectangle_Background_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //try
            //{
            //    Rectangle _rectangle = (Rectangle)sender;
            //    Label_Count.Content = ((_rectangle.Opacity-0.1) / 0.2).ToString();
            //}
            //catch
            //{

            //}


        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (ProgramFlow.ProgramWindow == (int)ProgramFlowE.HMIDisplayView)
            {
                //GlobalSharedData.HMIDisplayList.First().PDSThreat.First().ThreatZone;
              
                if(!GlobalSharedData.OnlyRadarSelected)
                {
                    Grid_RadarThreat1.Visibility = Visibility.Collapsed;
                    Grid_RadarThreat2.Visibility = Visibility.Collapsed;
                    Grid_RadarThreat3.Visibility = Visibility.Collapsed;
                    Grid_RadarThreat4.Visibility = Visibility.Collapsed;
                    Grid_RadarThreat5.Visibility = Visibility.Collapsed;

                    Slider_DateTime.Visibility = Visibility.Visible;
                    Button_Play.Visibility = Visibility.Visible;
                    TextBlock_EndDateTime.Visibility = Visibility.Visible;
                    TextBlock_SelectDateTime.Visibility = Visibility.Visible;
                    TextBlock_StartDateTime.Visibility = Visibility.Visible;
                    Slider_DateTime.Minimum = GlobalSharedData.StartDateTimeDatalog.Ticks + 1;
                    TextBlock_StartDateTime.Text = GlobalSharedData.StartDateTimeDatalog.ToString("MM/dd/yyyy hh:mm:ss.fff tt");


                    Slider_DateTime.Maximum = GlobalSharedData.EndDateTimeDatalog.Ticks + 1;
                    TextBlock_EndDateTime.Text = GlobalSharedData.EndDateTimeDatalog.ToString("MM/dd/yyyy hh:mm:ss.fff tt");
                    if (GlobalSharedData.HMIDisplayList.Count() != 0)
                    {

                        // PDSThreatOpacity(Convert.ToInt32(GlobalSharedData.HMIDisplayList.First().PDSThreat.First().ThreatZone), Convert.ToInt32(GlobalSharedData.HMIDisplayList.First().PDSThreat.First().ThreatSector), Convert.ToInt32(GlobalSharedData.HMIDisplayList.First().PDSThreat.First().ThreatWidth), 0.8);

                        Slider_DateTime.Value = Slider_DateTime.Minimum;
                    }

                }
                else
                {
                    ClearClusters();
                    Slider_DateTime.Visibility = Visibility.Collapsed;
                    Button_Play.Visibility = Visibility.Collapsed;
                    TextBlock_EndDateTime.Visibility = Visibility.Collapsed;
                    TextBlock_SelectDateTime.Visibility = Visibility.Collapsed;
                    TextBlock_StartDateTime.Visibility = Visibility.Collapsed;

                    Grid_RadarThreat1.Visibility = Visibility.Collapsed;
                    Grid_RadarThreat2.Visibility = Visibility.Collapsed;
                    Grid_RadarThreat3.Visibility = Visibility.Collapsed;
                    Grid_RadarThreat4.Visibility = Visibility.Collapsed;
                    Grid_RadarThreat5.Visibility = Visibility.Collapsed;

                    foreach ( HMIRadarDisplayEntry item in GlobalSharedData.HMIRadarDisplayList)
                    {

                        double _diameter = (double)item.ThreatWidth/scaleFactor;

                        if(_diameter > 60)
                        {
                            _diameter = 60;
                        }

                        if (item.ThreatID == 1)
                        {
                            if (item.ThreatWidth > 0 && item.ThreatCordinateY > -24 && item.ThreatCordinateY < 24)
                            {
                                Grid_RadarThreat1.Visibility = Visibility.Visible;
                                Ellipse_RadarThreat1.Height = _diameter;
                                Ellipse_RadarThreat1.Width = _diameter;
                                TextBlock_RadarDistance1.Text = item.ThreatDistance.ToString() + "m";
                                Thickness _margin = Grid_RadarThreat1.Margin;
                                _margin.Left = (double)item.ThreatCordinateX / scaleFactor;
                                _margin.Bottom = (double)item.ThreatCordinateY / scaleFactor;
                                Grid_RadarThreat1.Margin = _margin;
                            }
                            else
                            {
                                Grid_RadarThreat1.Visibility = Visibility.Collapsed;
                            }
                            
                        }
                        else if(item.ThreatID == 2)
                        {
                            if (item.ThreatWidth > 0 && item.ThreatCordinateY > -24 && item.ThreatCordinateY < 24)
                            {
                                Grid_RadarThreat2.Visibility = Visibility.Visible;
                                Ellipse_RadarThreat2.Height = _diameter;
                                Ellipse_RadarThreat2.Width = _diameter;
                                TextBlock_RadarDistance2.Text = item.ThreatDistance.ToString() + "m";
                                Thickness _margin = Grid_RadarThreat2.Margin;
                                _margin.Left = (double)item.ThreatCordinateX / scaleFactor;
                                _margin.Bottom = (double)item.ThreatCordinateY / scaleFactor;
                                Grid_RadarThreat2.Margin = _margin;
                        }
                        else
                        {
                                Grid_RadarThreat2.Visibility = Visibility.Collapsed;
                        }
                    }
                        else if (item.ThreatID == 3 && item.ThreatCordinateY > -24 && item.ThreatCordinateY < 24)
                        {
                            if (item.ThreatWidth > 0)
                            {
                                Grid_RadarThreat3.Visibility = Visibility.Visible;
                                Ellipse_RadarThreat3.Height = _diameter;
                                Ellipse_RadarThreat3.Width = _diameter;
                                TextBlock_RadarDistance3.Text = item.ThreatDistance.ToString() + "m";
                                Thickness _margin = Grid_RadarThreat3.Margin;
                                _margin.Left = (double)item.ThreatCordinateX / scaleFactor;
                                _margin.Bottom = (double)item.ThreatCordinateY / scaleFactor;
                                Grid_RadarThreat3.Margin = _margin;
                            }
                            else
                            {
                                Grid_RadarThreat3.Visibility = Visibility.Collapsed;
                            }
                        }
                        else if (item.ThreatID == 4)
                        {
                            if (item.ThreatWidth > 0 && item.ThreatCordinateY > -24 && item.ThreatCordinateY < 24)
                            {
                                Grid_RadarThreat4.Visibility = Visibility.Visible;
                                Ellipse_RadarThreat4.Height = _diameter;
                                Ellipse_RadarThreat4.Width = _diameter;
                                TextBlock_RadarDistance4.Text = item.ThreatDistance.ToString() + "m";
                                Thickness _margin = Grid_RadarThreat4.Margin;
                                _margin.Left = (double)item.ThreatCordinateX / scaleFactor;
                                _margin.Bottom = (double)item.ThreatCordinateY / scaleFactor;
                                Grid_RadarThreat4.Margin = _margin;
                        }
                        else
                        {
                                Grid_RadarThreat4.Visibility = Visibility.Collapsed;
                        }
                    }
                        else if (item.ThreatID == 5)
                        {
                            if (item.ThreatWidth > 0 && item.ThreatCordinateY > -24 && item.ThreatCordinateY < 24)
                            {
                                Grid_RadarThreat5.Visibility = Visibility.Visible;
                                Ellipse_RadarThreat5.Height = _diameter;
                                Ellipse_RadarThreat5.Width = _diameter;
                                TextBlock_RadarDistance5.Text = item.ThreatDistance.ToString() + "m";
                                Thickness _margin = Grid_RadarThreat5.Margin;
                                _margin.Left = (double)item.ThreatCordinateX / scaleFactor;
                                _margin.Bottom = (double)item.ThreatCordinateY / scaleFactor;
                                Grid_RadarThreat5.Margin = _margin;
                            }
                            else
                            {
                                Grid_RadarThreat5.Visibility = Visibility.Collapsed;
                            }
                        }
                    }
                }


            }

            else
            {
                ClearClusters();
            }
        }
    }
}
