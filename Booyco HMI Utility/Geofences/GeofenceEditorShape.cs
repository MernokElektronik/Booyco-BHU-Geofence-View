using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Booyco_HMI_Utility.Geofences.GeoFenceEditor;

namespace Booyco_HMI_Utility.Geofences
{
    public class EditableShapePoint
    {
        public enum EditableShapePointType { PolygonPoint, PolygonEdgeButton, ShapeCenter, CircleRadius, RectangleCorner };

        private EditableShapePointType type;
        private LatLonCoord coordinate;
        private bool selected;

        private GMapMarker marker;

        public EditableShapePoint(EditableShapePointType type, LatLonCoord coordinate, GMapOverlay overlay)
        {
            this.type = type;
            this.coordinate = coordinate;
            this.selected = false;

            if (type == EditableShapePointType.PolygonEdgeButton)
            {
                marker = new GMarkerArrow(this.coordinate.ToPointLatLng());
            }
            else
            {
                marker = new GMarkerGoogle(this.coordinate.ToPointLatLng(), GMarkerGoogleType.blue_dot);
            }
            marker.IsHitTestVisible = true;

            overlay.Markers.Add(marker);
        }

        public void SetSelected(bool selected)
        {
            this.selected = selected;
        }

        public void Clear()
        {

        }
    }

    interface IGeofenceEditorShapeInterface
    {
        List<EditableShapePoint> BuildEditableShapePoints();
    }
    public abstract class GeofenceEditorShape: IGeofenceEditorShapeInterface
    {
        public enum GeofenceEditorShapeType { Polygon, Rectangle, Circle };

        protected GMapControl map;
        protected GeofenceEditorShapeType type;

        protected LatLonCoord center = null;

        protected List<EditableShapePoint> editableShapePoints = null;

        protected bool selected = false;

        public GeofenceEditorShape(GMapControl map, GeofenceEditorShapeType type)
        {
            this.type = type;
            this.map = map;
        }

        public void setSelected(bool selected)
        {
            this.selected = selected;
        }

        public abstract List<EditableShapePoint> BuildEditableShapePoints();
    }

    public class GeofenceEditorPolygonShape: GeofenceEditorShape
    {
        public List<LatLonCoord> polygonCoordinates;

        public GeofenceEditorPolygonShape(GMapControl map, List<LatLonCoord> polygonCoordinates) : base(map, GeofenceEditorShapeType.Polygon)
        {
            this.polygonCoordinates = polygonCoordinates;
            this.editableShapePoints = this.BuildEditableShapePoints();
        }        

        public override List<EditableShapePoint> BuildEditableShapePoints()
        {
            List<EditableShapePoint> result = new List<EditableShapePoint>();
            GMapOverlay overlay = this.map.Overlays.Where((o) => { return o.Id == "objects"; }).FirstOrDefault();
            if(overlay == null)
            {
                throw new Exception("GeofenceEditorPolygonShape: No overlay 'objects' found on map");
            }
            // add the points
            foreach (LatLonCoord coord in polygonCoordinates)
            {
                result.Add(new EditableShapePoint(EditableShapePoint.EditableShapePointType.PolygonPoint, coord, overlay));
            }
            // add the edge buttons
            int l = polygonCoordinates.Count;
            for (var i = 0; i<l; i++)
            {
                if (i == 0)
                {
                    // last and first
                    LatLonCoord a = polygonCoordinates[l - 1];
                    LatLonCoord b = polygonCoordinates[0];
                    result.Add(new EditableShapePoint(EditableShapePoint.EditableShapePointType.PolygonEdgeButton, a.average(b), overlay)); // middle between a and b
                }
                else
                {
                    // this and previous
                    LatLonCoord a = polygonCoordinates[i];
                    LatLonCoord b = polygonCoordinates[i-1];
                    result.Add(new EditableShapePoint(EditableShapePoint.EditableShapePointType.PolygonEdgeButton, a.average(b), overlay)); // middle between a and b
                }
            }

            return result;
        }
    }
}
