
using Booyco_HMI_Utility.Geofences;
using GMap.NET.WindowsPresentation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Booyco_HMI_Utility
{
    public enum ProgramFlowE
    {
        Startup,
        LoginView,
        WiFi,
        USB,
        File,
        Bluetooth,
        GPRS,
        Bootload,
        ConfigureMenuView,
        ParametersView,
        DataLogView,
        DataExtractorView,
        Mapview,
        HMIDisplayView,
        AudioFilesView,
        ImageFilesView,
        FileMenuView,
        DataLogFileView,
        ParameterFileView,
        GeofenceMapView,
        ServerView,
    }
    public enum AccessLevelEnum
    {
        Limited,
        Basic,
        Extended,
        Full,
        Unaccessable,
    }
    public enum ApplicationEnum
    {
        None,
        bootloader,
        ERB_Bootloader,
        Application
    }
    public static class ProgramFlow
    {
        public static int ProgramWindow { get; set; }
        public static int SourseWindow { get; set; }
    }

    public static class GlobalSharedData
    { 
        public static int AccessLevel = (int)AccessLevelEnum.Limited;
        public static int ConnectedDeviceApplicationState = (int)ApplicationEnum.None;
        public static bool CommunicationSent = false;
        public static string ServerStatus { get; set; }
        public static byte[] ServerMessageSend { get; set; }
        public static bool BroadCast { get; set; }
        public static List<NetworkDevice> NetworkDevices = new List<NetworkDevice>();
        public static int SelectedDevice { get; set; }
        public static uint SelectedVID = 0;
        public static bool ActiveDevice { get; set; }

        public static string WiFiApStatus;
        public static bool WiFiConnectionStatus;

        
        public static bool  ViewMode = false;
        public static string FilePath = "";
        public static List<MarkerEntry> PDSMapMarkers = new List<MarkerEntry>();
        public static List<HMIDisplayEntry> HMIDisplayList = new List<HMIDisplayEntry>();
        public static List<HMIRadarDisplayEntry> HMIRadarDisplayList = new List<HMIRadarDisplayEntry>();
        public static bool OnlyRadarSelected = false;
        public static DateTime StartDateTimeDatalog = new DateTime();
        public static DateTime EndDateTimeDatalog = new DateTime();

        public static GeoFenceObject geoFenceData = null; 
    }

    public class GeneralFunctions
    {
        #region General Functions
        public string StringConditionerIP(string value)
        {
            string string2 = value;
            string[] name2;
            Regex regexItem = new Regex("[^0-9.]");
            if (string2.Length <= 15)
            {
                string2 = string2.ToUpper();
            }
            else
            {
                string2 = string2.Substring(0, 15).ToUpper();
                //               MessageBox.Show("Tag name my not exceed a length of 15");
            }


            if (!regexItem.IsMatch(string2))
            {
                value = string2;
            }
            else
            {
                //                MessageBox.Show("Tag name my not not contain any special charcters");
                name2 = regexItem.Split(string2);
                string2 = "";
                for (int i = 0; i < name2.Length; i++)
                {
                    if (name2[i] != "")
                    {
                        string2 += name2[i];
                    }

                }
                //name = regexItem.Replace(name, "$");
                value = string2;
            }

            return value;
        }

        public string StringConditioner(string value)
        {
            string string2 = value;
            string[] name2;
            Regex regexItem = new Regex(@"[^A-Z0-9 _]");
            if (string2.Length <= 15)
            {
                string2 = string2.ToUpper();
            }
            else
            {
                string2 = string2.Substring(0, 15).ToUpper();
                //               MessageBox.Show("Tag name my not exceed a length of 15");
            }


            if (!regexItem.IsMatch(string2))
            {
                value = string2;
            }
            else
            {
                //                MessageBox.Show("Tag name my not not contain any special charcters");
                name2 = regexItem.Split(string2);
                string2 = "";
                for (int i = 0; i < name2.Length; i++)
                {
                    if (name2[i] != "")
                    {
                        string2 += name2[i];
                    }

                }
                //name = regexItem.Replace(name, "$");
                value = string2;
            }

            return value;
        }

        public string StringConditionerAlphaNum(string value, int length)
        {
            string string2 = value;
            string[] name2;
            Regex regexItem = new Regex(@"[^A-Z0-9 _]");
            if (string2.Length <= length)
            {
                string2 = string2.ToUpper();
            }
            else
            {
                string2 = string2.Substring(0, length).ToUpper();
                //               MessageBox.Show("Tag name my not exceed a length of 15");
            }


            if (!regexItem.IsMatch(string2))
            {
                value = string2;
            }
            else
            {
                //                MessageBox.Show("Tag name my not not contain any special charcters");
                name2 = regexItem.Split(string2);
                string2 = "";
                for (int i = 0; i < name2.Length; i++)
                {
                    if (name2[i] != "")
                    {
                        string2 += name2[i];
                    }

                }
                //name = regexItem.Replace(name, "$");
                value = string2;
            }

            return value;
        }
        public string StringNumConditioner(string value)
        {
            string string2 = value;
            string[] name2;
            Regex regexItem = new Regex("[^0-9]");
            if (string2.Length <= 15)
            {
                string2 = string2.ToUpper();
            }
            else
            {
                string2 = string2.Substring(0, 15).ToUpper();
                //               MessageBox.Show("Tag name my not exceed a length of 15");
            }


            if (!regexItem.IsMatch(string2))
            {
                value = string2;
            }
            else
            {
                //                MessageBox.Show("Tag name my not not contain any special charcters");
                name2 = regexItem.Split(string2);
                string2 = "";
                for (int i = 0; i < name2.Length; i++)
                {
                    if (name2[i] != "")
                    {
                        string2 += name2[i];
                    }

                }
                //name = regexItem.Replace(name, "$");
                value = string2;
            }

            ulong test;

            if (value != "")
            {
                test = Convert.ToUInt64(value);
                if (test <= 0)
                    test = 1;
                else if (test > ushort.MaxValue)
                    test = ushort.MaxValue;

                value = test.ToString();
            }



            return value;
        }

        #endregion
    }
}
