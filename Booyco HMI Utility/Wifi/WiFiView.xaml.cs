﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
    /// Interaction logic for WiFiView.xaml
    /// </summary>
    public partial class WiFiView : UserControl , INotifyPropertyChanged
    {
        #region OnProperty Changed
        /////////////////////////////////////////////////////////////
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /////////////////////////////////////////////////////////////
        #endregion

        public static SolidColorBrush HBReceiveColour;
        public static SolidColorBrush HBConnectingColour;
        public static SolidColorBrush HBLostColour;
        private DispatcherTimer dispatcherTimer;
        WiFiconfig WiFiconfig;

        public WiFiView()
        {
            InitializeComponent();           
            DataContext = this;
            HBReceiveColour = new SolidColorBrush(Color.FromArgb(100, 0, 102, 0));
            HBConnectingColour = new SolidColorBrush(Color.FromArgb(100, 255, 183, 0));    
            HBLostColour = new SolidColorBrush(Color.FromArgb(100, 188, 0, 0));
            if(DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject()))
             {      
            TCPclients = new List<TCPclientR>();
                TCPclients.Add(new TCPclientR
                {
                    VID = 1000,
                    FirmRev = 1,
                    FirmSubRev = 1,
                    Name = "123456789ABCDEF",


                }) ;

            }
            //WiFiconfig = new WiFiconfig();
        }

        private void ClientListUpdater(object sender, EventArgs e)
        {            
            if (DGTCPclientList.SelectedItems.Count == 1)
            {
                SelectionUpdate();
            }

            bool VIDCheck = false;
            if (TCPclients != null && TCPclients.Count > 0)
            {
                foreach (TCPclientR item in TCPclients)
                {
                    if (item.VID == GlobalSharedData.SelectedVID)
                    {
                        VIDCheck = true;
                    }
                }
                GlobalSharedData.ActiveDevice = VIDCheck;

                if (GlobalSharedData.ActiveDevice)
                {
                    GlobalSharedData.ServerStatus = "Connected to VID:" + GlobalSharedData.SelectedVID;
                }
                else
                {
                    GlobalSharedData.ServerStatus = "";
                }
            }
            else
            {
                GlobalSharedData.ActiveDevice = false;
                GlobalSharedData.ServerStatus = "";
            }
            
            ServerStatusView = GlobalSharedData.ServerStatus;
            NetworkDevicesp = GlobalSharedData.NetworkDevices;
            TCPclients = WiFiconfig.ClientLsitChanged(TCPclients);
            if (WiFiconfig.clients != null)
            {
               
                foreach (TCPclientR item in TCPclients)
                {
                    if (item.HeartbeatTimestamp <= DateTime.Now.AddSeconds(-3) && item.HeartCount > 0)
                    {
                        item.Heartbeat_Colour = HBLostColour;
                    }
                    if(item.HeartbeatTimestamp <= DateTime.Now.AddSeconds(-20) && item.HeartCount > 0)
                    {
                       // _removeItem = item;
                       // TCPclients.Remove(item);
                        //WiFiconfig.ConnectionError
                    }
                }

               // TCPclients.Remove(_removeItem);
               
                if (WiFiconfig.clients.Count == 0)
                {
                    GlobalSharedData.SelectedDevice = -1;
                    //btnEnabler = false;
                }
                else if (WiFiconfig.clients.Count > 0 && GlobalSharedData.SelectedDevice != -1)
                {
                    //btnEnabler = true;
                }
            }
            if (DGTCPclientList.Items.Count == 1)
                GlobalSharedData.SelectedDevice = 0;
        }
            
        private void BtnMain_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.Startup;
        
        }

        private void BtnBootload_Click(object sender, RoutedEventArgs e)
        {
            TCPclientR _selectedItem = (TCPclientR)DGTCPclientList.SelectedItem;

            if (BtnBootload.Content.ToString() == "Bootload")
            {
                ProgramFlow.ProgramWindow = (int)ProgramFlowE.Bootload;
            }
            else
            {
                ProgramFlow.ProgramWindow = (int)ProgramFlowE.UploadFile;
            }
            
         
           
        }

        private void BtnDatView_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.DataExtractorView;
           
        }

        private void BtnConfig_Click(object sender, RoutedEventArgs e)
        {
          
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.ConfigureMenuView;

        }
  
        private List<TCPclientR> _TCPclients;
        public List<TCPclientR> TCPclients
        {
            get
            {
                return _TCPclients;
            }
            set
            {
                _TCPclients = value;
                OnPropertyChanged("TCPclients");
            }
        }

        private List<NetworkDevice> _NetworkDevicesp;
        public List<NetworkDevice> NetworkDevicesp
        {
            get
            {
                return _NetworkDevicesp;
            }

            set
            {
                _NetworkDevicesp = value;
                OnPropertyChanged("NetworkDevicesp");
            }
        }

        private bool _btnEnabler;

        public bool btnEnabler
        {
            get { return _btnEnabler; }
            set { _btnEnabler = value; OnPropertyChanged("btnEnabler"); }
        }

        private string _ServerStatusView;

        public string ServerStatusView
        {
            get { return _ServerStatusView; }
            set { _ServerStatusView = value; OnPropertyChanged("ServerStatusView"); }
        }
             
        private void DGTCPclientList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DGTCPclientList.SelectedIndex != -1)
            {
                SelectionUpdate();
            }
            else
            {
                BtnConfig.IsEnabled = false;
                BtnDatView.IsEnabled = false;
                BtnBootload.IsEnabled = false;
                BtnRestart.IsEnabled = false;
            }
        }

        void SelectionUpdate()
        {
            TCPclientR _selectedItem = (TCPclientR)DGTCPclientList.SelectedItem;

            
            GlobalSharedData.SelectedDevice = DGTCPclientList.SelectedIndex;
            GlobalSharedData.SelectedVID = _selectedItem.VID;

            GlobalSharedData.ServerStatus = "Connected to VID:" + GlobalSharedData.SelectedVID;
  
            if (_selectedItem.HeartCount < 1)
            {
                BtnBootload.IsEnabled = false;
                BtnConfig.IsEnabled = false;
                BtnDatView.IsEnabled = false;
                BtnRestart.IsEnabled = false;
            }
            else if (_selectedItem.ApplicationState == "BHU BT" || _selectedItem.ApplicationState == "Comms Bridge BT")
            {
                BtnBootload.IsEnabled = true;
                BtnConfig.IsEnabled = false;
                BtnDatView.IsEnabled = false;
                BtnRestart.IsEnabled = false;
                GlobalSharedData.ConnectedDeviceApplicationState = (int)ApplicationEnum.bootloader;
                BtnBootload.Content = "Bootload";
            }
            else if ((_selectedItem.ApplicationState == "BHU App") || (_selectedItem.ApplicationState == "Comms Bridge App") || (_selectedItem.ApplicationState == "BHU Test Station App") )
            {
                if(_selectedItem.ApplicationState == "Comms Bridge App")
                {
                    GlobalSharedData.Firmware = 45;
                }
                else if(_selectedItem.ApplicationState == "BHU Test Station App")
                {
                    GlobalSharedData.Firmware = 94;
                }
                else
                {
                    GlobalSharedData.Firmware = 56;
                }
                BtnConfig.IsEnabled = true;
                BtnDatView.IsEnabled = true;
                BtnRestart.IsEnabled = true;
                BtnBootload.Content = "Upload File";
                GlobalSharedData.ConnectedDeviceApplicationState = (int)ApplicationEnum.Application;

                if (WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].FirmRev != 1 || (_selectedItem.ApplicationState == "Comms Bridge App"))
                {
                    BtnBootload.IsEnabled = true;
                }
                else
                {
                    BtnBootload.IsEnabled = false;
                }

            }
            else if ((_selectedItem.ApplicationState == "ERB BT"))
            {
                BtnBootload.IsEnabled = true;
                BtnConfig.IsEnabled = false;
                BtnDatView.IsEnabled = false;
                BtnRestart.IsEnabled = false;
                BtnBootload.Content = "Bootload";
                GlobalSharedData.ConnectedDeviceApplicationState = (int)ApplicationEnum.ERB_Bootloader;
            }
}

