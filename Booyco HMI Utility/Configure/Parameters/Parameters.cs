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
        public int Ptype;
        public int AccessLevel;
        public int VersionControl;
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
        public string Group { get; set; }
        public string SubGroup { get; set; }
        private string _Name;
        public string Name
        {
            get { return _Name; }
            set { _Name = value; OnPropertyChanged("Name"); }
        }
        private string _value;
        public string Value
        {
            get { return _value; }
            set { _value = value; OnPropertyChanged("Value"); }
        }
        public Visibility BtnVisibility { get; set; }
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
}
