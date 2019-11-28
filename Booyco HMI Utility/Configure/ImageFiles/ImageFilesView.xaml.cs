using Booyco_BHU_Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
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
using static Booyco_BHU_Utility.FirmwareRevisionManagement;

namespace Booyco_HMI_Utility
{
    /// <summary>
    /// Interaction logic for ImageFilesView.xaml
    /// </summary>
    public partial class ImageFilesView : UserControl
    {
        private uint SelectVID = 0;
        static private RangeObservableCollection<ImageEntry> ImageFileList;
        string _savedFilesPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Saved Files\\Images";
        static private UInt16 TotalImageFiles = 0;
        private static int CurrentImageFileNumber = 1;
        private static int PreviousImageFileNumber = 1;
        static private UInt16 TotalCount = 0;
        List<FirmwareEntry> firmwareImageCountList = new List<FirmwareEntry>();
        FirmwareRevisionManagement FirmwareRevisionManager = new FirmwareRevisionManagement();
        DispatcherTimer updateDispatcherTimer;
        private static int TotalSize = 4096+14;
        private static int DataSize = TotalSize - 14;
        private uint _heartBeatDelay = 0;
        private static UInt16 TotalFilesCount = 0;
        private static UInt16 DataIndex = 0;
        private static UInt16 ProgressDataIndex = 0;
        private static bool errorFlag = false;
        private enum TransferStatusEnum
        {
            None,
            Initialize,
            Busy,
            WaitEnd,
            Complete
        }

        private static int TransferStatus = (int)TransferStatusEnum.None;
        public ImageFilesView()
        {
            InitializeComponent();
            ImageFileList = new RangeObservableCollection<ImageEntry>();

            DataGridImageFiles.AutoGenerateColumns = false;
            DataGridImageFiles.ItemsSource = ImageFileList;
            Label_StatusView.Content = "Waiting for user command..";
            Label_ProgressStatusPercentage.Content = "";
            firmwareImageCountList = FirmwareRevisionManager.ReadImageFirmwareRevision((int)EnumRevisionType.ImageRevision);
            ReadImageFiles();

            updateDispatcherTimer = new DispatcherTimer();
            updateDispatcherTimer.Tick += new EventHandler(InfoUpdater);
            updateDispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
        }

