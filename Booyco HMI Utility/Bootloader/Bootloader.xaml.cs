using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
    /// Interaction logic for Bootloader.xaml
    /// </summary>
    public partial class Bootloader : UserControl, INotifyPropertyChanged
    {

        Int32 Bootloader3_size = 38176;
        Int32 Bootloader2_size = 512;
        #region OnProperty Changed
        /////////////////////////////////////////////////////////////
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /////////////////////////////////////////////////////////////
        #endregion
        private DispatcherTimer dispatcherTimer;

        List<byte[]> BootFileList = new List<byte[]>();

        #region Bootloader static dependants
        public static bool BootStart { get; set; }

        public static int BootFlashPersentage { get; set; }

        public static bool BootReady { get; set; }

        public static bool BootStop { get; set; }

        public static string BootStatus { get; set; }

        public static bool BootDone { get; set; }

        public static int BootSentIndex { get; set; }

        public static bool bootContinue;

        public static int BootAckIndex { get; set; }

        public static string FileErrorMessage { get; set; }

        public static bool FileError { get; set; }
              
        static int bootchunks = 0;

        private static Thread BootloaderThread;


        #endregion

        /// <summary>
        /// Bootloader Constructor
        /// </summary>
        public Bootloader()
        {
            DataContext = this;
            BootBtnEnabled = false;
            InitializeComponent();
            BootFlashPersentage = 0;
            BootStatusView = "Waiting instructions..";
            LicenseBool = false;
            //BootStop = false;
        }

        /// <summary>
        /// Bootloader usercontrol on visibility changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                SureMessageVis = Visibility.Collapsed;
                BootStatusView = BootStatus = "Waiting for instructions...";
                BootBtnEnabled = (bootfile == null || bootfile == "") ? false : true;
                SelectFilebtnEnab = true;
                BackbtnText = "Back";
                DeviceName = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].Name;
                DeviceVID = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].VID;
                FirmwareRev = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].FirmRev;
                FirmSub = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].FirmSubRev;
                LicenseBool = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].Licensed;
                SelectedApplication = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].ApplicationState;
                if(SelectedApplication == "ERB BT")
                {
                    FirmwareApp = 57;
                }
                else if (SelectedApplication == "BHU BT")
                {
                    FirmwareApp = 56;
                }
                else
                {
                    FirmwareApp = 69;
                }

                FirmwareString = "M-PFW-" + ((FirmwareApp < 100)?"0"+ FirmwareApp.ToString() : FirmwareApp.ToString()) + "-" + 
                    ((FirmwareRev < 10) ? "0" + FirmwareRev.ToString() : FirmwareRev.ToString()) + "-" +
                    ((FirmSub < 10) ? "0" + FirmSub.ToString() : FirmSub.ToString());

                if (FirmwareApp == 56)
                {
                    _FirmwareTypeString = "BHU";
                }
                else if (FirmwareApp == 69)
                {
                    _FirmwareTypeString = "Communication Bridge";
                }

                WiFiconfig.SelectedIP = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].IP;

                dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Tick += new EventHandler(InfoUpdater);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
                dispatcherTimer.Start();
                BootFlashPersentage = 0;
            }
            else
            {
                BootReady = false;
                BootStop = true;
                dispatcherTimer.Stop();
                BootloadingProgress.Value = 0;

            }
        }

        /// <summary>
        /// Update information with a timer threat.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InfoUpdater(object sender, EventArgs e)
        {
            if(BootDone && Visibility == Visibility.Visible)
            {
                ButtonBack_Click(null, null);
            }
            else if(Visibility == Visibility.Visible && (WiFiconfig.clients.Count == 0 || WiFiconfig.clients.Where(t=> t.Client.RemoteEndPoint.ToString() == WiFiconfig.SelectedIP).ToList().Count == 0))
            {
                WiFiconfig.ConnectionError = true;
                ButtonBack_Click(null, null);
                BootReady = false;
            }
            else if (WiFiconfig.clients.Where(t => t.Client.RemoteEndPoint.ToString() == WiFiconfig.SelectedIP).ToList().Count > 0)
            {
                DeviceName = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].Name;
                DeviceVID = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].VID;
                FirmwareRev = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].FirmRev;
                FirmSub = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].FirmSubRev;
                FirmwareApp = 56;

                FirmwareString = "M-PFW-" + ((FirmwareApp < 100) ? "0" + FirmwareApp.ToString() : FirmwareApp.ToString()) + "-" +
                    ((FirmwareRev < 10) ? "0" + FirmwareRev.ToString() : FirmwareRev.ToString()) + "-" +
                    ((FirmSub < 10) ? "0" + FirmSub.ToString() : FirmSub.ToString());
            }

            if (bootchunks > 0 && !BootDone && BootFlashPersentage>0)
            {
                BootloadingProgress.Value = (BootSentIndex+ BootFlashPersentage/10) / ((double)bootchunks+10) * 1000;
                if(BootSentIndex>0)
                    BootFlashPersentage = 100;
            }                
            else
                BootloadingProgress.Value = 0;

            if(!BootDone && BootFlashPersentage > 0 && BootloadingProgress.Value != BootloadingProgress.Maximum)
            {
                SelectFilebtnEnab = BootBtnEnabled = false;
                BackbtnText = "Cancel";
            }

            

            BootStatuspersentage = (Math.Round(BootloadingProgress.Value/10 , 0)).ToString() + "%";

            BootStatusView = BootStatus;
        }

        /// <summary>
        /// ButtonClick - Back/Cancel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = ProgramFlow.SourseWindow;
            this.Visibility = Visibility.Collapsed;
        }
        
        /// <summary>
        /// ButtonClick - Send Bootloader over TCP
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonBootload_Click(object sender, RoutedEventArgs e)
        {
            SureMessageVis = Visibility.Visible;
        }

        /// <summary>
        /// ButtonClick - Confirm request
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonAccept_Click(object sender, RoutedEventArgs e)
        {
            SureMessageVis = Visibility.Collapsed;
            BootReady = false;
            BootStop = false;
            BootSentIndex = 0;
            BootAckIndex = -1;
            if (BootloaderThread != null && BootloaderThread.IsAlive)
            {

            }
            else
            {
                BootloaderThread = new Thread(BootloaderDo)
                {
                    IsBackground = true,
                    Name = "BootloaderThread"
                };
                BootloaderThread.Start();
            }

            BootStatus = "Asking device to boot...";
            //GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&BB00]");
            byte[] BoottMessage;
            BoottMessage = Enumerable.Repeat((byte)0, 522).ToArray();
            BoottMessage[0] = (byte)'[';
            BoottMessage[1] = (byte)'&';
            BoottMessage[2] = (byte)'B';
            BoottMessage[3] = (byte)'B';
            BoottMessage[4] = (byte)SelectedFirmRev;
            BoottMessage[5] = (byte)SelectedFirmSubRev;
            BoottMessage[6] = 0;
            BoottMessage[7] = 0;
            BoottMessage[8] = (byte)']';
            GlobalSharedData.ServerMessageSend = BoottMessage;
        }

        /// <summary>
        /// Thread Do - While for sending bootloader sections
        /// </summary>
        private void BootloaderDo()
        {
            
            while (!WiFiconfig.endAll && !BootStop)
            {
                if (SelectedFirm != 69 && WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].BootloaderFirmRev >= 3)
                {
                    Thread.Sleep(100);
                }
                if (BootReady)
                {

                    if (BootSentIndex == 0 && BootAckIndex == -1)
                    {
                        GlobalSharedData.ServerMessageSend = BootFileList.ElementAt(BootSentIndex);
                        BootSentIndex++;
                    }

                    if (BootSentIndex < BootFileList.Count && BootAckIndex == BootSentIndex - 1)
                    {
                        GlobalSharedData.ServerMessageSend = BootFileList.ElementAt(BootSentIndex);
                        BootSentIndex++;
                    }

                    if (BootSentIndex == BootFileList.Count)
                    {
                        Debug.WriteLine("====================Bootloading done======================");
                        //WIFIcofig.ServerMessageSend = 
                        //BootReady = false;
                        break;
                    }
                    
                }

            }
            SelectFilebtnEnab = BootBtnEnabled = true;
            BackbtnText = "Back";
            BootStop = false;
            BootFlashPersentage = 0;
        }

        /// <summary>
        /// Parse bootloader message received over TCP
        /// </summary>
        /// <param name="message"></param>
        /// <param name="endPoint"></param>
        public static void BootloaderParse(byte[] message, EndPoint endPoint)
        {
            if ((message.Length >= 7) && (message[0] == '[') && (message[1] == '&') && (message[2] == 'B'))
            {

                #region Bootloading ready to start
                if (message[3] == 'a' && message[6] == ']')
                {
                    BootStatus = "Device ready to boot...";
                    GlobalSharedData.ServerStatus = "Boot ready message recieved";
                    GlobalSharedData.BroadCast = false;
                    if (GlobalSharedData.ConnectedDeviceApplicationState != (int)ApplicationEnum.Application)
                        {
                        BootReady = true;
                    }
                    WiFiconfig.SelectedIP = endPoint.ToString();
                }
                #endregion

                #region Bootload next index
                if (message[3] == 'D')
                {                        
                    if (message[4] == 'a' && message[9] == ']')
                    {
                        bootContinue = true;
                        BootAckIndex = BitConverter.ToUInt16(message, 5);
                        BootStatus = "Device bootloading packet " + BootAckIndex.ToString() + " of " + bootchunks.ToString() + "...";
                        GlobalSharedData.ServerStatus = "Boot acknowledgment message recieved";

                    }                       
                }
                #endregion

                #region Bootloading complete message
                if (message[3] == 's' && message[6] == ']')
                {
                    //done bootloading
                    BootStatus = "Device bootloading done...";
                    BootDone = true;                      
                    BootFlashPersentage = 0;
                    BootReady = false;
                   // Thread.Sleep(20);
                   // GlobalSharedData.ServerMessageSend = WiFiconfig.HeartbeatMessage;
                    GlobalSharedData.ServerStatus = "Boot acknowledgment message recieved";
                }
                #endregion

                #region Bootload error message
                if (message[3] == 'e' && message[8] == ']')
                {
                    if(BitConverter.ToUInt16(message, 4) == 0xFFFF)
                    {
                        BootSentIndex = 0;
                        BootAckIndex = -1;
                        BootReady = true;
                        BootStatus = "Waiting for device, please be patient... " + BootAckIndex.ToString() + "...";
                    }
                    else
                    {
                        BootSentIndex--;
                        BootStatus = "Waiting for device, please be patient... " + BootAckIndex.ToString() + "...";
                    }                        
                        
                }
                #endregion

                #region Bootload flash message persentage
                if (message[3] == 'f' && message[7] == ']')
                {
                    BootFlashPersentage = message[4];
                    //if (BootFlashPersentage != 100)
                    //    BootStatus = "Device bootloading flash erase... " + BootFlashPersentage.ToString() + "%";
                    //else
                    //    BootFlashPersentage = 1;

                    BootStatus = "Device bootloading flash erase... " + BootFlashPersentage.ToString() + "%";
                }
                #endregion
            }
        }

        /// <summary>
        /// ButtonClick - File dialog to select boot file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void ButtonFileSelect_Click(object sender, RoutedEventArgs e)
        {
            byte[] bootfilebytes;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "binary files (*.binary)|*.binary|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
                bootfilebytes = File.ReadAllBytes(openFileDialog.FileName);
            else
                bootfilebytes = null;

            bootfile = openFileDialog.FileName;

            if(bootfile != "")
            {
                int fileNameStart = bootfile.LastIndexOf("\\") + 1;
                             //           File.WriteAllBytes(fileSub, bootfilebytes);
                try
                {
                    if (bootfile.Contains("M-PFW-") && bootfile.Contains(".binary"))
                    {
                        int start = bootfile.LastIndexOf("M-PFW");
                        start += 6;
                        string firm = bootfile.Substring(start, 3);
                        SelectedFirm = Int16.Parse(firm);
                        start += 4;
                        firm = bootfile.Substring(start, 2);
                        SelectedFirmRev = Int16.Parse(firm);
                        start += 3;
                        firm = bootfile.Substring(start, 2);
                        SelectedFirmSubRev = Int16.Parse(firm);

                        SelectedFirmwareString = "M-PFW-" + ((SelectedFirm < 100) ? "0" + SelectedFirm.ToString() : SelectedFirm.ToString()) + "-" +
                        ((SelectedFirmRev < 10) ? "0" + SelectedFirmRev.ToString() : SelectedFirmRev.ToString()) + "-" +
                        ((SelectedFirmSubRev < 10) ? "0" + SelectedFirmSubRev.ToString() : SelectedFirmSubRev.ToString());

                        if ((SelectedFirm == 56 || SelectedFirm == 58 || SelectedFirm == 93 || SelectedFirm == 94) && SelectedApplication == "BHU BT"  || (SelectedFirm == 69) && SelectedApplication == "Comms Bridge BT")
                        {
                            if (SelectedFirmRev == FirmwareRev && SelectedFirmSubRev == FirmSub && SelectedFirm == FirmwareApp)
                            {
                                //show that the fimware selected is the same as that of the device
                                FileErrorMessage = "Selected firmware is the same as device firmware, no need to update.";
                                FileError = true;
                         
                            }
                            //else
                            
                            //enable bootloading
                            int bytesleft = 0;
                            int bootfileSize = 0;
                            int fileChunck = Bootloader2_size;

                            if(SelectedFirm != 69 && WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].BootloaderFirmRev >= 3)
                            {
                                fileChunck = Bootloader3_size;
                            }
                        
                            BootFileList = new List<byte[]>();
                            bytesleft = bootfileSize = bootfilebytes.Length;
                            bootchunks = (int)Math.Round(bootfileSize / (double)fileChunck);
                            int shifter = 0;
                            for (int i = 0; i <= bootchunks; i++)
                            {
                                byte[] bootchunk = Enumerable.Repeat((byte)0xFF, fileChunck+10).ToArray();
                                byte[] bytes = BitConverter.GetBytes(i);
                                byte[] bytes2 = BitConverter.GetBytes(bootchunks);
                                bootchunk[0] = (byte)'[';
                                bootchunk[1] = (byte)'&';
                                bootchunk[2] = (byte)'B';
                                bootchunk[3] = (byte)'D';
                                bootchunk[4] = bytes[0];
                                bootchunk[5] = bytes[1];
                                bootchunk[6] = bytes2[0];
                                bootchunk[7] = bytes2[1];

                                if (bytesleft > fileChunck)
                                    Array.Copy(bootfilebytes, shifter, bootchunk, 8, fileChunck);
                                else if (bytesleft > 0)
                                    Array.Copy(bootfilebytes, shifter, bootchunk, 8, bytesleft);

                                bootchunk[fileChunck+8] = 0;
                                bootchunk[fileChunck+9] = (byte)']';
                                BootFileList.Add(bootchunk);
                                shifter += fileChunck;
                                bytesleft -= fileChunck;
                            }

                            //btnBootload.IsEnabled = true;
                            BootBtnEnabled = true;
                            

                        }
                        else if ((SelectedFirm == 57) && SelectedApplication == "ERB BT")
                        {
                            if (SelectedFirmRev == FirmwareRev && SelectedFirmSubRev == FirmSub)
                            {
                                //show that the fimware selected is the same as that of the device
                                FileErrorMessage = "Selected firmware is the same as device firmware, no need to update.";
                                FileError = true;
                            }
                            //else
                            
                            //enable bootloading
                            int bytesleft = 0;
                            int bootfileSize = 0;
                            int fileChunck = 512;
                            BootFileList = new List<byte[]>();
                            bytesleft = bootfileSize = bootfilebytes.Length;
                            bootchunks = (int)Math.Round(bootfileSize / (double)fileChunck);
                            int shifter = 0;
                            for (int i = 0; i <= bootchunks; i++)
                            {
                                byte[] bootchunk = Enumerable.Repeat((byte)0xFF, 522).ToArray();
                                byte[] bytes = BitConverter.GetBytes(i);
                                byte[] bytes2 = BitConverter.GetBytes(bootchunks);
                                bootchunk[0] = (byte)'[';
                                bootchunk[1] = (byte)'&';
                                bootchunk[2] = (byte)'B';
                                bootchunk[3] = (byte)'D';
                                bootchunk[4] = bytes[0];
                                bootchunk[5] = bytes[1];
                                bootchunk[6] = bytes2[0];
                                bootchunk[7] = bytes2[1];

                                if (bytesleft > fileChunck)
                                    Array.Copy(bootfilebytes, shifter, bootchunk, 8, fileChunck);
                                else if (bytesleft > 0)
                                    Array.Copy(bootfilebytes, shifter, bootchunk, 8, bytesleft);

                                bootchunk[520] = 0;
                                bootchunk[521] = (byte)']';
                                BootFileList.Add(bootchunk);
                                shifter += fileChunck;
                                bytesleft -= fileChunck;
                            }

                            //btnBootload.IsEnabled = true;
                            BootBtnEnabled = true;
                            

                        }
                        else
                        {
                            //show that the fimware is not the correct fimware for this device
                            FileErrorMessage = "Selected firmware can not be used for the connected device";
                            FileError = true;
                            BootBtnEnabled = false;
                            bootfile = null;
                            SelectedFirmwareString = "";
                            //FileSelect_Click(null, null);
                        }


                    }
                    else
                    {
                        if (!bootfile.Contains("M-PFW-") || !bootfile.Contains(".binary"))
                        {
                            //Please select a M-PFW-056-xx-yy.binary file to bootload
                            if (SelectedFirm == 69)
                            {
                                FileErrorMessage = "Please select a M-PFW-069-xx-yy.binary file to bootload.";
                            }
                            else
                            {
                                FileErrorMessage = "Please select a M-PFW-056-xx-yy.binary file to bootload.";
                            }
                            FileError = true;
                            BootBtnEnabled = false;
                            SelectedFirmwareString = "";
                            //FileSelect_Click(null, null);
                        }
                        else
                        {
                            //File error, please select a different file
                            FileErrorMessage = "File error, please select a different file.";
                            FileError = true;
                            BootBtnEnabled = false;
                            SelectedFirmwareString = "";
                            //FileSelect_Click(null, null);
                        }

                    }
                }
                catch
                {
                    if (SelectedFirm == 69)
                    {
                        FileErrorMessage = "Please select a M-PFW-069-xx-yy.binary file to bootload.";
                    }
                    else
                    {
                        FileErrorMessage = "Please select a M-PFW-056-xx-yy.binary file to bootload.";
                    }
               
                    FileError = true;
                    BootBtnEnabled = false;
                    SelectedFirmwareString = "";
                    //FileSelect_Click(null, null);
                }

            }
            else
            {
                //btnBootload.IsEnabled = false;
                BootBtnEnabled = false;
            }


        }

        /// <summary>
        /// ButtonClick - Decline request
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonDecline_Click(object sender, RoutedEventArgs e)
        {
            SureMessageVis = Visibility.Collapsed;
        }

        #region Properties
        private string _bootStatus = "";

        public string BootStatusView
        {
            get { return _bootStatus; }
            set
            {
                if (value != null)
                    _bootStatus = value;
                else
                {
                    _bootStatus = "Waiting for instructions...";
                }
                    OnPropertyChanged("BootStatusView");
            }
        }

        private string _BootStatuspersentage;

        public string BootStatuspersentage
        {
            get { return _BootStatuspersentage; }
            set { _BootStatuspersentage = value; OnPropertyChanged("BootStatuspersentage"); }
        }

        private Visibility _SureMessageVis;

        public Visibility SureMessageVis
        {
            get { return _SureMessageVis; }
            set { _SureMessageVis = value; OnPropertyChanged("SureMessageVis"); }
        }


        private string _Name;

        public string DeviceName
        {
            get { return _Name; }
            set { _Name = value; OnPropertyChanged("DeviceName"); }
        }

        private string _BackbtnText;

        public string BackbtnText
        {
            get { return _BackbtnText; }
            set { _BackbtnText = value; OnPropertyChanged("BackbtnText"); }
        }


        private int _FirmwareRev;

        public int FirmwareRev
        {
            get { return _FirmwareRev; }
            set { _FirmwareRev = value; OnPropertyChanged("FirmwareRev"); }
        }

        private int _FirmwareApp;

        public int FirmwareApp
        {
            get { return _FirmwareApp; }
            set { _FirmwareApp = value; OnPropertyChanged("FirmwareApp"); }
        }

        private string _FirmwareString;

        public string FirmwareString
        {
            get { return _FirmwareString; }
            set { _FirmwareString = value; OnPropertyChanged("FirmwareString"); }
        }


        private string _FirmwareTypeString;

        public string FirmwareTypeString
        {
            get { return _FirmwareTypeString; }
            set { _FirmwareTypeString = value; OnPropertyChanged("FirmwareTypeString"); }
        }

        private string _SelectedFirmwareTypeString;

        public string SelectedFirmwareTypeString
        {
            get { return _SelectedFirmwareTypeString; }
            set { _SelectedFirmwareTypeString = value; OnPropertyChanged("SelectedFirmwareTypeString"); }
        }

        
        private int _FirmSub;

        public int FirmSub
        {
            get { return _FirmSub; }
            set { _FirmSub = value; OnPropertyChanged("FirmSub"); }
        }

        private uint _VID;
        private string bootfile;

        public uint DeviceVID
        {
            get { return _VID; }
            set { _VID = value; OnPropertyChanged("DeviceVID"); }
        }

        private int _SelectedFirm;

        public int SelectedFirm
        {
            get { return _SelectedFirm; }
            set { _SelectedFirm = value; OnPropertyChanged("SelectedFirm"); }
        }

        private int _SelectedFirmRev;

        public int SelectedFirmRev
        {
            get { return _SelectedFirmRev; }
            set { _SelectedFirmRev = value; OnPropertyChanged("SelectedFirmRev"); }
        }

        private int _SelectedFirmSubRev;

        public int SelectedFirmSubRev
        {
            get { return _SelectedFirmSubRev; }
            set { _SelectedFirmSubRev = value; OnPropertyChanged("SelectedFirmSubRev"); }
        }


        private string _SelectedFirmwareString;

        public string SelectedFirmwareString
        {
            get { return _SelectedFirmwareString; }
            set { _SelectedFirmwareString = value; OnPropertyChanged("SelectedFirmwareString"); }
        }

        private string _SelectedApplication;

        public string SelectedApplication
        {
            get { return _SelectedApplication; }
            set { _SelectedApplication = value; OnPropertyChanged("SelectedApplication"); }
        }

        private bool _LicenseBool;

        public bool LicenseBool
        {
            get { return _LicenseBool; }
            set { _LicenseBool = value; OnPropertyChanged("LicenseBool"); }
        }

        private bool _BootBtnEnabled;

        public bool BootBtnEnabled
        {
            get { return _BootBtnEnabled; }
            set { _BootBtnEnabled = value; OnPropertyChanged("BootBtnEnabled"); }
        }

        private bool _selectFilebtnEnab;

        public bool SelectFilebtnEnab
        {
            get { return _selectFilebtnEnab; }
            set { _selectFilebtnEnab = value; OnPropertyChanged("SelectFilebtnEnab"); }
        }


        #endregion

    }
}
