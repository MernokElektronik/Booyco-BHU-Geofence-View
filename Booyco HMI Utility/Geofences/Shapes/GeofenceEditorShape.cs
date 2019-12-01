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

        protected LatLonCoord center = null;

        protected List<EditableShapePoint> editableShapePoints = null;

        protected bool selected = false;

        public event GeofenceEditorShapeClick OnShapeClick;

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

        internal void OnMouseMove(bool mouseDown, object sender, MouseEventArgs e)
        {
            foreach (EditableShapePoint point in this.editableShapePoints)
            {
                point.OnMouseMove(this.map, mouseDown, sender, e);
            }
        }

        internal void MarkerClicked(GMapMarker item, MouseEventArgs e)
        {
            bool myMarkerClicked = false;
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

        public virtual void SetSelected(bool selected)
        {
            this.selected = selected;
            foreach(EditableShapePoint point in this.editableShapePoints)
            {
                point.SetShapeSelected(this.selected);
            }
        }

        public void InvokeOnShapeClick(MouseEventArgs e)
        {
            OnShapeClick.Invoke(this, e);
        }

        public abstract List<EditableShapePoint> BuildEditableShapePoints();
    }
}
