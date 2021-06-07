using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using ExcelDataReader;
using System.IO;
using System.Data;
using System.Reflection;
using System.Diagnostics;

namespace Booyco_HMI_Utility
{
     class ExcelFileManagement
    {
        public List<LPDEntry> LPDInfoList_V2 = new List<LPDEntry>();
        public List<List<string>> LPDInfoEnumList_V2 = new List<List<string>>();
        public List<LPDDataLookupEntry> LPDLookupList_V2 = new List<LPDDataLookupEntry>();

        public List<LPDEntry> LPDInfoList_V1 = new List<LPDEntry>();
        public List<List<string>> LPDInfoEnumList_V1 = new List<List<string>>();
        public List<LPDDataLookupEntry> LPDLookupList_V1 = new List<LPDDataLookupEntry>();

        string _LPDPath_V1 = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/Resources/Documents/BHUEvents_v1.xlsx";
        string _LPDPath_V2 = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/Resources/Documents/BHUEvents_v2.xlsx";
        string _CommsBridge_EventsPath = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/Resources/Documents/Comms_Bridge_Events.xlsx";

        public void StoreLogProtocolInfo()
        {
            StoreLogProtocolInfo_V1();
            StoreLogProtocolInfo_V2(56);
        }


        public void StoreLogProtocolInfo_V2(int FirmwareNumber)
        {
            LPDInfoEnumList_V2.Clear();
            DataRowCollection _Eventdata;  
            DataRowCollection _lookup;
            DataRowCollection _Analogdata; 
           
            if(FirmwareNumber == 69)
            {
                 _Eventdata  = ReadExcelFile(_CommsBridge_EventsPath, 1);
                 _lookup = ReadExcelFile(_CommsBridge_EventsPath, 4);
                 _Analogdata = ReadExcelFile(_CommsBridge_EventsPath, 3);
            }
            else
            {
                 _Eventdata  = ReadExcelFile(_LPDPath_V2, 1);
                 _lookup = ReadExcelFile(_LPDPath_V2, 4);
                 _Analogdata = ReadExcelFile(_LPDPath_V2, 3);
            }
            List<string> tempStringList = new List<string>();
            int EnumIndex = 0;
            foreach (DataRow _row in _lookup)
            {
                try

                {
                    int unkownValue = 0;
                    string unkownText = "";


                    if (Convert.ToString(_row.ItemArray[8]) != "")
                    {
                        unkownValue=  Convert.ToInt32(_row.ItemArray[8]);
                    }
                    if (Convert.ToString(_row.ItemArray[9]) != "")
                    {
                        unkownText = Convert.ToString(_row.ItemArray[9]);
                    }

                    if (Convert.ToString(_row.ItemArray[1]) != "" && Convert.ToString(_row.ItemArray[2]) != "")
                    {

                        LPDLookupList_V2.Add(new LPDDataLookupEntry
                        {
                            DataLink = Convert.ToString(_row.ItemArray[1]),
                            DataName = Convert.ToString(_row.ItemArray[2]),
                            NumberBytes = Convert.ToInt32(_row.ItemArray[3]),
                            Scale = Convert.ToInt32(_row.ItemArray[4]),
                            IsInt = Convert.ToInt32(_row.ItemArray[5]),
                            Appendix = Convert.ToString(_row.ItemArray[6]),
                            EnumLink = Convert.ToInt32(_row.ItemArray[7]),
                            UnkownValue = unkownValue,
                            UnkownText = unkownText,
                            

                        });
                    }

                    if (Convert.ToString(_row.ItemArray[13]) != "" && Convert.ToString(_row.ItemArray[14]) != "")
                    {
                        if (Convert.ToInt32(_row.ItemArray[13]) == EnumIndex)
                        {
                            tempStringList.Add(Convert.ToString(_row.ItemArray[14]));
                        }
                        else
                        {
                            LPDInfoEnumList_V2.Add(tempStringList);
                            tempStringList = new List<string>
                        {
                            Convert.ToString(_row.ItemArray[14])
                        };
                            EnumIndex = Convert.ToInt32(_row.ItemArray[13]);
                        }
                    }

                }

               
                catch(Exception e)
                {
                    Debug.WriteLine("Exception - Lookup Excel Read Fail");
                }
                //if (Convert.ToInt32(_row.ItemArray[5]) == 1)
                //{
                //    int u = 0;
                //}

            }

            LPDInfoEnumList_V2.Add(tempStringList);
            foreach (DataRow _row in _Eventdata)

                {
                List<LPDDataLookupEntry> _tempData = new List<LPDDataLookupEntry>();

                    if (_row.ItemArray.Count() > 18)
                    {
                        for (int i = 10; i < 18; i++)
                        {
                            string _tempByteName = Convert.ToString(_row.ItemArray[i]);


                            if (LPDLookupList_V2.Any(p => p.DataLink == _tempByteName))
                            {
                                _tempData.Add(LPDLookupList_V2.FirstOrDefault(p => p.DataLink == _tempByteName));
                                //Buffer.BlockCopy(_logChuncks, 0, _logTimeStamp, 0, 6);    
                            }
                            else
                            {

                                if (_tempByteName == "0xFF" || _tempByteName == "Reserved" || _tempByteName == "")

                                {
                                    i = 18;
                                    _tempData.Add(new LPDDataLookupEntry
                                    {
                                        DataLink = "Empty"
                                    });
                                }                          
                       
                            }
                        }

                    try
                    {
                        LPDInfoList_V2.Add(new LPDEntry

                        {
                            EventID = Convert.ToUInt16(_row.ItemArray[0]),
                            EventName = Convert.ToString(_row.ItemArray[18]),
                            Data = _tempData

                        });
                    }
                    catch
                    {
                        Debug.WriteLine("Exception - Log Excel Read Fail");
                    }                    

                }
            }

            foreach (DataRow _row in _Analogdata)

            {
                List<LPDDataLookupEntry> _tempData = new List<LPDDataLookupEntry>();

                if (_row.ItemArray.Count() > 18)
                {
                    for (int i = 10; i < 18; i++)
                    {
                        string _tempByteName = Convert.ToString(_row.ItemArray[i]);


                        if (LPDLookupList_V2.Any(p => p.DataLink == _tempByteName))
                        {
                            _tempData.Add(LPDLookupList_V2.FirstOrDefault(p => p.DataLink == _tempByteName));
                            //Buffer.BlockCopy(_logChuncks, 0, _logTimeStamp, 0, 6);    
                        }
                        else
                        {

                            if (_tempByteName == "0xFF" || _tempByteName == "Reserved" || _tempByteName == "")

                            {
                                i = 18;
                                _tempData.Add(new LPDDataLookupEntry
                                {
                                    DataLink = "Empty"
                                });
                            }

                        }
                    }

                    try
                    {
                        LPDInfoList_V2.Add(new LPDEntry

                        {
                            EventID = Convert.ToUInt16(_row.ItemArray[0]),
                            EventName = Convert.ToString(_row.ItemArray[18]),
                            Data = _tempData

                        });
                    }
                    catch
                    {
                        Debug.WriteLine("Exception - Log Excel Read Fail");
                    }

                }
            }

            foreach (LPDEntry Uniqueitem in  LPDInfoList_V2.Distinct())
            {
                if (Uniqueitem.EventName == "Proximity Detection")
                {
                    int testc = 0;
                }
                int TotalCount = LPDInfoList_V2.FindAll(x => x.EventName == Uniqueitem.EventName).Count();
                foreach (LPDEntry item in LPDInfoList_V2.FindAll(x => x.EventName == Uniqueitem.EventName))
                {
                    item.TotalEntries = TotalCount;
                }
                
   
            }
        }

