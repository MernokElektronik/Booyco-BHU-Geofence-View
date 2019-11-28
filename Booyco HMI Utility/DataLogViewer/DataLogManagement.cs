
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProximityDetectionSystemInfo;

namespace Booyco_HMI_Utility
{
    /// <summary>
    /// DataLogManagement: Class
    /// Handle read and parsing of datalog files
    /// </summary>
    class DataLogManagement    
    {
        // === Public Variables ===
        public Action<int> ReportProgressDelegate { get; set; }
        public const int PDSThreatEventID = 150;
        public const int PDSThreatEventIDLast = 158;
        public const int PDSThreatEventEndID = 159;
        public const int PDSThreatEventEndIDLast = 168;
        public const int PDSThreatEventLength = 9;
        public RangeObservableCollection<LogEntry> TempList = new RangeObservableCollection<LogEntry>();
        public UInt32 TotalLogEntries = 0;
        public UInt32 LogErrorDateTimeCounter = 0;
        public ExcelFileManagement ExcelFilemanager = new ExcelFileManagement();
        public bool AbortRequest = false;

        // === Private Variables ===
        private const byte TOTAL_ENTRY_BYTES = 16;
        private void ReportProgress(int percent)
        {
            ReportProgressDelegate?.Invoke(percent);
        } 
          
        /// <summary>
        /// ReadFile
        /// Read datalog file and parse information
        /// String Log_Filename: datalog file path
        /// </summary>
        /// <param name="Log_Filename"></param>
        /// <returns></returns>
        public bool ReadFile(string Log_Filename, double Time_Offset)
        {
            ExcelFilemanager.StoreLogProtocolInfo();
            
            byte[] _logTimeStamp = { 0, 0, 0, 0, };          

            // === Read datalog file ===
            //string _logInfoRaw = System.IO.File.ReadAllText(Log_Filename, Encoding.Default);          
            BinaryReader _breader = new BinaryReader(File.OpenRead(Log_Filename));
            int _fileLength = (int)(new FileInfo(Log_Filename).Length);
            byte[] _logBytes = _breader.ReadBytes(_fileLength);

            int PercentageComplete = 0;      
     
            // === Calculate the total log entries ===
            TotalLogEntries = (UInt32)((float)_fileLength / (float)TOTAL_ENTRY_BYTES);

            // === Loop through the total log entries ===
            for (uint i = 0; i < (int)TotalLogEntries - 1; i++)
            {
                // === if the current log entry number (i) is one percent closer to complete update Perectangecomplete ===
                if ((i % (TotalLogEntries / 100)) == 0)
                {
                    PercentageComplete++;
                    ReportProgress(PercentageComplete);
                }
                // === if window is abort while looping though datalogs, stop everthing and return false ===
                if (AbortRequest)
                {
                    _breader.Close();
                    _breader.Dispose();
                    PercentageComplete=0;
                    ReportProgress(PercentageComplete);
                    return false;
                }

                // === clear information ===
                byte[] _logChuncks = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
               

                // === loop through the data to retreive the current log entry data ===
                for (int j = 0; j < TOTAL_ENTRY_BYTES; j++)
                {
                    _logChuncks[j] = _logBytes[i * TOTAL_ENTRY_BYTES + j];
                }

                // === Copy the logtimestamp(unix) from the log chunck ===
                Buffer.BlockCopy(_logChuncks, 0, _logTimeStamp, 0, 4);
                DateTime _eventDateTime;
                // === Determine if the logtimestamp is valid ===
                uint _dateTimeStatus = DateTimeCheck.CheckDateTimeStampUnix(BitConverter.ToUInt32(_logTimeStamp, 0), out _eventDateTime);
               
                // === Check if the logtimestamp is valid continue parsing information ===
                if ( _dateTimeStatus == (uint)DateTimeCheck.Status.Ok)
                {
                   
                    // === Check If the milisecond value is less than 100
                    if (_logChuncks[4] < 100)
                    {
                        // === calculate the miliseconds from the logged value ===
                        _eventDateTime = _eventDateTime.AddMilliseconds((double)_logChuncks[4] * 10);
                    }
                    // === Else if the value is equal or more than 100
                    else
                    {
                        // === Add default value ===
                        _eventDateTime = _eventDateTime.AddMilliseconds(0);
                    }

                    DateTime Datecorrectionnewer = DateTime.Now.AddYears(1);
                    DateTime Datecorrectionolder = DateTime.Now.AddYears(-1);
                    if (DateTime.Compare(_eventDateTime, Datecorrectionnewer) > 0 || DateTime.Compare(_eventDateTime, Datecorrectionolder) < 0)
                    {
                        _eventDateTime = _eventDateTime.AddSeconds(Time_Offset);
                    }

                    // === Copy Event ID from log chunck ===
                    UInt16 _tempEventID = BitConverter.ToUInt16(_logChuncks, 6);

                    LogEntry _LogEntry= new LogEntry
                    { 
                        Number = i,
                        EventID = _tempEventID,                           
                        RawEntry = _logChuncks,                       
                        DateTime = _eventDateTime,                     
                    } ;
                    // === Parse Data Log Entry into List ===
                    ParseDataEntry(_LogEntry);
                }
                // === Else if logtimestamp error 2 ===
                else if (_dateTimeStatus == (uint)DateTimeCheck.Status.Error_2)
                {
                    LogErrorDateTimeCounter++;
                }
                // === Else if logtimestamp error 3 ===
                else if (_dateTimeStatus == (uint)DateTimeCheck.Status.Error_3)
                {
                    LogErrorDateTimeCounter++;
                }
                // === Else if logtimestamp error 4 ===
                else if (_dateTimeStatus == (uint)DateTimeCheck.Status.Error_4)
                {
                    LogErrorDateTimeCounter++;
                }

            }
            // === Try to close the binary reader ===
            try
            {
                _breader.Close();
                _breader.Dispose();
            }
            catch
            {
                Debug.WriteLine("failed to close Binary reader.");
            }
            return true;
            }

