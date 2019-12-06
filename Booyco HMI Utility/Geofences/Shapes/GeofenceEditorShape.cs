using Booyco_HMI_Utility.Geofences.Shapes;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Booyco_HMI_Utility.Geofences.GeoFenceEditor;

namespace Booyco_HMI_Utility.Geofences
{
    public delegate void GeofenceEditorShapeClick(GeofenceEditorShape item, MouseEventArgs e);
    interface IGeofenceEditorShapeInterface
    {
        List<EditableShapePoint> BuildEditableShapePoints();
    }
    public abstract class GeofenceEditorShape: IGeofenceEditorShapeInterface
    {
        public enum GeofenceEditorShapeType { Polygon, Rectangle, Circle };
        protected string Id;
        protected GMapControl map;
        protected GeofenceEditorShapeType type;
        protected GeoFenceAreaType areaType;
        protected LatLonCoord center;
        protected List<EditableShapePoint> editableShapePoints = null;
        protected bool selected = false;
        protected GMapOverlay polygonOverlay;
        protected GMapPolygon mapPolygonObject = null;
        public event GeofenceEditorShapeClick OnShapeClick;
        protected int bearing = 0;

        public GeofenceEditorShape(GMapControl map, GeofenceEditorShapeType type)
        {
            this.type = type;
            this.map = map;
            // generate random id
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            int length = 12;
            Random random = new Random();
            this.Id = new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        internal void OnMouseMove(bool mouseDown, GMapMarker markerUnderMouse, object sender, MouseEventArgs e)
        {
            foreach (EditableShapePoint point in this.editableShapePoints)
            {
                point.OnMouseMove(this.map, mouseDown, markerUnderMouse, sender, e);
            }
        }

        internal void MarkerClicked(GMapMarker item, MouseEventArgs e)
        {
            foreach (EditableShapePoint point in this.editableShapePoints)
            {
                GMapMarker m = point.GetMarker();
                if ((m != null) && (m.Equals(item)))
                {
                    this.SetSelected(true); // clicking a marker for this shape also selects this shape
                    point.SetSelected(true);
                    point.MarkerClicked(item, e);
                }
                else
                {
                    point.SetSelected(false);
                }
            }
        }

        internal bool HasMarker(GMapMarker item)
        {
            foreach (EditableShapePoint point in this.editableShapePoints)
            {
                GMapMarker m = point.GetMarker();
                if (m.Equals(item))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual void SetSelected(bool selected)
        {
            bool oldSelected = this.selected;
            this.selected = selected;
            Console.Out.WriteLine("Shape: SetSelected " + this.Id + " Selected: " + selected);
            foreach (EditableShapePoint point in this.editableShapePoints)
            {
                if (oldSelected && (oldSelected == this.selected))
                {
                    // selected the shape that is already selected does not deselect any points
                }
                else
                {
                    point.SetSelected(false);
                }
                point.SetShapeSelected(this.selected);
            }
        }

        public void SetBearing(int bearing)
        {
            this.bearing = bearing;
            // communicate bearing to a shape center if it exists
            foreach (EditableShapePoint point in this.editableShapePoints)
            {
                if(point.GetShapePointType() == EditableShapePoint.EditableShapePointType.ShapeCenter)
                {
                    GMapMarker m = point.GetMarker();
                    if (m != null) {
                        ((GMarkerShapeCenter)m).SetBearing(this.bearing);
                    }
                }
            }
        }

        public void InvokeOnShapeClick(MouseEventArgs e)
        {
            if (OnShapeClick != null)
            {
                OnShapeClick.Invoke(this, e);
            }
        }

        internal EditableShapePoint GetSelectedPoint()
        {
            foreach (EditableShapePoint point in this.editableShapePoints)
            {
                if (point.GetSelected())
                {
                    return point;
                }
            }
            return null;
        }

        internal int GetBearing()
        {
            return this.bearing;
        }

        public GeofenceEditorShapeType GetShapeType()
        {
            return this.type;
        }

        internal GeoFenceAreaType GetAreaType()
        {
            return this.areaType;
        }

        public void Clear()
        {
            GMapOverlay overlay = this.map.Overlays.Where((o) => { return o.Id == "objects"; }).FirstOrDefault();
            if (overlay != null)
            {
                foreach (EditableShapePoint point in this.editableShapePoints)
                {
                    point.Clear(overlay);
                }
            }
        }

        internal void Delete()
        {
            Clear();
            mapPolygonObject.Dispose();
        }

        public abstract List<EditableShapePoint> BuildEditableShapePoints();

        public abstract bool IsValid();

        internal void SetAreaType(GeoFenceAreaType areaType)
        {
            this.areaType = areaType;
        }
    }
}