        void InfoUpdater(object sender, EventArgs e)
        {
            ButtonState();
            if (!GlobalSharedData.WiFiConnectionStatus)
            {

            }
            else
            {

                TCPclientR _foundTCPClient = WiFiconfig.TCPclients.FirstOrDefault(t => t.VID == SelectVID);
                if (_foundTCPClient != null)
                {
                    WiFiconfig.SelectedIP = _foundTCPClient.IP;
                }

                if (TransferStatus != (int)TransferStatusEnum.None)
                {
                    ProgressBar_ImageFiles.Maximum = TotalImageFiles * 100;
                    if (TotalCount > 0)
                    {
                        ProgressBar_ImageFiles.Value = (int)((ProgressDataIndex * 100) / TotalCount) + (CurrentImageFileNumber - 1) * 100;
                    }
                    Label_StatusView.Content = "Image File " + CurrentImageFileNumber.ToString() + " of " + TotalImageFiles.ToString();
                    Label_ProgressStatusPercentage.Content = (Convert.ToInt32( Math.Floor(ProgressBar_ImageFiles.Value / TotalImageFiles))).ToString() + "%";

                    // === check if heartbeat received ===
                    if (WiFiconfig.Heartbeat && TransferStatus == (int)TransferStatusEnum.Initialize)
                    {
                        _heartBeatDelay++;

                        if (_heartBeatDelay > 15)
                        {
                            WiFiconfig.Heartbeat = false;
                            _heartBeatDelay = 0;
                            GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&II00]");
                        }
                    }
                    if (WiFiconfig.Heartbeat && TransferStatus == (int)TransferStatusEnum.Busy)
                    {

                        _heartBeatDelay++;

                        if (_heartBeatDelay > 15)
                        {
                            WiFiconfig.Heartbeat = false;
                            _heartBeatDelay = 0;
                            GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&IU00]");
                        }

                    }
                    if (TransferStatus == (int)TransferStatusEnum.Complete)
                    {
                        ButtonBack_Click(null, null);
                        //TransferStatus = (int)TransferStatusEnum.None;
                    }
                }
            }

        }

        void ButtonState()
        {
            if (TransferStatus == (int)TransferStatusEnum.None)
            {
                if (ButtonNew.IsEnabled == false)
                {
                    ButtonNew.IsEnabled = true;
                    ButtonNext.IsEnabled = true;
                    ButtonPrevious.IsEnabled = true;
                    ButtonAppend.IsEnabled = true;
                    RectangleArrowRight.Fill = new SolidColorBrush(Color.FromRgb(140, 9, 9));
                    RectangleArrowLeft.Fill = new SolidColorBrush(Color.FromRgb(140, 9, 9));
                    ImageParameter.Opacity = 0.6;
                    ImagePicture.Opacity = 0.6;
                }

            }
            else
            {
                if (ButtonNew.IsEnabled == true)
                {
                    RectangleArrowRight.Fill = new SolidColorBrush(Colors.DarkGray);
                    RectangleArrowLeft.Fill = new SolidColorBrush(Colors.DarkGray);
                    ImageParameter.Opacity = 0.4;
                    ImagePicture.Opacity = 0.4;
                    ButtonNew.IsEnabled = false;
                    ButtonNext.IsEnabled = false;
                    ButtonPrevious.IsEnabled = false;
                    ButtonAppend.IsEnabled = false;
                }
            }
        }

        static bool ValidImageSizes(RangeObservableCollection<ImageEntry> DeviceImageFileList)
        {
            if (DeviceImageFileList.Count == ImageFileList.Count)
            {
                int index = 0;
                foreach (ImageEntry item in DeviceImageFileList)
                {
                    if (item.Size != ImageFileList.ElementAt(index).Size)
                    {
                        return false;
                    }
                    index++;
                }
                return true;
            }
            return false;
        }

        public static void ImageFileSendParse(byte[] message, EndPoint endPoint)
        {
            if (TransferStatus != (int)TransferStatusEnum.None)
            {
                if (CurrentImageFileNumber == 0)
                {
                    CurrentImageFileNumber = 1;
                }

                if (TotalCount > 0)
                {
                    if (PreviousImageFileNumber < CurrentImageFileNumber)
                    {
                        ProgressDataIndex = DataIndex;
                        ImageFileList.ElementAt(PreviousImageFileNumber - 1).Progress = 100;
                        ImageFileList.ElementAt(PreviousImageFileNumber - 1).ProgressString = "100%";
                        PreviousImageFileNumber = CurrentImageFileNumber;
                    }
                    else
                    {
                        if (ImageFileList.ElementAt(CurrentImageFileNumber - 1).Progress < (int)((DataIndex * 100) / TotalCount))
                        {
                            ProgressDataIndex = DataIndex;
                            ImageFileList.ElementAt(CurrentImageFileNumber - 1).Progress = (int)((DataIndex * 100) / TotalCount);
                            ImageFileList.ElementAt(CurrentImageFileNumber - 1).ProgressString = ImageFileList.ElementAt(CurrentImageFileNumber - 1).Progress.ToString() + "%";
                        }
                    }

                }
                if ((message.Length >= 7) && (message[0] == '[') && (message[1] == '&') && (message[2] == 'I'))
                {
                    int ReceivedImageFileNumber;
                    // === Check if it is Ready packet. The device is ready ===             
                    if ((message[3] == 'a') && (message[521] == ']'))
                    {
                     
                        TotalFilesCount = BitConverter.ToUInt16(message, 4);

                        RangeObservableCollection<ImageEntry> DeviceImageFileList = new RangeObservableCollection<ImageEntry>();

                        for (int i = 0; i < TotalFilesCount; i++)
                        {
                            DeviceImageFileList.Add(new ImageEntry
                            {
                                Size = (long)BitConverter.ToUInt32(message, 6 + i * 4)
                            });
                        }

                        // ===TODO: do something with return value ===
                        ValidImageSizes(DeviceImageFileList);
                        TransferStatus = (int)TransferStatusEnum.Busy;
                        SendImageFile(CurrentImageFileNumber);
                        Debug.WriteLine("Audio - Received Packet ready");

                    }

                    // === Check if it is an acknowdlegement packet. The device has received the previous packet, and is ready to receive next chunk ===
                    else if ((message[3] == 'D') && (message[4] == 'a') && (message[11] == ']'))
                    {
                        DataIndex = BitConverter.ToUInt16(message, 5);
                        DataIndex++;
                        ReceivedImageFileNumber = BitConverter.ToUInt16(message, 7);
                        CurrentImageFileNumber = ReceivedImageFileNumber;
                        SendImageFile(CurrentImageFileNumber);
                        Debug.WriteLine("Audio - Ready to receive next chunk -" + DataIndex.ToString() + ":" + TotalCount.ToString() + "-" + CurrentImageFileNumber.ToString());
                    }

                    // === Check if it is an error packet. The device request previous packet to be sent again ===
                    else if ((message[3] == 'e') && (message[10] == ']'))
                    {
                        ReceivedImageFileNumber = BitConverter.ToUInt16(message, 6);
                        if (ReceivedImageFileNumber == 0xFFFF)
                        {
                            SendImageFile(CurrentImageFileNumber);
                        }
                        else
                        {
                            DataIndex = BitConverter.ToUInt16(message, 4);
                            DataIndex++;
                            errorFlag = true;
                            Debug.WriteLine("Debug1 Error - Dataindex = " + DataIndex.ToString() + " FileNumber = " + ReceivedImageFileNumber.ToString());
                            CurrentImageFileNumber = ReceivedImageFileNumber;
                            SendImageFile(CurrentImageFileNumber);
                            Debug.WriteLine("Debug5");
                        }

                    }

                    // === Check if it is a complete packet. The device has received all packets ===
                    //else if ((message[3] == 's') && (message[11] == ']'))
                    //{

                    //}
                }
            }
        }

   
        static void SendImageFile(int ImageFileNumber)
        {

            if (ImageFileNumber == 0)
            {
                ImageFileNumber = 1;
            }

            if (DataIndex > 60000)
            {
                DataIndex = 0;
            }

            Debug.WriteLine("Debug2");
            //if (errorFlag)
            //{
            //    if(DataIndex == 0xffff)
            //    {
            //        DataIndex = 0;
            //    }
            //    else
            //    {
            //        DataIndex++;
            //    }
               
            //    errorFlag = false;
            //}

            if (TotalCount == 0)
            {
                StoreImageFile(ImageFileNumber);
            }

            if (DataIndex >= (TotalCount) && TransferStatus != (int)TransferStatusEnum.WaitEnd)
            {
                if (ImageFileNumber == (TotalImageFiles))
                {
                    // TransferStatus = (int)TransferStatusEnum.WaitEnd;
                    TransferStatus = (int)TransferStatusEnum.Complete;
                    //=== Send Complete ===
                }
                else
                {
                    ImageFileNumber++;
                }
                DataIndex = 0;
                TotalCount = 0;
            }

            
            Debug.WriteLine("Debug3");
            if (TransferStatus == (int)TransferStatusEnum.Busy)
            {
                StoreImageFile(ImageFileNumber);

                byte[] ImageFileChunk = Enumerable.Repeat((byte)0xFF, DataSize + 14).ToArray();
                byte[] SentIndexArray = BitConverter.GetBytes(DataIndex);
                byte[] TotalCountArray = BitConverter.GetBytes(TotalCount);
                byte[] FileNumberArray = BitConverter.GetBytes(ImageFileNumber);
                byte[] FileTotalArray = BitConverter.GetBytes(TotalImageFiles);

                ImageFileChunk[0] = (byte)'[';
                ImageFileChunk[1] = (byte)'&';
                ImageFileChunk[2] = (byte)'I';
                ImageFileChunk[3] = (byte)'D';
                ImageFileChunk[4] = SentIndexArray[0]; // === SentIndex ===
                ImageFileChunk[5] = SentIndexArray[1];
                ImageFileChunk[6] = TotalCountArray[0]; // === TotalCount ===
                ImageFileChunk[7] = TotalCountArray[1];
                ImageFileChunk[8] = FileNumberArray[0]; // === FileNumber ===
                ImageFileChunk[9] = FileNumberArray[1];
                ImageFileChunk[10] = FileTotalArray[0]; // === FileTotal ===
                ImageFileChunk[11] = FileTotalArray[1];
                Debug.WriteLine("Debug4 -" + CurrentImageFileArray.Count().ToString() + "---" + DataSize.ToString() + "+" + DataIndex.ToString());
                if ((DataSize) * DataIndex <= (CurrentImageFileArray.Count()- DataSize))
                {
                    Buffer.BlockCopy(CurrentImageFileArray, DataSize * DataIndex, ImageFileChunk, 12, DataSize);
                    ImageFileChunk[TotalSize - 1] = (byte)']';
                    GlobalSharedData.ServerMessageSend = ImageFileChunk;
                }
                else if (CurrentImageFileArray.Count() - (DataSize) * DataIndex > 0)
                {
                    Buffer.BlockCopy(CurrentImageFileArray, DataSize * DataIndex, ImageFileChunk, 12, CurrentImageFileArray.Count() -(DataSize) * DataIndex );
                    ImageFileChunk[TotalSize - 1] = (byte)']';
                    GlobalSharedData.ServerMessageSend = ImageFileChunk;
                }
                else
                {
                    

                }
              
            }

        }

        static byte[] CurrentImageFileArray = { 0 };
        static void StoreImageFile(int FileNumber)
        {
            ImageEntry _SelectedFile = ImageFileList.FirstOrDefault(t => t.ID == FileNumber);

            if (_SelectedFile != null)
            {
                // === Read datalog file ===
                //string _logInfoRaw = System.IO.File.ReadAllText(Log_Filename, Encoding.Default);          
                BinaryReader _breader = new BinaryReader(File.OpenRead(_SelectedFile.Path));
                CurrentImageFileArray = _breader.ReadBytes((int)_SelectedFile.Size);

                TotalCount = (UInt16)(Math.Ceiling((double)CurrentImageFileArray.Length / (double)DataSize));
                _breader.Dispose();
                _breader.Close();
            }
        }


        private void ButtonNew_Click(object sender, RoutedEventArgs e)
        {
            Grid_Message.Visibility = Visibility.Visible;
        }

        private void ButtonAppend_Click(object sender, RoutedEventArgs e)
        {
           
            GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&IU00]");
            ProgressDataIndex = 0;
            DataIndex = 0;
            foreach (ImageEntry item in ImageFileList)
            {
                item.Progress = 0;
                item.ProgressString = "0%";
            }

            for (int i = 0; i< 37; i++)
            { 
                ImageFileList.ElementAt(i).Progress = 100;
                ImageFileList.ElementAt(i).ProgressString = "100%";             
            }

            TransferStatus = (int)TransferStatusEnum.Busy;
            PreviousImageFileNumber = 38;
            CurrentImageFileNumber = 38;
            ButtonBack.Content = "Cancel";
            WiFiconfig.Heartbeat = false;

           
        }


        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            
            if (TransferStatus != (int)TransferStatusEnum.None)
            {

                TransferStatus = (int)TransferStatusEnum.None;
                ButtonBack.Content = "Back";
                ClearInfo();

            }
            else
            {
                if (ProgramFlow.SourseWindow == (int)ProgramFlowE.WiFi)
                {
                    ProgramFlow.ProgramWindow = (int)ProgramFlowE.ConfigureMenuView;
                }
                else
                {
                    ProgramFlow.ProgramWindow = (int)ProgramFlowE.FileMenuView;
                    this.Visibility = Visibility.Collapsed;
                    ClearInfo();
                }
            }
        }
        void ClearInfo()
        {
            Label_StatusView.Content = "Waiting for user command..";
            Label_ProgressStatusPercentage.Content = "";
            ProgressBar_ImageFiles.Value = 0;
            foreach (ImageEntry item in ImageFileList)
            {
                item.Progress = 0;
                item.ProgressString = "";
            }
        }
        private void ButtonNext_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.AudioFilesView;
        }

        private void ButtonPrevious_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.ParametersView;
        }
                private void ButtonNext_MouseEnter(object sender, MouseEventArgs e)
        {
            RectangleArrowRight.Fill = new SolidColorBrush(Color.FromRgb(60, 6, 6));
            ImageParameter.Opacity = 1;
        }

        private void ButtonNext_MouseLeave(object sender, MouseEventArgs e)
        {
            RectangleArrowRight.Fill = new SolidColorBrush(Color.FromRgb(140, 9, 9));
            ImageParameter.Opacity = 0.6;
        }

        private void ButtonPrevious_MouseEnter(object sender, MouseEventArgs e)
        {
            RectangleArrowLeft.Fill = new SolidColorBrush(Color.FromRgb(60, 6, 6));
            ImagePicture.Opacity = 1;
        }

        private void ButtonPrevious_MouseLeave(object sender, MouseEventArgs e)
        {
            RectangleArrowLeft.Fill = new SolidColorBrush(Color.FromRgb(140, 9, 9));
            ImagePicture.Opacity = 0.6;
        }

        private void Grid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                Grid_Message.Visibility = Visibility.Collapsed;

                if (ProgramFlow.SourseWindow == (int)ProgramFlowE.WiFi)
                {
                    DataGridImageFiles.Columns[4].Visibility = Visibility.Visible;
                    ButtonNew.Visibility = Visibility.Visible;
                    ButtonAppend.Visibility = Visibility.Visible;
                    Grid_Progressbar.Visibility = Visibility.Visible;
                    ButtonNext.Visibility = Visibility.Visible;
                    ButtonPrevious.Visibility = Visibility.Visible;
                    WiFiconfig.SelectedIP = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].IP;
                    SelectVID = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].VID;
                    updateDispatcherTimer.Start();

                }
                else
                {
                    DataGridImageFiles.Columns[4].Visibility = Visibility.Collapsed;
                    ButtonNew.Visibility = Visibility.Collapsed;
                    ButtonAppend.Visibility = Visibility.Collapsed;
                    ButtonNext.Visibility = Visibility.Collapsed;
                    ButtonPrevious.Visibility = Visibility.Collapsed;
                    Grid_Progressbar.Visibility = Visibility.Collapsed;
                    
                }

            }
            else
            {
                updateDispatcherTimer.Stop();
            }

        }
        void ReadImageFiles()
        {
            ImageFileList.Clear();
            if (Directory.Exists(_savedFilesPath))
            {
                DirectoryInfo d = new DirectoryInfo(_savedFilesPath);
                FileInfo[] FilesBMP = d.GetFiles("*.bmp");
                ushort count = 0;

                int _firmwareNumber = 0;
                int _totalImages = 0;

                foreach (FileInfo file in FilesBMP)
                {
                    count++;

                    if (count > _totalImages)
                    {
                        for (int i = _firmwareNumber; i < firmwareImageCountList.Count(); i++)
                        {
                            _firmwareNumber = i + 1;
                            _totalImages = firmwareImageCountList.ElementAt(i).ImageCount;
                            if (count < _totalImages)
                            {
                                i = firmwareImageCountList.Count();
                            }
                        }
                    }

                    if (count <= _totalImages)
                    {
                        string selectedFileName = file.FullName;
                        BitmapImage bitmap = new BitmapImage();

                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(selectedFileName);                        
                        bitmap.EndInit();

                        var tb = new TransformedBitmap();                    
                        tb.BeginInit();
                        tb.Source = bitmap;
                        var transform = new ScaleTransform(1, -1, 0, 0);
                        tb.Transform = transform;
                        tb.EndInit();


                        ImageFileList.Add(new ImageEntry
                        {
                            ID = count,
                            FileName = file.Name,
                            DateTimeCreated = file.CreationTime.ToString("yyyy-MM-dd HH-mm-ss"),
                            Path = file.FullName,
                            Size = file.Length,
                            ImageSource = tb,
                            Progress = 0,
                            ProgressString = "",
                            FirmwareNumber = _firmwareNumber
                        });
                    }

                    
                }
                TotalImageFiles = count;
            }
            else
            {
                Directory.CreateDirectory(_savedFilesPath);
            }
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Grid_Message.Visibility = Visibility.Collapsed;
        }

        private void ButtonContinue_Click(object sender, RoutedEventArgs e)
        {
            // === Send start datalog informaiton ===      
            ReadImageFiles();
            GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&II00]");
            CurrentImageFileNumber = 1;
            DataIndex = 0;

            TransferStatus = (int)TransferStatusEnum.Initialize;
            PreviousImageFileNumber = 1;
            CurrentImageFileNumber = 1;
            ButtonBack.Content = "Cancel";
            WiFiconfig.Heartbeat = false;

            foreach (ImageEntry item in ImageFileList)
            {
                item.Progress = 0;
                item.ProgressString = "0%";
            }

            Grid_Message.Visibility = Visibility.Collapsed;
        }
    }

       
    }
