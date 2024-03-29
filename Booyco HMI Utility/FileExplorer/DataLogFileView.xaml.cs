﻿
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace Booyco_HMI_Utility
{
    /// <summary>
    /// Interaction logic for FileView.xaml
    /// </summary>
    public partial class DataLogFileView : UserControl
    {
        string _savedFilesPath = System.IO.Path.Combine(Environment.ExpandEnvironmentVariables("%userprofile%"), "Documents") + "\\BHU Utility\\Datalogs";
        private RangeObservableCollection<FileEntry> FileList;

        public DataLogFileView()
        {
            InitializeComponent();     
            FileList = new RangeObservableCollection<FileEntry>();
            DataGridFiles.ItemsSource = FileList;
            ReadSavedFolder();
            Button_Delete.IsEnabled = false;
            Button_Save.IsEnabled = false;
        }

        private void ButtonDataViewer_Click(object sender, RoutedEventArgs e)
        {
            GlobalSharedData.FilePath = FileList.ElementAt(DataGridFiles.SelectedIndex).Path;
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.DataLogView;
        }
        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.FileMenuView;
            this.Visibility = Visibility.Collapsed;
        }

        private void ButtonConfigViewer_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.ParametersView;
        }

        private void ReadSavedFolder()
        {
            FileList.Clear();
            if (Directory.Exists(_savedFilesPath))
            {
                DirectoryInfo d = new DirectoryInfo(_savedFilesPath);
                RangeObservableCollection<FileEntry> UnsortedFileList = new RangeObservableCollection<FileEntry>();
                FileInfo[] FilesTxt = d.GetFiles("*.txt");
                FileInfo[] FilesMer = d.GetFiles("*.mer");
                FileInfo[] Files = FilesTxt.Union(FilesMer).ToArray();
                string str = "";
              
               
                foreach (FileInfo file in Files)
                {
                    string _type = "";
                    if(file.Name.Split('_')[0] == "DataLog")
                    {
                        _type = "DataLog";
                    }
                 
                 
                    UnsortedFileList.Add(new FileEntry
                    {
                        Number = 0,
                        FileName = file.Name,
                        Type = _type,
                        Path = file.FullName,
                        DateTimeCreated = file.CreationTime.ToString("yyyy-MM-dd HH-mm-ss")
                    });
                }

                FileList.AddRange((UnsortedFileList.OrderByDescending(p => p.DateTimeCreated)));
                uint count = 0;
                foreach (FileEntry item in FileList)
                {
                    count++;
                    item.Number = count;
                }
          
            }
            else
            {
                Directory.CreateDirectory(_savedFilesPath);
            }

        }

        private void DataGridFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid _dataGrid = (DataGrid)sender;
            try
            {

                if (_dataGrid.SelectedIndex >= 0)
                {
                    Button_Delete.IsEnabled = true;
                    Button_Save.IsEnabled = true;
                   
                }
                else
                {
                    Button_Delete.IsEnabled = false;
                    Button_Save.IsEnabled = false;
                  
                }

                if(_dataGrid.SelectedItems.Count == 1)
                {
                    ButtonOpen.IsEnabled = true;
                    GlobalSharedData.FilePath = FileList.ElementAt(_dataGrid.SelectedIndex).Path;


                    BinaryReader _breader = new BinaryReader(File.OpenRead(FileList.ElementAt(_dataGrid.SelectedIndex).Path));
                    int _fileLength = (int)(new FileInfo(FileList.ElementAt(_dataGrid.SelectedIndex).Path).Length);
                    byte[] _parameters = _breader.ReadBytes(_fileLength);

                    bool is_DatalogFile = true;

                    for (int k = 0; k < 16; k++)
                    {
                        if (_parameters[k] != '*')
                        {
                            is_DatalogFile = false;
                            break;
                        }
                    }

                    if(is_DatalogFile)
                    {
                        ButtonParameter.IsEnabled = true;
                    }
                    else
                    {
                        ButtonParameter.IsEnabled = false;
                    }
                }
                else
                {
                    ButtonParameter.IsEnabled = false;
                    ButtonOpen.IsEnabled = false;
                }

            }
            catch
            {
                ButtonOpen.IsEnabled = false;
              
            }
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

          
            openFileDialog.Filter = "Mer files (*.mer)|*.mer|txt files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true)
            {
                System.IO.File.Copy(openFileDialog.FileName, _savedFilesPath+"\\"+ openFileDialog.SafeFileName, true);
            }

            ReadSavedFolder();


        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            var _selectedItems = DataGridFiles.SelectedItems;

            foreach (FileEntry item in _selectedItems)
            {
                try
                {                             

                    if (DataGridFiles.SelectedIndex >= 0)
                    {
                        System.IO.File.Delete(_savedFilesPath + "\\" + FileList.ElementAt((int)item.Number-1).FileName);
                       
                    }
                }
                catch(Exception ex)
                {
                    Debug.WriteLine("Cannot Delete file " + item.FileName);
                }
            }
            ReadSavedFolder();

        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog _saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            string _filename = GlobalSharedData.FilePath.Split('\\').Last();
            _saveFileDialog.FileName = _filename.Remove(_filename.Length - 4, 4);
            //openFileDialog.InitialDirectory = "c:\\";
            _saveFileDialog.Filter = "Mer files (*.mer)|*.mer";
            //openFileDialog.FilterIndex = 1;
            //openFileDialog.RestoreDirectory = true;
            var _selectedItems = DataGridFiles.SelectedItems;
          
            if (_saveFileDialog.ShowDialog() == true)
            {
                foreach (FileEntry item in _selectedItems)
                {

                    try
                    {
                        if (_selectedItems.Count == 1)
                        {
                            System.IO.File.Copy(_savedFilesPath + "\\" + FileList.ElementAt((int)item.Number - 1).FileName, _saveFileDialog.FileName, true);
                        }
                        else
                        {
                            System.IO.File.Copy(_savedFilesPath + "\\" + FileList.ElementAt((int)item.Number - 1).FileName, System.IO.Path.GetDirectoryName(_saveFileDialog.FileName) + "\\" + FileList.ElementAt((int)item.Number - 1).FileName, true);
                        }
                    }
                    catch
                    {

                    }
                    
                }
            }
                    
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                ReadSavedFolder();
            }
        }

        private void ButtonParameter_Click(object sender, RoutedEventArgs e)
        {
            GlobalSharedData.FilePath = FileList.ElementAt(DataGridFiles.SelectedIndex).Path;
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.ParametersView;
        }
    }
}
