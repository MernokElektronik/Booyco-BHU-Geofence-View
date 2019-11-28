using Booyco_HMI_Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace Booyco_HMI_Utility
{

    class FilterManagement
    {
        public DateTime filter_Start_Date = new DateTime();
        public DateTime filter_End_Date = new DateTime();
        public ICollectionView Filter_CollectionView;
        public String Filter_Text = "";
        public String RawDataFilter_Text = ""; 
        public bool Check_All_Selected = false;
        public IList<String> Events_Selected;
        public UInt32 Total_Filtered_Entries = 0;
        public UInt32 Total_Log_Entries = 0;

        public string[] Filter_And_String_Array;
        public string[] Filter_Or_String_Array;
        public string Single_Filter = "";

        public Action<int> ReportProgressDelegate { get; set; }

        private void ReportProgress(int percent)
        {
            ReportProgressDelegate?.Invoke(percent);
        }

        public void Filter()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                Filter_CollectionView.Filter = Filter_Item;
            }));
            ReportProgress(100);
        }

        public bool Filter_Item(object item2)
        {
                       
            bool Value = false;
            LogEntry p = item2 as LogEntry;

            //filter selected events
            if (!Events_Selected.Contains("Select All"))
            {
                foreach (var item in Events_Selected)
                {
                    if (p.EventName == item.ToString())
                    {
                        Value = true;
                    }
                }
            }
            else
            {
                Value = true;
            }
            
            return Value;
        }
    }
}
