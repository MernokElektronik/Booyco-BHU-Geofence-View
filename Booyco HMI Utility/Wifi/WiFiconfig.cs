using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using NativeWifi;

namespace Booyco_HMI_Utility
{
    public class WiFiconfig
    {

        public string WiFiHotspotSSID = "BooycoHMIUtility";
        public string WiFiKey = "BC123456";

        public static bool Heartbeat = false;

        static DateTime timestamp;
        static bool FailFlag = false;

        //ConfigView Config = new ConfigView(); 

        //get status of the wifi hotspot created by the device
        public List<NetworkDevice> GetAllLocalIPv4(NetworkInterfaceType _type)
        {
            
            List<NetworkDevice> ipAddrList = new List<NetworkDevice>();
            var NetWorkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface item in NetWorkInterfaces)
            {

                if (item.OperationalStatus == OperationalStatus.Up) //item.NetworkInterfaceType == _type && 
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {                        
                            ipAddrList.Add(new NetworkDevice() { DeviceName = item.Name, DeviceTipe = item.NetworkInterfaceType.ToString(), DeviceIP = ip.Address.ToString()});                            
                        }
                    }
                }
            }
            return ipAddrList;
        }

        public static void WirelessHotspot(string ssid, string key, bool status)
        {
            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd.exe")
                {
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                Process process = Process.Start(processStartInfo);

                if (process != null)
                {
                    if (status)
                    {
                        process.StandardInput.WriteLine("netsh wlan set hostednetwork mode=allow ssid=" + ssid + " key=" + key);
                        process.StandardInput.WriteLine("netsh wlan start hostednetwork");
                        process.StandardInput.Close();
                    }
                    else
                    {
                        process.StandardInput.WriteLine("netsh wlan stop hostednetwork");
                        process.StandardInput.Close();
                    }
                }
            }
            catch
            {
                Debug.WriteLine(" ================= Wifi hotspot creation/close broke ==============");
            }

        }

        int prevCount = 0;
        public void IpWatcherStart()
        {
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(IPWatch);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 2);
            dispatcherTimer.Start();
        }

        private void IPWatch(object sender, EventArgs e)
        {

            GlobalSharedData.NetworkDevices = GetAllLocalIPv4(NetworkInterfaceType.Ethernet);
            if (GlobalSharedData.NetworkDevices.Count != prevCount)
            {
                prevCount = GlobalSharedData.NetworkDevices.Count;
            }

            if (GlobalSharedData.NetworkDevices.Where(t => t.DeviceTipe.Contains("Wireless")).Count() > 0 && GlobalSharedData.NetworkDevices.Where(t => t.DeviceIP == "192.168.137.1").ToList().Count >= 1)
            {
                GlobalSharedData.WiFiApStatus = "Wifi Access point created - " + WiFiHotspotSSID;
            }
            else if(GlobalSharedData.NetworkDevices.Where(t => t.DeviceTipe.Contains("Ethernet")).Count() > 0 && GlobalSharedData.NetworkDevices.Where(t => t.DeviceIP == "192.168.137.1").ToList().Count >= 1)
            {
                GlobalSharedData.WiFiApStatus = "Ethernet External Router - " + WiFiHotspotSSID;
            }
            else if (GlobalSharedData.NetworkDevices.Where(t => t.DeviceTipe.Contains("Wireless")).Count() == 0)
            {
                GlobalSharedData.WiFiApStatus = "Wifi Access point failed to create...";

            }
            else if (GlobalSharedData.NetworkDevices.Where(t => t.DeviceTipe.Contains("Wireless")).Count() > 0 && GlobalSharedData.NetworkDevices.Where(t => t.DeviceIP == "192.168.137.1").ToList().Count < 1)
            {
                GlobalSharedData.WiFiApStatus = "Wifi Access point failed to create...";
            }

        }
     
        
        public static bool BusyReconnecting = false;
        public static bool ConnectionError = false;
        public static string Hearted = "";
        public static string PCName = Environment.MachineName;
        public static byte[] HeartbeatMessage;

      
        public void ServerRun()
        {
            WirelessHotspot(WiFiHotspotSSID, WiFiKey, true);
            IpWatcherStart();

            byte[] bytes = new byte[30];
            Array.Copy(Encoding.ASCII.GetBytes(PCName), bytes, Encoding.ASCII.GetBytes(PCName).Length);
         
 
            HeartbeatMessage = Enumerable.Repeat((byte)0, 522).ToArray();
            HeartbeatMessage[0] = (byte)'[';
            HeartbeatMessage[1] = (byte)'&';
            HeartbeatMessage[2] = (byte)'B';
            HeartbeatMessage[3] = (byte)'h';
            long UnixTimeStamp = DateTimeCheck.DateTimeStampToUnixTime(DateTime.Now);
            byte[] UnixByteArray = BitConverter.GetBytes(Convert.ToUInt32(UnixTimeStamp));
            HeartbeatMessage[34] = UnixByteArray[0];
            HeartbeatMessage[35] = UnixByteArray[1];
            HeartbeatMessage[36] = UnixByteArray[2];
            HeartbeatMessage[37] = UnixByteArray[3];          
            HeartbeatMessage[521] = (byte)']';

            Array.Copy(bytes, 0, HeartbeatMessage, 4, 30);
            Debug.WriteLine(Encoding.ASCII.GetString(HeartbeatMessage, 0, 34));
     

            Thread newThread = new Thread(new ThreadStart(StartServer))
            {
                IsBackground = true,
                Name = "ServerThread"
            };
            newThread.Start();
        }

      
        IPEndPoint ip;
        TcpListener server;
        Socket socket;
        TcpClient client;
        private DispatcherTimer dispatcherTimer;
        public static bool endAll = false;
        public static string SelectedIP;
        public static List<TcpClient> clients;
        public static List<TCPclientR> TCPclients = new List<TCPclientR>();
        private int clientnum = 0;
        private static int pretCount = 0;
        private static bool overflow = false;


        static void ConfigureTcpSocket(Socket tcpSocket)
        {
            // Don't allow another socket to bind to this port.
            tcpSocket.ExclusiveAddressUse = true;

            tcpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            //tcpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);


            //byte[] InValue = new byte[12];
            //bool ON_ = true;
            //uint KeepInetrVal = 1000;
            //uint KeepLive = 5000;

            //byte[] ON_bytes = BitConverter.GetBytes(Convert.ToUInt32(ON_));
            //byte[] KeepInetrVal_bytes = BitConverter.GetBytes(Convert.ToUInt32(KeepInetrVal));
            //byte[] KeepLive_bytes = BitConverter.GetBytes(Convert.ToUInt32(KeepLive));

            //Array.Copy(ON_bytes, InValue, 4);
            //Array.Copy(KeepLive_bytes, 0, InValue, 4, 4);
            //Array.Copy(KeepInetrVal_bytes, 0, InValue, 8, 4);

            //tcpSocket.IOControl(IOControlCode.KeepAliveValues, InValue, null);

            // Disable the Nagle Algorithm for this tcp socket.
            tcpSocket.NoDelay = true;
           
             //Set the receive buffer size to 8k
            tcpSocket.ReceiveBufferSize = 8192;

            //           // Set the timeout for synchronous receive methods to 
            //           // 1 second (1000 milliseconds.)
            //tcpSocket.ReceiveTimeout = 3000;

            // Set the send buffer size to 8k.
            //tcpSocket.SendBufferSize = 8192;

            //           // Set the timeout for synchronous send methods
            //           // to 1 second (1000 milliseconds.)			
            //           tcpSocket.SendTimeout = 3000;

            // Set the Time To Live (TTL) to 42 router hops.
            //tcpSocket.Ttl = 42;

        }

        private void StartServer()
        {
            endAll = false;
            SelectedIP = "";
            clients = new List<TcpClient>();
            try
            {                            
                server = new TcpListener(IPAddress.Any, 13000);
                server.Start();
        
                //                ip = new IPEndPoint(IPAddress.Any, 13000); //Any IPAddress that connects to the server on any port
                //                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); //Initialize a new Socket
                //                ConfigureTcpSocket(socket);
                //                socket.Bind(ip); //Bind to the client's IP
                //                socket.Listen(10); //Listen for maximum 10 connections

                Thread ClientsThread = new Thread(new ThreadStart(GetClients))
                {
                    IsBackground = true,
                    Name = "ClientsThread"
                };
                ClientsThread.Start();

            }
            catch (SocketException e)
            {
                Debug.WriteLine("SocketException: {0}", e);
               // var prc = new ProcManager();
               // prc.KillByPort(13000);
               // Thread.Sleep(20);
               /// StartServer();
            }
        }


