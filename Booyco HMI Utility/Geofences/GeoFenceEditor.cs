using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Windows.Forms;
using Booyco_HMI_Utility.Geofences.Shapes;
using static Booyco_HMI_Utility.Geofences.GeofenceEditorShape;

namespace Booyco_HMI_Utility.Geofences
{
    public enum GeofenceEditorNotificationSeverity { Success, Notice, Warning, Error };
    public delegate void GeofenceEditorShapeSelectionChanged(GeofenceEditorShape item);
    public delegate void GeofenceEditorPointSelectionChanged(EditableShapePoint point);
    public delegate void GeofenceEditorNotification(GeofenceEditorNotificationSeverity severity, String Message);

    public class GeoFenceEditor
    {
        public static GeoFenceEditor instance = null; // singeton

        private GeofenceEditorShape selectedShape;
        private List<GeofenceEditorShape> shapes;
        private GMapControl map;
        private bool mouseDown = false;
        private GMapMarker markerUnderMouse = null;

        private GMapOverlay PolygonOverlay = new GMapOverlay("polygons");
        internal readonly GMapOverlay MarkerOverlay = new GMapOverlay("objects");

        public event GeofenceEditorShapeSelectionChanged OnShapeSelectionChanged;
        public event GeofenceEditorPointSelectionChanged OnShapePointSelectionChanged;
        public event GeofenceEditorNotification OnError;

        public GeoFenceEditor(GMapControl map)
        {
            GeoFenceEditor.instance = this; // set instance, does mean only 1 can be made
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
                SetSelectedShape(shapeForMarker);
                shapeForMarker.MarkerClicked(item, e);
            }
            else
            {
                ShowError(GeofenceEditorNotificationSeverity.Error, "Error, marker clicked that is not in a shape.");
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

        public void ShowError(GeofenceEditorNotificationSeverity severity, string message)
        {
            if(OnError != null)
            {
                OnError.Invoke(severity, message);
            }
        }

        public void SetSelectedShape(GeofenceEditorShape selectedShape) {
            this.selectedShape = selectedShape;
            // clear old stuff
            foreach(GeofenceEditorShape shape in shapes)
            {
                if (!shape.Equals(selectedShape))
                {
                    shape.SetSelected(false);
                }
            }
            if (selectedShape != null)
            {
                selectedShape.SetSelected(true);
            }
            // notify of change
            if(OnShapeSelectionChanged != null)
            {
                OnShapeSelectionChanged.Invoke(this.selectedShape);
            }
        }

        public void OnShapeClick(GeofenceEditorShape item, MouseEventArgs e)
        {
            this.SetSelectedShape(item);
        }

        public void AddShape(GeofenceEditorShape geofenceEditorShape)
        {
            this.shapes.Add(geofenceEditorShape);
            geofenceEditorShape.OnShapeClick += OnShapeClick;
        }

        internal void DeletedSelectedPoint()
        {
            if(this.selectedShape != null)
            {
                EditableShapePoint shapePoint = this.selectedShape.GetSelectedPoint();
                if(shapePoint != null)
                {
                    GeofenceEditorShapeType shapeType = this.selectedShape.GetShapeType();
                    if (shapeType == GeofenceEditorShapeType.Polygon)
                    {
                        ((GeofenceEditorPolygonShape)this.selectedShape).RemovePoint(shapePoint);
                    }
                    else
                    {
                        ShowError(GeofenceEditorNotificationSeverity.Warning, "This type of point can not be removed");
                    }
                }
            }
        }

        internal void DeleteSelectedShape()
        {
            if(this.selectedShape != null)
            {
                this.selectedShape.Delete();
                this.shapes.Remove(this.selectedShape);
                this.SetSelectedShape(null);
            }
        }

        internal void SetSelectedShapeBearing(int bearing)
        {
            if (this.selectedShape != null)
            {
                this.selectedShape.SetBearing(bearing);
                // refresh marker overlay
                this.MarkerOverlay.IsVisibile = false;
                this.MarkerOverlay.IsVisibile = true;
            }
        }
    }
}
