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
    public delegate void BearingChanged(int bearing);
    public delegate void OverspeedChanged(int overspeed);
    public delegate void WarningSpeedChanged(int warningSpeed);

    interface IGeofenceEditorShapeInterface
    {
        List<EditableShapePoint> BuildEditableShapePoints();
    }
    public abstract class GeofenceEditorShape: IGeofenceEditorShapeInterface
    {
        public enum GeofenceEditorShapeType { Polygon, Rectangle, Circle };
        protected string id;
        protected GMapControl map;
        protected GeofenceEditorShapeType type;
        protected GeoFenceAreaType areaType;
        protected LatLonCoord center;
        protected List<EditableShapePoint> editableShapePoints = null;
        protected bool selected = false;
        protected GMapOverlay polygonOverlay;
        protected GMapPolygon mapPolygonObject = null;
        protected List<GMapPolygon> mapDebugPolygonObject = new List<GMapPolygon>();
        public event GeofenceEditorShapeClick OnShapeClick;
        public event BearingChanged OnBearingChanged;
        public event OverspeedChanged OnOverspeedChanged;
        public event WarningSpeedChanged OnWarningSpeedChanged;
        protected int bearing = 0;
        protected int warningSpeed = 0;
        protected int overspeed = 0;       

        private static Random random;
        private static readonly object syncObj = new object();

        public GeofenceEditorShape(GMapControl map, GeofenceEditorShapeType type)
        {
            // generate random id
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            int length = 12;
            lock (syncObj) // static random is not thread safe and needs to be locked
            {
                if (random == null)
                    random = new Random();
                this.id = new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
            }
            this.type = type;
            this.map = map;
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
            if (this.editableShapePoints != null)
            {
                foreach (EditableShapePoint point in this.editableShapePoints)
                {
                    if (point.GetShapePointType() == EditableShapePoint.EditableShapePointType.ShapeCenter)
                    {
                        GMapMarker m = point.GetMarker();
                        if (m != null)
                        {
                            ((GMarkerShapeCenter)m).SetBearing(this.bearing);
                        }
                    }
                }
            }
            if(OnBearingChanged != null)
            {
                OnBearingChanged.Invoke(this.bearing);
            }
        }

        public void SetOverspeed(int overspeed)
        {
            this.overspeed = overspeed;
            // communicate overspeed to a shape center if it exists
            if (this.editableShapePoints != null)
            {
                foreach (EditableShapePoint point in this.editableShapePoints)
                {
                    if (point.GetShapePointType() == EditableShapePoint.EditableShapePointType.ShapeCenter)
                    {
                        GMapMarker m = point.GetMarker();
                        if (m != null)
                        {
                            ((GMarkerShapeCenter)m).SetOverspeed(this.overspeed);
                        }
                    }
                }
            }
            if (OnOverspeedChanged != null)
            {
                OnOverspeedChanged.Invoke(this.overspeed);
            }
        }

        public void SetWarningSpeed(int warningSpeed)
        {
            this.warningSpeed = warningSpeed;
            // communicate warning speed to a shape center if it exists
            if (this.editableShapePoints != null)
            {
                foreach (EditableShapePoint point in this.editableShapePoints)
                {
                    if (point.GetShapePointType() == EditableShapePoint.EditableShapePointType.ShapeCenter)
                    {
                        GMapMarker m = point.GetMarker();
                        if (m != null)
                        {
                            ((GMarkerShapeCenter)m).SetWarningSpeed(this.warningSpeed);
                        }
                    }
                }
            }
            if (OnWarningSpeedChanged != null)
            {
                OnWarningSpeedChanged.Invoke(this.warningSpeed);
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

        internal int GetOverspeed()
        {
            return this.overspeed;
        }

        internal int GetWarningSpeed()
        {
            return this.warningSpeed;
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
            if(mapPolygonObject != null){
                mapPolygonObject.Dispose();
            }
            if(mapDebugPolygonObject != null)
            {
                foreach(GMapPolygon mapDebugPolygon in mapDebugPolygonObject)
                {
                    mapDebugPolygon.Dispose();
                }
            }
        }

        public abstract List<EditableShapePoint> BuildEditableShapePoints();

        public abstract bool IsValid();

        public abstract void OnPolygonClick(GMapPolygon item, MouseEventArgs e);

        internal void SetAreaType(GeoFenceAreaType areaType)
        {
            this.areaType = areaType;
        }

        internal bool HasPolygon(GMapPolygon gMapPolygon)
        {
            return ((gMapPolygon != null) && (this.mapPolygonObject != null) && (this.mapPolygonObject.Name == gMapPolygon.Name));
        }
    }
}
