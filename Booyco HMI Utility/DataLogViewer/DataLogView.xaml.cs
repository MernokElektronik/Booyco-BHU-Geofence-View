
using GMap.NET.WindowsPresentation;
using GMap.NET;
using ProximityDetectionSystemInfo;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.IO;
using OfficeOpenXml;
using ProximityDetectionSystemInfo;

namespace Booyco_HMI_Utility
{
    /// <summary>
    /// Interaction logic for DataExtractorView.xaml
    /// </summary>
    /// 
    public partial class DataLogView : UserControl
    {
        double Time_Offset = -1654606029;
        // === Public Variables ===
        public bool DataLogIsExpanded
        {
            get
            {
                return _dataLogIsExpanded;
            }
            set
            {
                _dataLogIsExpanded = value;
                OnPropertyChanged("DataLogIsExpanded");
            }
        }       
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        // === Private Variables ===
        private RangeObservableCollection<LogEntry> DataLogs;
        private string logFilename = "";
        private static BackgroundWorker backgroundWorkerReadFile = new BackgroundWorker();
        private DataLogManagement dataLogManager = new DataLogManagement();
        private bool _dataLogIsExpanded = false;
        FilterManagement FilterManager;
        private RangeObservableCollection<LogEntry> AnalogLogs = new RangeObservableCollection<LogEntry>();
        private RangeObservableCollection<LogEntry> EventLogs = new RangeObservableCollection<LogEntry>();
        bool IsToggleExpand = false;
        bool IsSelectAll = false;     
        private ExtendedWindow extendedWindow = new ExtendedWindow();
        int counter;
        bool IsButtonClickedSelectAll = false;
        /// <summary>
        /// DataLogView: The constructor function
        /// Setup required variables 
        /// </summary>
        public DataLogView()
        {
            InitializeComponent();
            DataLogs = new RangeObservableCollection<LogEntry>();
            DataGridLogs.AutoGenerateColumns = false;
            DataGridLogs.ItemsSource = DataLogs;
            DataGridLogs.IsReadOnly = true;
            dataLogManager.ReportProgressDelegate += backgroundWorkerReadFile.ReportProgress;
            backgroundWorkerReadFile.WorkerReportsProgress = true;
            backgroundWorkerReadFile.DoWork += new DoWorkEventHandler(ProcessLogFile);
            backgroundWorkerReadFile.ProgressChanged += new ProgressChangedEventHandler(backgroundWorkerProgressChanged);
            DataGridLogs.CommandBindings.Add(new CommandBinding(ApplicationCommands.SelectAll, SelectAll_Executed));
            FilterManager = new FilterManagement();
            DataLogIsExpanded = new bool();
                
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            if (this.Visibility == Visibility.Visible)
            {
                Grid_ProgressBar.Visibility = Visibility.Visible;
                this.ButtonSelectAll.Visibility = Visibility.Collapsed;
                this.ButtonToggleExpand.Visibility = Visibility.Collapsed;
                ProgressbarDataLogs.Visibility = Visibility.Visible;
                dataLogManager.AbortRequest = false;
                if (GlobalSharedData.FilePath != "" && File.Exists(GlobalSharedData.FilePath))
                {
                    logFilename = GlobalSharedData.FilePath;
                    if (!backgroundWorkerReadFile.IsBusy)
                    {
                        backgroundWorkerReadFile.RunWorkerAsync();
                    }
                }
                if (GlobalSharedData.AccessLevel == (int)AccessLevelEnum.Extended)
                {
                    TextBox_Offset.Visibility = Visibility.Visible;
                    Label_Offset.Visibility = Visibility.Visible;
                }
                else
                {
                    TextBox_Offset.Visibility = Visibility.Hidden;
                    Label_Offset.Visibility = Visibility.Hidden;
                }
              
            }
            else
            {
                if (!IsToggleExpand)
                {
                    if (ButtonToggleExpand.Content.ToString() == "Collapse")
                    {
                        ButtonToggleExpanded_Click(null, null);
                    }
                }
                if (IsSelectAll)
                {
                    ButtonSelectAll.Content = "Select All";
                    IsSelectAll = false;
                }
                TextBlockProgressStatus.Text = "";
                ProgressbarDataLogs.Value = 0;

            }
        }

        /// <summary>
        /// SelectAll_Executed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectAll_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
            if (counter++ % 2 == 0) //select on every other click
                dataGrid.SelectAll();
            else //and unselect on every other click
                dataGrid.UnselectAll();
        }
         
        private void ProcessLogFile(object sender, DoWorkEventArgs e)
        {
            dataLogManager.ReadFile(logFilename, Time_Offset);
            List<string> Combobox_EventsString = new List<string>();
            foreach (LPDEntry item in dataLogManager.ExcelFilemanager.LPDInfoList)
            {
                Combobox_EventsString.Add(item.EventName);

            }
       
         //   CheckComboBox_Events.ItemsSource = Combobox_EventsString.ToArray();
         //   Select_All_CheckComboBox(CheckComboBox_Events, true);
        }


