using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Windows.Forms;

namespace Booyco_HMI_Utility.Geofences
{
    public class GeoFenceEditor
    {
        private GeofenceEditorShape selectedShape;
        private List<GeofenceEditorShape> shapes;
        private GMapControl map;
        private bool mouseDown = false;

        private GMapOverlay PolygonOverlay = new GMapOverlay("polygons");
        internal readonly GMapOverlay MarkerOverlay = new GMapOverlay("objects");

        public GeoFenceEditor(GMapControl map)
        {
            this.map = map;
            this.map.Overlays.Add(PolygonOverlay);
            this.map.Overlays.Add(MarkerOverlay);
            this.shapes = new List<GeofenceEditorShape>();


            this.map.OnMarkerClick += OnMarkerClick;
            this.map.MouseMove += OnMouseMove;
            this.map.MouseDown += OnMouseDown;
            this.map.MouseUp += OnMouseUp;
        }

        void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) // only move we care about is a normal left click
            {
                foreach (GeofenceEditorShape shape in shapes)
                {
                    shape.OnMouseMove(mouseDown, sender, e);
                }
            }
        }

        void OnMouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;
        }

        void OnMouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        void OnMarkerClick(GMapMarker item, MouseEventArgs e)
        {
            foreach(GeofenceEditorShape shape in shapes)
            {
                shape.MarkerClicked(item, e);
            }
        }

        public void setSelectedShape(GeofenceEditorShape selectedShape) {
            this.selectedShape = selectedShape;
            // clear old stuff
            foreach(GeofenceEditorShape shape in shapes)
            {
                shape.SetSelected(false);
            }
            selectedShape.SetSelected(true);
        }

        public void OnShapeClick(GeofenceEditorShape item, MouseEventArgs e)
        {
            this.setSelectedShape(item);
        }

        public void addShape(GeofenceEditorShape geofenceEditorShape)
        {
            this.shapes.Add(geofenceEditorShape);
            geofenceEditorShape.OnShapeClick += OnShapeClick;
        }
    }
}