private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(this.Visibility == Visibility.Visible)
            {
                WiFiconfig = new WiFiconfig();          
                WiFiconfig.ServerRun();
                dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Tick += new EventHandler(ClientListUpdater);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 300);
                dispatcherTimer.Start();
                DGTCPclientList.SelectedIndex = -1;
            }
            else
            {
                WiFiconfig.endAll = true;
                DGTCPclientList.SelectedIndex = -1;
                Bootloader.BootReady = Bootloader.BootDone = false;
                dispatcherTimer.Stop();
                TCPclients = new List<TCPclientR>();
                WiFiconfig.ServerStop();
                try
                {                   
                //    WiFiconfig.WirelessHotspot(null, null, false);
                //    var prc = new ProcManager();
                //    prc.KillByPort(13000);
                }
                catch
                {

                }
                Debug.WriteLine("====================== Not Visible");
            }
        }

        private void DGTCPclientList_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            if (DGTCPclientList.SelectedIndex != -1)
                GlobalSharedData.SelectedDevice = DGTCPclientList.SelectedIndex;
        }

 
        private void Label_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Label t = (Label)sender;
            if(t.Content.ToString() == "1")
            {

            }
        }

        private void BtnRestart_Click(object sender, RoutedEventArgs e)
        {
            if (WiFiconfig.clients.Count > 0 && GlobalSharedData.SelectedDevice != -1)
            {
                WiFiconfig.SelectedIP = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].IP;
                GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&BB00]");
            }
        }
    }

}