public string GetMacAddress(string ipAddress)
{
    string macAddress = string.Empty;
    System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
    pProcess.StartInfo.FileName = "arp";
    pProcess.StartInfo.Arguments = "-a " + ipAddress;
    pProcess.StartInfo.UseShellExecute = false;
    pProcess.StartInfo.RedirectStandardOutput = true;
      pProcess.StartInfo.CreateNoWindow = true;
    pProcess.Start();
    string strOutput = pProcess.StandardOutput.ReadToEnd();
    string[] substrings = strOutput.Split('-');
    if (substrings.Length >= 8)
    {
       macAddress = substrings[3].Substring(Math.Max(0, substrings[3].Length - 2)) 
                + "-" + substrings[4] + "-" + substrings[5] + "-" + substrings[6] 
                + "-" + substrings[7] + "-" 
                + substrings[8].Substring(0, 2);
        return macAddress;
    }

    else
    {
        return "not found";
    }
}
        private void GetClients()
        {
            int clientslot = 0;
            GlobalSharedData.ServerStatus = " Server running. Waiting for a client...";

           

            while (clientnum < 10 && !endAll)
            {

                Debug.WriteLine("Waiting for a client...");
                try
                {
                    client = server.AcceptTcpClient();
                    IPEndPoint clientel = (IPEndPoint)client.Client.RemoteEndPoint;

                    //if (clients.Count> 0 && (clients.Where(t => t.Client.Connected == false).Count() != 0))
                    //{
                    //    clients.Where(t => t.Client.RemoteEndPoint.ToString().Contains(clientel.Address.ToString())).ToList().First().Client.Close();
                    //    clients.Remove(clients.Where(t => t.Client.RemoteEndPoint.ToString().Contains(clientel.Address.ToString())).ToList().First());
                    //    // List<TcpClient> clientR = clients.Where(t => t.Client.RemoteEndPoint.ToString().Contains(clientel.Address.ToString())).ToList();
                    //    // clients.Remove(clientR[0]);
                    //    //ClientLsitChanged(TCPclients);
                    //    clientnum--;
                    //}

                   
                  

                    if (clients.Count > 0 && clients.Where(t => t.Client.RemoteEndPoint.ToString().Contains(clientel.Address.ToString())).ToList().Count() != 0)
                    {
                         
                        clients.Where(t => t.Client.RemoteEndPoint.ToString().Contains(clientel.Address.ToString())).ToList().First().Client.Close();
                        clients.Remove(clients.Where(t => t.Client.Connected == false).ToList().First());
                        Debug.WriteLine("********************************************************************************************");

                        // List<TcpClient> clientR = clients.Where(t => t.Client.RemoteEndPoint.ToString().Contains(clientel.Address.ToString())).ToList();
                        // clients.Remove(clientR[0]);
                        ClientLsitChanged(TCPclients);
                        clientnum--;
                    }
                    
                    clients.Add(client);

                    IPEndPoint clientep = (IPEndPoint)clients[clientnum].Client.RemoteEndPoint;

                    Debug.WriteLine("Connected with {0} at port {1}", clientep.Address, clientep.Port);
               
                    ClientLsitChanged(TCPclients);
                    TCPclients.Last().Heartbeat_Colour = WiFiView.HBConnectingColour;
                    if (!endAll)
                    {
                        Thread readThread = new Thread(() => RecieveBytes(client.Client.RemoteEndPoint))
                        {
                            IsBackground = true,
                            Name = "ServerRecieve:" + clientnum.ToString()
                        };

                        readThread.Start();
                        clientslot = clientnum;
                    }

                    if (!endAll)
                    {
                        Thread sendThread = new Thread(() => ClientSendBytes(client.Client.RemoteEndPoint, clientslot))
                        {
                            IsBackground = true,
                            Name = "ServerSend:" + clientnum.ToString()
                        };
                        sendThread.Start();
                    }


                    //Thread PollThread = new Thread(() => ClientsPoll(client.Client.RemoteEndPoint))
                    //{
                    //    IsBackground = true,
                    //    Name = "ServerPoll:" + clientnum.ToString()
                    //};
                    //PollThread.Start();

                    clientnum++;
                  

                }
                catch
                {
                    Debug.WriteLine("Client connection failed...");
                    GlobalSharedData.ServerStatus = "Client connection failed...";
                }

            }

            Debug.WriteLine("Maximum amount of clients reached!");
            GlobalSharedData.ServerStatus = "Server closed";

        }

        private void RecieveBytes(EndPoint clientnumr)
        {
            int messagecount = 0;
            int ValidMessages = 0;
            bool messageReceived = false;

            List<TcpClient> clientR = clients.Where(t => t.Client.RemoteEndPoint == clientnumr).ToList();
            byte[] data2 = new byte[DataExtractorView.DATALOG_RX_SIZE + 10];
            byte[] Buffer = new byte[DataExtractorView.DATALOG_RX_SIZE + 10];
            int i;
            int count = 0;
            int totalCount = 0;
            int heartbeatCounter = 0;
            clientR[0].ReceiveTimeout = 10000;
            clientR[0].NoDelay = true;
      
           
          
            while (clientR[0].Connected && !endAll /*&& !clientR[0].Client.Poll(50, SelectMode.SelectRead)*/)
            {
                NetworkStream stream = clientR[0].GetStream();
                try
                {
                    if ((i = stream.Read(data2, 0, data2.Length)) != 0)
                    {

                        // if (clientR[0].ReceiveTimeout!= 10000)
                        //   {
                        //       clientR[0].ReceiveTimeout = 10000;
                        //   }
                        Debug.WriteLine(" i: " + i.ToString() + " totalcount:" + totalCount.ToString() + " Buffer" + Buffer[0].ToString() + "-" + Buffer[512 + 9].ToString());
                       
                        if (i == DataExtractorView.DATALOG_RX_SIZE + 10 && data2[0] == '[' && data2[DataExtractorView.DATALOG_RX_SIZE + 9] == ']')
                        {
                            Buffer = new byte[DataExtractorView.DATALOG_RX_SIZE + 10];
                            Array.Copy(data2, Buffer, DataExtractorView.DATALOG_RX_SIZE + 10);
                            messagecount++;
                            messageReceived = true;
                            totalCount = i;
                        }
                        else if (i >= 522 && data2[0] == '[' && data2[1] == '&' && data2[521] == ']')
                        {
                            Buffer = new byte[DataExtractorView.DATALOG_RX_SIZE + 10];
                            Array.Copy(data2, Buffer, 522);
                            messagecount++;
                            messageReceived = true;
                            totalCount = 522;
                        }
                        else if (i == 12 && data2[0] == '[' && data2[1] == '&' && data2[11] == ']')
                        {
                            Buffer = new byte[DataExtractorView.DATALOG_RX_SIZE + 10];
                            Array.Copy(data2, Buffer, 12);
                            messagecount++;
                            messageReceived = true;
                            totalCount = 12;
                        }
                        else if (i == 11 && data2[0] == '[' && data2[1] == '&' && data2[10] == ']')
                        {
                            Buffer = new byte[DataExtractorView.DATALOG_RX_SIZE + 10];
                            Array.Copy(data2, Buffer, 11);
                            messagecount++;
                            messageReceived = true;
                            totalCount = 11;
                        }
                        else if (i == 10 && data2[0] == '[' && data2[1] == '&' && data2[9] == ']')
                        {
                            Buffer = new byte[DataExtractorView.DATALOG_RX_SIZE + 10];
                            Array.Copy(data2, Buffer, 10);
                            messagecount++;
                            messageReceived = true;
                            totalCount = 10;
                        }
                        else if (i == 9 && data2[0] == '[' && data2[1] == '&' && data2[8] == ']')
                        {
                            Buffer = new byte[DataExtractorView.DATALOG_RX_SIZE + 10];
                            Array.Copy(data2, Buffer, 9);
                            messagecount++;
                            messageReceived = true;
                            totalCount = 9;
                        }
                        else if (i == 8 && data2[0] == '[' && data2[1] == '&' && data2[7] == ']')
                        {
                            Buffer = new byte[DataExtractorView.DATALOG_RX_SIZE + 10];
                            Array.Copy(data2, Buffer, 9);
                            messagecount++;
                            messageReceived = true;
                            totalCount = 8;
                        }
                        else if (i == 7 && data2[0] == '[' && data2[1] == '&' && data2[6] == ']')
                        {
                            Buffer = new byte[DataExtractorView.DATALOG_RX_SIZE + 10];
                            Array.Copy(data2, Buffer, 7);
                            messagecount++;
                            messageReceived = true;
                            totalCount = i;
                        }

                        else if (data2[0] == '[' && data2[1] == '&' && (data2[2] == 'L' || data2[2] == 'O'))
                        {
                            Debug.WriteLine(Buffer[0] + "Receive Parsing - First: " + i.ToString() +"  " + DateTime.Now.ToString());

                            Buffer = new byte[DataExtractorView.DATALOG_RX_SIZE + 10];
                            Array.Copy(data2, 0, Buffer, count, i);
                            count = i;
                            totalCount = count;                        
                        }

                        else if (count > 0)
                        {
                           
                            if ((totalCount + i) > (DataExtractorView.DATALOG_RX_SIZE + 10))
                            {
                                Array.Copy(data2, 0, Buffer, totalCount, (DataExtractorView.DATALOG_RX_SIZE + 10) - totalCount);
                                overflow = true;
                                Debug.WriteLine("Receive Parsing - Overflow:" + (totalCount + i).ToString());
                            }
                            else
                            {                               
                                Array.Copy(data2, 0, Buffer, totalCount, i);
                                Debug.WriteLine("Receive Parsing - Append:" + (totalCount + i).ToString());
                            }

                            totalCount += i;
                            if (totalCount >= DataExtractorView.DATALOG_RX_SIZE + 10)
                            {                             
                                count = 0;
                            }                    

                            if (Buffer[0] == '[' && Buffer[DataExtractorView.DATALOG_RX_SIZE + 9] == ']')
                            {
                                Debug.WriteLine("Receive Parsing - Successful: " + totalCount + i);
                                messagecount++;
                                messageReceived = true;
                            }
                            else if (totalCount >= 522 && Buffer[0] == '[' && Buffer[1] == '&' && Buffer[521] == ']')
                            {                             
                                messagecount++;
                                messageReceived = true;
                                count = 0;
                            }

                        }

                   

                        if (messageReceived)
                        {
                      
                           messageReceived = false;

                            

                            //GlobalSharedData.ServerStatus = "Received: " + recmeg + " from: " + clientR[0].RemoteEndPoint;
                            Debug.WriteLine("Process Message - " +totalCount.ToString() + " -Recieved: " + Encoding.UTF8.GetString(Buffer, 0, 5) + "-0x" + Buffer[5].ToString("X2")  + " Time: " + DateTime.Now.ToLongTimeString());
                          
                            if (Buffer[0] == '[' && Buffer[1] == '&' && Buffer[2] == 'B' && Buffer[3] == 'h' /*&& Buffer[521] == ']'*/)
                            {
                                ValidMessages++;
                               
                                if (!Bootloader.BootReady)

                                {

                                    //  GlobalSharedData.ServerStatus = "Heartbeat message recieved: " + heartbeatCounter++.ToString();
                                    try
                                    {
                                        TCPclients.ElementAt(clients.IndexOf(clientR[0])).Name = Encoding.ASCII.GetString(Buffer, 8, 15);
                                        TCPclients.ElementAt(clients.IndexOf(clientR[0])).VID = BitConverter.ToUInt32(Buffer, 4);
                                        TCPclients.ElementAt(clients.IndexOf(clientR[0])).FirmRev = Buffer[23];
                                        TCPclients.ElementAt(clients.IndexOf(clientR[0])).FirmSubRev = Buffer[24];
                                        TCPclients.ElementAt(clients.IndexOf(clientR[0]))._ApplicationState = Buffer[25];
                                        TCPclients.ElementAt(clients.IndexOf(clientR[0])).FirmwareString = Buffer[23].ToString() + "." + Buffer[24].ToString().PadLeft(2, '0');
                                        TCPclients.ElementAt(clients.IndexOf(clientR[0])).BootloaderFirmRev = Buffer[26];
                                        TCPclients.ElementAt(clients.IndexOf(clientR[0])).BootloaderFirmSubRev = Buffer[27];
                                        TCPclients.ElementAt(clients.IndexOf(clientR[0])).BootloaderrevString = Buffer[26].ToString() + "." + Buffer[27].ToString().PadLeft(2, '0');
                                        TCPclients.ElementAt(clients.IndexOf(clientR[0])).HeartCount++;
                                        TCPclients.ElementAt(clients.IndexOf(clientR[0])).Licensed = Convert.ToBoolean(Buffer[28]);
                                        TCPclients.ElementAt(clients.IndexOf(clientR[0])).Heartbeat_Colour = WiFiView.HBReceiveColour;
                                        TCPclients.ElementAt(clients.IndexOf(clientR[0])).HeartbeatTimestamp = DateTime.Now;

                                        long UnixTimeStamp = DateTimeCheck.DateTimeStampToUnixTime(DateTime.Now);
                                        byte[] UnixByteArray = BitConverter.GetBytes(Convert.ToUInt32(UnixTimeStamp));
                                        HeartbeatMessage[34] = UnixByteArray[0];
                                        HeartbeatMessage[35] = UnixByteArray[1];
                                        HeartbeatMessage[36] = UnixByteArray[2];
                                        HeartbeatMessage[37] = UnixByteArray[3];

                                        if (GlobalSharedData.SelectedVID == (uint)TCPclients.ElementAt(clients.IndexOf(clientR[0])).VID)
                                        {
                                            GlobalSharedData.WiFiConnectionStatus = true;   
                                        }


                                    }
                                    catch
                                    {
                                        Debug.WriteLine("Heartbeat not parsed!");
                         
                                        TCPclients.ElementAt(clients.IndexOf(clientR[0])).Name = "UNKNOWN DEVICE";
                                        TCPclients.ElementAt(clients.IndexOf(clientR[0])).VID = 0;
                                        TCPclients.ElementAt(clients.IndexOf(clientR[0])).FirmRev = 0;

                                        TCPclients.ElementAt(clients.IndexOf(clientR[0])).FirmSubRev = 0;
                                        TCPclients.ElementAt(clients.IndexOf(clientR[0]))._ApplicationState = 3;
                                        TCPclients.ElementAt(clients.IndexOf(clientR[0])).FirmwareString = "-.-";
                                        TCPclients.ElementAt(clients.IndexOf(clientR[0])).BootloaderFirmRev = 0;
                                        TCPclients.ElementAt(clients.IndexOf(clientR[0])).BootloaderFirmSubRev = 0;
                                        TCPclients.ElementAt(clients.IndexOf(clientR[0])).BootloaderrevString = "-.-";
                                        TCPclients.ElementAt(clients.IndexOf(clientR[0])).HeartCount++;
                                        TCPclients.ElementAt(clients.IndexOf(clientR[0])).Licensed = false;
                                       
                                    }

                                    stream.Write(HeartbeatMessage, 0, HeartbeatMessage.Length); //Send the data to the client  
                                    if (GlobalSharedData.SelectedVID == (uint)TCPclients.ElementAt(clients.IndexOf(clientR[0])).VID)
                                    {
                                        Debug.WriteLine("HeartBeat", clientnumr);
                                        Heartbeat = true;
                                        DataExtractorView.Heartbeat = true;                                                            //Console.WriteLine("====================heartbeat recieved ======================:" + ValidMessages.ToString());

                                    }

                                    //DataExtractorView.Heartbeat = true;                                                            //Console.WriteLine("====================heartbeat recieved ======================:" + ValidMessages.ToString());
                                }
                           
                            }

                            if (GlobalSharedData.SelectedVID == (uint)TCPclients.ElementAt(clients.IndexOf(clientR[0])).VID)
                            {
                                if (Buffer[2] == 'b' || Buffer[2] == 'w')
                                {
                                    ValidMessages++;
                                    UploadFile.FileUploadParse(Buffer, clientnumr);
                                }
                                if (Buffer[2] == 'B')
                                {
                                    ValidMessages++;
                                    Bootloader.BootloaderParse(Buffer, clientnumr);
                                }
                                else if (Buffer[2] == 'P')
                                {
                                    ParametersView.ConfigSendParse(Buffer, clientnumr);
                                }
                                else if (Buffer[2] == 'p')
                                {
                                    ParametersView.ConfigReceiveParamsParse(Buffer, clientnumr);
                                }
                                else if (Buffer[2] == 'A')
                                {
                                    AudioFilesView.AudioFileSendParse(Buffer, clientnumr);
                                }
                                else if (Buffer[2] == 'I')
                                {
                                    ImageFilesView.ImageFileSendParse(Buffer, clientnumr);
                                }
                                else if (Buffer[2] == 'L' || Buffer[2] == 'O')
                                {
                                    DataExtractorView.DataExtractorParser(Buffer, clientnumr);
                                }
                            }
                            
                             Hearted = " message recieved:" + ValidMessages.ToString() + " of " + messagecount.ToString();
                            
                            

                        }

                        if (overflow)
                        {
                            try
                            {
                                count = totalCount - (DataExtractorView.DATALOG_RX_SIZE + 10);
                                Buffer = new byte[DataExtractorView.DATALOG_RX_SIZE + 10];

                                totalCount = count;
                                Debug.WriteLine(" LAST: " + totalCount.ToString());
                                Array.Copy(data2, i - count, Buffer, 0, count);
                                overflow = false;
                            }
                            catch
                            {
                                Debug.WriteLine("Last Failed", clientnumr);
                            }
                        }
                        data2 = new byte[DataExtractorView.DATALOG_RX_SIZE + 10];
                    }

                   
                }
                catch (Exception e)
                {

                    Debug.WriteLine("-------------- {0} recieve broke", clientnumr);
                    //Console.WriteLine(e.ToString());
                    break;
                }
              
            }
            Debug.WriteLine("-------------- {0} closed recieve", clientnumr);

            try
            {
                if (clientR[0].Connected)
                {

                    if (GlobalSharedData.SelectedVID == TCPclients.ElementAt(clients.IndexOf(clientR[0])).VID)
                    {
                        GlobalSharedData.WiFiConnectionStatus = false;

                    }
                    IPEndPoint clientel = (IPEndPoint)clientR[0].Client.RemoteEndPoint;
                    if (clients.Where(t => t.Client.RemoteEndPoint.ToString().Contains(clientel.Address.ToString())).ToList().Count() != 0)
                    {
                        clientR[0].Close();
                        ClientLsitChanged(TCPclients);
                        clients.Remove(clientR[0]);
                        clientnum--;
                        Debug.WriteLine("-------------- {0} Client Removed", clientnumr);
                    }
                }
            }
            catch
            {
                Debug.WriteLine("-------------- {0} Client Removed failed", clientnumr);
            }
        }

        private void ClientSendBytes(EndPoint clientnumr, int remover)
        {
            List<TcpClient> clientR = clients.Where(t => t.Client.RemoteEndPoint == clientnumr).ToList();

       
        
            try
            { 

                NetworkStream stream = clientR[0].GetStream();

                stream.Write(HeartbeatMessage, 0, HeartbeatMessage.Length);

                byte[] data = new byte[HeartbeatMessage.Length];
                int counter = 0;
                while (clientR[0].Connected && !endAll /*&& !clientR[0].Client.Poll(20, SelectMode.SelectRead)*/)
                {
                        try
                        {
                      
                            if ((SelectedIP == clientnumr.ToString() || GlobalSharedData.BroadCast == true) && GlobalSharedData.ServerMessageSend != null && GlobalSharedData.ServerMessageSend != null)
                            {
                                data = new byte[GlobalSharedData.ServerMessageSend.Length];
                                data = GlobalSharedData.ServerMessageSend;
                                try
                                {
                                    stream.Write(data, 0, data.Length); //Send the data to the client
                                }
                                catch
                                {
                                    try
                                    {
                                        clientR = clients.Where(t => t.Client.RemoteEndPoint == clientnumr).ToList();
                                        stream.Write(data, 0, data.Length); //Send the data to the client
                                    }
                                    catch
                                    {
                                        Debug.WriteLine("client not found in list!");
                                        break;
                                    }
                                }

                                //ServerStatus = "Sent: " + ServerMessageSend + " to " + clientR[0].RemoteEndPoint;
                                //GlobalSharedData.ServerStatus = "Message sent";
                                GlobalSharedData.CommunicationSent = true;
                            //Console.WriteLine("Sent: {0}", Encoding.UTF8.GetString(GlobalSharedData.ServerMessageSend));
                            string SentString = Encoding.UTF8.GetString(GlobalSharedData.ServerMessageSend, 0, 4);
                                if (SentString[3] == 'D')
                            {
                                //Debug.WriteLine("Sent: " + SentString + "-0x" + GlobalSharedData.ServerMessageSend[6].ToString("X2")  + GlobalSharedData.ServerMessageSend[5].ToString("X2") + GlobalSharedData.ServerMessageSend[8].ToString("X2") + GlobalSharedData.ServerMessageSend[7].ToString("X2")+ "       Time: " + DateTime.Now.ToLongTimeString());

                            }
                            else
                            {
                                //Debug.WriteLine("Sent: " + SentString + "-0x" + GlobalSharedData.ServerMessageSend[5].ToString("X2") + "-0x" + GlobalSharedData.ServerMessageSend[6].ToString("X2") + "-0x" + GlobalSharedData.ServerMessageSend[7].ToString("X2") + "-0x" + GlobalSharedData.ServerMessageSend[8].ToString("X2") + "       Time: " + DateTime.Now.ToLongTimeString());

                            }
                            GlobalSharedData.ServerMessageSend = null;
                            }                        
                      
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("-------------- The sending failed");
                            //break;
                        }                    
                }       
            }
            catch
            {
                Debug.WriteLine("-------------- {0} closed send due to error", clientnumr);
                return;
            }

            Debug.WriteLine("-------------- {0} closed send", clientnumr);
        }

        public List<TCPclientR> ClientLsitChanged(List<TCPclientR> tCPclientR)
        {
            try
            {
                if (clients != null && clients.Count != pretCount)
                {
                    List<TCPclientR> TCPclientsdumm = new List<TCPclientR>();
                    foreach (var item in clients)
                    {
                     
                        if (tCPclientR.Where(t => t.IP == item.Client.RemoteEndPoint.ToString()).ToList().Count > 0)
                        {
                            TCPclientsdumm.Add(tCPclientR.Where(t => t.IP == item.Client.RemoteEndPoint.ToString()).First());
                        }
                        else
                        {
                            var mac = GetMacAddress(item.Client.RemoteEndPoint.ToString().Split(':').ElementAt(0)).Replace('-',':');
                            TCPclientsdumm.Add(new TCPclientR() { IP = item.Client.RemoteEndPoint.ToString(), MACAddress = mac});
                        }
                    }
                    TCPclients = TCPclientsdumm;
                    pretCount = clients.Count;
                    return TCPclientsdumm;
                }
                else
                    return TCPclients;
            }
            catch
            {
                Debug.WriteLine("----failed to update client list----");
                return TCPclients;
            }
        }

        public void ServerStop()
        {
            endAll = true;
            try
            {
                foreach (TcpClient item in clients)
                {
                    try
                    {
                        item.Close();
                    }
                    catch
                    {
                        Debug.WriteLine("failed to close port..");
                        try
                        {
                            item.Dispose();
                        }
                        catch
                        {
                            Debug.WriteLine("Failed to dispose port..");
                        }
                    }
                }

                try
                {

                    server.Stop();
                   
                  
                }
                catch(Exception ex)
                {
                    Debug.WriteLine("Server failed to stop...");
                    //var prc = new ProcManager();
                    //prc.KillByPort(13000);
                }

                try
                {
                    WirelessHotspot(null, null, false);
                }
                catch
                {

                    Debug.WriteLine("Wifi failed to stop...");
                }

            }
            catch(Exception ex)
            {
                Debug.WriteLine("Server close error=========");
            }                    
        }

     
    }

    
    public class PRC
    {
        public int PID { get; set; }
        public int Port { get; set; }
        public string Protocol { get; set; }
    }
    public class ProcManager
    {
        public void KillByPort(int port)
        {
            var processes = GetAllProcesses();
            if (processes.Any(p => p.Port == port))
                try
                {
                    Process.GetProcessById(processes.First(p => p.Port == port).PID).Kill();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            else
            {
                Debug.WriteLine("No process to kill!");
            }
        }

        public List<PRC> GetAllProcesses()
        {
            var pStartInfo = new ProcessStartInfo();
            pStartInfo.FileName = "netstat.exe";
            pStartInfo.Arguments = "-a -n -o";
            pStartInfo.WindowStyle = ProcessWindowStyle.Maximized;
            pStartInfo.UseShellExecute = false;
            pStartInfo.RedirectStandardInput = true;
            pStartInfo.RedirectStandardOutput = true;
            pStartInfo.RedirectStandardError = true;
            pStartInfo.CreateNoWindow = true;
            var process = new Process()
            {
                StartInfo = pStartInfo
            };
            process.Start();

            var soStream = process.StandardOutput;

            var output = soStream.ReadToEnd();
            if (process.ExitCode != 0)
                throw new Exception("something broke");

            var result = new List<PRC>();

            var lines = Regex.Split(output, "\r\n");
            foreach (var line in lines)
            {
                if (line.Trim().StartsWith("Proto"))
                    continue;

                var parts = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                var len = parts.Length;
                if (len > 2)
                    result.Add(new PRC
                    {
                        Protocol = parts[0],
                        Port = int.Parse(parts[1].Split(':').Last()),
                        PID = int.Parse(parts[len - 1])
                    });


            }
            return result;
        }
    }
  

 
    public class NetworkDevice : INotifyPropertyChanged
    {
       
        /////////////////////////////////////////////////////////////
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /////////////////////////////////////////////////////////////
      

        private string _deviceName = "NO NAME";

        public string DeviceName
        {
            get { return _deviceName; }
            set
            {
                if (_deviceName != value)
                {
                    _deviceName = value;
                    OnPropertyChanged("DeviceName");
                }
            }
        }

        private string _deviceType = "0";

        public string DeviceTipe
        {
            get { return _deviceType; }
            set
            {
                if (_deviceType != value)
                {
                    _deviceType = value;
                    OnPropertyChanged("DeviceTipe");
                }
            }
        }

        private string _deviceIP = "0" ;

        public string DeviceIP
        {
            get { return _deviceIP; }
            set
            {
                if (_deviceIP != value)
                {
                    _deviceIP = value;
                    OnPropertyChanged("DeviceIP");
                }
            }
        }

      
    }

    public class TCPclientR : INotifyPropertyChanged
    {
        GeneralFunctions generalFunctions = new GeneralFunctions();
    
        /////////////////////////////////////////////////////////////
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /////////////////////////////////////////////////////////////
   

        private string _IP ;
        public string IP
        {
            get { return _IP; }
            set
            {
                if (_IP != value)
                {
                    _IP = value;
                    OnPropertyChanged("IP");
                }
            }
        }

        private string _Name ;
        public string Name
        {
            get
            {
                char[] NameCahr;
                try
                {
                    if (_Name != null)
                    {
                        NameCahr = _Name.ToCharArray();
                    }
                    else
                    {
                        return "NO NAME";
                    }
                }
                catch
                {
                    return "NO NAME";
                }
                int c = 0;
                foreach (var item in NameCahr)
                {

                    if (item == ' ')
                        c++;
                }
                if (c >= 15)
                    return "NO NAME";
                else
                    return generalFunctions.StringConditioner(_Name);
            }
            set
            {
                _Name = value;
                OnPropertyChanged("Name");
            }
        }

        private uint _VID ;
        public uint VID
        {
            get { return _VID; }
            set
            {
                if (_VID != value)
                {
                    _VID = value;
                    OnPropertyChanged("VID");
                }
            }
        }

        private string _MACAddress;
        public string MACAddress
        {
            get { return _MACAddress; }
            set
            {
                if (_MACAddress != value)
                {
                    _MACAddress = value;
                    OnPropertyChanged("MACAddress");
                }
            }
        }
        private int _FirmRev ;
        public int FirmRev
        {
            get { return _FirmRev; }
            set
            {
                if (_FirmRev != value)
                {
                    _FirmRev = value;
                    OnPropertyChanged("FirmRev");
                }
            }
        }

        private bool _Licensed = false;

        public bool Licensed
        {
            get { return _Licensed; }
            set
            {
                if (_Licensed != value)
                {
                    _Licensed = value;
                    OnPropertyChanged("Licensed");
                }
            }
        }


        private string _FirmwareString ;
        public string FirmwareString
        {
            get { return _FirmwareString; }
            set { _FirmwareString = value; OnPropertyChanged("FirmwareString"); }
        }

        private int _FirmSubRev ;
        public int FirmSubRev
        {
            get { return _FirmSubRev; }
            set { _FirmSubRev = value; OnPropertyChanged("FirmSubRev"); }
        }

        private double _PacketLoss ;
        public double PacketLoss
        {
            get { return _PacketLoss; }
            set { _PacketLoss = value; OnPropertyChanged("PacketLoss"); }
        }

        private int _applicationState ;
        public int _ApplicationState
        {
            get { return _applicationState; }
            set
            {
                _applicationState = value;
                OnPropertyChanged("_ApplicationState");
                OnPropertyChanged("ApplicationState");
            }

        }
        public string ApplicationState
        {
            get
            {
                if (_ApplicationState == 1)
                    return "BHU App";
                else if (_ApplicationState == 0)
                    return "BHU BT";
                else if (_ApplicationState == 2)
                    return "ERB BT";
                else if (_ApplicationState == 10)
                    return "Comms Bridge BT";
                else if (_ApplicationState == 11)
                    return "Comms Bridge App";
                else if(_ApplicationState == 21)
                    return "BHU Test Station App";

                return "UNKOWN";
            }
            set
            {

                if (value == "BHU App")
                    _ApplicationState = 1;
                else if (value == "ERB BT")
                    _ApplicationState = 2;
                else if (value == "Comms Bridge BT")
                    _ApplicationState = 11;
                else if (value == "Comms Bridge App")
                    _ApplicationState = 10;
                else if (value == "BHU Test Station App")
                    _ApplicationState = 21;
                else
                    _ApplicationState = 0;

                OnPropertyChanged("ApplicationState");

            }
        }

        private int _BootloaderFirmRev ;
        public int BootloaderFirmRev
        {
            get { return _BootloaderFirmRev; }
            set
            {
                if (_BootloaderFirmRev != value)
                {

                    _BootloaderFirmRev = value;
                    OnPropertyChanged("BootloaderFirmRev");
                }
            }
        }

        private int _BootloaderFirmSubRev;
        public int BootloaderFirmSubRev
        {
            get { return _BootloaderFirmSubRev; }
            set
            {
                if (_BootloaderFirmSubRev != value)
                {
                    _BootloaderFirmSubRev = value;
                    OnPropertyChanged("BootloaderFirmSubRev");
                }
            }
        }

        private string _BootloaderrevString ;

        public string BootloaderrevString
        {
            get { return _BootloaderrevString; }
            set
            {
                if (_BootloaderrevString != value)
                {
                    _BootloaderrevString = value;
                    OnPropertyChanged("BootloaderrevString");
                }
            }
        }

        private int _HeartCount;

        public int HeartCount
        {
            get { return _HeartCount; }
            set {
                if (_HeartCount != value)
                {
                    if (value > 10000)
                    {
                        _HeartCount = 0;
                    }
                    else
                    {
                        _HeartCount = value;
                    }
                    OnPropertyChanged("HeartCount");
                }
            }
        }

        private DateTime _HeartbeatTimestamp;

        public DateTime HeartbeatTimestamp
        {
            get { return _HeartbeatTimestamp; }
            set
            {
                if (_HeartbeatTimestamp != value)
                {
                    _HeartbeatTimestamp = value;
                    OnPropertyChanged("HeartbeatTimestamp");
                }
            }
        }
        private SolidColorBrush _Heartbeat_Colour;
        public SolidColorBrush Heartbeat_Colour
        {
            get { return _Heartbeat_Colour; }
            set
            {
                if (_Heartbeat_Colour != value)
                {
                    _Heartbeat_Colour = value;
                    OnPropertyChanged("Heartbeat_Colour");
                }
            }
        }
    

}

}
