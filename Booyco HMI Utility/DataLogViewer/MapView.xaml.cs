using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using Booyco_HMI_Utility.CustomMarkers;
using GMap.NET;

using GMap.NET.WindowsPresentation;

namespace Booyco_HMI_Utility
{
    /// <summary>
    /// Interaction logic for MapView.xaml
    /// </summary>
    public partial class MapView : UserControl
    {
        private bool isWindow = false;
        public bool CloseRequest = false;
       
        public double StartLat = -25.882784;
        public double StartLon = 28.163630;
        public double scalefactor = 70000;



        public MapView()
        {
            InitializeComponent();
        }

        public void UpdateMapMarker()
        {
            this.MainMap.Markers.Clear();

            foreach (MarkerEntry item in GlobalSharedData.PDSMapMarkers.FindAll(p => p.Type == (int)MarkerType.Ellipse))
            {
                if (GlobalSharedData.PDSMapMarkers.Exists(p => p.MapMarker.LocalPositionX != item.MapMarker.LocalPositionX && p.MapMarker.LocalPositionY != item.MapMarker.LocalPositionY))
                {
                    item.Scale = scalefactor / (Math.Pow(2, (ushort)MainMap.Zoom));
                    item.MapMarker.Shape = new CustomMarkerEllipse(this.MainMap, item);
                    this.MainMap.Markers.Add(item.MapMarker);                   
                }
            }

            foreach (MarkerEntry item in GlobalSharedData.PDSMapMarkers.FindAll(p => p.Type == (int)MarkerType.Point))
            {
                item.MapMarker.Shape = new CustomMarkerPoint(this.MainMap, item);
                this.MainMap.Markers.Add(item.MapMarker);
            }
            foreach (MarkerEntry item in GlobalSharedData.PDSMapMarkers.FindAll(p => p.Type == (int)MarkerType.Cross))
            {
                item.MapMarker.Shape = new CustomMarkerCross(this.MainMap, item);
                this.MainMap.Markers.Add(item.MapMarker);
            }
            foreach (MarkerEntry item in GlobalSharedData.PDSMapMarkers.FindAll(p => p.Type == (int)MarkerType.Indicator))
            {
                item.Scale = scalefactor / (Math.Pow(2, (ushort)MainMap.Zoom));
                item.MapMarker.Shape = new CustomMarkerIndicator(this.MainMap, item);
                this.MainMap.Markers.Add(item.MapMarker);
                StartLat = item.VehicleInfo.Latitude;
                StartLon = item.VehicleInfo.Longitude;
            }
           

            MainMap.Position = new GMap.NET.PointLatLng(StartLat, StartLon);
          
        }

        private void mapView_Loaded(object sender, RoutedEventArgs e)
        {


            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;

            // choose your provider here
            //MainMap.MapProvider = GMap.NET.MapProviders.OpenStreetMapProvider.Instance;
            MainMap.MapProvider = GMap.NET.MapProviders.GoogleSatelliteMapProvider.Instance;
            //MainMap.MapProvider = GMap.NET.MapProviders.GoogleTerrainMapProvider.Instance;

            // whole world zoom
            MainMap.MinZoom = 3;
            MainMap.MaxZoom = 20;
            MainMap.Zoom = 19;

            // lets the map use the mousewheel to zoom
            MainMap.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionWithoutCenter;

            // lets the user drag the map
            MainMap.CanDragMap = true;
            //MainMap.ShowCenter = false;

            // lets the user drag the map with the left mouse button
            MainMap.DragButton = MouseButton.Left;

            MainMap.Position = new GMap.NET.PointLatLng(StartLat, StartLon);
         
            MainMap.ShowCenter = false;
      
        }
        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            
             ProgramFlow.ProgramWindow = (int)ProgramFlowE.DataLogView;
            CloseRequest = true;

        }
        private void ButtonNewWindow_Click(object sender, RoutedEventArgs e)
        {

            GlobalSharedData.ViewMode = true;
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.DataLogView;
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(this.Visibility == Visibility.Visible)
            {
                UpdateMapMarker();
            }
        }

        private void MainMap_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            this.MainMap.Markers.Clear();

            // double zoom = MainMap.Zoom;

