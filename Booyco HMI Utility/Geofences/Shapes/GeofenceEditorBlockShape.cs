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
    public class GeofenceEditorBlockShape : GeofenceEditorShape
    {
        public Double blockWidth; // meters
        public Double blockHeight; // meters        
        private EditableShapePoint shapeCenterPoint;
        private EditableShapePoint shapeCornerPoint;

        public GeofenceEditorBlockShape(GMapControl map, LatLonCoord center, Double blockWidth, Double blockHeight) : base(map, GeofenceEditorShapeType.Rectangle)
        {
            // set vars
            this.center = center;
            this.blockWidth = blockWidth;
            this.blockHeight = blockHeight;
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
            if(item.Name == this.Id + "Rect")
            {
                this.InvokeOnShapeClick(e);
                Console.Out.WriteLine("OnPolygonClick " + this.Id + " Rect");
            }
        }

        public override List<EditableShapePoint> BuildEditableShapePoints()
        {
            List<EditableShapePoint> result = new List<EditableShapePoint>();
            GMapOverlay overlay = this.map.Overlays.Where((o) => { return o.Id == "objects"; }).FirstOrDefault();
            if (overlay == null)
            {
                throw new Exception("GeofenceEditorBlockShape: No overlay 'objects' found on map");
            }
            // add the center
            LatLonCoord coord = center;
            shapeCenterPoint = new EditableShapePoint(EditableShapePoint.EditableShapePointType.ShapeCenter, coord, overlay);
            shapeCenterPoint.OnPositionChanged += OnVertexPositionChanged;
            result.Add(shapeCenterPoint);
            // add the edge point
            coord = LatLonCoord.FindPointAtOffSet(center, this.blockWidth / 2, this.blockHeight / 2);
            shapeCornerPoint = new EditableShapePoint(EditableShapePoint.EditableShapePointType.RectangleCorner, coord, overlay);
            shapeCornerPoint.OnPositionChanged += OnVertexPositionChanged;
            result.Add(shapeCornerPoint);

            return result;
        }

        private void OnVertexPositionChanged(EditableShapePoint thisPoint)
        {
            if (thisPoint.GetShapePointType() == EditableShapePoint.EditableShapePointType.RectangleCorner) // thing we can move
            {
                var movingPoint = thisPoint.GetCoordinate();
                LatLonCoord xCoord = new LatLonCoord(center.GetLatitude(), movingPoint.GetLongitude());
                this.blockWidth = LatLonCoord.Distance(center, xCoord) * 2;
                this.blockHeight = LatLonCoord.Distance(movingPoint, xCoord) * 2;
                RedrawPolygon();
            }
            else if (thisPoint.GetShapePointType() == EditableShapePoint.EditableShapePointType.ShapeCenter) // thing we can move
            {
                this.center = thisPoint.GetCoordinate();
                LatLonCoord newCornerCoord = LatLonCoord.FindPointAtOffSet(center, this.blockWidth / 2, this.blockHeight / 2);
                shapeCornerPoint.SetPosition(newCornerCoord, false);
                RedrawPolygon();
            }
        }

        private void RedrawPolygon()
        {
            List<PointLatLng> points = new List<PointLatLng>();
            points.Add(LatLonCoord.FindPointAtOffSet(center, this.blockWidth / 2, this.blockHeight / 2).ToPointLatLng());
            points.Add(LatLonCoord.FindPointAtOffSet(center, -this.blockWidth / 2, this.blockHeight / 2).ToPointLatLng());
            points.Add(LatLonCoord.FindPointAtOffSet(center, -this.blockWidth / 2, -this.blockHeight / 2).ToPointLatLng());
            points.Add(LatLonCoord.FindPointAtOffSet(center, this.blockWidth / 2, -this.blockHeight / 2).ToPointLatLng());

            // set maps object, new or existing
            if (mapPolygonObject == null) 
            {
                mapPolygonObject = new GMapPolygon(points, this.Id + "Rect");
                mapPolygonObject.IsHitTestVisible = true;
                polygonOverlay.Polygons.Add(mapPolygonObject);
            }
            else
            {
                mapPolygonObject.Points.Clear();
                mapPolygonObject.Points.AddRange(points);
                mapPolygonObject.IsVisible = false;
                mapPolygonObject.IsVisible = true;
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