        public void backgroundWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage > 100)
            {
                DataLogs.Clear();
                EventLogs.Clear();
                AnalogLogs.Clear();
                try
                { 
                foreach (LogEntry item in dataLogManager.TempList)
                {
                    if (item.EventID < 500)
                    {
                        EventLogs.Add(item);

                    }
                    else
                    {
                        AnalogLogs.Add(item);
                    }

                }
                DataLogs.AddRange(EventLogs);
                DataLogs.AddRange(AnalogLogs);
                dataLogManager.TempList.Clear();
                ButtonSave.IsEnabled = true;
                ProgressbarDataLogs.Visibility = Visibility.Collapsed;
                this.ButtonSelectAll.Visibility = Visibility.Visible;
                this.ButtonToggleExpand.Visibility = Visibility.Visible;
                Grid_ProgressBar.Visibility = Visibility.Collapsed;
            }
                catch
                {

                }

            }
            else
            {

                ProgressbarDataLogs.Value = e.ProgressPercentage;
                TextBlockProgressStatus.Text = "Upload (" + e.ProgressPercentage.ToString().PadLeft(3, '0') + "%)";
            }
        }

        private void ButtonSaveFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog _saveFileDialog = new Microsoft.Win32.SaveFileDialog();

            // == Default extension ===
            _saveFileDialog.DefaultExt = ".xlsx";
            // == filter types ===
            _saveFileDialog.Filter = "Excel File (*.xlsx)|*.xlsx";
            string _filename = logFilename.Split('\\').Last();
            _saveFileDialog.FileName = _filename.Remove(_filename.Length - 4, 4);
            _saveFileDialog.FilterIndex = 1;
            _saveFileDialog.RestoreDirectory = true;

            if (_saveFileDialog.ShowDialog() == true)
            {
                if (_saveFileDialog.FileName.Contains(".csv"))
                {
                    //if (!File.Exists(_saveFileDialog.FileName))
                    //{
                    //    File.Create(_saveFileDialog.FileName);
                    //}
                    // === open streamwrite to save file ===
                    StreamWriter writer = new StreamWriter(_saveFileDialog.FileName);
                    int counter = 0;



                    // === write each entry from data log in to .csv file ===
                    foreach (LogEntry _logEntry in DataLogs)
                    {
                        counter++;

                        writer.WriteLine(_logEntry.DateTime.Date + ";" +
                            _logEntry.DateTime.TimeOfDay + ";" +
                            _logEntry.EventID + ";" +
                            _logEntry.EventName + ";" +
                            _logEntry.RawEntry
                           //Single_Log_Data._EventInformation.ToString()
                           );

                        // === dispose streamwrite ===
                        writer.Dispose();
                        // === close stramwrite ===
                        writer.Close();
                    }

                }
                if (_saveFileDialog.FileName.Contains(".xlsx"))
                {
                    using (var p = new ExcelPackage())
                    {


                        //A workbook must have at least on cell, so lets add one... 
                        var ws = p.Workbook.Worksheets.Add("MySheet");

                        var dataRange = ws.Cells["A1"].LoadFromCollection
                       (
                       from s in DataLogs
                       orderby s.Number, s.EventName
                       select s,
                      true, OfficeOpenXml.Table.TableStyles.Medium2);

                        //To set values in the spreadsheet use the Cells indexer.

                        // === Header ===
                        ws.Cells[1, 1].Value = "No.";
                        ws.Cells[1, 2].Value = "Date";
                        ws.Cells[1, 3].Value = "Time ";
                        ws.Cells[1, 4].Value = "Event ID";
                        ws.Cells[1, 5].Value = "Event Name";
                        ws.Cells[1, 6].Value = "Event Description";
              

                        int count = 2;
                        foreach (LogEntry _logEntry in DataLogs)
                        {
                            ws.Cells[count, 1].Value = _logEntry.Number;
                            ws.Cells[count, 2].Value = _logEntry.DateTime.Date.ToString();
                            ws.Cells[count, 3].Value = _logEntry.DateTime.TimeOfDay.ToString();
                            ws.Cells[count, 4].Value = _logEntry.EventID;
                            ws.Cells[count, 5].Value = _logEntry.EventName;                          
                 
                            count++;
                        }
                        dataRange.AutoFitColumns();

                        //Save the new workbook. We haven't specified the filename so use the Save as method.
                        p.SaveAs(new FileInfo(_saveFileDialog.FileName));
                    }
                }



            }
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {

            ButtonSave.IsEnabled = false;
            dataLogManager.AbortRequest = true;

            DataLogs.Clear();
            this.Visibility = Visibility.Collapsed;
            if(ProgramFlow.SourseWindow ==  (int)ProgramFlowE.WiFi)
            {
                ProgramFlow.ProgramWindow = (int)ProgramFlowE.DataExtractorView;
            }
            else
            {
                ProgramFlow.ProgramWindow = ProgramFlow.SourseWindow;
            }
          
        }

        private void Datagrid_Logs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsSelectAll)
            {
                ButtonSelectAll.Content = "Select All";
                IsSelectAll = false;
            }
            if (DataGridLogs.SelectedItems.Count >= 1)
            {
                // ButtonMap.IsEnabled = true;
                ButtonDisplay.IsEnabled = true;
            }
            else
            {
                // ButtonMap.IsEnabled = false;
                ButtonDisplay.IsEnabled = false;
            }
            if (ThreadsIsSelected())
            {
                ButtonMap.IsEnabled = true;
                // ButtonDisplay.IsEnabled = true;
            }
            else
            {
                ButtonMap.IsEnabled = false;
                // ButtonDisplay.IsEnabled = false;
            }
            //IsButtonClickedSelectAll = false;
        }
        private void ButtonMap_Click(object sender, RoutedEventArgs e)
        {
            //MapWindow MapWindow = new MapWindow();
            //MapWindow.Show();

            //Window window = new Window
            //{
            //    Title = "Booyco HMI Utility: Map",
            //    Content = new MapView(),
            //    Height = 800,
            //    Width = 1280
            //};



            //window.Show();
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.Mapview;
            PlotThreads();

        }
        private RangeObservableCollection<ProximityDetectionEvent> ProximityDetectionEventList = new RangeObservableCollection<ProximityDetectionEvent>();
        private RangeObservableCollection<ProximityDetectionEvent> AnalogList = new RangeObservableCollection<ProximityDetectionEvent>();
        private bool ThreadsIsSelected()
        {

            uint Event_Count = 0;

            foreach (LogEntry item in DataGridLogs.SelectedItems)
            {
                if (item != null)
                {
                    if (item.EventID == 150 || item.EventID == 159 || item.EventID == 500)
                    {

                        Event_Count++;
                    }


                }

            }

            if (Event_Count > 0)
            {
                return true;
            }

            return false;

        }

        private void PlotThreads()
        {

            ProximityDetectionEvent TempEvent = new ProximityDetectionEvent();
         
            uint Event_Count = 0;
            ProximityDetectionEventList = new RangeObservableCollection<ProximityDetectionEvent>();
            AnalogList = new RangeObservableCollection<ProximityDetectionEvent>();
            foreach (LogEntry item in DataGridLogs.SelectedItems)
            {

                if (item != null)
                {


                    if ((item.EventID == 150 || item.EventID == 159) && ((BitConverter.ToInt32(item.RawData, 40)) != 0 && (BitConverter.ToInt32(item.RawData, 44)) != 0))

                    {
                        // TODO: Fix this two variables
                        TempEvent.DateTimeStamp = item.DateTime;
                        TempEvent.ThreatNumberStart = item.Number;
                        TempEvent.ThreatNumberStop = item.Number + 6;
                        TempEvent.PrimaryID = BitConverter.ToUInt32(item.RawData, 0);
                        TempEvent.ThreatTechnology = item.RawData[4];
                        //uint PDS_01_Group = Map_Information1.RawDataEntry[5];
                        TempEvent.ThreatType = item.RawData[6];
                        //TempEvent.ThreatDisplayWidth = (UInt16)(item.RawData[7] * 10);

                        Event_Count++;

                        TempEvent.ThreatDisplaySector = item.RawData[8];

                        if (item.EventID == 150)
                        {
                            TempEvent.ThreatDisplayZone = Convert.ToUInt16(item.DataList[(int)PDS_Index.Threat_Zone]);
                        }
                        else
                        {
                            TempEvent.ThreatDisplayZone = 0;
                        }
                        if (DataLogs.Count() == 0)
                        {

                        }
                        TempEvent.EventInfo = item.EventInfoList;
                        TempEvent.ThreatSpeed = item.DataList[(int)PDS_Index.Threat_Speed];
                        TempEvent.ThreatDistance = Convert.ToUInt16(item.DataList[(int)PDS_Index.Threat_Distance]);
                        TempEvent.ThreatHeading = item.DataList[(int)PDS_Index.Threat_Heading];
                        Event_Count++;

                        TempEvent.ThreatLatitude = item.DataList[(int)PDS_Index.Threat_LAT];
                        TempEvent.ThreatLongitude = item.DataList[(int)PDS_Index.Threat_LON];

                        Event_Count++;
                        TempEvent.ThreatHorizontalAccuracy = Convert.ToUInt16(item.DataList[(int)PDS_Index.Threat_Acc]);
                        TempEvent.ThreatPriority = Convert.ToUInt16(item.DataList[(int)PDS_Index.Threat_display_Priority]);
                        TempEvent.ThreatPOILOGDistance = item.DataList[(int)PDS_Index.POI_Distance];
                        TempEvent.CriticalDistance = Convert.ToUInt16(item.DataList[(int)PDS_Index.Critical_Distance]);
                        TempEvent.WarningDistance = Convert.ToUInt16(item.DataList[(int)PDS_Index.Warning_Distance]);
                        TempEvent.PresenceDistance = Convert.ToUInt16(item.DataList[(int)PDS_Index.Presence_Distance]);                    

                        Event_Count++;

                        TempEvent.UnitSpeed = item.DataList[(int)PDS_Index.Speed];
                        TempEvent.UnitHeading = item.DataList[(int)PDS_Index.Heading];
                        TempEvent.UnitHorizontalAccuracy = Convert.ToUInt16(item.DataList[(int)PDS_Index.Accuracy]);
                        TempEvent.ThreatDisplayWidth = Convert.ToUInt16(item.DataList[(int)PDS_Index.Threat_Width]);

                        Event_Count++;

                        TempEvent.UnitLatitude = item.DataList[(int)PDS_Index.LAT];
                        TempEvent.UnitLongitude = item.DataList[(int)PDS_Index.LON];
                        Event_Count++;

                        TempEvent.ThreatScenario = Convert.ToUInt16(item.DataList[(int)PDS_Index.Threat_Scenario]);
                        TempEvent.POILatitude = item.DataList[(int)PDS_Index.POI_LAT];
                        TempEvent.POILongitude = item.DataList[(int)PDS_Index.POI_LON];
                        TempEvent.ThreatBrakeDistance = item.DataList[(int)PDS_Index.Threat_Brake_Distance];
                       // TempEvent.POCDistance = Convert.ToUInt16(item.DataList[(int)PDS_Index.POC_Distance]);
                        Event_Count++;

                        try
                        {
                            TempEvent.POCLatitude = item.DataList[(int)PDS_Index.POC_LAT];
                            TempEvent.POCLongitude = item.DataList[(int)PDS_Index.POC_LON];
                        }
                        catch
                        {
                            TempEvent.POCLatitude =0;
                            TempEvent.POCLongitude = 0;
                        }
          
                        Event_Count++;
                        try
                        {
                            //Convert Geodetic decimal to UTM for Unit and POI
                            GeoUtility.GeoSystem.Geographic LatLon_Unit = new GeoUtility.GeoSystem.Geographic(TempEvent.UnitLongitude, TempEvent.UnitLatitude);
                            GeoUtility.GeoSystem.UTM UTM_Unit = (GeoUtility.GeoSystem.UTM)LatLon_Unit;
                            GeoUtility.GeoSystem.Geographic POI_LatLon = new GeoUtility.GeoSystem.Geographic(TempEvent.POILongitude, TempEvent.POILatitude);
                            GeoUtility.GeoSystem.UTM POI_UTM = (GeoUtility.GeoSystem.UTM)POI_LatLon;
                            GeoUtility.GeoSystem.Geographic POC_LatLon = new GeoUtility.GeoSystem.Geographic(TempEvent.POCLongitude, TempEvent.POCLatitude);
                            GeoUtility.GeoSystem.UTM POC_UTM = (GeoUtility.GeoSystem.UTM)POC_LatLon;
                            GeoUtility.GeoSystem.Geographic Threat_LatLon = new GeoUtility.GeoSystem.Geographic(TempEvent.ThreatLongitude, TempEvent.ThreatLatitude);
                            GeoUtility.GeoSystem.UTM Threat_UTM = (GeoUtility.GeoSystem.UTM)Threat_LatLon;
                            //Calculate distance between unit and POI
                            TempEvent.ThreatPOIUTMDistance = Math.Sqrt((Math.Pow((UTM_Unit.East - POI_UTM.East), 2) + Math.Pow((UTM_Unit.North - POI_UTM.North), 2)));
                            TempEvent.POCThreatDistance = Math.Sqrt((Math.Pow((POC_UTM.East - Threat_UTM.East), 2) + Math.Pow((POC_UTM.North - Threat_UTM.North), 2)));
                            TempEvent.POCUnitDistance = Math.Sqrt((Math.Pow((POC_UTM.East - UTM_Unit.East), 2) + Math.Pow((POC_UTM.North - UTM_Unit.North), 2)));
                            TempEvent.ThreatDistanceCalculated = Math.Sqrt((Math.Pow((UTM_Unit.East - Threat_UTM.East), 2) + Math.Pow((UTM_Unit.North - Threat_UTM.North), 2)));
                            Event_Count = 0;
                            ProximityDetectionEventList.Add(TempEvent);
                            TempEvent = new ProximityDetectionEvent();
                        }
                        catch
                        {

                        }

                    }

                    else if(item.EventID == 500)
                    {
                        try
                        {
                            TempEvent.DateTimeStamp = item.DateTime;
                            TempEvent.ThreatNumberStart = item.Number;
                            TempEvent.ThreatNumberStop = item.Number + 1;
                            TempEvent.UnitLatitude = item.DataList[0];
                            TempEvent.UnitLongitude = item.DataList[1];
                            TempEvent.UnitHeading = item.DataList[3];
                            TempEvent.EventInfo = item.EventInfoList;
                            TempEvent.ThreatSpeed = item.DataList[2];

                            TempEvent.ThreatDisplayZone = 20;

                            AnalogList.Add(TempEvent);
                            TempEvent = new ProximityDetectionEvent();
                        }
                        catch
                        {

                        }
                    }

                }

            }
            GlobalSharedData.PDSMapMarkers = new List<MarkerEntry>();
            double LastLat = 0;
            double LastLon = 0;
            foreach (ProximityDetectionEvent EventItem in ProximityDetectionEventList)
            {

                //if(TempEvent.ThreatTechnology == (int)Tech_Kind.Pulse_GPS || TempEvent.ThreatTechnology == (int)Tech_Kind_GPS)
                //{ 
                GeoUtility.GeoSystem.Geographic LatLon_ThreatUnit = new GeoUtility.GeoSystem.Geographic(EventItem.ThreatLongitude, EventItem.ThreatLatitude);
                GeoUtility.GeoSystem.UTM UTM_ThreatUnit = (GeoUtility.GeoSystem.UTM)LatLon_ThreatUnit;
                GeoUtility.GeoSystem.Geographic LatLon_Unit2 = new GeoUtility.GeoSystem.Geographic(EventItem.UnitLongitude, EventItem.UnitLatitude);
                GeoUtility.GeoSystem.UTM UTM_Unit2 = (GeoUtility.GeoSystem.UTM)LatLon_Unit2;

                double x_dif = (UTM_ThreatUnit.East - UTM_Unit2.East);
                double y_dif = (UTM_ThreatUnit.North - UTM_Unit2.North);
                string calculated_position = "";

                if (x_dif >= 0 && y_dif >= 0)
                {
                    calculated_position = "Front Right";
                }
                else if (x_dif < 0 && y_dif >= 0)
                {
                    calculated_position = "Front Left";
                }
                else if (x_dif < 0 && y_dif < 0)
                {
                    calculated_position = "Rear Left";
                }
                else if (x_dif > 0 && y_dif < 0)
                {
                    calculated_position = "Rear Right";
                }

                string PDS_Event_Information = "Data Entry (PDS): " + EventItem.ThreatNumberStart.ToString() + " - " + EventItem.ThreatNumberStop.ToString() + "\n" +
                                                "Timestamp: " + EventItem.DateTimeStamp.ToString() + " \n" +
                                                EventItem.EventInfo[0] + "\n" +         //Thread BID
                                                EventItem.EventInfo[1] + "\n" +         //THreat Kind
                                                EventItem.EventInfo[2] + "\n" +         //Threat Group
                                                EventItem.EventInfo[3] + "\n" +         //Threat Type
                                                EventItem.EventInfo[4] + "\n" +         //Threat Cluster
                                                EventItem.EventInfo[5] + "\n" +         //Threat Sector
                                                EventItem.EventInfo[6] + "\n" +         //Threat Zone
                                                EventItem.EventInfo[7] + "  (" + (EventItem.ThreatSpeed / 3.6).ToString("0.##") + "ms)" + "\n" +          //Threat Speed
                                                EventItem.EventInfo[8] + "\n" +         //Threat Distance
                                                EventItem.EventInfo[9] + "\n" +         //Threat Heading
                                                EventItem.EventInfo[10] + "\n" +         //Threat Latitude
                                                EventItem.EventInfo[11] + "\n" +         //Threat Longitude
                                                EventItem.EventInfo[12] + "\n" +
                                                EventItem.EventInfo[24] + "\n" +         //Threat brake distance
                                                "X: " + UTM_ThreatUnit.EastString + "\n" +
                                                "Y:" 
                                                ;         //Threat Acc

                string Unit_Information = "";

                try
                    { 
                Unit_Information = "Data Entry (PDS): " + EventItem.ThreatNumberStart.ToString() + " - " + EventItem.ThreatNumberStop.ToString() + "\n" +
                                            "Timestamp: " + EventItem.DateTimeStamp.ToString() + " \n" +
                                              EventItem.EventInfo[19] + "  (" + (EventItem.UnitSpeed / 3.6).ToString("0.##") + "ms)" + "\n" +         //Speed
                                              EventItem.EventInfo[20] + "\n" +         //Heading
                                              EventItem.EventInfo[21] + "\n" +         //Accuracy
                                                                                       // EventItem.EventInfo[22] + "\n" +        
                                                                                       // EventItem.EventInfo[23] + "\n" +         

                                              EventItem.EventInfo[25] + "\n" +         //LON
                                                EventItem.EventInfo[26] + "\n" +         //Lat

                                              EventItem.EventInfo[16] + "\n" +         //Presence distance
                                                EventItem.EventInfo[17] + "\n" +         //Warning distance
                                              EventItem.EventInfo[18] + "\n" +       //Critical distance
                                              EventItem.EventInfo[29] + "\n" +       //Scenario
                                              EventItem.EventInfo[32] + "\n" +       //Scenario Position
                                              EventItem.EventInfo[33] + "\n" 
                                              + UTM_ThreatUnit.NorthString + "\n" + "Threat Calculated Distance: " + EventItem.ThreatDistanceCalculated;       //Bearing
                }
                catch 
                {

                }

                string POI_Information = "Data Entry (PDS): " + EventItem.ThreatNumberStart.ToString() + " - " + EventItem.ThreatNumberStop.ToString() + "\n" +
                                          "Timestamp: " + EventItem.DateTimeStamp.ToString() + " \n" +
                                          "POI Distance (UTM Plot): " + EventItem.ThreatPOIUTMDistance.ToString("##,##00.00") + " m\n" +
                                          "POI Distance (Log): " + EventItem.ThreatPOILOGDistance.ToString() + " m\n" +
                                          "POI Latitude: " + EventItem.POILatitude.ToString() + " deg\n" +
                                          "POI Longitude: " + EventItem.POILongitude.ToString() + " deg" + "\n" +
                                          EventItem.EventInfo[29] + "\n";        //Scenario;


                string POC_Information = "Data Entry (PDS): " + EventItem.ThreatNumberStart.ToString() + " - " + EventItem.ThreatNumberStop.ToString() + "\n" +
                                          "Timestamp: " + EventItem.DateTimeStamp.ToString() + " \n" +
                                          "POC Latitude: " + EventItem.POCLatitude.ToString() + " deg\n" +
                                          "POC Longitude: " + EventItem.POCLongitude.ToString() + " deg\n" +
                                          "POC to Threat Distance: " + EventItem.POCThreatDistance.ToString("##,##00.00") + " m\n" +
                                          "POC to Unit Distance: " + EventItem.POCUnitDistance.ToString("##,##00.00") + " m";

                MarkerEntry PDSMarker1 = new MarkerEntry();
                MarkerEntry PDSMarker2 = new MarkerEntry();
                MarkerEntry PDSMarkerPOI = new MarkerEntry();
                MarkerEntry PDSMarkerPOC = new MarkerEntry();


                int LastGeofenceIndex = -1;

                if (EventItem.ThreatTechnology == (int)Tech_Kind.Pulse_GPS && EventItem.ThreatType != 7)
                {
                    PDSMarker1.VehicleInfo.Heading = EventItem.ThreatHeading;
                    PDSMarker1.VehicleInfo.Zone = 10;
                    PDSMarker1.title = PDS_Event_Information;
                    PDSMarker1.Type = (int)MarkerType.Indicator;
                    PDSMarker1.VehicleInfo.BrakeDistance = EventItem.ThreatBrakeDistance;
                    PDSMarker1.VehicleInfo.Heading = EventItem.ThreatHeading;
                    PDSMarker1.VehicleInfo.Accuracy = EventItem.UnitHorizontalAccuracy;
                }
                else
                {
                    PDSMarker1.title = PDS_Event_Information;
                    PDSMarker1.VehicleInfo.Width = 3;
                    PDSMarker1.VehicleInfo.Length = 3;
                    PDSMarker1.Type = (int)MarkerType.Ellipse;
                }

                if (EventItem.ThreatScenario == 8)
                {
                    PDSMarker1.title = PDS_Event_Information;
                    PDSMarker1.VehicleInfo.Width = EventItem.ThreatDisplayWidth;
                    PDSMarker1.VehicleInfo.Length = EventItem.ThreatDisplayWidth;

                    PDSMarker1.Type = (int)MarkerType.Ellipse;
                }
                PDSMarker2.VehicleInfo.Width = 12;
                PDSMarker2.VehicleInfo.Length = 16;

                LastGeofenceIndex = GlobalSharedData.PDSMapMarkers.FindLastIndex(x => x.MapMarker.Position.Lat == EventItem.ThreatLatitude && x.MapMarker.Position.Lng == EventItem.ThreatLongitude && x.Type == (int)MarkerType.Ellipse);

                PDSMarker1.MapMarker = new GMapMarker(new PointLatLng(EventItem.ThreatLatitude, EventItem.ThreatLongitude));
                PDSMarker2.MapMarker = new GMapMarker(new PointLatLng(EventItem.UnitLatitude, EventItem.UnitLongitude));
                PDSMarkerPOI.MapMarker = new GMapMarker(new PointLatLng(EventItem.POILatitude, EventItem.POILongitude));
                PDSMarkerPOC.MapMarker = new GMapMarker(new PointLatLng(EventItem.POCLatitude, EventItem.POCLongitude));

                PDSMarker2.VehicleInfo.Heading = EventItem.UnitHeading;
                PDSMarker2.VehicleInfo.Zone = EventItem.ThreatDisplayZone;
                PDSMarker2.title = Unit_Information;
                PDSMarker2.VehicleInfo.Accuracy = EventItem.UnitHorizontalAccuracy;
                PDSMarker2.VehicleInfo.PresenceZoneSize = EventItem.PresenceDistance + 1;
                PDSMarker2.VehicleInfo.WarningZoneSize = EventItem.WarningDistance + 1;
                PDSMarker2.VehicleInfo.CriticalZoneSize = EventItem.CriticalDistance + 1;
                PDSMarker2.Type = (int)MarkerType.Indicator;

                PDSMarkerPOI.VehicleInfo.Zone = EventItem.ThreatDisplayZone;
                PDSMarkerPOI.title = POI_Information;
                PDSMarkerPOI.Type = (int)MarkerType.Point;

                PDSMarkerPOC.VehicleInfo.Zone = EventItem.ThreatDisplayZone;
                PDSMarkerPOC.title = POC_Information;
                PDSMarkerPOC.Type = (int)MarkerType.Cross;

                if (LastGeofenceIndex == -1)
                {
                    GlobalSharedData.PDSMapMarkers.Add(PDSMarker1);
                }
                PDSMarker2.VehicleInfo.Latitude = EventItem.UnitLatitude;
                PDSMarker2.VehicleInfo.Longitude = EventItem.UnitLongitude;
                GlobalSharedData.PDSMapMarkers.Add(PDSMarker2);
                GlobalSharedData.PDSMapMarkers.Add(PDSMarkerPOI);
                if (EventItem.ThreatScenario == 4 || EventItem.ThreatScenario == 5)
                {
                    GlobalSharedData.PDSMapMarkers.Add(PDSMarkerPOC);
                }
                }
            
            foreach (ProximityDetectionEvent AnalogItem in AnalogList)
            {
                string Unit_Information = "Data Entry (PDS): " + AnalogItem.ThreatNumberStart.ToString() + " - " + AnalogItem.ThreatNumberStop.ToString() + "\n" +
                                        "Timestamp: " + AnalogItem.DateTimeStamp.ToString() + " \n" +
                                          AnalogItem.EventInfo[2] + "  (" + (AnalogItem.UnitSpeed / 3.6).ToString("0.##") + "ms)" + "\n" +         //Speed
                                          AnalogItem.EventInfo[3] + "\n" +         //Heading
                                          AnalogItem.EventInfo[0] + "\n" +         //Lat
                                          AnalogItem.EventInfo[1] + "\n";        //Lon
                                                     
                MarkerEntry AnalogUnitMarker = new MarkerEntry();
                AnalogUnitMarker.MapMarker = new GMapMarker(new PointLatLng(AnalogItem.UnitLatitude, AnalogItem.UnitLongitude));
                AnalogUnitMarker.title = Unit_Information;
                AnalogUnitMarker.VehicleInfo.Heading = AnalogItem.UnitHeading;
                AnalogUnitMarker.VehicleInfo.Latitude = AnalogItem.UnitLatitude;
                AnalogUnitMarker.VehicleInfo.Longitude = AnalogItem.UnitLongitude;
                AnalogUnitMarker.VehicleInfo.Zone = AnalogItem.ThreatDisplayZone;
                GlobalSharedData.PDSMapMarkers.Add(AnalogUnitMarker);
            }
                extendedWindow.MapView.UpdateMapMarker();
            
        }

        public void DisplayWindowMap()
        {

            if (GlobalSharedData.ViewMode == true)
            {

                extendedWindow.Visibility = Visibility.Visible;
                GlobalSharedData.ViewMode = false;
            }
            try
            {
                if (extendedWindow.MapView.CloseRequest)
                {
                    extendedWindow.Visibility = Visibility.Collapsed;
                    extendedWindow.MapView.CloseRequest = false;
                    // extendedWindow.Close();
                }
            }
            catch
            {

            }
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
        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                if (vis is DataGridRow)
                {
                    var row = (DataGridRow)vis;
                    row.DetailsVisibility = row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    break;
                }
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
                if (vis is DataGridRow)
                {
                    var row = (DataGridRow)vis;
                    row.DetailsVisibility = row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
                    break;
                }
        }

       
        private void ButtonToggleExpanded_Click(object sender, RoutedEventArgs e)
        {
            //for (int i = 0; i < DataGridLogs.Items.Count; i++)
            //{
            //    DataGridRow row = (DataGridRow)DataGridLogs.ItemContainerGenerator
            //                                               .ContainerFromIndex(i);
            //    if (row != null)
            //    { 
            //    row.DetailsVisibility = row.DetailsVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            //}
            //}

            if (DataGridLogs.Columns[5].Visibility == Visibility.Visible)
            {
                DataGridLogs.Columns[5].Visibility = Visibility.Collapsed;
                ButtonToggleExpand.Content = "Expand";
                IsToggleExpand = true;
            }
            else
            {
                DataGridLogs.Columns[5].Visibility = Visibility.Visible;
                ButtonToggleExpand.Content = "Collapse";
                IsToggleExpand = false;
            }



        }

        private void PreviewKeyDown_Filter_Key(object sender, KeyEventArgs e)
        {
            // == check if enter is pressed ===
            if (e.Key == Key.Enter)
            {
                // === if enter is pressed filter data logs ===
                //Button_Filter_Click(null, null);
            }

        }
        /// <summary>
        /// CheckComboBox mouse Enter event
        ///  - add border to checkcombobox when mouse enters the user control(CheckComboBox)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckComboBox_Events_Mouse_Enter(object sender, MouseEventArgs e)
        {
            // CheckComboBox_Events.BorderThickness = new Thickness(1.0);
        }

        /// <summary>
        /// CheckComboBox mouse exit event
        ///  - remove border to checkcombobox when mouse leaves the user control (CheckComboBox)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckComboBox_Events_Mouse_Leave(object sender, MouseEventArgs e)
        {
            //CheckComboBox_Events.BorderThickness = new Thickness(0.0);
        }
        /// <summary>
        /// Check if "Select All" entry is selected in CheckComboBox_Events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckComboBox_Events_ItemSelectionChanged(object sender, Xceed.Wpf.Toolkit.Primitives.ItemSelectionChangedEventArgs e)
        {
            string selectedItem = e.Item as string; //cast to the type in the ItemsSource
            if (selectedItem == "Select All" && e.IsSelected)
            {
                // Select_All_CheckComboBox(CheckComboBox_Events, true);
            }
            if (selectedItem == "Select All" && !e.IsSelected)
            {
                //Select_All_CheckComboBox(CheckComboBox_Events, false);
            }
        }
        /// <summary>
        /// This function is used to select/unslect all the entries in a CheckCombobox.
        /// </summary>
        /// <param name="ChekComboBox_Temp"></param>
        /// <param name="Checked"></param>
        private void Select_All_CheckComboBox(Xceed.Wpf.Toolkit.CheckComboBox CheckComboBox_Temp, bool Checked)
        {
            // === If checked is true ===
            if (Checked)
            {

                // === check each checkbox in the checkCombobox ===
                foreach (var item in CheckComboBox_Temp.Items)
                {
                    if (!CheckComboBox_Temp.SelectedItems.Contains(item))
                    {
                        CheckComboBox_Temp.SelectedItems.Add(item);
                    }
                }
            }

            // === if check is false ===
            else
            {
                // === uncheck each checkbox in the checkcombobox ===
                foreach (var item in CheckComboBox_Temp.Items)
                {
                    if (CheckComboBox_Temp.SelectedItems.Contains(item))
                    {
                        CheckComboBox_Temp.SelectedItems.Remove(item);
                    }
                }
            }
        }



        private void ButtonDisplay_Click(object sender, RoutedEventArgs e)
        {

            GlobalSharedData.HMIDisplayList.Clear();
            DateTime _clearTime = new DateTime(2100, 01, 01);
            bool RadarCheck = true;
            List<LogEntry> _tempList = new List<LogEntry>();
            foreach (LogEntry item in DataGridLogs.SelectedItems)
            {
                _tempList.Add(item);
                //=== check if selected is only radar event
                if(item.EventID != 168)
                {                    
                    RadarCheck = false;
                }
            }

            //=== if selected data is not only radar plot normally
            if (!RadarCheck)
            {
                GlobalSharedData.OnlyRadarSelected = false;
                DateTime StartSelectedDateTime = new DateTime(2100, 01, 01);
                DateTime EndSelectedDateTime = new DateTime(1700, 01, 01);

                foreach (LogEntry item in DataGridLogs.SelectedItems)
                {
                    if (item.DateTime < StartSelectedDateTime)
                    {
                        StartSelectedDateTime = item.DateTime;
                    }
                    if (item.DateTime > EndSelectedDateTime)
                    {
                        EndSelectedDateTime = item.DateTime;
                    }
                }

                GlobalSharedData.EndDateTimeDatalog = EndSelectedDateTime;
                GlobalSharedData.StartDateTimeDatalog = StartSelectedDateTime;

                List<LogEntry> _sortedList = DataLogs.OrderBy(a => a.DateTime).ToList();

                foreach (LogEntry item in _sortedList)
                {
                    HMIDisplayEntry _tempHMIDisplayEntry = new HMIDisplayEntry();
                    PDSThreatEvent _tempPDSThreatEvent = new PDSThreatEvent();

                    try
                    {
                        if (item.EventID == 150)
                        {
                            _tempPDSThreatEvent.ThreatBIDHex = item.DataListString.ElementAt(0);
                            _tempPDSThreatEvent.ThreatBID = uint.Parse(item.DataListString.ElementAt(0).Remove(0, 2), System.Globalization.NumberStyles.HexNumber).ToString();
                            _tempPDSThreatEvent.ThreatGroup = item.DataListString.ElementAt(2);
                            _tempPDSThreatEvent.ThreatType = item.DataListString.ElementAt(3);
                            _tempPDSThreatEvent.ThreatWidth = item.DataListString.ElementAt(4);
                            _tempPDSThreatEvent.ThreatSector = item.DataListString.ElementAt(5);
                            _tempPDSThreatEvent.ThreatZone = Convert.ToInt16(item.DataList.ElementAt(6));
                            _tempPDSThreatEvent.ThreatDistance = item.DataListString.ElementAt(8);
                            _tempPDSThreatEvent.ThreatHeading = item.DataListString.ElementAt(9);
                            _tempPDSThreatEvent.DateTime = item.DateTime;

                            HMIDisplayEntry _foundItem = GlobalSharedData.HMIDisplayList.FindLast(p => p.ThreatBID == uint.Parse(item.DataListString.First().Remove(0, 2), System.Globalization.NumberStyles.HexNumber).ToString());


                            if (_foundItem != null)
                            {
                                if (_foundItem.EndDateTime != _clearTime)
                                {
                                    _tempHMIDisplayEntry.StartDateTime = item.DateTime;
                                    _tempHMIDisplayEntry.ThreatBID = uint.Parse(item.DataListString.First().Remove(0, 2), System.Globalization.NumberStyles.HexNumber).ToString();
                                    _tempHMIDisplayEntry.ThreatPriority = item.DataListString.ElementAt(15);
                                    _tempHMIDisplayEntry.PDSThreat.Add(_tempPDSThreatEvent);
                                    _tempHMIDisplayEntry.EndDateTime = new DateTime(2100, 01, 01);
                                    GlobalSharedData.HMIDisplayList.Add(_tempHMIDisplayEntry);

                                }
                                else
                                {
                                    _foundItem.ThreatPriority = item.DataListString.ElementAt(15);
                                    _foundItem.PDSThreat.Add(_tempPDSThreatEvent);

                                }
                            }
                            else
                            {
                                _tempHMIDisplayEntry.StartDateTime = item.DateTime;
                                _tempHMIDisplayEntry.ThreatBID = uint.Parse(item.DataListString.First().Remove(0, 2), System.Globalization.NumberStyles.HexNumber).ToString();
                                _tempHMIDisplayEntry.ThreatPriority = item.DataListString.ElementAt(15);
                                _tempHMIDisplayEntry.PDSThreat.Add(_tempPDSThreatEvent);
                                _tempHMIDisplayEntry.EndDateTime = new DateTime(2100, 01, 01);
                                GlobalSharedData.HMIDisplayList.Add(_tempHMIDisplayEntry);
                            }
                        }



                        if (item.EventID == 159)
                        {

                            if (uint.Parse(item.DataListString.First().Remove(0, 2), System.Globalization.NumberStyles.HexNumber) == 0)
                            {

                                HMIDisplayEntry _foundItem1 = GlobalSharedData.HMIDisplayList.FindLast(p => p.ThreatPriority == item.DataListString.ElementAt(15));

                                if (_foundItem1 != null && _foundItem1.EndDateTime > item.DateTime)
                                {
                                    _foundItem1.EndDateTime = item.DateTime;
                                }

                            }
                            else
                            {
                                HMIDisplayEntry _foundItem = GlobalSharedData.HMIDisplayList.FindLast(p => p.ThreatBID == uint.Parse(item.DataListString.First().Remove(0, 2), System.Globalization.NumberStyles.HexNumber).ToString());
                                if (_foundItem != null)
                                {
                                    _foundItem.EndDateTime = item.DateTime;
                                }
                            }
                        }

                        if (item.EventID == 2)
                        {


                            HMIDisplayEntry _foundItem1 = GlobalSharedData.HMIDisplayList.FindLast(p => p.ThreatPriority == "1");

                            if (_foundItem1 != null && _foundItem1.EndDateTime > item.DateTime)
                            {
                                _foundItem1.EndDateTime = item.DateTime;
                            }

                            HMIDisplayEntry _foundItem2 = GlobalSharedData.HMIDisplayList.FindLast(p => p.ThreatPriority == "2");

                            if (_foundItem2 != null && _foundItem1.EndDateTime > item.DateTime)
                            {
                                _foundItem2.EndDateTime = item.DateTime;
                            }

                            HMIDisplayEntry _foundItem3 = GlobalSharedData.HMIDisplayList.FindLast(p => p.ThreatPriority == "3");

                            if (_foundItem3 != null && _foundItem1.EndDateTime > item.DateTime)
                            {
                                _foundItem3.EndDateTime = item.DateTime;
                            }

                            HMIDisplayEntry _foundItem4 = GlobalSharedData.HMIDisplayList.FindLast(p => p.ThreatPriority == "4");

                            if (_foundItem4 != null && _foundItem1.EndDateTime > item.DateTime)
                            {
                                _foundItem4.EndDateTime = item.DateTime;
                            }

                            HMIDisplayEntry _foundItem5 = GlobalSharedData.HMIDisplayList.FindLast(p => p.ThreatPriority == "5");

                            if (_foundItem5 != null && _foundItem1.EndDateTime > item.DateTime)
                            {
                                _foundItem5.EndDateTime = item.DateTime;
                            }
                        }
                    }
                    catch
                    {

                    }
                }
            }
            // === else plot only radar
            else
            {
                int testnow = 0;
                GlobalSharedData.OnlyRadarSelected = true;
                GlobalSharedData.HMIRadarDisplayList = new List<HMIRadarDisplayEntry>();
                foreach (LogEntry item in _tempList)
                {
                    HMIRadarDisplayEntry _tempEntry = new HMIRadarDisplayEntry();

                    _tempEntry.ThreatID = (int)item.DataList.ElementAt(0);
                    _tempEntry.ThreatCordinateX = (int)item.DataList.ElementAt(1);
                    _tempEntry.ThreatCordinateY = (int)item.DataList.ElementAt(2);
                    _tempEntry.ThreatWidth = (int)item.DataList.ElementAt(3);
                    _tempEntry.ThreatDistance = (int)item.DataList.ElementAt(4);

                    GlobalSharedData.HMIRadarDisplayList.Add(_tempEntry);
                }
                       
            }
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.HMIDisplayView;
        }


        private void ButtonSelectAll_Click(object sender, RoutedEventArgs e)
        {
            if (IsSelectAll)
            {
                ButtonSelectAll.Content = "Select All";
                DataGridLogs.UnselectAll();
                IsSelectAll = false;
              
            }
            else
            {
                ButtonSelectAll.Content = "Unselect All";
                DataGridLogs.SelectAll();
                IsSelectAll = true;
                IsButtonClickedSelectAll = true; 
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
        /// <summary>
        /// filter button to start filtering the data logs.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Filter_Click(object sender, RoutedEventArgs e)
        {
            FilterManager.Filter_CollectionView = CollectionViewSource.GetDefaultView(DataGridLogs.ItemsSource);
            FilterManager.Filter_CollectionView = CollectionViewSource.GetDefaultView(DataGridLogs.ItemsSource);
            FilterManager.Events_Selected = CheckComboBox_Events.SelectedItems.Cast<String>().ToList();
            FilterManager.Filter();
            // === start background worker to filter data === 

            //Filter_Manager.Filter_CollectionView = CollectionViewSource.GetDefaultView(DataGrid_Log.ItemsSource);

            //Filter_Manager.Total_Log_Entries = Data_Log_Management.Total_Log_Entries;
            //Filter_Manager.Filter_Text = TextBox_EventInformationFilter.Text;
            //Filter_Manager.RawDataFilter_Text = TextBox_RawDataFilter.Text;
            //Filter_Manager.Events_Selected = CheckComboBox_Events.SelectedItems.Cast<String>().ToList();

            //BackgroundWorker_Filter.RunWorkerAsync();

        }


        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
          
          
            //backgroundWorkerReadFile.DoWork += new DoWorkEventHandler(ProcessLogFile);


        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textbox = sender as TextBox;

            try
                {
                Time_Offset = Convert.ToDouble(textbox.Text);
                DataLogs.Clear();
                backgroundWorkerReadFile.RunWorkerAsync();
            }
            catch
            { }
       

        }
    }
}
