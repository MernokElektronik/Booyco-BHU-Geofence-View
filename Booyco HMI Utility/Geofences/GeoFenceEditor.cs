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
        private GMapMarker markerUnderMouse = null;

        private GMapOverlay PolygonOverlay = new GMapOverlay("polygons");
        internal readonly GMapOverlay MarkerOverlay = new GMapOverlay("objects");

        public GeoFenceEditor(GMapControl map)
        {
            this.map = map;
            this.map.Overlays.Add(PolygonOverlay);
            this.map.Overlays.Add(MarkerOverlay);
            this.shapes = new List<GeofenceEditorShape>();


            this.map.OnMarkerClick += OnMarkerClick;
            this.map.OnMarkerEnter += OnMarkerEnter;
            this.map.OnMarkerLeave += OnMarkerLeave;
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
                    shape.OnMouseMove(mouseDown, markerUnderMouse, sender, e);
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
            GeofenceEditorShape shapeForMarker = null;
            foreach (GeofenceEditorShape shape in shapes)
            {
                if (shape.HasMarker(item))
                {
                    shapeForMarker = shape;
                    break;
                }
            }
            if (shapeForMarker != null)
            {
                setSelectedShape(shapeForMarker);
                shapeForMarker.MarkerClicked(item, e);
            }
            else
            {
                throw new Exception("OnMarkerClick error, marker clicked that is not in a shape.");
            }
        }

        void OnMarkerEnter(GMapMarker item)
        {
            markerUnderMouse = item;
        }

        void OnMarkerLeave(GMapMarker item)
        {
            markerUnderMouse = null;
        }

        public void setSelectedShape(GeofenceEditorShape selectedShape) {
            this.selectedShape = selectedShape;
            // clear old stuff
            foreach(GeofenceEditorShape shape in shapes)
            {
                if (!shape.Equals(selectedShape))
                {
                    shape.SetSelected(false);
                }
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
