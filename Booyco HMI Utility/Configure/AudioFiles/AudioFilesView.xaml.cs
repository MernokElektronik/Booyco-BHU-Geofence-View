
using System;
using System.Collections.Generic;
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



namespace Booyco_HMI_Utility
{
    /// <summary>
    /// Interaction logic for AudioFilesView.xaml
    /// </summary>
    public partial class AudioFilesView : UserControl
    {
        string _savedFilesPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Saved Files\\Audio";
        static private RangeObservableCollection<AudioEntry> AudioFileList;
        private uint SelectVID = 0;
        static private UInt16 TotalAudioFiles = 0;       
        private static UInt16 CurrentAudioFileNumber = 1;
        private static UInt16 PreviousAudioFileNumber = 1;
        static private UInt16 TotalCount = 0;
        DispatcherTimer updateDispatcherTimer;
        private static int TotalSize = 522;
        private static int DataSize = TotalSize - 14;
        private uint _heartBeatDelay = 0;
        private static UInt16 TotalFilesCount = 0;
        private static UInt16 DataIndex = 0;
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
      
        
        public AudioFilesView()
        {
            InitializeComponent();
            AudioFileList = new RangeObservableCollection<AudioEntry>();
            DataGridAudioFiles.AutoGenerateColumns = false;
            DataGridAudioFiles.ItemsSource = AudioFileList;
            Label_StatusView.Content = "Waiting for user command..";
            Label_ProgressStatusPercentage.Content = "";
            ReadAudioFiles();
            StoreAudioFile(1);

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
                    ProgressBar_AudioFiles.Maximum = TotalAudioFiles * 100;
                    if (TotalCount > 0)
                    {
                        ProgressBar_AudioFiles.Value = (int)((DataIndex * 100) / TotalCount) + (CurrentAudioFileNumber - 1) * 100;
                        Label_StatusView.Content = "Audio File " + CurrentAudioFileNumber.ToString() + " of " + TotalAudioFiles.ToString();
                        Label_ProgressStatusPercentage.Content = (Convert.ToInt32(ProgressBar_AudioFiles.Value / TotalAudioFiles)).ToString() + "%";
                    }
                   

                    // === check if heartbeat received ===
                    if (WiFiconfig.Heartbeat && TransferStatus == (int)TransferStatusEnum.Initialize)
                    {
                        _heartBeatDelay++;

                        if (_heartBeatDelay > 15)
                        {
                            WiFiconfig.Heartbeat = false;
                            _heartBeatDelay = 0;
                            GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&AA00]");
                        }
                    }
                    if (WiFiconfig.Heartbeat && TransferStatus == (int)TransferStatusEnum.Busy)
                    {

                        _heartBeatDelay++;

                        if (_heartBeatDelay > 15)
                        {
                            WiFiconfig.Heartbeat = false;
                            _heartBeatDelay = 0;
                            GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&AU00]");
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

        void ReadAudioFiles()
        {
            AudioFileList.Clear();
            if (Directory.Exists(_savedFilesPath))
            {
                DirectoryInfo d = new DirectoryInfo(_savedFilesPath);               
                FileInfo[] FilesWav = d.GetFiles("*.wav");
                ushort count = 0;

                foreach (FileInfo file in FilesWav)
                {
                     count++;

                    AudioFileList.Add(new AudioEntry
                    {
                        ID = count,
                        FileName = file.Name,
                        DateTimeCreated = file.CreationTime.ToString("yyyy-MM-dd HH-mm-ss"),
                        Path = file.FullName,
                        Size = file.Length,
                        Progress = 0,
                        ProgressString = ""
                    });                 
                }
                TotalAudioFiles = count;
            }
            else
            {
                Directory.CreateDirectory(_savedFilesPath);
            }
        }
        
        static bool ValidAudioSizes(RangeObservableCollection<AudioEntry> DeviceAudioFileList)
        {
            if(DeviceAudioFileList.Count == AudioFileList.Count)
            {
                int index = 0;
                foreach(AudioEntry item in DeviceAudioFileList)
                {                    
                    if(item.Size != AudioFileList.ElementAt(index).Size)
                    {
                        return false;
                    }
                    index++;
                }
                return true;
            }
            return false;
        }
   

        public static void AudioFileSendParse(byte[] message, EndPoint endPoint)
        {
            if (TransferStatus != (int)TransferStatusEnum.None)
            {
                if (TotalCount > 0)
                {
                    if (PreviousAudioFileNumber < CurrentAudioFileNumber)
                    {
                        AudioFileList.ElementAt(PreviousAudioFileNumber - 1).Progress = 100;
                        AudioFileList.ElementAt(PreviousAudioFileNumber - 1).ProgressString = "100%";
                        PreviousAudioFileNumber = CurrentAudioFileNumber;
                    }
                    else
                    {
                        AudioFileList.ElementAt(CurrentAudioFileNumber - 1).Progress = (int)((DataIndex * 100) / TotalCount);
                        AudioFileList.ElementAt(CurrentAudioFileNumber - 1).ProgressString = AudioFileList.ElementAt(CurrentAudioFileNumber - 1).Progress.ToString() + "%";
                    }

                }
                if ((message.Length >= 7) && (message[0] == '[') && (message[1] == '&') && (message[2] == 'A'))
                {
                    int ReceivedAudioFileNumber;
                    // === Check if it is Ready packet. The device is ready ===             
                    if ((message[3] == 'a') && (message[521] == ']'))
                    {
                       
                        TotalFilesCount = BitConverter.ToUInt16(message, 4);

                        RangeObservableCollection<AudioEntry> DeviceAudioFileList = new RangeObservableCollection<AudioEntry>();

                        for (int i = 0; i < TotalFilesCount; i++)
                        {
                            DeviceAudioFileList.Add(new AudioEntry
                            {
                                Size = (long)BitConverter.ToUInt32(message, 6 + i * 4)
                            });
                        }

                        // ===TODO: do something with return value ===
                        ValidAudioSizes(DeviceAudioFileList);
                        TransferStatus = (int)TransferStatusEnum.Busy;
                        SendAudioFile(CurrentAudioFileNumber);
                        Debug.WriteLine("Audio - Received Packet ready");

                    }

                    // === Check if it is an acknowdlegement packet. The device has received the previous packet, and is ready to receive next chunk ===
                    else if ((message[3] == 'D') && (message[4] == 'a') && (message[11] == ']'))
                    {
                        DataIndex = BitConverter.ToUInt16(message, 5);
                        DataIndex++;
                        ReceivedAudioFileNumber = BitConverter.ToUInt16(message, 7);
                        SendAudioFile(ReceivedAudioFileNumber);
                        Debug.WriteLine("Audio - Ready to receive next chunk -" + DataIndex.ToString() + ":" + TotalCount.ToString() + "-" + CurrentAudioFileNumber.ToString());
                    }

                    // === Check if it is an error packet. The device request previous packet to be sent again ===
                    else if ((message[3] == 'e') && (message[10] == ']'))
                    {
                        DataIndex = BitConverter.ToUInt16(message, 4);
                        DataIndex++;
                        ReceivedAudioFileNumber = BitConverter.ToUInt16(message, 6);
                        Debug.WriteLine("Debug1");
           
                        SendAudioFile(ReceivedAudioFileNumber);
                        Debug.WriteLine("Debug5");
                   

                    }

                    // === Check if it is a complete packet. The device has received all packets ===
                    //else if ((message[3] == 's') && (message[11] == ']'))
                    //{

                    //}
                }
            }          
        }

        static void SendAudioFile(int AudioFileNumber)
        {
          
            if (AudioFileNumber == 0)
            {
                AudioFileNumber = 1;
            }

            if (DataIndex > 60000)
            {
                DataIndex = 0;
            }

            Debug.WriteLine("Debug2");

            if(TotalCount == 0)
            {
                StoreAudioFile(CurrentAudioFileNumber);
            }
           
            if (DataIndex == (TotalCount) && TransferStatus != (int)TransferStatusEnum.WaitEnd )
            {
                if (CurrentAudioFileNumber == (TotalAudioFiles))
                {
                   // TransferStatus = (int)TransferStatusEnum.WaitEnd;
                    TransferStatus = (int)TransferStatusEnum.Complete;
                    //=== Send Complete ===
                }
                else
                {
                    CurrentAudioFileNumber++;
                }
                DataIndex = 0;
                TotalCount = 0;
            }
            

          

            Debug.WriteLine("Debug3");
            if (TransferStatus == (int)TransferStatusEnum.Busy)
            {
                StoreAudioFile(CurrentAudioFileNumber);

                byte[] AudioFileChunk = Enumerable.Repeat((byte)0xFF, DataSize+14).ToArray();
                byte[] SentIndexArray = BitConverter.GetBytes(DataIndex);
                byte[] TotalCountArray = BitConverter.GetBytes(TotalCount);
                byte[] FileNumberArray = BitConverter.GetBytes(CurrentAudioFileNumber);
                byte[] FileTotalArray = BitConverter.GetBytes(TotalAudioFiles);

                AudioFileChunk[0] = (byte)'[';
                AudioFileChunk[1] = (byte)'&';
                AudioFileChunk[2] = (byte)'A';
                AudioFileChunk[3] = (byte)'D';
                AudioFileChunk[4] = SentIndexArray[0]; // === SentIndex ===
                AudioFileChunk[5] = SentIndexArray[1];
                AudioFileChunk[6] = TotalCountArray[0]; // === TotalCount ===
                AudioFileChunk[7] = TotalCountArray[1];
                AudioFileChunk[8] = FileNumberArray[0]; // === FileNumber ===
                AudioFileChunk[9] = FileNumberArray[1];
                AudioFileChunk[10] = FileTotalArray[0]; // === FileTotal ===
                AudioFileChunk[11] = FileTotalArray[1];
                Debug.WriteLine("Debug4 -" + CurrentAudioFileArray.Count().ToString() + "---" + DataSize .ToString() + "+"+ DataIndex.ToString());
                if ((DataSize + 1) * DataIndex <= CurrentAudioFileArray.Count())
                {
                    Buffer.BlockCopy(CurrentAudioFileArray, DataSize * DataIndex, AudioFileChunk, 12, DataSize);
             
                AudioFileChunk[TotalSize - 1] = (byte)']';

                GlobalSharedData.ServerMessageSend = AudioFileChunk;
                }
            }

        }

        static byte[] CurrentAudioFileArray = { 0 };
        static void StoreAudioFile(int FileNumber)
        {           
            AudioEntry _SelectedFile = AudioFileList.FirstOrDefault(t => t.ID == FileNumber);

            if (_SelectedFile != null)
            {
                // === Read datalog file ===
                //string _logInfoRaw = System.IO.File.ReadAllText(Log_Filename, Encoding.Default);          
                BinaryReader _breader = new BinaryReader(File.OpenRead(_SelectedFile.Path));
                CurrentAudioFileArray = _breader.ReadBytes((int)_SelectedFile.Size);
              
                TotalCount = (UInt16)(CurrentAudioFileArray.Length/ DataSize);
                _breader.Dispose();
                _breader.Close();
            }
        }
        private void ButtonNew_Click(object sender, RoutedEventArgs e)
        {
            // === Send start datalog informaiton ===      
            ReadAudioFiles();
            GlobalSharedData.ServerMessageSend = Encoding.ASCII.GetBytes("[&AA00]");
            CurrentAudioFileNumber = 1;
            DataIndex = 0;
        
            TransferStatus = (int)TransferStatusEnum.Initialize;
            PreviousAudioFileNumber = 1;
            CurrentAudioFileNumber = 1;
            ButtonBack.Content = "Cancel";
            WiFiconfig.Heartbeat = false;

            foreach ( AudioEntry item in AudioFileList)
            {
                item.Progress = 0;
                item.ProgressString = "0%";
            }
        }

       void ButtonState()
        {
            if(TransferStatus == (int)TransferStatusEnum.None)
            {
                if (ButtonNew.IsEnabled == false)
                {
                    ButtonNew.IsEnabled = true;
                    ButtonNext.IsEnabled = true;
                    ButtonPrevious.IsEnabled = true;
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
                }
            }
        }

        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            AudioEntry _item = (AudioEntry)DataGridAudioFiles.SelectedItem;
            SoundPlayer snd = new SoundPlayer(_item.Path);
            snd.Play();            
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
            ProgressBar_AudioFiles.Value = 0;
            foreach (AudioEntry item in AudioFileList)
            {
                item.Progress = 0;
                item.ProgressString = "";
            }
        }

