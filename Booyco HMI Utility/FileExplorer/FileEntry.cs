using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Booyco_HMI_Utility
{
    class FileEntry : INotifyPropertyChanged
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
        private string _fileName;
        public string FileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                _fileName = value;
                PathFile = System.IO.Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%"), "Documents") + "\\BHU Utility\\Parameters\\" + _fileName;
                OnPropertyChanged("FileName");
            }
        }

        private string _dateTimeCreated;
        public string DateTimeCreated
        {
            get
            {
                return _dateTimeCreated;
            }
            set
            {
                _dateTimeCreated = value;
                OnPropertyChanged("DateTimeCreated");
            }
        }

        private string _type;
        public string Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
                OnPropertyChanged("Type");
            }
        }


        private string _path;
        public string Path
        {
            get
            {
                return _path;
            }
            set
            {
                _path = value;
                OnPropertyChanged("Path");
            }
        }

        private string _pathFile;
        public string PathFile
        {
            get
            {
                return _pathFile;
            }
            set
            {
                _pathFile = value;
                OnPropertyChanged("PathFile");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
    
}
