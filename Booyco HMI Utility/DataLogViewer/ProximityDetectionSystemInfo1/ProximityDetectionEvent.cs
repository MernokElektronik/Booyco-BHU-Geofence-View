using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProximityDetectionSystemInfo
{
    public enum Tech_Kind
    {
        unknown = 0,
        PDS = 1,
        Ranging = 2,
        Pulse = 3,
        Ranging_pulse = 4,
        Pulse_GPS = 5,
        GPS = 6,
        GSM = 7,
        RADAR = 8
    }

   
   

    class ProximityDetectionEvent
    {
       

        private uint _ThreatNumberStart;
        public uint ThreatNumberStart
        {
            get { return _ThreatNumberStart; }
            set
            {
                if (_ThreatNumberStart != value)
                {
                    _ThreatNumberStart = value;
                    OnPropertyChanged("ThreatNumberStart");
                }
            }
        }

        private uint _ThreatNumberStop;
        public uint ThreatNumberStop
        {
            get { return _ThreatNumberStop; }
            set
            {
                if (_ThreatNumberStop != value)
                {
                    _ThreatNumberStop = value;
                    OnPropertyChanged("ThreatNumberStop");
                }
            }
        }

        private double _ThreatBrakeDistance;
        public double ThreatBrakeDistance
        {
            get { return _ThreatBrakeDistance; }
            set
            {
                if (_ThreatBrakeDistance != value)
                {
                    _ThreatBrakeDistance = value;
                    OnPropertyChanged("ThreatBrakeDistance");
                }
            }
        }
        private DateTime _DateTimeStamp;
        public DateTime DateTimeStamp
        {
            get { return _DateTimeStamp; }
            set
            {
                if (_DateTimeStamp != value)
                {
                    _DateTimeStamp = value;
                    OnPropertyChanged("DateTimeStamp");
                }
            }
        }

        private UInt32 _PrimaryID;
        public UInt32 PrimaryID
        {
            get { return _PrimaryID; }
            set
            {
                if (PrimaryID != value)
                {
                    _PrimaryID = value;
                    OnPropertyChanged("PrimaryID");
                }
            }
        }

        private uint _ThreatTechnology;
        public uint ThreatTechnology
        {
            get { return _ThreatTechnology; }
            set
            {
                if (_ThreatTechnology != value)
                {
                    _ThreatTechnology = value;
                    OnPropertyChanged("ThreatTechnology");
                }
            }
        }

        private uint _ThreatType;
        public uint ThreatType
        {
            get { return _ThreatType; }
            set
            {
                if (_ThreatType != value)
                {
                    _ThreatType = value;
                    OnPropertyChanged("ThreatType");
                }
            }
        }

        private uint _ThreatScenario;
        public uint ThreatScenario
        {
            get { return _ThreatScenario; }
            set
            {
                if (_ThreatScenario != value)
                {
                    _ThreatScenario = value;
                    OnPropertyChanged("ThreatScenario");
                }
            }
        }

        private UInt16 _ThreatDisplayWidth;
        public UInt16 ThreatDisplayWidth
        {
            get { return _ThreatDisplayWidth; }
            set
            {
                if (_ThreatDisplayWidth != value)
                {
                    _ThreatDisplayWidth = value;
                    OnPropertyChanged("ThreatDisplayWidth");
                }
            }
        }

        private uint _ThreatDisplaySector;
        public uint ThreatDisplaySector
        {
            get { return _ThreatDisplaySector; }
            set
            {
                if (_ThreatDisplaySector != value)
                {
                    _ThreatDisplaySector = value;
                    OnPropertyChanged("ThreatDisplaySector");
                }
            }
        }

        private uint _ThreatDisplayZone;
        public uint ThreatDisplayZone
        {
            get { return _ThreatDisplayZone; }
            set
            {
                if (_ThreatDisplayZone != value)
                {
                    _ThreatDisplayZone = value;
                    OnPropertyChanged("ThreatDisplayZone");
                }
            }
        }
        private List<string> _eventInfo;
        public List<string> EventInfo
        {
            get { return _eventInfo; }
            set
            {
                if (_eventInfo != value)
                {
                    _eventInfo = value;
                    OnPropertyChanged("EventInfo");
                }
            }
        }

        private double _ThreatSpeed;
        public double ThreatSpeed
        {
            get { return _ThreatSpeed; }
            set
            {
                if (_ThreatSpeed != value)
                {
                    _ThreatSpeed = value;
                    OnPropertyChanged("ThreatSpeed");
                }
            }
        }

        private UInt16 _PresenceDistance;
        public UInt16 PresenceDistance
        {
            get { return _PresenceDistance; }
            set
            {
                if (_PresenceDistance != value)
                {
                    _PresenceDistance = value;
                    OnPropertyChanged("PresenceDistance");
                }
            }
        }

        private UInt16 _WarningDistance;
        public UInt16 WarningDistance
        {
            get { return _WarningDistance; }
            set
            {
                if (_WarningDistance != value)
                {
                    _WarningDistance = value;
                    OnPropertyChanged("WarningDistance");
                }
            }
        }

        private UInt16 _CriticalDistance;
        public UInt16 CriticalDistance
        {
            get { return _CriticalDistance; }
            set
            {
                if (_CriticalDistance != value)
                {
                    _CriticalDistance = value;
                    OnPropertyChanged("CriticalDistance");
                }
            }
        }

        private UInt16 _ThreatDistance;
        public UInt16 ThreatDistance
        {
            get { return _ThreatDistance; }
            set
            {
                if (_ThreatDistance != value)
                {
                    _ThreatDistance = value;
                    OnPropertyChanged("ThreatDistance");
                }
            }
        }


        private double _ThreatDistanceCalculated;

        public double ThreatDistanceCalculated
        {
            get { return _ThreatDistanceCalculated; }
            set
            {
                if (_ThreatDistanceCalculated != value)
                {
                    _ThreatDistanceCalculated = value;
                    OnPropertyChanged("ThreatDistanceCalculated");
                }
            }
        }

        private UInt16 _POCDistance;
        public UInt16 POCDistance
        {
            get { return _POCDistance; }
            set
            {
                if (_POCDistance != value)
                {
                    _POCDistance = value;
                    OnPropertyChanged("POCDistance");
                }
            }
        }
        private double _ThreatPOIUTMDistance;
        public double ThreatPOIUTMDistance
        {
            get { return _ThreatPOIUTMDistance; }
            set
            {
                if (_ThreatPOIUTMDistance != value)
                {
                    _ThreatPOIUTMDistance = value;
                    OnPropertyChanged("ThreatPOIUTMDistance");
                }
            }
        }

        private double _POCUnitDistance;
        public double POCUnitDistance
        {
            get { return _POCUnitDistance; }
            set
            {
                if (_POCUnitDistance != value)
                {
                    _POCUnitDistance = value;
                    OnPropertyChanged("POCUnitDistance");
                }
            }
        }

        private double _POCThreatDistance;
        public double POCThreatDistance
        {
            get { return _POCThreatDistance; }
            set
            {
                if (_POCThreatDistance != value)
                {
                    _POCThreatDistance = value;
                    OnPropertyChanged("POCThreatDistance");
                }
            }
        }
        private double _ThreatPOILOGDistance;
        public double ThreatPOILOGDistance
        {
            get { return _ThreatPOILOGDistance; }
            set
            {
                if (_ThreatPOILOGDistance != value)
                {
                    _ThreatPOILOGDistance = value;
                    OnPropertyChanged("ThreatPOILOGDistance");
                }
            }
        }

        private double _ThreatHeading;
        public double ThreatHeading
        {
            get { return _ThreatHeading; }
            set
            {
                if (_ThreatHeading != value)
                {
                    _ThreatHeading = value;
                    OnPropertyChanged("ThreatHeading");
                }
            }
        }

        private double _ThreatLatitude;
        public double ThreatLatitude
        {
            get { return _ThreatLatitude; }
            set
            {
                if (_ThreatLatitude != value)
                {
                    _ThreatLatitude = value;
                    OnPropertyChanged("ThreatLatitude");
                }
            }
        }

        private double _ThreatLongitude;
        public double ThreatLongitude
        {
            get { return _ThreatLongitude; }
            set
            {
                if (_ThreatLongitude != value)
                {
                    _ThreatLongitude = value;
                    OnPropertyChanged("ThreatLongitude");
                }
            }
        }

        private uint _ThreatHorizontalAccuracy;
        public uint ThreatHorizontalAccuracy
        {
            get { return _ThreatHorizontalAccuracy; }
            set
            {
                if (_ThreatHorizontalAccuracy != value)
                {
                    _ThreatHorizontalAccuracy = value;
                    OnPropertyChanged("ThreatHorizontalAccuracy");
                }
            }
        }

        private uint _ThreatPriority;
        public uint ThreatPriority
        {
            get { return _ThreatPriority; }
            set
            {
                if (_ThreatPriority != value)
                {
                    _ThreatPriority = value;
                    OnPropertyChanged("ThreatPriority");
                }
            }
        }

        private double _UnitSpeed;
        public double UnitSpeed
        {
            get { return _UnitSpeed; }
            set
            {
                if (_UnitSpeed != value)
                {
                    _UnitSpeed = value;
                    OnPropertyChanged("UnitSpeed");
                }
            }
        }

        private double _UnitHeading;
        public double UnitHeading
        {
            get { return _UnitHeading; }
            set
            {
                if (_UnitHeading != value)
                {
                    _UnitHeading = value;
                    OnPropertyChanged("UnitHeading");
                }
            }
        }

        private uint _UnitHorizontalAccuracy;
        public uint UnitHorizontalAccuracy
        {
            get { return _UnitHorizontalAccuracy; }
            set
            {
                if (_UnitHorizontalAccuracy != value)
                {
                    _UnitHorizontalAccuracy = value;
                    OnPropertyChanged("UnitHorizontalAccuracy");
                }
            }
        }

        private double _UnitLatitude;
        public double UnitLatitude
        {
            get { return _UnitLatitude; }
            set
            {
                if (_UnitLatitude != value)
                {
                    _UnitLatitude = value;
                    OnPropertyChanged("UnitLatitude");
                }
            }
        }

        private double _UnitLongitude;
        public double UnitLongitude
        {
            get { return _UnitLongitude; }
            set
            {
                if (_UnitLongitude != value)
                {
                    _UnitLongitude = value;
                    OnPropertyChanged("UnitLongitude");
                }
            }
        }

        private double _POILatitude;
        public double POILatitude
        {
            get { return _POILatitude; }
            set
            {
                if (_POILatitude != value)
                {
                    _POILatitude = value;
                    OnPropertyChanged("POILatitude");
                }
            }
        }

        private double _POILongitude;
        public double POILongitude
        {
            get { return _POILongitude; }
            set
            {
                if (_POILongitude != value)
                {
                    _POILongitude = value;
                    OnPropertyChanged("POILongitude");
                }
            }
        }

        private double _POCLatitude;
        public double POCLatitude
        {
            get { return _POCLatitude; }
            set
            {
                if (_POCLatitude != value)
                {
                    _POCLatitude = value;
                    OnPropertyChanged("POCLatitude");
                }
            }
        }

        private double _POCLongitude;
        public double POCLongitude
        {
            get { return _POCLongitude; }
            set
            {
                if (_POCLongitude != value)
                {
                    _POCLongitude = value;
                    OnPropertyChanged("POCLongitude");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string PropertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
