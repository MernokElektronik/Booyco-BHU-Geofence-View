using System;
using System.Collections.Generic;
using System.IO;
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
using Booyco_HMI_Utility.CustomMarkers;
using GMap.NET;

using GMap.NET.WindowsPresentation;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Data.SqlClient;

namespace Booyco_HMI_Utility
{
    /// <summary>
    /// Interaction logic for GPRSView.xaml
    /// </summary>


    public partial class GPRSView : UserControl
    {
        static private DataLogManagement dataLogManager = new DataLogManagement();
        double StartLat = -25.834329;
        double StartLon = 29.986633;
        double scalefactor = 70000;
        static bool New_Data = false;
        public static List<MarkerEntry> GPSMapMarkers = new List<MarkerEntry>();
        MQTTDataLogs MQTTDataLogsControl = new MQTTDataLogs();
        bool BrokerConnectFail = false;
        bool InformationActive = true;
        static RangeObservableCollection<MarkerEntry> VehicleMarkerList ;
     
        private DispatcherTimer updateDispatcherTimer;
      
        public GPRSView()
        {
            InitializeComponent();
           
            updateDispatcherTimer = new DispatcherTimer();
            updateDispatcherTimer.Tick += new EventHandler(InfoUpdater);
            updateDispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            dataLogManager.ExcelFilemanager.StoreLogProtocolInfo();
            VehicleMarkerList = new RangeObservableCollection<MarkerEntry>();
            Datagrid_VehicleList.AutoGenerateColumns = false;
            Datagrid_VehicleList.ItemsSource = VehicleMarkerList;
        }
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                try
                {
                    MQTTDataLogsControl.MQTT_Init();
                    MQTTDataLogsControl.MQTTManager.client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
                    MQTTDataLogsControl.MQTT_Start();
                    BrokerConnectFail = false;
                  
                }
                catch
                {
                    BrokerConnectFail = true;
                }

