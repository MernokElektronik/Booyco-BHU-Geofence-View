using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booyco_HMI_Utility
{
    public class HMIDisplayEntry : INotifyPropertyChanged
    {
        private DateTime _startDateTime;
        public DateTime StartDateTime
        {
            get
            {
                return _startDateTime;
            }
            set
            {
                _startDateTime = value;
                OnPropertyChanged("StartDateTime");
            }
        }

        private DateTime _endDateTime;
        public DateTime EndDateTime
        {
            get
            {
                return _endDateTime;
            }
            set
            {
                _endDateTime = value;
                OnPropertyChanged("EndDateTime");
            }
        }



        private string _threatBID;
        public string ThreatBID
        {
            get
            {
                return _threatBID;
            }
            set
            {
                _threatBID = value;
                OnPropertyChanged("ThreatBID");
            }
        }

        private string _threatPriority;
        public string ThreatPriority
        {
            get
            {
                return _threatPriority;
            }
            set
            {
                _threatPriority = value;
                OnPropertyChanged("ThreatPriority");
            }
        }
        private List<PDSThreatEvent> _PDSThreat = new List<PDSThreatEvent>();
        public List<PDSThreatEvent> PDSThreat
        {
            get
            {
                return _PDSThreat;
            }
            set
            {
                _PDSThreat = value;
                OnPropertyChanged("PDSThreat");
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    public class PDSThreatEvent : INotifyPropertyChanged
    {

        private string _threatBIDHex;
        public string ThreatBIDHex
        {
            get
            {
                return _threatBID;
            }
            set
            {
                _threatBID = value;
                OnPropertyChanged("ThreatBIDHex");
            }
        }

        private string _threatBID;
        public string ThreatBID
        {
            get
            {
                return _threatBID;
            }
            set
            {
                _threatBID = value;
                OnPropertyChanged("ThreatBID");
            }
        }

        private DateTime _DateTime;
        public DateTime DateTime
        {
            get
            {
                return _DateTime;
            }
            set
            {
                _DateTime = value;
                OnPropertyChanged("DateTime");
            }
        }

        private string _threatType;
        public string ThreatType
        {
            get
            {
                return _threatType;
            }
            set
            {
                _threatType = value;
                OnPropertyChanged("ThreatType");
            }
        }

        private string _threatGroup;
        public string ThreatGroup
        {
            get
            {
                return _threatGroup;
            }
            set
            {
                _threatGroup = value;
                OnPropertyChanged("ThreatGroup");
            }
        }

        private string _threatSector;
        public string ThreatSector
        {
            get
            {
                return _threatSector;
            }
            set
            {
                _threatSector = value;
                OnPropertyChanged("ThreatSector");
            }
        }

        private string _threatWidth;
        public string ThreatWidth
        {
            get
            {
                return _threatWidth;
            }
            set
            {
                _threatWidth = value;
                OnPropertyChanged("ThreatWidth");
            }
        }

        private int _threatZone;
        public int ThreatZone
        {
            get
            {
                return _threatZone;
            }
            set
            {
                _threatZone = value;
                OnPropertyChanged("ThreatZone");
            }
        }

        private string _threatDistance;
        public string ThreatDistance
        {
            get
            {
                return _threatDistance;
            }
            set
            {
                _threatDistance = value;
                OnPropertyChanged("ThreatDistance");
            }
        }
        private string _threatHeading;
        public string ThreatHeading
        {
            get
            {
                return _threatHeading;
            }
            set
            {
                _threatHeading = value;
                OnPropertyChanged("ThreatHeading");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    public class HMIRadarDisplayEntry : INotifyPropertyChanged
    {
        private int _threatID;
        public int ThreatID
        {
            get
            {
                return _threatID;
            }
            set
            {
                _threatID = value;
                OnPropertyChanged("ThreatID");
            }
        }

        private int _threatCordinateX;
        public int ThreatCordinateX
        {
            get
            {
                return _threatCordinateX;
            }
            set
            {
                _threatCordinateX = value;
                OnPropertyChanged("ThreatCordinateX");
            }
        }

        private int _threatCordinateY;
        public int ThreatCordinateY
        {
            get
            {
                return _threatCordinateY;
            }
            set
            {
                _threatCordinateY = value;
                OnPropertyChanged("ThreatCordinateY");
            }
        }

        private int _threatWidth;
        public int ThreatWidth
        {
            get
            {
                return _threatWidth;
            }
            set
            {
                _threatWidth = value;
                OnPropertyChanged("ThreatWidth");
            }
        }

        private int _threatDistance;
        public int ThreatDistance
        {
            get
            {
                return _threatDistance;
            }
            set
            {
                _threatDistance = value;
                OnPropertyChanged("ThreatDistance");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

