using GMap.NET;
using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Booyco_HMI_Utility.Geofences.Shapes
{
    public class GeofenceEditorPolygonShape : GeofenceEditorShape
    {
        public List<LatLonCoord> polygonCoordinates;

        private GMapOverlay polygonOverlay;
        private GMapPolygon mapPolygonObject = null;

        public GeofenceEditorPolygonShape(GMapControl map, List<LatLonCoord> polygonCoordinates) : base(map, GeofenceEditorShapeType.Polygon)
        {
            // set vars
            this.polygonCoordinates = polygonCoordinates;
            this.polygonOverlay = this.map.Overlays.Where((o) => { return o.Id == "polygons"; }).FirstOrDefault();
            // build
            this.editableShapePoints = this.BuildEditableShapePoints();
            RedrawPolygon();
            // events
            this.map.OnPolygonClick += OnPolygonClick;
        }

        public override void SetSelected(bool selected)
        {
            base.SetSelected(selected);
            RedrawPolygon();
        }

        private void OnPolygonClick(GMapPolygon item, MouseEventArgs e)
        {
            // check if this object is clicked
            if(item.Name == this.Id + "Polygon")
            {
                this.InvokeOnShapeClick(e);
            }
        }

        public override List<EditableShapePoint> BuildEditableShapePoints()
        {
            List<EditableShapePoint> result = new List<EditableShapePoint>();
            GMapOverlay overlay = this.map.Overlays.Where((o) => { return o.Id == "objects"; }).FirstOrDefault();
            if (overlay == null)
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
            for (var i = 0; i < l; i++)
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
                    LatLonCoord b = polygonCoordinates[i - 1];
                    result.Add(new EditableShapePoint(EditableShapePoint.EditableShapePointType.PolygonEdgeButton, a.average(b), overlay)); // middle between a and b
                }
            }

            return result;
        }

        private void RedrawPolygon()
        {
            List<PointLatLng> points = new List<PointLatLng>();
            foreach (EditableShapePoint point in this.editableShapePoints)
            {
                if (point.GetShapePointType() == EditableShapePoint.EditableShapePointType.PolygonPoint) 
                { 
                    points.Add(point.GetCoordinate().ToPointLatLng());
                }
            }            
            // set maps object, new or existing
            if (mapPolygonObject == null) 
            {
                mapPolygonObject = new GMapPolygon(points, this.Id + "Polygon");
                mapPolygonObject.IsHitTestVisible = true;
                polygonOverlay.Polygons.Add(mapPolygonObject);
            }
            else
            {
                mapPolygonObject.Points.Clear();
                mapPolygonObject.Points.AddRange(points);
            }
            // set colour
            if (this.selected)
            {
                mapPolygonObject.Stroke = new Pen(Brushes.Blue, 5);
            }
            else
            {
                mapPolygonObject.Stroke = new Pen(Brushes.Gray, 5);
            }
        }
    }
}
