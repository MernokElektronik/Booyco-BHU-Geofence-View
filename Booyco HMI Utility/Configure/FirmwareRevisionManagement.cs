
using Booyco_HMI_Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Booyco_BHU_Utility
{
    class FirmwareRevisionManagement
    {
       
     
        string _RevisionTrackerPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/Resources/Documents/FirmwareRevision.xlsx";

       public enum EnumRevisionType
        {
            ImageRevision = 0,
            AudioRevision = 1

        }

        public List<FirmwareEntry> ReadImageFirmwareRevision(int RevisionType)
        {
            List<FirmwareEntry> firmwareCountList = new List<FirmwareEntry>();
            ExcelFileManagement ExcelFileManager = new ExcelFileManagement();

            DataRowCollection _imageRevisionData = ExcelFileManager.ReadExcelFile(_RevisionTrackerPath, RevisionType);
           

            foreach (DataRow _row in _imageRevisionData)
            {
                try
                {
                    firmwareCountList.Add(new FirmwareEntry
                    {
                        FirmwareNumber = Convert.ToInt32(_row.ItemArray[0]),
                        ImageCount = Convert.ToInt32(_row.ItemArray[1]),
                    });
                }

                catch (Exception e)
                {
                    Debug.WriteLine("Exception - Lookup Excel Read Fail");
                }
                //if (Convert.ToInt32(_row.ItemArray[5]) == 1)
                //{
                //    int u = 0;
                //}

            }

            return firmwareCountList;

        }

    }

    class FirmwareEntry
    {
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

        private int _ImageCount;
        public int ImageCount
        {
            get
            {
                return _ImageCount;
            }
            set
            {
                _ImageCount = value;
                OnPropertyChanged("ImageCount");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
