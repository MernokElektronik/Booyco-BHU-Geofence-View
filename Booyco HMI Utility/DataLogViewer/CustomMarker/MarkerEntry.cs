using GMap.NET.WindowsPresentation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booyco_HMI_Utility
{

    public enum MarkerType
    {
        Indicator,
        Ellipse,
        Point,
        Cross,
        Vehicle

    }

    public enum SpeedConditionEnum
    {
        UnderSpeed,
        WarningSpeed,
        OverSpeed,     
    }

    public class MarkerEntry: INotifyPropertyChanged
    {

        public GMapMarker MapMarker;
  
       
        public string title;
        public string titleSpeed;
        public uint Type;
        public double Scale;
        private Vehicle_Entry _VehicleInfo = new Vehicle_Entry();
        public Vehicle_Entry VehicleInfo
        {
            get
            {
                return _VehicleInfo;
            }
            set
            {
                _VehicleInfo = value;
                OnPropertyChanged("VehicleInfo");
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
