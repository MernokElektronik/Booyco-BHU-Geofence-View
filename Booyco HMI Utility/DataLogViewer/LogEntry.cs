using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booyco_HMI_Utility
{

    enum PDS_Index
    {
        Threat_BID = 0,
        Thtreat_Kind = 1,
        Threat_Group = 2,
        Threat_Type = 3,
        Threat_Cluster_Width = 4,
        Threat_Sector = 5,
        Threat_Zone = 6,
        Threat_Speed = 7,
        Threat_Distance = 8,
        Threat_Heading = 9,
        Threat_LAT = 10,
        Threat_LON = 11,
        Threat_Acc = 12,
        Breake_Distance = 13,
        POI_Distance = 14,
        Threat_display_Priority = 15,
        Critical_Distance = 16,
        Warning_Distance = 17,
        Presence_Distance = 18,
        Speed = 19,
        Heading = 20,
        Accuracy = 21,
        Threat_Length = 22,
        Threat_Width = 23,
        Threat_Brake_Distance = 24,
        LAT = 25,
        LON = 26,
        POI_LAT = 27,
        POI_LON = 28,
        Threat_Scenario = 29,
        Threat_Display_x = 30,
        Threat_Display_y = 31,
        Threat_Positon = 32,
        Threat_Bearing = 33,

        POC_LAT = 34,
        POC_LON = 35 
    }

    class LogEntry : INotifyPropertyChanged
    {
        private uint _number;
        public uint Number
        {
            get
            {
                return _number;
            }
            set
            {
                _number = value;
                OnPropertyChanged("Number");
            }
        }

        private DateTime _dateTime;
        public DateTime DateTime
        {
            get
            {
                return _dateTime;
            }
            set
            {
                _dateTime = value;
                OnPropertyChanged("DateTime");
            }
        }

        private UInt16 _eventID;
        public UInt16 EventID
        {
            get
            {
                return _eventID;
            }
            set
            {
                _eventID = value;
                OnPropertyChanged("EventID");
            }
        }

        private string _eventName;
        public string EventName
        {
            get
            {
                return _eventName;
            }
            set
            {
                _eventName = value;
                OnPropertyChanged("EventName");
            }
        }
        private byte[] _rawData;
        public byte[] RawData
        {
            get
            {
                return _rawData;
            }
            set
            {
                _rawData = value;
                OnPropertyChanged("RawData");
            }
        }

        private byte[] _rawEntry;
        public byte[] RawEntry
        {
            get
            {
                return _rawEntry;
            }
            set
            {
                _rawEntry = value;
                OnPropertyChanged("RawEntry");
            }
        }

        private string _eventInfo;
        public string EventInfo
        {
            get
            {
                return _eventInfo;
            }
            set
            {
                _eventInfo = value;
                OnPropertyChanged("EventInfo");
            }
        }

        private List<string> _eventInfoList;
        public List<string> EventInfoList
        {
            get
            {
                return _eventInfoList;
            }
            set
            {
                _eventInfoList = value;
                OnPropertyChanged("EventInfoList");
            }
        }

        private List<string> _dataListString;
        public List<string> DataListString
        {
            get
            {
                return _dataListString;
            }
            set
            {
                _dataListString = value;
                OnPropertyChanged("DataListString");
            }
        }

        private List<double> _dataList;
        public List<double> DataList
        {
            get
            {
                return _dataList;
            }
            set
            {
                _dataList = value;
                OnPropertyChanged("DataList");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