        /// <summary>
        /// ParseDataEntry
        /// Parse log data into list
        /// -Return 
        /// bool - Check is data is  parsed
        /// -Parameters
        /// byte[] _logChuncks, byte array of the log entry
        /// byte[] _logData, byte array of the data inside the log entry
        /// </summary>
        /// <param name="_logChuncks"></param>
        /// <param name="_logData"></param>
        /// <returns></returns>
        bool ParseDataEntry(LogEntry _LogEntry)
        {
            byte[] _logData = { 0, 0, 0, 0, 0, 0, 0, 0 };
            //  === Copy the log data form the log chunck ===
            Buffer.BlockCopy(_LogEntry.RawEntry, 8, _logData, 0, 8);
            
            // === Check if eventID is not zero ===
            if (_LogEntry.EventID > 0)
            {
                string _tempEventInfo = "";
                List<string> _tempDataListString = new List<string>();
                List<string> _tempEventInfoList = new List<string>();
                List<double> _tempDataList = new List<double>();

                // === try to parse data ===
                try
                {
                    
                    // === Check if datalog has no addtional information ===
                    if (ExcelFilemanager.LPDInfoList.ElementAt(_LogEntry.EventID - 1).Data[0].DataLink == "Empty" && _LogEntry.EventID != 65535)
                    {
                        _tempEventInfo = "No Information";
                        _tempEventInfoList.Add("No Information");
                    }
                    // === else parse additional information ===
                    else
                    {
                        int _count = 0;
                        int _index = 0;
                        // for loop through all the sub-data of an data entry ====
                        foreach (LPDDataLookupEntry _dataLookupEntry in ExcelFilemanager.LPDInfoList.ElementAt(_LogEntry.EventID - 1).Data)
                        {
                            // === Check if not the end of all the sub-data ===
                            if (_dataLookupEntry.DataLink != "Empty")
                            {
                                // === Detemine the Math scale of the LPD file for the sub-data entry ===
                                double _scale = Math.Pow(10, _dataLookupEntry.Scale);

                                // === Add a seperator after each sub-data entry ===
                                if (_count > 0)
                                {
                                    _tempEventInfo += " , ";
                                }

                                // === Check if the number of byte are 4 (32-bit) for the sub-data entry ===
                                if (_dataLookupEntry.NumberBytes == 4)
                                {
                                    // === Check if the sub-data is an hex value ===
                                    if (_dataLookupEntry.IsInt == 2)
                                    {
                                        byte[] _tempByteArray = { 0, 0, 0, 0 };

                                        Array.Copy(_logData, _index, _tempByteArray, 0, 4);

                                        UInt32 HexValue = BitConverter.ToUInt32(_tempByteArray, 0);

                                        _tempDataListString.Add("0x" + (Convert.ToUInt32(HexValue * _scale)).ToString("X8"));
                                        _tempDataList.Add(Convert.ToDouble(HexValue));


                                    }
                                    // === Else check if the sub-data is an signed integer ===
                                    else if (_dataLookupEntry.IsInt == 1)
                                    {
                                        _tempDataListString.Add((BitConverter.ToInt32(_logData, _index) * _scale).ToString());
                                        _tempDataList.Add(double.Parse(_tempDataListString.Last()));

                                    }
                                    // ===  Else sub-data is an unsigned integer ===
                                    else
                                    {

                                        _tempDataListString.Add((BitConverter.ToUInt32(_logData, _index) * _scale).ToString());
                                        _tempDataList.Add(double.Parse(_tempDataListString.Last()));

                                    }

                                        _tempEventInfo += _dataLookupEntry.DataName + ": " + _tempDataListString.Last();
                                        _tempEventInfoList.Add(_dataLookupEntry.DataName + ": " + _tempDataListString.Last() + _dataLookupEntry.Appendix);


                                        _index += 4;
                                    
                                }
                                // === Else check if the number of byte are 2 (16-bit) for the sub-data entry ===
                                else if (_dataLookupEntry.NumberBytes == 2)
                                {
                                    string Appendix = _dataLookupEntry.Appendix;
                                    // === Else check if the sub-data is an signed integer ===
                                    if (_dataLookupEntry.IsInt == 1)
                                    {
                                        _tempDataListString.Add((BitConverter.ToInt16(_logData, _index) * _scale).ToString());
                                    }
                                    // ===  Else sub-data is an unsigned integer ===
                                    else
                                    {                                     

                                        UInt32 tempvalue = (UInt32)(BitConverter.ToUInt16(_logData, _index) * _scale);
                                       
                                        if (_dataLookupEntry.UnkownText == "" && _dataLookupEntry.UnkownValue != tempvalue)
                                        {
                                            _tempDataListString.Add(tempvalue.ToString());
                                            _tempDataList.Add(double.Parse(_tempDataListString.Last()));
                                        }
                                        else
                                        {
                                            _tempDataListString.Add(_dataLookupEntry.UnkownText);
                                            _tempDataList.Add(double.Parse(_dataLookupEntry.UnkownValue.ToString()));
                                            Appendix = "";
                                        }
                                    }
                                   // _tempDataList.Add(double.Parse(_tempDataListString.Last()));
                                    _tempEventInfo += _dataLookupEntry.DataName + ": " + _tempDataListString.Last();
                                    _tempEventInfoList.Add(_dataLookupEntry.DataName + ": " + _tempDataListString.Last() + Appendix);
                                    _index += 2;
                                }
                                // === Else check if the number of byte are 1 (8-bit) for the sub-data entry ===
                                else if (_dataLookupEntry.NumberBytes == 1)
                                {

                                    if (_dataLookupEntry.EnumLink == 0 || ExcelFilemanager.LPDInfoEnumList[_dataLookupEntry.EnumLink].Count < _logData[_index])
                                    {
                                        // === Else check if the sub-data is an signed integer ===
                                        if (_dataLookupEntry.IsInt == 1)
                                        {
                                            _tempDataListString.Add(((ushort)_logData[_index] * _scale).ToString());
                                        }
                                        // ===  Else sub-data is an unsigned integer ===
                                        else
                                        {
                                            _tempDataListString.Add((_logData[_index] * _scale).ToString());
                                        }
                                        _tempDataList.Add(double.Parse(_tempDataListString.Last()));

                                    }

                                    else if (_dataLookupEntry.EnumLink != 0)
                                    {
                                        _tempDataList.Add((_logData[_index] * _scale));
                                        _tempDataListString.Add((ExcelFilemanager.LPDInfoEnumList[_dataLookupEntry.EnumLink].ElementAt(_logData[_index])));
                                    }

                                    _tempEventInfo += _dataLookupEntry.DataName + ": " + _tempDataListString.Last();
                                    _tempEventInfoList.Add(_dataLookupEntry.DataName + ": " + _tempDataListString.Last() + _dataLookupEntry.Appendix);


                                    _index += 1;
                                }

                            }
                            _tempEventInfo += " " + _dataLookupEntry.Appendix;
                            _count++;

                        }

                    }

                }
                catch
                {
                    _tempEventInfo = "No Information";
                    _tempEventInfoList.Add("No Information");
                }

                // === Try saving the informaiton into a list ===
                try
                {

                    // === check if EventID is a PDS threat Event ===
                    if (_LogEntry.EventID > PDSThreatEventID && _LogEntry.EventID <= PDSThreatEventIDLast)
                    {
                        // === Combine PDS threat events into a single Entry ===
                        if (TempList.Last().EventID == PDSThreatEventID && _tempDataList.Count() > 0)
                        {
                            TempList.Last().EventInfo += Environment.NewLine + _tempEventInfo;
                            TempList.Last().DataList.AddRange(_tempDataList);
                            TempList.Last().DataListString.AddRange(_tempDataListString);
                            TempList.Last().EventInfoList.AddRange(_tempEventInfoList);
                            Buffer.BlockCopy(_logData, 0, TempList.Last().RawData, (_LogEntry.EventID - PDSThreatEventID) * 8, 8);

                        }

                    }
                    // === check if EventID is a PDS threat End Event ===
                    else if (_LogEntry.EventID > PDSThreatEventEndID && _LogEntry.EventID < PDSThreatEventEndIDLast)
                    {
                        // === Combine PDS threat End events into a single Entry ===
                        if (TempList.Last().EventID == PDSThreatEventEndID && _tempDataList.Count() > 0)
                        {
                            TempList.Last().EventInfo += Environment.NewLine + _tempEventInfo;
                            TempList.Last().DataList.AddRange(_tempDataList);
                            TempList.Last().DataListString.AddRange(_tempDataListString);
                            TempList.Last().EventInfoList.AddRange(_tempEventInfoList);
                            Buffer.BlockCopy(_logData, 0, TempList.Last().RawData, (_LogEntry.EventID - PDSThreatEventEndID) * 8, 8);
                        }

                    }
                    // === check if EventID is an Analog log ===
                    else if (_LogEntry.EventID == 501)
                    {
                        // === Combine Analog logs into a single Entry ===
                        if (TempList.Last().EventID == 500 && _tempDataList.Count() > 0)
                        {
                            TempList.Last().EventInfo += Environment.NewLine + _tempEventInfo;
                            TempList.Last().DataList.AddRange(_tempDataList);
                            TempList.Last().DataListString.AddRange(_tempDataListString);
                            TempList.Last().EventInfoList.AddRange(_tempEventInfoList);
                            Buffer.BlockCopy(_logData, 0, TempList.Last().RawData, 8, 8);
                        }
                    }

                    // === Check if event ID is is first entry of PDS threat, PDS threat end or analog ===
                    else if (_LogEntry.EventID == PDSThreatEventID || _LogEntry.EventID == PDSThreatEventEndID || _LogEntry.EventID == 500)
                    {
                        // === Add to the list ===
                        byte[] byteArray = new byte[PDSThreatEventLength * 8];
                        byteArray[0] = _logData[0];
                        byteArray[1] = _logData[1];
                        byteArray[2] = _logData[2];
                        byteArray[3] = _logData[3];
                        byteArray[4] = _logData[4];
                        byteArray[5] = _logData[5];
                        byteArray[6] = _logData[6];
                        byteArray[7] = _logData[7];
                        _LogEntry.EventName = ExcelFilemanager.LPDInfoList.ElementAt(_LogEntry.EventID - 1).EventName;
                        _LogEntry.RawData = byteArray;
                        _LogEntry.DataListString = _tempDataListString;
                        _LogEntry.DataList = _tempDataList;
                        _LogEntry.EventInfoList = _tempEventInfoList;
                        TempList.Add(_LogEntry);
                    }                    
                    else
                    {
                        if (_LogEntry.EventID <= ExcelFilemanager.LPDInfoList.Count())
                        {
                            // === Add to the list ===
                            _LogEntry.EventName = ExcelFilemanager.LPDInfoList.ElementAt(_LogEntry.EventID - 1).EventName;
                            _LogEntry.RawData = _logData;
                            _LogEntry.DataListString = _tempDataListString;
                            _LogEntry.DataList = _tempDataList;
                            _LogEntry.EventInfoList = _tempEventInfoList;
                            TempList.Add(_LogEntry);
                        }
                    }

                }
                catch (Exception e)
                {
                    Debug.WriteLine("unable to add event");
                }
            }

            return true;
        }
    }

   
}

 
