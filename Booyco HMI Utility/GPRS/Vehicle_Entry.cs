using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booyco_HMI_Utility
{
    public class Vehicle_Entry: INotifyPropertyChanged
    {               
       
           
           
            public uint Zone;
           
            public int SpeedCondition;
       
            public uint Type;
            public ushort Width;
            public double Scale;
            public ushort Length;
            public int PresenceZoneSize;
            public int WarningZoneSize;
            public int CriticalZoneSize;
            public int Active;
           
            public double Accuracy;
            public double BrakeDistance;
     

        private VehicleParameters _Parameters;
        public VehicleParameters Parameters
        {
            get
            {
                return _Parameters;
            }
            set
            {
                _Parameters = value;
                OnPropertyChanged("Parameters");
            }
        }
        private bool _IsPlotRequired = true;
        public bool IsPlotRequired
        {
            get
            {
                return _IsPlotRequired;
            }
            set
            {
                _IsPlotRequired = value;
                OnPropertyChanged("IsPlotRequired");
            }
        }

        private DateTime _TimeStamp;
        public DateTime TimeStamp
        {
            get
            {
                return _TimeStamp;
            }
            set
            {
                _TimeStamp = value;
                OnPropertyChanged("TimeStamp");
            }
        }
        private DateTime _LastReceived;
        public DateTime LastReceived
        {
            get
            {
                return _LastReceived;
            }
            set
            {
                _LastReceived = value;
                OnPropertyChanged("LastReceived");
            }
        }
        private double _Longitude;
        public double Longitude
        {
            get
            {
                return _Longitude;
            }
            set
            {
                _Longitude = value;
                OnPropertyChanged("Longitude");
            }
        }
        private double _Latitude;
        public double Latitude
        {
            get
            {
                return _Latitude;
            }
            set
            {
                _Latitude = value;
                OnPropertyChanged("Latitude");
            }
        }
        private double _Speed;
        public double Speed
        {
            get
            {
                return _Speed;
            }
            set
            {
                _Speed = value;
                OnPropertyChanged("Speed");
            }
        }
        private double _Heading;
        public double Heading
        {
            get
            {
                return _Heading;
            }
            set
            {
                _Heading = value;
                OnPropertyChanged("Heading");
            }
        }
        private string _UID;
        public string UID
        {
            get
            {
                return _UID;
            }
            set
            {
                _UID = value;
                OnPropertyChanged("UID");
            }
        }
        private double _ActiveEvent;
        public double ActiveEvent
        {
            get
            {
                return _ActiveEvent;
            }
            set
            {
                _ActiveEvent = value;
                OnPropertyChanged("ActiveEvent");
            }
        }
        private string _ActiveEventString;
        public string ActiveEventString
        {
            get
            {
                return _ActiveEventString;
            }
            set
            {
                _ActiveEventString = value;
                OnPropertyChanged("ActiveEventString");
            }
        }
        private string _Status;
        public string Status
        {
            get
            {
                return _Status;
            }
            set
            {
                _Status = value;
                OnPropertyChanged("Status");
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class VehicleParameters : INotifyPropertyChanged
    {
        private double _WarningSpeed =30;
        public double WarningSpeed
        {
            get
            {
                return _WarningSpeed;
            }
            set
            {
                _WarningSpeed = value;
                OnPropertyChanged("WarningSpeed");
            }
        }
        private double _OverSpeedTrip = 40;
        public double OverSpeedTrip
        {
            get
            {
                return _OverSpeedTrip;
            }
            set
            {
                _OverSpeedTrip = value;
                OnPropertyChanged("OverSpeedTrip");
            }
        }

        public Geofence_Entry Geofence_01;
        public Geofence_Entry Geofence_02;
        public Geofence_Entry Geofence_03;
        public Geofence_Entry Geofence_04;
        public Geofence_Entry Geofence_05;

        private int _VehicleType;
        public int VehicleType
        {
            get
            {
                return _VehicleType;
            }
            set
            {
                _VehicleType = value;
                OnPropertyChanged("VehicleType");
            }
        }
        private double _Revision;
        public double Revision
        {
            get
            {
                return _Revision;
            }
            set
            {
                _Revision = value;
                OnPropertyChanged("Revision");
            }
        }
        private int _Subrevision;
        public int Subrevision
        {
            get
            {
                return _Subrevision;
            }
            set
            {
                _Subrevision = value;
                OnPropertyChanged("Subrevision");
            }
        }

        private string _VehicleName;
        public string VehicleName
        {
            get
            {
                return _VehicleName;
            }
            set
            {
                _VehicleName = value;
                OnPropertyChanged("VehicleName");
            }
        }
      

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    public class Geofence_Entry
    {
        public double Latitude;
        public double Longitude;
        public int radius;
        public int Type;
    }
}
