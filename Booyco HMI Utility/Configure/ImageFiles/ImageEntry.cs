using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace Booyco_HMI_Utility
{
    class ImageEntry:INotifyPropertyChanged
    {
        private UInt16 _ID;
        public UInt16 ID
        {
            get
            {
                return _ID;
            }
            set
            {
                _ID = value;
                OnPropertyChanged("ID");
            }
        }

        private int _FirmwareNumber;
        public int FirmwareNumber
        {
            get
            {
                return _FirmwareNumber;
            }
            set
            {
                _FirmwareNumber = value;
                OnPropertyChanged("FirmwareNumber");
            }
        }

        private string _FileName;
        public string FileName
        {
            get
            {
                return _FileName;
            }
            set
            {
                _FileName = value;
                OnPropertyChanged("FileName");
            }
        }

        private TransformedBitmap _ImageSource;
        public TransformedBitmap ImageSource
        {
            get
            {
                return _ImageSource;
            }
            set
            {
                _ImageSource = value;
                OnPropertyChanged("ImageSource");
            }
        }

        private string _DateTimeCreated;
        public string DateTimeCreated
        {
            get
            {
                return _DateTimeCreated;
            }
            set
            {
                _DateTimeCreated = value;
                OnPropertyChanged("DateTimeCreated");
            }
        }

        private string _Path;
        public string Path
        {
            get
            {
                return _Path;
            }
            set
            {
                _Path = value;
                OnPropertyChanged("Path");
            }
        }
        private long _Size;
        public long Size
        {
            get
            {
                return _Size;
            }
            set
            {
                _Size = value;
                OnPropertyChanged("Size");
            }
        }

        private int _Progress;
        public int Progress
        {
            get
            {
                return _Progress;
            }
            set
            {
                _Progress = value;
                OnPropertyChanged("Progress");
            }
        }

        private string _ProgressString;
        public string ProgressString
        {
            get
            {
                return _ProgressString;
            }
            set
            {
                _ProgressString = value;
                OnPropertyChanged("ProgressString");
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