                updateDispatcherTimer.Start();
            }
            else
            {
                MQTTDataLogsControl.MQTT_Stop();
                updateDispatcherTimer.Stop();
            }
        }
        private void InfoUpdater(object sender, EventArgs e)
        {
            if (BrokerConnectFail)
            { 
                try
                {
                    MQTTDataLogsControl.MQTT_Start();
                    BrokerConnectFail = false;
                }
                catch
                {
                    BrokerConnectFail = true;
                }
            }

            if (!BrokerConnectFail)
            {
               // if (New_Data)
               // {
                    UpdateMapMarker();
                    New_Data = false;
              //  }
            }

            foreach (MarkerEntry item in VehicleMarkerList)
            {
                System.TimeSpan diff1 = DateTime.Now.Subtract(item.VehicleInfo.TimeStamp);
                if (diff1.TotalSeconds > 60)
                {
                    item.VehicleInfo.Status = "Offline";
                }
                else
                {
                    item.VehicleInfo.Status = "Online";
                }
            }

        }
        static void Insert_Analog(Vehicle_Entry VehicleInfo)
        {

            //using (var connection = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=\"C:\\Users\\HermiduPlessis\\Dropbox(Mernok Elektronik)\\Mernok\\DEVPRJ - 168(Booyco HMI)\\Software\\Github\\M - PSW - 045\\Booyco HMI Utility\\Database\\DatabaseDataLogs.mdf\";Integrated Security=True"))
            //{
            //    connection.Open();
            //    var sql = "INSERT INTO Analogs(TimeStamp, UID,Latitude,Longitude,Speed,Heading) VALUES(@TimeStamp, @UID,@Latitude,@Longitude,@Speed,@Heading)";
            //    using (var cmd = new SqlCommand(sql, connection))
            //    {
            //        cmd.Parameters.AddWithValue("@TimeStamp", VehicleInfo.LastReceived);
            //        cmd.Parameters.AddWithValue("@UID", VehicleInfo.UID);
            //        cmd.Parameters.AddWithValue("@Latitude", VehicleInfo.Latitude);
            //        cmd.Parameters.AddWithValue("@Longitude", VehicleInfo.Longitude);
            //        cmd.Parameters.AddWithValue("@Speed", VehicleInfo.Speed);
            //        cmd.Parameters.AddWithValue("@Heading", VehicleInfo.Heading);
            //        cmd.ExecuteNonQuery();
            //    }
            //}

        }

        static void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            
            string[] TopicSplit = e.Topic.Split('/');
            // handle message received
            if (e.Topic.Contains("/Booyco/BHU/") && e.Topic.Contains("/DataLogs/Analog/") && TopicSplit.Count() == 7)
            {
                byte[] splitMessage = e.Message;
                if (splitMessage.Count() == 28)
                {
                    
                    DateTime TimeStamp = DateTimeCheck.UnixTimeStampToDateTime(BitConverter.ToUInt32(splitMessage, 0) - 2 * 60 * 60);
                    double Lat = ((double)BitConverter.ToInt32(splitMessage, 4)) * Math.Pow(10, -7);
                    double Lon = ((double)BitConverter.ToInt32(splitMessage,8)) * Math.Pow(10, -7);
                    double Speed = (double)BitConverter.ToUInt32(splitMessage,12)/100;
                    double Heading = (double)BitConverter.ToUInt16(splitMessage,16);                  
                    uint ThreatZone = (uint)BitConverter.ToUInt16(splitMessage, 18);
                    int ActiveEvent = (int)BitConverter.ToUInt32(splitMessage, 20);
                    int WarningSpeed = (int)BitConverter.ToUInt16(splitMessage, 24);
                    int OverSpeedTrip = (int)BitConverter.ToUInt16(splitMessage, 26);
                    //uint ThreatZone = (uint)splitMessage.ElementAt(14);
                    uint Type = (int)MarkerType.Vehicle;
                    ushort VehicleWidth = 2;
                    ushort VehilceLength = 4;
                    MarkerEntry VehicleMarker = new MarkerEntry();
                    VehicleMarker.VehicleInfo = new Vehicle_Entry();
                    VehicleMarker.VehicleInfo.Parameters = new VehicleParameters();
                    VehicleMarker.VehicleInfo.UID = TopicSplit[3];
                    
                    MarkerEntry VehicleMarkerFound = VehicleMarkerList.FirstOrDefault(item => item.VehicleInfo.UID == VehicleMarker.VehicleInfo.UID);
                    if (VehicleMarkerFound != null)
                    {
                        
                        int index = VehicleMarkerList.IndexOf(VehicleMarkerFound);
                        VehicleMarkerList[index].VehicleInfo.LastReceived = DateTime.Now;
                        VehicleMarkerList[index].VehicleInfo.Heading = Heading;
                        VehicleMarkerList[index].MapMarker = new GMapMarker(new PointLatLng(Lat, Lon));
                        VehicleMarkerList[index].VehicleInfo.TimeStamp = TimeStamp;
                        VehicleMarkerList[index].VehicleInfo.Latitude = Lat;
                        VehicleMarkerList[index].VehicleInfo.Longitude = Lon;
                        VehicleMarkerList[index].VehicleInfo.Speed = Speed;
                        VehicleMarkerList[index].VehicleInfo.Zone = ThreatZone;
                        VehicleMarkerList[index].VehicleInfo.ActiveEvent = ActiveEvent;
                      

                        try
                        {
                            VehicleMarkerList[index].VehicleInfo.ActiveEventString = dataLogManager.ExcelFilemanager.LPDInfoList_V1.ElementAt(ActiveEvent - 1).EventName;
                        }
                        catch
                        {
                            VehicleMarkerList[index].VehicleInfo.ActiveEventString = "";
                        }
                        VehicleMarkerList[index].VehicleInfo.Parameters.WarningSpeed = WarningSpeed;
                        VehicleMarkerList[index].VehicleInfo.Parameters.OverSpeedTrip = OverSpeedTrip;
                        VehicleMarkerList[index].title = "VID: " + VehicleMarker.VehicleInfo.UID;
                        VehicleMarkerList[index].titleSpeed = "Speed: " + Speed + " km/h";

                      

                        //VehicleMarkerList[index].title = "UID:" + VehicleMarker.VehicleInfo.UID + Environment.NewLine +
                        //                "Speed:" + Speed + Environment.NewLine +
                        //                "Warning Speed:" + WarningSpeed + Environment.NewLine +
                        //                "Overspeed Speed:" + OverSpeedTrip
                        ;                       
                    }
                    else
                    {
                      
                        VehicleMarker.MapMarker = new GMapMarker(new PointLatLng(Lat, Lon));
                        VehicleMarker.VehicleInfo.LastReceived = DateTime.Now;
                        VehicleMarker.VehicleInfo.Heading = Heading;
                        VehicleMarker.Type = Type;
                        VehicleMarker.VehicleInfo.TimeStamp = TimeStamp;
                        VehicleMarker.VehicleInfo.Latitude = Lat;
                        VehicleMarker.VehicleInfo.Longitude = Lon;
                        VehicleMarker.VehicleInfo.Width = VehicleWidth;
                        VehicleMarker.VehicleInfo.Length = VehilceLength;
                        VehicleMarker.VehicleInfo.IsPlotRequired = true;
                        VehicleMarker.VehicleInfo.Speed = Speed;
                        VehicleMarker.VehicleInfo.ActiveEvent = ActiveEvent;
                    


                        try
                        {
                            VehicleMarker.VehicleInfo.ActiveEventString = dataLogManager.ExcelFilemanager.LPDInfoList_V1.ElementAt(ActiveEvent - 1).EventName;
                        }
                        catch
                        {
                            VehicleMarker.VehicleInfo.ActiveEventString = "";
                        }
                        VehicleMarker.VehicleInfo.Zone = ThreatZone;
                        VehicleMarker.VehicleInfo.Parameters.WarningSpeed = WarningSpeed;
                        VehicleMarker.VehicleInfo.Parameters.OverSpeedTrip = OverSpeedTrip;
                        //VehicleMarker.title = "UID:" + VehicleMarker.VehicleInfo.UID + Environment.NewLine +
                        //                      "Speed:" +Speed + Environment.NewLine +
                        //                      "Warning Speed:" + WarningSpeed + Environment.NewLine +
                        //                      "Overspeed Speed:" + OverSpeedTrip 
                        //                      ;
                        VehicleMarker.title = "VID:" + VehicleMarker.VehicleInfo.UID;
                        VehicleMarker.titleSpeed = "Speed: " + Speed + " km/h";
                        App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
                        {
                            VehicleMarkerList.Add(VehicleMarker);
                        });
                    
                    }
                }

                GPSMapMarkers.Clear();
                foreach (MarkerEntry item in VehicleMarkerList)
                {
                    GPSMapMarkers.Add(item);
                }

                
            }
            else if (e.Topic.Contains("/Booyco/BHU/") && e.Topic.Contains("/Parameters/") && TopicSplit.Count() == 6)
            {
                MarkerEntry VehicleMarker = new MarkerEntry();
                VehicleMarker.VehicleInfo = new Vehicle_Entry();
                VehicleMarker.VehicleInfo.Parameters = new VehicleParameters();
                VehicleParameters ReceivedParameters = new VehicleParameters();
                VehicleMarker.VehicleInfo.UID = TopicSplit[3];
                byte[] splitMessage = e.Message;
                if (splitMessage.Count() == 80)
                {
                    int count = 0;
                   
                    ReceivedParameters.Geofence_01 = new Geofence_Entry();
                    ReceivedParameters.Geofence_01.Latitude = ((double)BitConverter.ToInt32(splitMessage, count)) * Math.Pow(10, -7);
                    ReceivedParameters.Geofence_01.Longitude = ((double)BitConverter.ToInt32(splitMessage, count += 4)) * Math.Pow(10, -7);
                    ReceivedParameters.Geofence_01.radius = (int)splitMessage.ElementAt(count += 4);
                    ReceivedParameters.Geofence_01.Type = (int)splitMessage.ElementAt(count += 1);
                    ReceivedParameters.Geofence_02 = new Geofence_Entry();
                    ReceivedParameters.Geofence_02.Latitude = ((double)BitConverter.ToInt32(splitMessage, count += 3)) * Math.Pow(10, -7);
                    ReceivedParameters.Geofence_02.Longitude = ((double)BitConverter.ToInt32(splitMessage, count += 4)) * Math.Pow(10, -7);
                    ReceivedParameters.Geofence_02.radius = (int)splitMessage.ElementAt(count += 4);
                    ReceivedParameters.Geofence_02.Type = (int)splitMessage.ElementAt(count += 1);
                    ReceivedParameters.Geofence_03 = new Geofence_Entry();
                    ReceivedParameters.Geofence_03.Latitude = ((double)BitConverter.ToInt32(splitMessage, count += 3)) * Math.Pow(10, -7);
                    ReceivedParameters.Geofence_03.Longitude = ((double)BitConverter.ToInt32(splitMessage, count += 4)) * Math.Pow(10, -7);
                    ReceivedParameters.Geofence_03.radius = (int)splitMessage.ElementAt(count += 4);
                    ReceivedParameters.Geofence_03.Type = (int)splitMessage.ElementAt(count += 1);
                    ReceivedParameters.Geofence_04 = new Geofence_Entry();
                    ReceivedParameters.Geofence_04.Latitude = ((double)BitConverter.ToInt32(splitMessage, count += 3)) * Math.Pow(10, -7);
                    ReceivedParameters.Geofence_04.Longitude = ((double)BitConverter.ToInt32(splitMessage, count += 4)) * Math.Pow(10, -7);
                    ReceivedParameters.Geofence_04.radius = (int)splitMessage.ElementAt(count += 4);
                    ReceivedParameters.Geofence_04.Type = (int)splitMessage.ElementAt(count += 1);
                    ReceivedParameters.Geofence_05 = new Geofence_Entry();
                    ReceivedParameters.Geofence_05.Latitude = ((double)BitConverter.ToInt32(splitMessage, count += 3)) * Math.Pow(10, -7);
                    ReceivedParameters.Geofence_05.Longitude = ((double)BitConverter.ToInt32(splitMessage, count += 4)) * Math.Pow(10, -7);
                    ReceivedParameters.Geofence_05.radius = (int)splitMessage.ElementAt(count += 4);
                    ReceivedParameters.Geofence_05.Type = (int)splitMessage.ElementAt(count += 1);
                    ReceivedParameters.VehicleType = splitMessage.ElementAt(count += 3);
                    ReceivedParameters.Revision = (int)splitMessage.ElementAt(count+=1);
                    ReceivedParameters.Subrevision = (int)splitMessage.ElementAt(count += 1);
                    ReceivedParameters.VehicleName = System.Text.Encoding.UTF8.GetString(splitMessage.Skip(count += 1).Take(15).ToArray()).Trim();
                   
                    MarkerEntry VehicleMarkerFound = VehicleMarkerList.FirstOrDefault(item => item.VehicleInfo.UID == VehicleMarker.VehicleInfo.UID);
                    if (VehicleMarkerFound != null)
                    {
                        int index = VehicleMarkerList.IndexOf(VehicleMarkerFound);
                        VehicleMarkerList[index].title = "UID: " + VehicleMarker.VehicleInfo.UID;
                        VehicleMarkerList[index].VehicleInfo.Parameters = ReceivedParameters;
                   
                        //VehicleMarkerList[index].title = "UID:" + VehicleMarker.VehicleInfo.UID + Environment.NewLine +
                        //                "Speed:" + Speed + Environment.NewLine +
                        //                "Warning Speed:" + WarningSpeed + Environment.NewLine +
                        //                "Overspeed Speed:" + OverSpeedTrip
                        ;
                        Insert_Analog(VehicleMarkerList[index].VehicleInfo);
                    }
                    else
                    {
                        VehicleMarker.VehicleInfo.Parameters = ReceivedParameters;
                  
                        //VehicleMarker.title = "UID:" + VehicleMarker.VehicleInfo.UID + Environment.NewLine +
                        //                      "Speed:" +Speed + Environment.NewLine +
                        //                      "Warning Speed:" + WarningSpeed + Environment.NewLine +
                        //                      "Overspeed Speed:" + OverSpeedTrip 
                        //                      ;
                        VehicleMarker.title = "UID:" + VehicleMarker.VehicleInfo.UID;
                        App.Current.Dispatcher.Invoke((Action)delegate // <--- HERE
                        {
                            VehicleMarkerList.Add(VehicleMarker);
                        });
               
                    }
                }
            }
            else if (e.Topic == "/Booyco/BHU/42000/DataLogs/Events")
            {

            }
            New_Data = true;

        }

        int SpeedCondition(double speed, double waringSpeed, double overSpeedTrip)
        {
            if(speed > overSpeedTrip)
            {
                return (int)SpeedConditionEnum.OverSpeed;
            }
            else if(speed > waringSpeed)
            {
                return (int)SpeedConditionEnum.WarningSpeed;
            }            

            return  (int)SpeedConditionEnum.UnderSpeed;
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.Startup;

        }

        private void ButtonNewWindow_Click(object sender, RoutedEventArgs e)
        {

        }
        public void UpdateMapMarker()
        {

            this.GMap_GPRS.Markers.Clear();

            //foreach (MarkerEntry item in GPSMapMarkers.FindAll(p => p.Type == (int)MarkerType.Ellipse))
            //{
            //    if (GPSMapMarkers.Exists(p => p.MapMarker.LocalPositionX != item.MapMarker.LocalPositionX && p.MapMarker.LocalPositionY != item.MapMarker.LocalPositionY))
            //    {
            //        item.Scale = scalefactor / (Math.Pow(2, (ushort)GMap_GPRS.Zoom));
            //        item.MapMarker.Shape = new CustomMarkerEllipse(this.GMap_GPRS, item);
            //        this.GMap_GPRS.Markers.Add(item.MapMarker);
            //    }
            //}

            //foreach (MarkerEntry item in GPSMapMarkers.FindAll(p => p.Type == (int)MarkerType.Point))
            //{
            //    item.MapMarker.Shape = new CustomMarkerPoint(this.GMap_GPRS, item);
            //    this.GMap_GPRS.Markers.Add(item.MapMarker);
            //}
            //foreach (MarkerEntry item in GPSMapMarkers.FindAll(p => p.Type == (int)MarkerType.Cross))
            //{
            //    item.MapMarker.Shape = new CustomMarkerCross(this.GMap_GPRS, item);
            //    this.GMap_GPRS.Markers.Add(item.MapMarker);
            //}
            //foreach (MarkerEntry item in GPSMapMarkers.FindAll(p => p.Type == (int)MarkerType.Indicator))
            //{
            //    item.Scale = scalefactor / (Math.Pow(2, (ushort)GMap_GPRS.Zoom));
            //    item.MapMarker.Shape = new CustomMarkerIndicator(this.GMap_GPRS, item);
            //    this.GMap_GPRS.Markers.Add(item.MapMarker);
            //    StartLat = item.Latitude;
            //    StartLon = item.Longitude;
            //}
            List<Geofence_Entry> GeofenceAddedList = new List<Geofence_Entry>();
            foreach (MarkerEntry item in GPSMapMarkers.FindAll(p => p.Type == (int)MarkerType.Vehicle))
            {

                if (item.VehicleInfo.IsPlotRequired)
                {
                    item.Scale = scalefactor / (Math.Pow(2, (ushort)GMap_GPRS.Zoom));

                    CustomMarkerVehicle Marker = new CustomMarkerVehicle(this.GMap_GPRS, item);
                    if (InformationActive)
                    {
                        Marker.Label_PopupSpeed.Visibility = Visibility.Visible;
                        Marker.Label_PopupUID.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        Marker.Label_PopupSpeed.Visibility = Visibility.Collapsed;
                        Marker.Label_PopupUID.Visibility = Visibility.Collapsed;
                    }

                    if (item.VehicleInfo.Parameters.Geofence_01 != null)
                    {
                        if (GeofenceAddedList.FirstOrDefault(x => x == item.VehicleInfo.Parameters.Geofence_01) == null && item.VehicleInfo.Parameters.Geofence_01.radius != 0 && item.VehicleInfo.Parameters.Geofence_01.Type != 0)
                        {
                            GeofenceAddedList.Add(item.VehicleInfo.Parameters.Geofence_01);
                        }
                    }
                    if (item.VehicleInfo.Parameters.Geofence_02 != null)
                    {
                        if (GeofenceAddedList.FirstOrDefault(x => x == item.VehicleInfo.Parameters.Geofence_02) == null && item.VehicleInfo.Parameters.Geofence_02.radius != 0 && item.VehicleInfo.Parameters.Geofence_02.Type != 0)
                        {
                            GeofenceAddedList.Add(item.VehicleInfo.Parameters.Geofence_02);
                        }
                    }
                    if (item.VehicleInfo.Parameters.Geofence_03 != null)
                    { 
                    if (GeofenceAddedList.FirstOrDefault(x => x == item.VehicleInfo.Parameters.Geofence_03) == null && item.VehicleInfo.Parameters.Geofence_03.radius != 0 && item.VehicleInfo.Parameters.Geofence_03.Type != 0)
                    {
                        GeofenceAddedList.Add(item.VehicleInfo.Parameters.Geofence_03);
                    }
                }
                    if (item.VehicleInfo.Parameters.Geofence_04 != null)
                    {
                        if (GeofenceAddedList.FirstOrDefault(x => x == item.VehicleInfo.Parameters.Geofence_04) == null && item.VehicleInfo.Parameters.Geofence_04.radius != 0 && item.VehicleInfo.Parameters.Geofence_04.Type != 0)
                        {
                            GeofenceAddedList.Add(item.VehicleInfo.Parameters.Geofence_04);
                        }
                    }
                    if (item.VehicleInfo.Parameters.Geofence_05 != null)
                    {
                        if (GeofenceAddedList.FirstOrDefault(x => x == item.VehicleInfo.Parameters.Geofence_05) == null && item.VehicleInfo.Parameters.Geofence_05.radius != 0 && item.VehicleInfo.Parameters.Geofence_05.Type != 0)
                        {
                            GeofenceAddedList.Add(item.VehicleInfo.Parameters.Geofence_05);
                        }
                    }

                    item.MapMarker.Shape = Marker;
               

                    int speedfCondition = SpeedCondition(item.VehicleInfo.Speed, item.VehicleInfo.Parameters.WarningSpeed, item.VehicleInfo.Parameters.OverSpeedTrip);

                    if (speedfCondition == (int)SpeedConditionEnum.OverSpeed)
                    {
                        Marker.Label_PopupSpeed.Foreground = new SolidColorBrush(Colors.Red);
                    }
                    else if (speedfCondition == (int)SpeedConditionEnum.WarningSpeed)
                    {
                        Marker.Label_PopupSpeed.Foreground = new SolidColorBrush(Colors.Yellow);
                    }
                    //GMap_GPRS.Position = new GMap.NET.PointLatLng(StartLat, StartLon);
                    else
                    {
                        Marker.Label_PopupSpeed.Foreground = new SolidColorBrush(Colors.White);

                    }
                    this.GMap_GPRS.Markers.Add(item.MapMarker);
                    StartLat = item.VehicleInfo.Latitude;
                    StartLon = item.VehicleInfo.Longitude;


                }
            }
            foreach (Geofence_Entry GeoItem in GeofenceAddedList)
            {
                MarkerEntry Geofence = new MarkerEntry();
                Geofence.MapMarker = new GMapMarker(new PointLatLng(GeoItem.Latitude, GeoItem.Longitude));

                if (GPSMapMarkers.Exists(p => p.MapMarker.LocalPositionX != Geofence.MapMarker.LocalPositionX && p.MapMarker.LocalPositionY != Geofence.MapMarker.LocalPositionY))
                {
                    Geofence.Scale = scalefactor / (Math.Pow(2, (ushort)GMap_GPRS.Zoom));
                    Geofence.Type = (int)MarkerType.Ellipse;
                    Geofence.VehicleInfo.Width = (ushort)GeoItem.radius;
                    Geofence.VehicleInfo.Length = (ushort)GeoItem.radius;
                    Geofence.MapMarker.Shape = new CustomMarkerEllipse(this.GMap_GPRS, Geofence);

                    this.GMap_GPRS.Markers.Add(Geofence.MapMarker);

                }
            }
        }

           
        private void GPRSMapView_Loaded(object sender, RoutedEventArgs e)
        {


            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;

            // choose your provider here
            //MainMap.MapProvider = GMap.NET.MapProviders.OpenStreetMapProvider.Instance;
             GMap_GPRS.MapProvider = GMap.NET.MapProviders.GoogleSatelliteMapProvider.Instance;
            //MainMap.MapProvider = GMap.NET.MapProviders.GoogleTerrainMapProvider.Instance;
            
            // whole world zoom
            GMap_GPRS.MinZoom = 3;
            GMap_GPRS.MaxZoom = 20;
            GMap_GPRS.Zoom = 13;

            // lets the map use the mousewheel to zoom
            GMap_GPRS.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionWithoutCenter;

            // lets the user drag the map
            GMap_GPRS.CanDragMap = true;
            //MainMap.ShowCenter = false;

            // lets the user drag the map with the left mouse button
            GMap_GPRS.DragButton = MouseButton.Left;

            GMap_GPRS.Position = new GMap.NET.PointLatLng(StartLat, StartLon);

            GMap_GPRS.ShowCenter = false;
        
        }

        private void Button_ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            GMap_GPRS.Zoom++;

            if (GMap_GPRS.Zoom == GMap_GPRS.MaxZoom)
            {
                Button_ZoomIn.IsEnabled = false;
            }

            if (GMap_GPRS.Zoom != GMap_GPRS.MinZoom)
            {
                Button_ZoomOut.IsEnabled = true;
            }


          
        }

        private void Button_ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            GMap_GPRS.Zoom--;

            if (GMap_GPRS.Zoom == GMap_GPRS.MinZoom)
            {
                Button_ZoomOut.IsEnabled = false;
            }

            if (GMap_GPRS.Zoom != GMap_GPRS.MaxZoom)
            {
                Button_ZoomIn.IsEnabled = true;
            }
        }

        private void GMap_GPRS_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            UpdateMapMarker();

            if (GMap_GPRS.Zoom == GMap_GPRS.MinZoom)
            {
                Button_ZoomOut.IsEnabled = false;
            }
            else
            {
                Button_ZoomOut.IsEnabled = true;
            }


            if (GMap_GPRS.Zoom == GMap_GPRS.MaxZoom)
            {
                Button_ZoomIn.IsEnabled = false;
            }
            else
            {
                Button_ZoomIn.IsEnabled = true;
            }

        }

      
        int Mapcounter =0;
        private void Button_MapType_Click(object sender, RoutedEventArgs e)
        {
            Mapcounter++;
            if (Mapcounter == 0)
            {
                GMap_GPRS.MapProvider = GMap.NET.MapProviders.GoogleSatelliteMapProvider.Instance;
            }
            else if(Mapcounter == 1)
            {
                GMap_GPRS.MapProvider = GMap.NET.MapProviders.GoogleHybridMapProvider.Instance;
            }
            else
            {
                GMap_GPRS.MapProvider = GMap.NET.MapProviders.GoogleTerrainMapProvider.Instance;
                Mapcounter = -1;
            }
   
        }

        private void Button_PrintMap_Click(object sender, RoutedEventArgs e)
        {
            // === Open Save File Dialog ===
            Microsoft.Win32.SaveFileDialog _saveFileDialog = new Microsoft.Win32.SaveFileDialog();

            // == Default extension ===
            _saveFileDialog.DefaultExt = "jpeg";
            // == filter types ===
            _saveFileDialog.Filter = "JPEG File (*.jpeg)|*.jpeg";
            _saveFileDialog.FileName = "Map";
            _saveFileDialog.FilterIndex = 2;
            _saveFileDialog.RestoreDirectory = true;
            if (_saveFileDialog.ShowDialog() == true)
            {
                RenderTargetBitmap renderTargetBitmap =
                 new RenderTargetBitmap
                 (
                     (int)Grid_GPRSMapView.ColumnDefinitions[1].ActualWidth + (int)Grid_GPRSMapView.ColumnDefinitions[2].ActualWidth + (int)Grid_GPRSMapView.ColumnDefinitions[3].ActualWidth + (int)Grid_GPRSMapView.ColumnDefinitions[4].ActualWidth + (int)Grid_GPRSMapView.ColumnDefinitions[5].ActualWidth,
                     (int)Grid_GPRSMapView.RowDefinitions[0].ActualHeight + (int)Grid_GPRSMapView.RowDefinitions[1].ActualHeight + (int)Grid_GPRSMapView.RowDefinitions[2].ActualHeight + (int)Grid_GPRSMapView.RowDefinitions[3].ActualHeight + (int)Grid_GPRSMapView.RowDefinitions[4].ActualHeight + (int)Grid_GPRSMapView.RowDefinitions[5].ActualHeight + (int)Grid_GPRSMapView.RowDefinitions[6].ActualHeight + (int)Grid_GPRSMapView.RowDefinitions[7].ActualHeight ,
                     96,
                     96,
                     PixelFormats.Pbgra32
                );
                renderTargetBitmap.Render(GMap_GPRS);
        
                PngBitmapEncoder pngImage = new PngBitmapEncoder();
                pngImage.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                using (Stream fileStream = File.Create(_saveFileDialog.FileName))
                {
                    pngImage.Save(fileStream);
                }
            }
        }

        private void Button_Information_Click(object sender, RoutedEventArgs e)
        {
            if(InformationActive)
            {
                InformationActive = false;
            }
            else
            {
                InformationActive = true;
            }
            UpdateMapMarker();
        }

        private void Datagrid_VehicleList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dg = sender as DataGrid;

            MarkerEntry VehicleItem = (MarkerEntry)dg.SelectedItem;
            GMap_GPRS.Position = new GMap.NET.PointLatLng(VehicleItem.VehicleInfo.Latitude, VehicleItem.VehicleInfo.Longitude);
        }

  
        private void Button_DatagridControl_Click(object sender, RoutedEventArgs e)
        {
            if(Datagrid_VehicleList.Visibility == Visibility.Hidden)
            {
                Datagrid_VehicleList.Visibility = Visibility.Visible;
                Button_DatagridControl.Margin = new Thickness(41, 20, 0, 0);
                ImageAwesome_UpArrow.Visibility = Visibility.Visible;
                ImageAwesome_DownArrow.Visibility = Visibility.Hidden;
            

            }
            else
            {
                Datagrid_VehicleList.Visibility = Visibility.Hidden;              
                Button_DatagridControl.Margin = new Thickness(41, -465, 0, 0);
                ImageAwesome_DownArrow.Visibility = Visibility.Visible;
                ImageAwesome_UpArrow.Visibility = Visibility.Hidden;
            }
          
        }
        bool touchActive = false;
        // private void DataGridRow_TouchDown(object sender, TouchEventArgs e)
        private void DataGridRow_TouchDown(object sender, TouchEventArgs e)
        {

            DataGridRow row = (DataGridRow)sender;
            if (row.IsSelected)
            {
                touchActive = false;
            }
            else
            {
                touchActive = true;
            }
        }

        private void DataGridRow_MouseMove(object sender, MouseEventArgs e)
        {
            if (touchActive)
            {
                DataGridRow row = (DataGridRow)sender;
                if (row.IsSelected)
                {
                    row.IsSelected = true;
                }
            }
        }

        private void DataGridRow_TouchMove(object sender, TouchEventArgs e)
        {



        }
        private void RowDoubleClick(object sender, RoutedEventArgs e)
        {
            var row = (DataGridRow)sender;
            //if (row.DetailsVisibility == Visibility.Collapsed)
            //{
            //    DataLogIsExpanded = true;

            //  //  Expander_Expanded(sender, e);
            //}
            //else
            //{
            //    DataLogIsExpanded = false;
            // //   Expander_Collapsed(sender, e);
            //}
            row.DetailsVisibility = row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

        }

    }
}
