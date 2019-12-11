using Booyco_HMI_Utility.Geofences;
using Booyco_HMI_Utility.Geofences.Shapes;
using GMap.NET;
using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Booyco_HMI_Utility
{
    /// <summary>
    /// Interaction logic for GeofeceMapView.xaml
    /// </summary>
    public partial class GeofenceMapView : UserControl
    {
        private GeoFenceObject editableGeoFenceData; // geoFenceData currently being edited, on accept this will be set as the Globalshaderdata.geoFenceData
        private GeoFenceEditor geoFenceEditor = null;
        public static double StartLat = -25.882784;
        public static double StartLon = 28.163630;
        private GMapControl MainMap = null;

        bool shapeSelected = false;
        bool pointSelected = false;

        public GeofenceMapView()
        {
            InitializeComponent();
        }

        public void InitGeoFenceModal()
        {
            // create components if they are not created
            if (geoFenceEditor == null)
            {
                geoFenceEditor = new GeoFenceEditor(MainMap);
                geoFenceEditor.OnShapeSelectionChanged += GeoFenceEditor_OnShapeSelectionChanged;
                geoFenceEditor.OnShapePointSelectionChanged += GeoFenceEditor_OnShapePointSelectionChanged;
                geoFenceEditor.OnError += ShowMessage;
                // init buttons
                LabelBearing.Visibility = Visibility.Collapsed;
                SliderBearing.Visibility = Visibility.Collapsed;
                RemoveShapeButton.Visibility = Visibility.Collapsed;
                RemovePointButton.Visibility = Visibility.Collapsed;
                DropdownType.Visibility = Visibility.Collapsed;
            }
            geoFenceEditor.LoadGeoFenceObject(editableGeoFenceData); // clear and reload all shapes from this object
            // set correct map parameters
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache; // set to cache only when offline
            // choose your provider here
            //MainMap.MapProvider = GMap.NET.MapProviders.OpenStreetMapProvider.Instance;
            MainMap.MapProvider = GMap.NET.MapProviders.GoogleHybridMapProvider.Instance;
            //MainMap.MapProvider = GMap.NET.MapProviders.GoogleTerrainMapProvider.Instance;
            // whole world zoom
            MainMap.MinZoom = 2;
            MainMap.MaxZoom = 21;
            MainMap.Zoom = 19;
            // lets the map use the mousewheel to zoom
            MainMap.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionWithoutCenter;
            // lets the user drag the map
            MainMap.CanDragMap = true;
            //MainMap.ShowCenter = false;
            // lets the user drag the map with the left mouse button
            MainMap.DragButton = System.Windows.Forms.MouseButtons.Left;
            MainMap.Position = new GMap.NET.PointLatLng(editableGeoFenceData.StartLatitude, editableGeoFenceData.StartLongitude);
            MainMap.ShowCenter = false;
        }

        private void ShowMessage(GeofenceEditorNotificationSeverity severity, string message)
        {
            Color c = Color.FromRgb(0, 0, 0); // black
            switch (severity)
            {
                case GeofenceEditorNotificationSeverity.Error: { c = Color.FromRgb(200, 0, 0); break; }
                case GeofenceEditorNotificationSeverity.Notice: { c = Color.FromRgb(0, 0, 200); break; }
                case GeofenceEditorNotificationSeverity.Success: { c = Color.FromRgb(0, 200, 0); break; }
                case GeofenceEditorNotificationSeverity.Warning: { c = Color.FromRgb(255, 165, 0); break; }
            }
            LabelNotification.Text = message;
            LabelNotification.Foreground = new SolidColorBrush(c);
            PulseEvent(10000, ClearMessage);
        }

        private void ClearMessage(object sender, EventArgs e)
        {
            LabelNotification.Text = "";
        }

        private void GeoFenceEditor_OnShapePointSelectionChanged(EditableShapePoint point)
        {
            pointSelected = (point != null);
            if (pointSelected)
            {
                RemovePointButton.Visibility = Visibility.Visible;
            }
            else
            {
                RemovePointButton.Visibility = Visibility.Collapsed;
            }
        }

        private void GeoFenceEditor_OnShapeSelectionChanged(GeofenceEditorShape item)
        {
            shapeSelected = (item != null);
            if (shapeSelected)
            {
                SliderBearing.Value = item.GetBearing();
                LabelBearing.Content = "Bearing: " + item.GetBearing();
                ComboBoxItem selectedItem = this.GetAreaTypeObject(item);
                if (selectedItem != null)
                {
                    DropdownType.SelectedItem = selectedItem;
                }
                else
                {
                    DropdownType.SelectedIndex = 0;
                }
                LabelBearing.Visibility = Visibility.Visible;
                SliderBearing.Visibility = Visibility.Visible;
                RemoveShapeButton.Visibility = Visibility.Visible;
                RemovePointButton.Visibility = Visibility.Visible;
                DropdownType.Visibility = Visibility.Visible;
            }
            else
            {
                SliderBearing.Value = 0;
                LabelBearing.Content = "Bearing";
                DropdownType.SelectedIndex = 0;
                LabelBearing.Visibility = Visibility.Collapsed;
                SliderBearing.Visibility = Visibility.Collapsed;
                RemoveShapeButton.Visibility = Visibility.Collapsed;
                RemovePointButton.Visibility = Visibility.Collapsed;
                DropdownType.Visibility = Visibility.Collapsed;
            }
        }

        private ComboBoxItem GetAreaTypeObject(GeofenceEditorShape item)
        {
            int areaTypeInt = (int)item.GetAreaType();
            int l = DropdownType.Items.Count;
            for (int i = 0; i < l; i++)
            {
                ComboBoxItem dropdownItem = (ComboBoxItem)DropdownType.Items.GetItemAt(i);
                if (int.Parse((string)dropdownItem.Tag) == areaTypeInt)
                {
                    return dropdownItem;
                }
            }
            return null; // default to index 0
        }

        #pragma warning disable IDE0060 // Remove unused parameter
        private void GridMapView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        #pragma warning restore IDE0060 // Remove unused parameter
        {
            if (GlobalSharedData.GeoFenceData == null)
            {
                // not initialised, load from file, throw error, up to you
                GlobalSharedData.GeoFenceData = new GeoFenceObject();
            }
            if (GridMapView.IsVisible)
            {
                this.editableGeoFenceData = GlobalSharedData.GeoFenceData.Clone();
                this.editableGeoFenceData.CalculateStartLatitudeAndLongitude();
                this.InitGeoFenceModal();
            }
        }

        private void MapPanel_Loaded(object sender, RoutedEventArgs e)
        {
            // Create the MaskedTextBox control.
            if (this.MainMap == null) // only once
            {
                this.MainMap = new GMapControl
                {
                    Dock = System.Windows.Forms.DockStyle.Fill
                };
                // Assign the MaskedTextBox control as the host control's child.
                WindowsFormHost.Child = this.MainMap;
            }
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            geoFenceEditor.SetSelectedShape(null);
            ProgramFlow.ProgramWindow = (int)ProgramFlowE.Startup;
            this.Visibility = Visibility.Collapsed;

        }

        private void RemovePointButton_Click(object sender, RoutedEventArgs e)
        {
            this.geoFenceEditor.DeletedSelectedPoint();
        }

        private void PulseEvent(int milliseconds, EventHandler OnElapsedCallback)
        {
            DispatcherTimer aTimer = new DispatcherTimer();
            aTimer.Tick += OnElapsedCallback;
            aTimer.Interval = TimeSpan.FromMilliseconds(milliseconds);
            aTimer.IsEnabled = true;            
        }

        private void AddPolygonButton_Click(object sender, RoutedEventArgs e)
        {
            double h = MainMap.ViewArea.HeightLat;
            PointLatLng m = MainMap.ViewArea.LocationMiddle;
            this.geoFenceEditor.AddShape(
                new GeofenceEditorPolygonShape(MainMap, new List<LatLonCoord>{
                    new LatLonCoord(m.Lat-(h/6), m.Lng),
                    new LatLonCoord(m.Lat+(h/6), m.Lng+(h/6)),
                    new LatLonCoord(m.Lat+(h/6), m.Lng-(h/6))
                }, 0, GeoFenceAreaType.MedSpeed)
            );
        }

        private void AddCircleButton_Click(object sender, RoutedEventArgs e)
        {
            double diagonalMeters = LatLonCoord.Distance(LatLonCoord.FromPointLatLng(MainMap.ViewArea.LocationRightBottom), LatLonCoord.FromPointLatLng(MainMap.ViewArea.LocationTopLeft));
            LatLonCoord center = new LatLonCoord(MainMap.Position.Lat, MainMap.Position.Lng);
            this.geoFenceEditor.AddShape(
                new GeofenceEditorCircleShape(MainMap, center, diagonalMeters/8, GeoFenceAreaType.MedSpeed, 0)
            );
        }

        private void AddBlockButton_Click(object sender, RoutedEventArgs e)
        {
            double diagonalMeters = LatLonCoord.Distance(LatLonCoord.FromPointLatLng(MainMap.ViewArea.LocationRightBottom), LatLonCoord.FromPointLatLng(MainMap.ViewArea.LocationTopLeft));
            LatLonCoord center = new LatLonCoord(MainMap.Position.Lat, MainMap.Position.Lng);
            this.geoFenceEditor.AddShape(
                new GeofenceEditorBlockShape(MainMap, center, diagonalMeters/4, diagonalMeters / 4, 0, GeoFenceAreaType.MedSpeed)
            );
        }

        private void RemoveShapeButton_Click(object sender, RoutedEventArgs e)
        {
            geoFenceEditor.DeleteSelectedShape();
        }

        private void SliderBearing_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int bearing = (int)Math.Round(SliderBearing.Value);
            this.geoFenceEditor.SetSelectedShapeBearing(bearing);
            LabelBearing.Content = "Bearing: " + bearing;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this.geoFenceEditor.TrySave())
            {
                this.ShowMessage(GeofenceEditorNotificationSeverity.Success, "Geofence saved successfully");
            }
        }

        private void DropdownType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GeoFenceAreaType areaType = (GeoFenceAreaType)int.Parse((string)((ComboBoxItem)DropdownType.SelectedItem).Tag); // area type is stored in the tag
            if (geoFenceEditor != null)
            {
                geoFenceEditor.SetSelectedShapeAreaType(areaType);
            }
        }
    }
}
