using Booyco_HMI_Utility.Geofences;
using Booyco_HMI_Utility.Geofences.Shapes;
using GMap.NET;
using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.Drawing;
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

namespace Booyco_HMI_Utility
{
    /// <summary>
    /// Interaction logic for GeofeceMapView.xaml
    /// </summary>
    public partial class GeofenceMapView : UserControl
    {
        private GeoFenceObject editableGeoFenceData; // geoFenceData currently being edited, on accept this will be set as the Globalshaderdata.geoFenceData
        private GeoFenceEditor geoFenceEditor = null;
        public double StartLat = -25.882784;
        public double StartLon = 28.163630;
        private GMapControl MainMap = null;

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
            }

            // set correct map parameters

            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;

            // choose your provider here
            //MainMap.MapProvider = GMap.NET.MapProviders.OpenStreetMapProvider.Instance;
            MainMap.MapProvider = GMap.NET.MapProviders.GoogleHybridMapProvider.Instance;
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
            MainMap.DragButton = System.Windows.Forms.MouseButtons.Left;

            MainMap.Position = new GMap.NET.PointLatLng(editableGeoFenceData.Latitude, editableGeoFenceData.Longitude);
            MainMap.ShowCenter = false;

            /*GMapOverlay polygons = new GMapOverlay("polygons");
            List<PointLatLng> points = new List<PointLatLng>();
            points.Add(new PointLatLng(editableGeoFenceData.Latitude-0.0001, editableGeoFenceData.Longitude - 0.0001));
            points.Add(new PointLatLng(editableGeoFenceData.Latitude + 0.0001, editableGeoFenceData.Longitude - 0.0001));
            points.Add(new PointLatLng(editableGeoFenceData.Latitude + 0.0001, editableGeoFenceData.Longitude + 0.0001));
            points.Add(new PointLatLng(editableGeoFenceData.Latitude - 0.0001, editableGeoFenceData.Longitude + 0.0001));
            GMapPolygon polygon = new GMapPolygon(points, "Area");
            polygons.Polygons.Add(polygon);
            MainMap.overlo*/

        }

        // public override void OnMapRender(Graphics g)
        // {
            //int r = (int)((Radius) / Overlay.Control.MapProvider.Projection.GetGroundResolution((int)Overlay.Control.Zoom, Position.Lat)) * 2;

            //if (IsFilled)
            //{
            //    g.FillEllipse(Fill, new Rectangle(LocalPosition.X - r / 2, LocalPosition.Y - r / 2, r, r));
            //}
            //g.DrawEllipse(Stroke, new Rectangle(LocalPosition.X - r / 2, LocalPosition.Y - r / 2, r, r));
        // }

        private void GridMapView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (GlobalSharedData.geoFenceData == null)
            {
                // not initialised, load from file, throw error, up to you
                GlobalSharedData.geoFenceData = new GeoFenceObject();
                GlobalSharedData.geoFenceData.Latitude = StartLat;
                GlobalSharedData.geoFenceData.Longitude = StartLon;
            }
            this.editableGeoFenceData = GlobalSharedData.geoFenceData.Clone();
            this.InitGeoFenceModal();
        }

        private void AddPolygonButton_Click(object sender, RoutedEventArgs e)
        {
            LatLonCoord center = new LatLonCoord(MainMap.Position.Lat, MainMap.Position.Lng);
            double w = MainMap.ViewArea.WidthLng;
            double h = MainMap.ViewArea.HeightLat;
            PointLatLng m = MainMap.ViewArea.LocationMiddle;

            this.geoFenceEditor.addShape(
                new GeofenceEditorPolygonShape(MainMap, new List<LatLonCoord>{
                    new LatLonCoord(m.Lat-(h/6), m.Lng),
                    new LatLonCoord(m.Lat+(h/6), m.Lng+(h/6)),
                    new LatLonCoord(m.Lat+(h/6), m.Lng-(h/6))
                })
            );
        }

        private void MapPanel_Loaded(object sender, RoutedEventArgs e)
        {
            // Create the MaskedTextBox control.
            if (this.MainMap == null) // only once
            {
                this.MainMap = new GMapControl();
                this.MainMap.Dock = System.Windows.Forms.DockStyle.Fill;
                // Assign the MaskedTextBox control as the host control's child.
                WindowsFormHost.Child = this.MainMap;
            }
        }
    }
}
