using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
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
    /// Interaction logic for DataExtractor.xaml
    /// </summary>
    public partial class DataExtractorView : UserControl
    {

        // === Public Variables ===
        public const int DATALOG_RX_SIZE = 8192;
        public static bool Heartbeat = false;
        public static int DataIndex { get; set; }
        static bool _fileCreated = false;
        public static int TotalCount { get; set; }
        private uint SelectVID = 0;

        private static byte DatalogMode = 0;
        private enum DatalogModeEnum
        {
            None = 0,
            Analog = 'O', // 0x4F, //'O'
            Event = 'L' // 0x4c   //'L'
        }

        private enum TransferStatusEnum
        {
            None,
            Initialize,
            MovingLogs,
            ReadytoReceive,
            ReceivingLogs,
            Complete,
            Cancel

        }
        private static int TransferStatus = (int)TransferStatusEnum.None;
        // === Static Variables ===

        static string _savedFilesPath = System.IO.Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%"), "Documents") + "\\BHU Utility\\Datalogs\\";
        static string _newLogFilePath = "";
        static int StoredIndex = 0;

        bool ConnectionLost = false;
        private DispatcherTimer updateDispatcherTimer;
        private static int DataLogProgress = 0;
        private uint _heartBeatDelay = 0;
        static bool StartDataReceiving = false;
        private DateTime ExtractionStartTimeStamp;
        private DateTime ExtractionEndTimeStamp;
        /// <summary>
        /// DataExtractorView: The constructor function
        /// Setup required variables 
        /// </summary>
        public DataExtractorView()
        {
            InitializeComponent();
            Label_StatusView.Content = "Waiting for user command..";
            updateDispatcherTimer = new DispatcherTimer();
            updateDispatcherTimer.Tick += new EventHandler(InfoUpdater);
            updateDispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
       
        DataLogProgress = 0;
            DataIndex = 0;
            TotalCount = 0;
            Button_ViewLogs.Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// InfoUpdater
        /// Update screen information based on set interval by means of a timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InfoUpdater(object sender, EventArgs e)
        {

            if (GlobalSharedData.WiFiConnectionStatus)
            {

                TCPclientR _foundTCPClient = WiFiconfig.TCPclients.FirstOrDefault(t => t.VID == SelectVID);
                if (_foundTCPClient == null)
                {
                    Button_ExtractAnalogs.IsEnabled = false;
                    Button_ExtractEvents.IsEnabled = false;

                }
                else
                {
                    WiFiconfig.SelectedIP = _foundTCPClient.IP;

                   if(TransferStatus == (int)TransferStatusEnum.None)
                    {
                        Button_ExtractAnalogs.IsEnabled = true;
                        Button_ExtractEvents.IsEnabled = true;
                    }


                    //// === if device disconnect delete half complete log file and go back to view connections ===
                    //if (Visibility == Visibility.Visible && (WiFiconfig.clients.Count == 0 || WiFiconfig.clients.Where(t => t.Client.RemoteEndPoint.ToString() == WiFiconfig.SelectedIP).ToList().Count == 0))
                    //{
                    //    WiFiconfig.BusyReconnecting = true;

                    //    TransferStatus = (int)TransferStatusEnum.None;

                    //    Button_ExtractEvents.Content = "Events";
                    //    ConnectionLost = true;

                    //}
                    // === check if datalogs started, busy with extraction ===
                    if (TransferStatus != (int)TransferStatusEnum.Cancel && TransferStatus != (int)TransferStatusEnum.Complete && TransferStatus != (int)TransferStatusEnum.None)
                    {
                        // === check if datalogs are busy moving ===
                        if (TransferStatus == (int)TransferStatusEnum.MovingLogs)
                        {
                            ProgressBar_DataLogExtract.Value = DataLogProgress;
                            Label_ProgressStatusPercentage.Content = (DataLogProgress / 10).ToString() + "%";
                            Label_StatusView.Content = "Moving Logs to Flash";
                        }
                        // === else the datalogs are being received ===
                        else
                        {
                            ProgressBar_DataLogExtract.Value = DataLogProgress;
                            Label_ProgressStatusPercentage.Content = (DataLogProgress / 10).ToString() + "%";
                            if (DatalogMode == (byte)DatalogModeEnum.Event)
                            {
                                Label_StatusView.Content = "Event Log packet " + DataIndex.ToString() + " of " + TotalCount.ToString() + "...";
                            }
                            else
                            {
                                Label_StatusView.Content = "Analog Log packet " + DataIndex.ToString() + " of " + TotalCount.ToString() + "...";
                            }


                        }

                        // === check if heartbeat received ===
                        if (Heartbeat)
                        {

                            if (TransferStatus == (int)TransferStatusEnum.ReceivingLogs)
                            {
                                _heartBeatDelay++;

                                if (_heartBeatDelay > 5)
                                {
                                    if (DatalogMode == (byte)DatalogModeEnum.Analog)
                                    {
                                        GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&OR00]");
                                    }
                                    else
                                    {
                                        GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&LR00]");
                                    }
                                    // === clear heartbeat information and start over ===
                                    Heartbeat = false;
                                    _heartBeatDelay = 0;

                                    Debug.WriteLine("DataIndex: " + DataIndex.ToString());

                                }
                            }
                            else
                            {
                                _heartBeatDelay++;

                                if (_heartBeatDelay > 5)
                                {
                                    Heartbeat = false;
                                    _heartBeatDelay = 0;
                                    if (DatalogMode == (byte)DatalogModeEnum.Analog)
                                    {
                                        GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&OL00]");
                                    }
                                    else
                                    {
                                        GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&LL00]");
                                    }
                                }
                            }

                        }
                    }

                    // === else no datalogs are being extracted ===
                    else
                    {
                        if (Heartbeat)
                        {
                            Button_ExtractEvents.IsEnabled = true;
                            Button_ExtractAnalogs.IsEnabled = true;
                        }
                        // === if the extrection failed ===
                        if (TransferStatus == (int)TransferStatusEnum.Cancel)
                        {
                            // == clear information, show failed message, and delete half downloaded log file ===
                            Label_ProgressStatusPercentage.Content = "";

                            if (File.Exists(_newLogFilePath))
                            {
                                TransferStatus = (int)TransferStatusEnum.None;
                                Label_ProgressStatusPercentage.Content = "Process Cancelled...";
                                File.Delete(_newLogFilePath);
                            }
                        }
                        // === else the extraction was successful ===
                        if (TransferStatus == (int)TransferStatusEnum.Complete)
                        {
                            // === update information and save filepath for file view ===
                            Label_ProgressStatusPercentage.Content = "File Completed...";
                            TransferStatus = (int)TransferStatusEnum.None;
                            Button_ViewLogs.Visibility = Visibility.Visible;
                            GlobalSharedData.FilePath = _newLogFilePath;
                            ExtractionEndTimeStamp = DateTime.Now;
                            Debug.Write("Complete Time: " + new TimeSpan(ExtractionEndTimeStamp.Ticks - ExtractionStartTimeStamp.Ticks).ToString());
                        }

                        // === information to be cleared regardless of the logs extraction status ===
                        DataLogProgress = 0;
                        DataIndex = 0;

                        Button_Back.Content = "Back";
                        _fileCreated = false;
                        StartDataReceiving = false;
                        ProgressBar_DataLogExtract.Value = 0;
                        Label_StatusView.Content = "Waiting for user command..";
                    }
                }
            }
            else
            {
                Button_ExtractAnalogs.IsEnabled = false;
                Button_ExtractEvents.IsEnabled = false;

            }
            ConnectionInfoUpdate();
        }

        /// <summary>
        /// Button_Back_Click: Back Button click event
        /// Close view and open source(previous) view 
        /// Or
        /// Cancel datalog extraction
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Back_Click(object sender, RoutedEventArgs e)
        {
            // === if datalogs are busy extracting ===
            if (TransferStatus != (int)TransferStatusEnum.Complete && TransferStatus != (int)TransferStatusEnum.Cancel && TransferStatus != (int)TransferStatusEnum.None)
            {
                // Cancel datalogs and change text from cancel to back
                ExtractionEndTimeStamp = DateTime.Now;
                TransferStatus = (int)TransferStatusEnum.Cancel;
                Button_Back.Content = "Back";
                GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&LDs00]");
                StartDataReceiving = false;
                StoredIndex = -1;

                Debug.Write("Cancel Time: " + new TimeSpan(ExtractionEndTimeStamp.Ticks - ExtractionStartTimeStamp.Ticks).ToString());

            }
            // === else if datalogs are not busy extracting ===
            else
            {
                // === Close view and open source(previous) view ===
                this.Visibility = Visibility.Collapsed;
                ProgramFlow.ProgramWindow = ProgramFlow.SourseWindow;
            }
        }



        /// <summary>
        ///  DataExtractorParser
        ///  Parse the Wifi received information from the unit
        /// </summary>
        /// <param name="message"></param>
        /// <param name="endPoint"></param>
        public static void DataExtractorParser(byte[] message, EndPoint endPoint)
        {
            if (TransferStatus != (int)TransferStatusEnum.None)
            {
                // === Check if device is busy moving logs ===
                if ((message.Length >= 7) && (message[0] == '[') && (message[1] == '&') && (message[2] == DatalogMode) && (message[3] == 'k'))
                {
                    DataLogProgress = (50 * message[4]) / message[5];
                    Debug.WriteLine(message[4].ToString() + "-" + message[5].ToString() + "-" + DataLogProgress.ToString());
                    if (!StartDataReceiving)
                    {
                        TransferStatus = (int)TransferStatusEnum.MovingLogs;

                    }
                }

                // === Check if device is ready to receive ===
                if ((message.Length >= 7) && (message[0] == '[') && (message[1] == '&') && (message[2] == DatalogMode) && (message[3] == 'a'))
                {
                    TransferStatus = (int)TransferStatusEnum.ReadytoReceive;

                }

                // === Check if device is ready to receive ===
                if ((message.Length >= 9) && (message[0] == '[') && (message[1] == '&') && (message[2] == DatalogMode) && (message[3] == 'c'))
                {

                    byte[] StoredIndexBytes = BitConverter.GetBytes(StoredIndex);
                    byte[] Logchunk = Enumerable.Repeat((byte)0xFF, 10).ToArray();
                    Logchunk[0] = (byte)'[';
                    Logchunk[1] = (byte)'&';
                    Logchunk[2] = (byte)DatalogMode;
                    Logchunk[3] = (byte)'D';
                    Logchunk[4] = (byte)'a';
                    Logchunk[5] = StoredIndexBytes[0];
                    Logchunk[6] = StoredIndexBytes[1];
                    Logchunk[7] = 0;
                    Logchunk[8] = 0;
                    Logchunk[9] = (byte)']';

                    GlobalSharedData.ServerMessageSend = Logchunk;
                }

                // === Check if device is ready to send datalog file ===
                if ((message.Length >= 7) && (message[0] == '[') && (message[1] == '&') && (message[2] == DatalogMode) && (message[3] == 'D'))
                {
                    TransferStatus = (int)TransferStatusEnum.ReceivingLogs;

                    StartDataReceiving = true;
                    DataIndex = BitConverter.ToUInt16(message, 4);
                    TotalCount = BitConverter.ToUInt16(message, 6);
                    DataLogProgress = (DataIndex * 950) / TotalCount + 50;

                    // === check if datalog extraction has not started ===
                    if (TransferStatus == (int)TransferStatusEnum.Complete || TransferStatus == (int)TransferStatusEnum.Cancel)
                    {
                        // === Send  stop request ===
                        GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&" + DatalogMode.ToString() + "Ds00]");
                    }

                    //else if (DataIndex < TotalCount && DataIndex > StoredIndex) 
                    // === Check if received packet number is one more than the stored previous packet number ===
                    if (DataIndex == (StoredIndex + 1))
                    {

                        using (var stream = new FileStream(_newLogFilePath, FileMode.Append))
                        {
                            stream.Write(message, 8, DATALOG_RX_SIZE);
                        }

                        byte[] Logchunk = Enumerable.Repeat((byte)0xFF, 10).ToArray();

                        Logchunk[0] = (byte)'[';
                        Logchunk[1] = (byte)'&';
                        Logchunk[2] = (byte)DatalogMode;
                        Logchunk[3] = (byte)'D';
                        Logchunk[4] = (byte)'a';
                        Logchunk[5] = message[4];
                        Logchunk[6] = message[5];
                        Logchunk[7] = 0;
                        Logchunk[8] = 0;
                        Logchunk[9] = (byte)']';

                        GlobalSharedData.ServerMessageSend = Logchunk;
                        Debug.WriteLine("DataIndex: " + DataIndex.ToString());
                        StoredIndex = DataIndex;

                    }
                    // === Check if received packet number is the same as the total packet count ===
                    else if ((DataIndex-1) == TotalCount)
                    {
                       // using (var stream = new FileStream(_newLogFilePath, FileMode.Append))
                      //  {
                       //     stream.Write(message, 8, DATALOG_RX_SIZE);
                      //  }
                        GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&LDs00]");
                        TransferStatus = (int)TransferStatusEnum.Complete;
                        _fileCreated = false;
                    }
                    // === Check if the received packet number is the same as the stored previous packet number === 
                    else if (DataIndex == StoredIndex)
                    {

                        byte[] Logchunk = Enumerable.Repeat((byte)0xFF, 10).ToArray();
                        Logchunk[0] = (byte)'[';
                        Logchunk[1] = (byte)'&';
                        Logchunk[2] = (byte)DatalogMode;
                        Logchunk[3] = (byte)'D';
                        Logchunk[4] = (byte)'a';
                        Logchunk[5] = message[4];
                        Logchunk[6] = message[5];
                        Logchunk[7] = 0;
                        Logchunk[8] = 0;
                        Logchunk[9] = (byte)']';

                        GlobalSharedData.ServerMessageSend = Logchunk;
                        Debug.WriteLine("DataIndex: " + DataIndex.ToString());
                        // StoredIndex = -1;
                    }

                    else
                    {


                        byte[] StoredIndexBytes = BitConverter.GetBytes(StoredIndex);
                        byte[] Logchunk = Enumerable.Repeat((byte)0xFF, 10).ToArray();
                        Logchunk[0] = (byte)'[';
                        Logchunk[1] = (byte)'&';
                        Logchunk[2] = (byte)DatalogMode;
                        Logchunk[3] = (byte)'D';
                        Logchunk[4] = (byte)'a';
                        Logchunk[5] = StoredIndexBytes[0];
                        Logchunk[6] = StoredIndexBytes[1];
                        Logchunk[7] = 0;
                        Logchunk[8] = 0;
                        Logchunk[9] = (byte)']';

                        GlobalSharedData.ServerMessageSend = Logchunk;
                    }
                }
            }
        }

        /// <summary>
        /// UserControl_IsVisibleChanged: DataExtractorView isVisibleChanged event
        /// display/retreive information, start/stop timer according to visibility
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // === Check if DataExtractorView is visible ===
            if (this.Visibility == Visibility.Visible)
            {
                WiFiconfig.SelectedIP = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].IP;

                Label_DeviceName.Content = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].Name;
                Label_DeviceVID.Content = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].VID;
                SelectVID = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].VID;
                Label_Firmware.Content = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].FirmRev + "." + WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].FirmSubRev;
                Label_StatusView.Content = "Waiting for user command..";
                ProgressBar_DataLogExtract.Value = 0;
                Label_ProgressStatusPercentage.Content = "";
                StoredIndex = -1;
                _heartBeatDelay = 0;
                TransferStatus = (int)TransferStatusEnum.None;
                _fileCreated = false;
                updateDispatcherTimer.Start();
                ConnectionLost = false;
                StartDataReceiving = false;

            }
            // === else Dataextractor is not visible ===
            else
            {

                SelectVID = 0;
                TransferStatus = (int)TransferStatusEnum.None;
                Button_ViewLogs.Visibility = Visibility.Hidden;
                Button_ExtractEvents.Content = "Events";
                updateDispatcherTimer.Stop();
            }
            DataLogProgress = 0;
            DataIndex = 0;
        }

        /// <summary>
        /// Button_ViewLogs_Click: ViewLogs Button event
        /// Open the Datalogview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_ViewLogs_Click(object sender, RoutedEventArgs e)
        {
            // === Clear information ===
            DataLogProgress = 0;
            DataIndex = 0;

            // === If datalogs were successfull ===
            //if (TransferStatus == (int)TransferStatusEnum.Complete)
            //  {     
            // === Open Datalogview ===
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.DataLogView;
            //   }
        }

        void ConnectionInfoUpdate()
        {
            if (!GlobalSharedData.WiFiConnectionStatus)
            {
                UserControlSpinnerLoad_Disconnect.Visibility = Visibility.Visible;
                // Label_Lock.Visibility = Visibility.Visible;
                Label_DeviceConnection.Foreground = new SolidColorBrush(Colors.Red);
                Label_DeviceConnection.Content = "Disconnected";
            }
            else
            {
                Label_DeviceConnection.Content = "Active";
                Label_DeviceConnection.Foreground = new SolidColorBrush(Colors.Green);
                UserControlSpinnerLoad_Disconnect.Visibility = Visibility.Collapsed;
                // Label_Lock.Visibility = Visibility.Collapsed;
            }
        }
        /// <summary>
        /// Button_Extract_Click: Extract Button click event
        /// Start Extracting datalogs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_ExtractEvents_Click(object sender, RoutedEventArgs e)
        {
            TransferStatus = (int)TransferStatusEnum.Initialize;

            ExtractionStartTimeStamp = DateTime.Now;
            // === Update information and buttons ===
            Button_ExtractEvents.IsEnabled = false;
            Button_ExtractAnalogs.IsEnabled = false;
            Button_Back.Content = "Cancel";
            StartDataReceiving = false;
            Button_ViewLogs.Visibility = Visibility.Hidden;

            // === clear heartbeat information ===
            Heartbeat = false;
            _heartBeatDelay = 0;
            StoredIndex = 0;
            DatalogMode = (byte)DatalogModeEnum.Event;
            // === Send start datalog informaiton ===
            GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&LL00]");

            // === Check if log file is created ===
            if (!_fileCreated)
            {
                // === Create log file name ===
                String MacAddress = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].MACAddress.Replace(":", "");              
                _newLogFilePath = _savedFilesPath + "\\DataLog_Events_BooycoPDS_" + WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].VID.ToString() + "_" + MacAddress+ "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".Mer";
                int Filecount = 1;

                // === append a number if the log file already exists ===
                while (File.Exists(_newLogFilePath))
                {
                    if (Filecount > 1)
                    {
                        _newLogFilePath.Remove(_newLogFilePath.Length - 1);
                        _newLogFilePath += Filecount;
                    }
                    else
                    {
                        _newLogFilePath += Filecount;
                    }

                    if (Filecount >= 10)
                    {
                        break;
                    }
                }

                // === Create Log file ===
                File.Create(_newLogFilePath).Dispose();
                _fileCreated = true;
            }
        }
        private void Button_ExtractAnalogs_Click(object sender, RoutedEventArgs e)
        {
            TransferStatus = (int)TransferStatusEnum.Initialize;

            ExtractionStartTimeStamp = DateTime.Now;
            // === Update information and buttons ===
            Button_ExtractEvents.IsEnabled = false;
            Button_ExtractAnalogs.IsEnabled = false;
            Button_Back.Content = "Cancel";
            StartDataReceiving = false;
            Button_ViewLogs.Visibility = Visibility.Hidden;

            // === clear heartbeat information ===
            Heartbeat = false;
            _heartBeatDelay = 0;
            StoredIndex = 0;
            DatalogMode = (byte)DatalogModeEnum.Analog;
            // === Send start datalog informaiton ===
            GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&OL00]");

            // === Check if log file is created ===
            if (!_fileCreated)
            {
                // === Create log file name ===
                String MacAddress = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].MACAddress.Replace(":", "");          
                _newLogFilePath = _savedFilesPath + "\\DataLog_Analog_BooycoPDS_" + WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].VID.ToString() + "_" + MacAddress + "_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".Mer";
                int Filecount = 1;

                // === append a number if the log file already exists ===
                while (File.Exists(_newLogFilePath))
                {
                    if (Filecount > 1)
                    {
                        _newLogFilePath.Remove(_newLogFilePath.Length - 1);
                        _newLogFilePath += Filecount;
                    }
                    else
                    {
                        _newLogFilePath += Filecount;
                    }

                    if (Filecount >= 10)
                    {
                        break;
                    }
                }

                // === Create Log file ===
                File.Create(_newLogFilePath).Dispose();

            }
        }
    }
}