        public void StoreLogProtocolInfo_V1()
        {
            DataRowCollection _data = ReadExcelFile(_LPDPath_V1, 0);
            DataRowCollection _lookup = ReadExcelFile(_LPDPath_V1, 1);
            List<string> tempStringList = new List<string>();
            int EnumIndex = 0;
            foreach (DataRow _row in _lookup)
            {
                try

                {
                    int unkownValue = 0;
                    string unkownText = "";


                    if (Convert.ToString(_row.ItemArray[8]) != "")
                    {
                        unkownValue = Convert.ToInt32(_row.ItemArray[8]);
                    }
                    if (Convert.ToString(_row.ItemArray[9]) != "")
                    {
                        unkownText = Convert.ToString(_row.ItemArray[9]);
                    }

                    
                    LPDLookupList_V1.Add(new LPDDataLookupEntry
                    {
                        DataLink = Convert.ToString(_row.ItemArray[1]),
                        DataName = Convert.ToString(_row.ItemArray[2]),
                        NumberBytes = Convert.ToInt32(_row.ItemArray[3]),
                        Scale = Convert.ToInt32(_row.ItemArray[4]),
                        IsInt = Convert.ToInt32(_row.ItemArray[5]),
                        Appendix = Convert.ToString(_row.ItemArray[6]),
                        EnumLink = Convert.ToInt32(_row.ItemArray[7]),
                        UnkownValue = unkownValue,
                        UnkownText = unkownText,

                    });

                    if (Convert.ToInt32(_row.ItemArray[11]) == EnumIndex)
                    {
                        tempStringList.Add(Convert.ToString(_row.ItemArray[12]));
                    }
                    else
                    {
                        LPDInfoEnumList_V1.Add(tempStringList);
                        tempStringList = new List<string>
                        {
                            Convert.ToString(_row.ItemArray[12])
                        };
                        EnumIndex = Convert.ToInt32(_row.ItemArray[11]);
                    }

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

            LPDInfoEnumList_V1.Add(tempStringList);
            foreach (DataRow _row in _data)

            {
                List<LPDDataLookupEntry> _tempData = new List<LPDDataLookupEntry>();

                if (_row.ItemArray.Count() == 12)
                {
                    for (int i = 2; i < 10; i++)
                    {
                        string _tempByteName = Convert.ToString(_row.ItemArray[i]);


                        if (LPDLookupList_V1.Any(p => p.DataLink == _tempByteName))
                        {
                            _tempData.Add(LPDLookupList_V1.FirstOrDefault(p => p.DataLink == _tempByteName));
                            //Buffer.BlockCopy(_logChuncks, 0, _logTimeStamp, 0, 6);    
                        }
                        else
                        {

                            if (_tempByteName == "0xFF" || _tempByteName == "Reserved" || _tempByteName == "")

                            {
                                i = 10;
                                _tempData.Add(new LPDDataLookupEntry
                                {
                                    DataLink = "Empty"
                                });
                            }

                        }
                    }

                    try
                    {
                        LPDInfoList_V1.Add(new LPDEntry

                        {
                            EventID = Convert.ToUInt16(_row.ItemArray[0]),
                            EventName = Convert.ToString(_row.ItemArray[10]),
                            Data = _tempData,                      

                        });
                    }
                    catch
                    {
                        Debug.WriteLine("Exception - Log Excel Read Fail");
                    }

                }
            }

            foreach (LPDEntry Uniqueitem in LPDInfoList_V1.Distinct())
            {
                if (Uniqueitem.EventName == "Proximity Detection")
                {
                    int testc = 0;
                }
                int TotalCount = LPDInfoList_V1.FindAll(x => x.EventName == Uniqueitem.EventName).Count();
                foreach (LPDEntry item in LPDInfoList_V1.FindAll(x => x.EventName == Uniqueitem.EventName))
                {
                    item.TotalEntries = TotalCount;
                }
            }
        }

        public List<Parameters> ParametersfromFile(string fileName)
        {
            List<Parameters> parameters = new List<Parameters>();

            List<ParameterEnum> enums = new List<ParameterEnum>();

            List<string> AssetType = new List<string>();
            List<int> AssetIndex = new List<int>();
            MernokAssetFile mernokAssetFile = new MernokAssetFile();
            mernokAssetFile = MernokAssetManager.ReadMernokAssetFile();

            AssetType = mernokAssetFile.mernokAssetList.Select(t => t.TypeName).ToList();

            foreach (string item in AssetType)
            {
                AssetIndex.Add(AssetIndex.Count());
            }

            DataRowCollection _data = ReadExcelFile(fileName, 0);
            DataRowCollection _dataGroupList = ReadExcelFile(fileName, 1);


            // === Read Group and Subgroup order ===
            List<ParameterGroup> GroupNames = new List<ParameterGroup>();
            if (_dataGroupList != null)
            {
                foreach (DataRow _row in _dataGroupList)
                {
                    try
                    {
                        ParameterGroup _Group = new ParameterGroup();
                        _Group.SubGroupNames = new List<String>();
                        //=== store group order from sheet ===
                        _Group.GroupName = Convert.ToString(_row.ItemArray[3]);
                        //=== store subgroup order from sheet ===
                        String _SubGroupName = Convert.ToString(_row.ItemArray[4]);
                        if (_Group.GroupName != "" && !GroupNames.Exists(x => x.GroupName == _Group.GroupName))
                        {
                            _Group.SubGroupNames.Add(_SubGroupName);
                            GroupNames.Add(_Group);

                        }
                        else
                        {
                            GroupNames.ElementAt(GroupNames.FindIndex(x => x.GroupName == _Group.GroupName)).SubGroupNames.Add(_SubGroupName);
                        }
                    }
                    catch
                    {
                        Debug.WriteLine("Exception - Enum Parameter from excel error");
                    }

                }
            }

            // === Read Parameter information ===
            if (_data != null)
            {
                int _rowCount = 0;
                foreach (DataRow _row in _data)
                {
                    // if (_rowCount != 0)
                    // {

                    int _order = 0;
                    if (_row.ItemArray[18].ToString() != "")
                    {
                        _order = Convert.ToInt16(_row.ItemArray[18]);
                    }
                    string _group = Convert.ToString(_row.ItemArray[10]);
                    string _subGroup = Convert.ToString(_row.ItemArray[11]);
                    int _groupOrder = 0;
                    int _subGroupOrder = 0;
                    if (GroupNames.Exists(x => x.GroupName == _group))
                    {
                        _groupOrder = GroupNames.FindIndex(x => x.GroupName == _group);
                    }

                    if (GroupNames.ElementAt(_groupOrder).SubGroupNames.Exists(x => x == _subGroup))
                    {
                        _subGroupOrder = GroupNames.ElementAt(_groupOrder).SubGroupNames.FindIndex(x => x == _subGroup);
                    }

                    try
                    {

                        string UnitString = "";
                        if (Convert.ToString(_row.ItemArray[7]) != "-")
                        {
                            UnitString = "(" + Convert.ToString(_row.ItemArray[7]) + ")";
                        }
                 
                        parameters.Add(new Parameters
                        {
                            Number = Convert.ToInt32(_row.ItemArray[0]),
                            Name = Convert.ToString(_row.ItemArray[2]),
                            CurrentValue = Convert.ToInt32(_row.ItemArray[3]),
                            MaximumValue = Convert.ToInt32(_row.ItemArray[4]),
                            MinimumValue = Convert.ToInt32(_row.ItemArray[5]),
                            DefaultValue = Convert.ToInt32(_row.ItemArray[6]),
                            Unit = UnitString,
                            Ptype = Convert.ToInt16(_row.ItemArray[8]),
                            enumVal = Convert.ToInt16(_row.ItemArray[9]),
                            Group = _group,
                            SubGroup = _subGroup,
                            Active = Convert.ToInt16(_row.ItemArray[12]),
                            Description = Convert.ToString(_row.ItemArray[13]),
                            Dependency = Convert.ToInt16(_row.ItemArray[14]),
                            AccessLevel = Convert.ToInt16(_row.ItemArray[15]),
                            Version = Convert.ToInt16(_row.ItemArray[16]),
                            SubVersion = Convert.ToInt16(_row.ItemArray[17]),
                            Order = _order,
                            GroupOrder = _groupOrder,
                            SubGroupOrder = _subGroupOrder,


                        }); ;

                        if (_row.ItemArray[19].ToString() != "")
                        {
                            enums.Add(new ParameterEnum
                            {
                                enumVal = Convert.ToInt16(_row.ItemArray[19]),
                                enumName = Convert.ToString(_row.ItemArray[20]),
                                enumIndex = Convert.ToInt16(_row.ItemArray[21])
                            });
                        }

                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Exception - Parameter from excel error: " + e.ToString());
                    }

                }

                foreach (var item in parameters)
                {
                    if (item.enumVal != 3)
                    {
                        item.parameterEnumsName = enums.Where(t => t.enumVal == item.enumVal).Select(t => t.enumName).ToList();
                        item.parameterEnumsValue = enums.Where(t => t.enumVal == item.enumVal).Select(t => t.enumIndex).ToList();
                    }
                    else
                    {
                        item.parameterEnumsName = AssetType;
                        item.parameterEnumsValue = AssetIndex;
                        item.MaximumValue = AssetType.Count - 1;
                    }

                    if (item.Ptype == 2)
                    {
                        item.CurrentValue = item.parameterEnumsValue.ElementAt(item.CurrentValue);
                    }

                }
            }
            else
            {
                //if this point is reached then there was no parameters to display.
                Debug.WriteLine("No parameters to display. Please view error logs");
            }

            return parameters;


        }

        public DataRowCollection ReadExcelFile(string _fileName, int TableNumber)
        {
            System.Data.DataSet ds;

            var extension = System.IO.Path.GetExtension(_fileName).ToLower();
            using (var stream = new FileStream(_fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {

                IExcelDataReader reader = null;
                if (extension == ".xls")
                {
                    reader = ExcelReaderFactory.CreateBinaryReader(stream);
                }
                else if (extension == ".xlsx")
                {
                    reader = ExcelReaderFactory.CreateOpenXmlReader(stream);
                }
                else if (extension == ".csv")
                {
                    reader = ExcelReaderFactory.CreateCsvReader(stream);
                }

                // if (reader == null)
                //     return;

                //reader.IsFirstRowAsColumnNames = firstRowNamesCheckBox.Checked;
                using (reader)
                {

                    ds = reader.AsDataSet(new ExcelDataSetConfiguration()
                    {
                        UseColumnDataType = false,
                        ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
                        {
                            UseHeaderRow = true
                        }
                    });
                }

                try
                {
                    return ds.Tables[TableNumber].DefaultView.ToTable().Rows;
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Exception - Parameter from excel error: " + e.ToString());
                }

                return null;
            }

        }
        
    }

}
