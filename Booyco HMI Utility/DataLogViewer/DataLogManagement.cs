
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
            byte[] _logTimeStamp = { 0, 0, 0, 0, };

            // === Read datalog file ===
            //string _logInfoRaw = System.IO.File.ReadAllText(Log_Filename, Encoding.Default);          
            BinaryReader _breader = new BinaryReader(File.OpenRead(Log_Filename));
            int _fileLength = (int)(new FileInfo(Log_Filename).Length);
            byte[] _logBytes = _breader.ReadBytes(_fileLength);


            bool is_DatalogFile = true;

            for (int k = 0; k < 16; k++)
            {
                if (_logBytes[k] != '*')
                {
                    is_DatalogFile = false;
                    break;
                }
            }

            int headerCount = _logBytes[16];

            if (headerCount % 16 != 0)
            {
                headerCount += 16 - headerCount % 16;
            }

            for (int k = headerCount + 16; k < headerCount + 32; k++)
            {
                if (_logBytes[k] != '&')
                {
                    is_DatalogFile = false;
                    break;
                }
            }
           // ExcelFilemanager.StoreLogProtocolInfo_V2(69);
            if (is_DatalogFile)
            {
                ExcelFilemanager.StoreLogProtocolInfo_V2(_logBytes.ElementAt(25));
                
                _logBytes = _logBytes.Skip(headerCount + 32).ToArray();
                // === Calculate the total log entries ===
                TotalLogEntries = (UInt32)((float)_fileLength / (float)TOTAL_ENTRY_BYTES) - (UInt32)(headerCount + 48);

                

            }
            else
            {
                // === Calculate the total log entries ===
                TotalLogEntries = (UInt32)((float)_fileLength / (float)TOTAL_ENTRY_BYTES);
            }

            int PercentageComplete = 0;

            int prevPercentageComplete = 0;

            // === Loop through the total log entries ===
            for (uint i = 0; i < (int)TotalLogEntries - 1; i++)
            {
                PercentageComplete = ((int)i * 100) / (int)TotalLogEntries;

                if (i == TotalLogEntries - 2)
                {
                    PercentageComplete = 100;
                    }

                    if (PercentageComplete > prevPercentageComplete )
                {
                    prevPercentageComplete = PercentageComplete;
                                        
                    //PercentageComplete++;
                    ReportProgress(PercentageComplete);
                }
                // === if window is abort while looping though datalogs, stop everthing and return false ===
                if (AbortRequest)
                {
                    _breader.Close();
                    _breader.Dispose();
                    PercentageComplete = 0;
                    ReportProgress(PercentageComplete);
                    return false;
                }


                byte[] _logChuncks = { 0 };

                // === Copy Event ID from log chunck ===

                UInt16 _tempEventID = BitConverter.ToUInt16(_logBytes, (int)i * TOTAL_ENTRY_BYTES + 6);

                bool check = false;

                if (is_DatalogFile)
                {
                    if (ExcelFilemanager.LPDInfoList_V2.FindIndex(x => x.EventID == _tempEventID) != -1)
                    {
                        check = true;
                    }
                   
                }
                else
                {
                    if (ExcelFilemanager.LPDInfoList_V1.FindIndex(x => x.EventID == _tempEventID) != -1)
                    {
                        check = true;
                    }
                }

                if (check)
                {
                    int TotalEventEntries = 1;
                    if (is_DatalogFile)
                    {
                        TotalEventEntries = ExcelFilemanager.LPDInfoList_V2.FindLast(x => x.EventID == _tempEventID).TotalEntries;

                        int tempcount = 1;
                        for (int k = 1; k < TotalEventEntries; k++)
                        {
                             if (_tempEventID + k == BitConverter.ToUInt16(_logBytes, (int)i * TOTAL_ENTRY_BYTES + k * TOTAL_ENTRY_BYTES + 6))
                            {
                                tempcount++;
                            }
                        }
                        TotalEventEntries = tempcount;
                        Array.Resize(ref _logChuncks, 16 + (TotalEventEntries) * (TOTAL_ENTRY_BYTES ));
                    }
                    else
                    {
                        TotalEventEntries = ExcelFilemanager.LPDInfoList_V1.FindLast(x => x.EventID == _tempEventID).TotalEntries;

                        int tempcount = 1;
                        for (int k = 1; k < TotalEventEntries; k++)
                        {
                            if (_tempEventID + k == BitConverter.ToUInt16(_logBytes, (int)i * TOTAL_ENTRY_BYTES + k * TOTAL_ENTRY_BYTES + 6))
                            {
                                tempcount++;
                            }
                        }
                        TotalEventEntries = tempcount;

                        Array.Resize(ref _logChuncks, (TotalEventEntries) * TOTAL_ENTRY_BYTES);
                    }                    

                    int index = 0;
                    try
                    {
                        for (int k = 0; k < TotalEventEntries; k++)
                        {
                            int offset = 0;
                            if (!is_DatalogFile)
                            {
                                offset = 8;
                            }

                            if (k == 0)
                            {
                                Buffer.BlockCopy(_logBytes, (int)i * TOTAL_ENTRY_BYTES + (k * TOTAL_ENTRY_BYTES), _logChuncks, 0, TOTAL_ENTRY_BYTES);

                            }
                            else
                            {

                                Buffer.BlockCopy(_logBytes, (int)i * TOTAL_ENTRY_BYTES + (k * TOTAL_ENTRY_BYTES + offset), _logChuncks, TOTAL_ENTRY_BYTES + k * TOTAL_ENTRY_BYTES, TOTAL_ENTRY_BYTES - offset);


                            }
                        }
                    }
catch
                    {

                    }
                    // === loop through the data to retreive the current log entry data ===


                    // === Copy the logtimestamp(unix) from the log chunck ===
                    Buffer.BlockCopy(_logChuncks, 0, _logTimeStamp, 0, 4);
                    DateTime _eventDateTime;
                    // === Determine if the logtimestamp is valid ===
                    uint _dateTimeStatus = DateTimeCheck.CheckDateTimeStampUnix(BitConverter.ToUInt32(_logTimeStamp, 0), out _eventDateTime);

                    // === Check if the logtimestamp is valid continue parsing information ===
                    if (_dateTimeStatus == (uint)DateTimeCheck.Status.Ok)
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


                        // UInt16 _tempEventID = BitConverter.ToUInt16(_logChuncks, 6);

                        LogEntry _LogEntry = new LogEntry
                        {
                            Number = i,
                            EventID = _tempEventID,
                            RawEntry = _logChuncks,
                            DateTime = _eventDateTime,
                        };
                        // === Parse Data Log Entry into List ===
                        if (is_DatalogFile)
                        {
                            ParseDataEntry_V2(_LogEntry);
                        }
                        else
                        {
                            ParseDataEntry_V1(_LogEntry);
                        }

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

                    i += (uint)TotalEventEntries - 1;


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
        bool ParseDataEntry_V1(LogEntry _LogEntry)
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
                    if (ExcelFilemanager.LPDInfoList_V1.ElementAt(_LogEntry.EventID - 1).Data[0].DataLink == "Empty" && _LogEntry.EventID != 65535)
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
                        foreach (LPDDataLookupEntry _dataLookupEntry in ExcelFilemanager.LPDInfoList_V1.ElementAt(_LogEntry.EventID - 1).Data)
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

                                    if (_dataLookupEntry.EnumLink == 0 || ExcelFilemanager.LPDInfoEnumList_V1[_dataLookupEntry.EnumLink].Count < _logData[_index])
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
                                        _tempDataListString.Add((ExcelFilemanager.LPDInfoEnumList_V1[_dataLookupEntry.EnumLink].ElementAt(_logData[_index])));
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
                        if (_LogEntry.EventID <= ExcelFilemanager.LPDInfoList_V1.Count())
                        {
                            // === Add to the list ===
                            _LogEntry.EventName = ExcelFilemanager.LPDInfoList_V1.ElementAt(_LogEntry.EventID - 1).EventName;
                            _LogEntry.RawData = _logData;
                            _LogEntry.DataListString = _tempDataListString;
                            _LogEntry.DataList = _tempDataList;
                            _LogEntry.EventInfoList = _tempEventInfoList;
                            TempList.Add(_LogEntry);
                        }                   

                }
                catch (Exception e)
                {
                    Debug.WriteLine("unable to add event");
                }
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
        bool ParseDataEntry_V2(LogEntry _LogEntry)
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
                    if (ExcelFilemanager.LPDInfoList_V2.ElementAt(_LogEntry.EventID - 1).Data[0].DataLink == "Empty" && _LogEntry.EventID != 65535)
                    {
                        _tempEventInfo = "No Information";
                        _tempEventInfoList.Add("No Information");
                    }
                    // === else parse additional information ===
                    else
                    {
                   
                        int _dataIndex = 0;
                        int _DataLookupEntryIndex = 0;
                        
                        // for loop through all the sub-data of an data entry ====
                        foreach (LPDDataLookupEntry _dataLookupEntry in ExcelFilemanager.LPDInfoList_V2.ElementAt(_LogEntry.EventID - 1).Data)
                        {
                            // === Check if not the end of all the sub-data ===
                            if (_dataLookupEntry.DataLink != "Empty")
                            {
                                // === Detemine the Math scale of the LPD file for the sub-data entry ===
                                double _scale = Math.Pow(10, _dataLookupEntry.Scale);

                                // === Add a seperator after each sub-data entry ===
                                if (_DataLookupEntryIndex > 0)
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
                                                                          
                                        Array.Copy(_logData, _dataIndex, _tempByteArray, 0, 4);

                                        UInt32 HexValue = BitConverter.ToUInt32(_tempByteArray, 0);

                                        _tempDataListString.Add("0x" + (Convert.ToUInt32(HexValue * _scale)).ToString("X8"));
                                        _tempDataList.Add(Convert.ToDouble(HexValue));


                                    }
                                    // === Else check if the sub-data is an signed integer ===
                                    else if (_dataLookupEntry.IsInt == 1)
                                    {
                                        _tempDataListString.Add((BitConverter.ToInt32(_logData, _dataIndex) * _scale).ToString());
                                        _tempDataList.Add(double.Parse(_tempDataListString.Last()));

                                    }
                                    // ===  Else sub-data is an unsigned integer ===
                                    else
                                    {

                                        _tempDataListString.Add((BitConverter.ToUInt32(_logData, _dataIndex) * _scale).ToString());
                                        _tempDataList.Add(double.Parse(_tempDataListString.Last()));

                                    }

                                    _tempEventInfo += _dataLookupEntry.DataName + ": " + _tempDataListString.Last();
                                    _tempEventInfoList.Add(_dataLookupEntry.DataName + ": " + _tempDataListString.Last() + _dataLookupEntry.Appendix);


                                    _dataIndex += 4;

                                }
                                // === Else check if the number of byte are 2 (16-bit) for the sub-data entry ===
                                else if (_dataLookupEntry.NumberBytes == 2)
                                {
                                    string Appendix = _dataLookupEntry.Appendix;
                                    // === Else check if the sub-data is an signed integer ===
                                    if (_dataLookupEntry.IsInt == 1)
                                    {
                                        _tempDataListString.Add((BitConverter.ToInt16(_logData, _dataIndex) * _scale).ToString());
                                    }
                                    // ===  Else sub-data is an unsigned integer ===
                                    else
                                    {

                                        UInt32 tempvalue = (UInt32)(BitConverter.ToUInt16(_logData, _dataIndex) * _scale);

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
                                    _dataIndex += 2;
                                }
                                // === Else check if the number of byte are 1 (8-bit) for the sub-data entry ===
                                else if (_dataLookupEntry.NumberBytes == 1)
                                {

                                    if (_dataLookupEntry.EnumLink == 0 || ExcelFilemanager.LPDInfoEnumList_V2[_dataLookupEntry.EnumLink].Count < _logData[_dataIndex])
                                    {
                                        // === Else check if the sub-data is an signed integer ===
                                        if (_dataLookupEntry.IsInt == 1)
                                        {
                                            _tempDataListString.Add(((ushort)_logData[_dataIndex] * _scale).ToString());
                                        }
                                        // ===  Else sub-data is an unsigned integer ===
                                        else
                                        {
                                            _tempDataListString.Add((_logData[_dataIndex] * _scale).ToString());
                                        }
                                        _tempDataList.Add(double.Parse(_tempDataListString.Last()));

                                    }

                                    else if (_dataLookupEntry.EnumLink != 0)
                                    {
                                        _tempDataList.Add((_logData[_dataIndex] * _scale));
                                        _tempDataListString.Add((ExcelFilemanager.LPDInfoEnumList_V2[_dataLookupEntry.EnumLink].ElementAt(_logData[_dataIndex])));
                                    }

                                    _tempEventInfo += _dataLookupEntry.DataName + ": " + _tempDataListString.Last();
                                    _tempEventInfoList.Add(_dataLookupEntry.DataName + ": " + _tempDataListString.Last() + _dataLookupEntry.Appendix);


                                    _dataIndex += 1;
                                }

                            }

                            _tempEventInfo += " " + _dataLookupEntry.Appendix;
                            _DataLookupEntryIndex++;
                          
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
                        
                    if (_LogEntry.EventID <= ExcelFilemanager.LPDInfoList_V2.Count())
                    {
                        // === Add to the list ===
                        _LogEntry.EventName = ExcelFilemanager.LPDInfoList_V2.ElementAt(_LogEntry.EventID - 1).EventName;
                        _LogEntry.RawData = _logData;
                        _LogEntry.DataListString = _tempDataListString;
                        _LogEntry.DataList = _tempDataList;
                        _LogEntry.EventInfoList = _tempEventInfoList;
                        TempList.Add(_LogEntry);
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

 