        private void ButtonPrevious_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.ImageFilesView;
        }

        private void ButtonNext_Click(object sender, RoutedEventArgs e)
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
                if (ProgramFlow.SourseWindow == (int)ProgramFlowE.WiFi)
                {
                    DataGridAudioFiles.Columns[2].Visibility = Visibility.Visible;
                    ButtonNew.Visibility = Visibility.Visible;
         
                    Grid_Progressbar.Visibility = Visibility.Visible;
                    ButtonNext.Visibility = Visibility.Visible;
                    ButtonPrevious.Visibility = Visibility.Visible;
                    WiFiconfig.SelectedIP = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].IP;
                    SelectVID = WiFiconfig.TCPclients[GlobalSharedData.SelectedDevice].VID;
                    updateDispatcherTimer.Start();
                    ReadAudioFiles();
                    ProgressBar_AudioFiles.Value = 0;            
                }
                else
                {
                    DataGridAudioFiles.Columns[2].Visibility = Visibility.Collapsed;
                    ButtonNew.Visibility = Visibility.Collapsed;
          
                    Grid_Progressbar.Visibility = Visibility.Collapsed;
                    ButtonNext.Visibility = Visibility.Collapsed;
                    ButtonPrevious.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                updateDispatcherTimer.Stop();
                SelectVID = 0;
            }
        }

        private void DataGridAudioFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(DataGridAudioFiles.SelectedItems.Count == 1)
            {
                ButtonPlay.IsEnabled = true;
            }
            else
            {
                ButtonPlay.IsEnabled = false;
            }
        }
    }
}
