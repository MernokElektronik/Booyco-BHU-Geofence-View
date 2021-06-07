using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Booyco_HMI_Utility
{
    public class Parameters
    {
        public int Number;
        public string Name;
        public int CurrentValue;
        public int MaximumValue;
        public int MinimumValue;
        public int DefaultValue;
        public string Group;
        public int GroupOrder;
        public string SubGroup;
        public int SubGroupOrder;
        public string Unit;
        public int Ptype;
        public int Dependency;
        public int AccessLevel;
        public int Version;
        public int SubVersion;
        public int Active;
        public int Order;
        public int enumVal;
        public List<string> parameterEnumsName;
        public List<int> parameterEnumsValue;
        public string Description;

    }

    public class ParameterGroup
    {
        public string GroupName { get; set; }

        public List<string> SubGroupNames { get; set; }

    }
    public class GroupType : INotifyPropertyChanged
    {
        #region OnProperty Changed
        /////////////////////////////////////////////////////////////
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /////////////////////////////////////////////////////////////
        #endregion
      
        private object _groupName;
        public object GroupName
        {
            get { return _groupName; }
            set
            {
                if (_groupName != value)
                {
                    _groupName = value;
                    OnPropertyChanged("GroupName");
                }
            }
        }
        public string _Changed;
        public string Changed
        {
            get { return _Changed; }
            set { _Changed = value; OnPropertyChanged("Changed"); }
        }
        public override bool Equals(object obj)
        {
            return object.Equals(obj, _groupName);
        }

    }
    public class ParametersDisplay : INotifyPropertyChanged
    {
        #region OnProperty Changed
        /////////////////////////////////////////////////////////////
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /////////////////////////////////////////////////////////////
        #endregion

        public int OriginIndx { get; set; }
        public int Number { get; set; }
        private GroupType _group { get; set; }

        public GroupType Group
        {
            get { return _group; }
            set { _group = value; OnPropertyChanged("Group"); }
        }

        public string SubGroup { get; set; }
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set { _Name = value; OnPropertyChanged("Name"); }
        }
        private string _Unit;
        public string Unit
        {
            get { return _Unit; }
            set { _Unit = value; OnPropertyChanged("Unit"); }
        }
        private string _oldValue;
        public string OldValue
        {
            get { return _oldValue; }
            set { _oldValue = value; OnPropertyChanged("OldValue"); }
        }
        public string Changed { get; set; }

        private string _value;
        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                if(_value != _oldValue)
                {
                    Changed = "*";                  
                }
                else
                {
                    Changed = "";                   
                }
                OnPropertyChanged("Value");
            }
        }
        public Visibility BtnVisibility { get; set; }
        public Visibility BtnFullVisibility { get; set; }
        public Visibility textBoxVisibility { get; set; }
        public Visibility dropDownVisibility { get; set; }       
        public bool LablEdit { get; set; }
        public List<string> parameterEnums { get; set; }
        public int EnumIndx { get; set; }
        public string Description { get; set; }

        public int Order { get; set; }
        public int GroupOrder { get; set; }
        public int SubGroupOrder { get; set; }
    }

    public class ParameterEnum
    {
        public int enumVal;
        public string enumName;
        public int enumIndex;
    }

    public class HeaderType: INotifyPropertyChanged
    {
        #region OnProperty Changed
        /////////////////////////////////////////////////////////////
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /////////////////////////////////////////////////////////////
        #endregion

        private UInt32 _headerSize;
        public UInt32 HeaderSize
        {
            get { return _headerSize; }
            set
            {
                if (_headerSize != value)
                {
                    _headerSize = value;
                    OnPropertyChanged("HeaderSize");
                }
            }
        }

        private UInt32 _headerVersion;
        public UInt32 HeaderVersion
        {
            get { return _headerVersion; }
            set
            {
                if (_headerVersion != value)
                {
                    _headerVersion = value;
                    OnPropertyChanged("HeaderVersion");
                }
            }
        }

        private UInt32 _startOfFile;
        public UInt32 StartOfFile
        {
            get { return _startOfFile; }
            set
            {
                if (_startOfFile != value)
                {
                    _startOfFile = value;
                    OnPropertyChanged("StartOfFile");
                }
            }
        }

        private UInt32 _endOfHeader;
        public UInt32 EndOfHeader
        {
            get { return _endOfHeader; }
            set
            {
                if (_endOfHeader != value)
                {
                    _endOfHeader = value;
                    OnPropertyChanged("EndOfHeader");
                }
            }
        }

        private UInt32 _endPfParam;
        public UInt32 EndofParam
        {
            get { return _endPfParam; }
            set
            {
                if (_endPfParam != value)
                {
                    _endPfParam = value;
                    OnPropertyChanged("EndofParam");
                }
            }
        }

        private UInt32 _endOfLog;
        public UInt32 EndOfLog
        {
            get { return _endOfLog; }
            set
            {
                if (_endOfLog != value)
                {
                    _endOfLog = value;
                    OnPropertyChanged("EndOfLog");
                }
            }
        }

        private UInt32 _endOfFile;
        public UInt32 EndOfFile
        {
            get { return _endOfFile; }
            set
            {
                if (_endOfFile != value)
                {
                    _endOfFile = value;
                    OnPropertyChanged("EndOfFile");
                }
            }
        }

        private UInt32 _productCode;
        public UInt32 ProductCode
        {
            get { return _productCode; }
            set
            {
                if (_productCode != value)
                {
                    _productCode = value;
                    OnPropertyChanged("ProductCode");
                }
            }
        }

        private UInt32 _logType;
        public UInt32 LogType
        {
            get { return _logType; }
            set
            {
                if (_logType != value)
                {
                    _logType = value;
                    OnPropertyChanged("LogType");
                }
            }
        }
        private UInt32 _VID;
        public UInt32 VID
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

        private string _MAC;
        public string MAC
        {
            get { return _MAC; }
            set
            {
                if (_MAC != value)
                {
                    _MAC = value;
                    OnPropertyChanged("MAC");
                }
            }
        }

        private UInt32 _paramSize;
        public UInt32 ParamSize
        {
            get { return _paramSize; }
            set
            {
                if (_paramSize != value)
                {
                    _paramSize = value;
                    OnPropertyChanged("ParamSize");
                }
            }
        }

        private UInt32 _paramTotalSize;
        public UInt32 ParamTotalSize
        {
            get { return _paramTotalSize; }
            set
            {
                if (_paramTotalSize != value)
                {
                    _paramTotalSize = value;
                    OnPropertyChanged("ParamTotalSize");
                }
            }
        }

        private UInt32 _eventLogSize;
        public UInt32 EventLogSize
        { 
            get { return _eventLogSize; }
            set
            {
                if (_eventLogSize != value)
                {
                    _eventLogSize = value;
                    OnPropertyChanged("EventLogSize");
                }
            }
        }

        private UInt32 _analogLogSize;
        public UInt32 AnalogLogSize
        {
            get { return _analogLogSize; }
            set
            {
                if (_analogLogSize != value)
                {
                    _analogLogSize = value;
                    OnPropertyChanged("AnalogLogSize");
                }
            }
        }


        private UInt32 _timeStamp;
        public UInt32 Timestamp
        {
            get { return _timeStamp; }
            set
            {
                if (_timeStamp != value)
                {
                    _timeStamp = value;
                    OnPropertyChanged("Timestamp");
                }
            }
        }
    }
}