            foreach (MarkerEntry item in GlobalSharedData.PDSMapMarkers.FindAll(p => p.Type == (int)MarkerType.Ellipse))
            {
                if (GlobalSharedData.PDSMapMarkers.Exists(p => p.MapMarker.LocalPositionX != item.MapMarker.LocalPositionX && p.MapMarker.LocalPositionY != item.MapMarker.LocalPositionY))
                {
                    item.Scale = scalefactor / (Math.Pow(2, (ushort)MainMap.Zoom));
                    item.MapMarker.Shape = new CustomMarkerEllipse(this.MainMap, item);
                    this.MainMap.Markers.Add(item.MapMarker);
                }
            }
            foreach (MarkerEntry item in GlobalSharedData.PDSMapMarkers.FindAll(p => p.Type == (int)MarkerType.Point))
            {
                item.MapMarker.Shape = new CustomMarkerPoint(this.MainMap, item);
                this.MainMap.Markers.Add(item.MapMarker);
            }
            foreach (MarkerEntry item in GlobalSharedData.PDSMapMarkers.FindAll(p => p.Type == (int)MarkerType.Cross))
            {
                item.MapMarker.Shape = new CustomMarkerCross(this.MainMap, item);
                this.MainMap.Markers.Add(item.MapMarker);
            }
            foreach (MarkerEntry item in GlobalSharedData.PDSMapMarkers.FindAll(p => p.Type == (int)MarkerType.Indicator))
            {
                item.Scale = scalefactor / (Math.Pow(2, (ushort)MainMap.Zoom));
                item.MapMarker.Shape = new CustomMarkerIndicator(this.MainMap, item);
                this.MainMap.Markers.Add(item.MapMarker);
            }

        }

        private void ButtonMapType_Click(object sender, RoutedEventArgs e)
        {
            if (MainMap.MapProvider == GMap.NET.MapProviders.GoogleSatelliteMapProvider.Instance)
            {

                MainMap.MapProvider = GMap.NET.MapProviders.GoogleTerrainMapProvider.Instance;
                ButtonMapType.Content = "Satellite";
                }
            else
            {
               MainMap.MapProvider = GMap.NET.MapProviders.GoogleSatelliteMapProvider.Instance;
                ButtonMapType.Content = "Map";
            }
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            MainMap.Markers.Clear();
        }

        private void ButtonPrintMap_Click(object sender, RoutedEventArgs e)
        {
            // === Open Save File Dialog ===
            Microsoft.Win32.SaveFileDialog _saveFileDialog = new Microsoft.Win32.SaveFileDialog();

            // == Default extension ===
            _saveFileDialog.DefaultExt = "jpeg";
            // == filter types ===
            _saveFileDialog.Filter = "JPEG File (*.jpeg)|*.jpeg";
            _saveFileDialog.FileName = "Map";
            _saveFileDialog.FilterIndex = 2;
            _saveFileDialog.RestoreDirectory = true;
            if (_saveFileDialog.ShowDialog() == true)
            {
                RenderTargetBitmap renderTargetBitmap =
                 new RenderTargetBitmap
                 (
                     (int)GridMapView.ColumnDefinitions[1].ActualWidth + (int)GridMapView.ColumnDefinitions[2].ActualWidth + (int)GridMapView.ColumnDefinitions[3].ActualWidth + (int)GridMapView.ColumnDefinitions[4].ActualWidth + (int)GridMapView.ColumnDefinitions[5].ActualWidth + (int)GridMapView.ColumnDefinitions[6].ActualWidth + (int)GridMapView.ColumnDefinitions[7].ActualWidth,
                     (int)GridMapView.RowDefinitions[2].ActualHeight + (int)GridMapView.RowDefinitions[3].ActualHeight + (int)GridMapView.RowDefinitions[4].ActualHeight + (int)GridMapView.RowDefinitions[5].ActualHeight + (int)GridMapView.RowDefinitions[6].ActualHeight,
                     300,
                     300, 
                     PixelFormats.Pbgra32
                );
                renderTargetBitmap.Render(MainMap);
                PngBitmapEncoder pngImage = new PngBitmapEncoder();
                pngImage.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                using (Stream fileStream = File.Create(_saveFileDialog.FileName))
                {
                    pngImage.Save(fileStream);
                }
            }
        }
    }



}
