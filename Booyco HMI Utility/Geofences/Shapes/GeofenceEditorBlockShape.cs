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

        public GeofenceEditorBlockShape(GMapControl map, LatLonCoord center, Double blockWidth, Double blockHeight, int bearing, GeoFenceAreaType areaType) : base(map, GeofenceEditorShapeType.Rectangle)
        {
            // set vars
            this.center = center;
            this.blockWidth = blockWidth;
            this.blockHeight = blockHeight;
            this.polygonOverlay = this.map.Overlays.Where((o) => { return o.Id == "polygons"; }).FirstOrDefault();
            this.SetBearing(bearing);
            this.OnBearingChanged += GeofenceEditorBlockShape_OnBearingChanged;
            this.SetAreaType(areaType);
            // build
            this.editableShapePoints = this.BuildEditableShapePoints();
            RedrawPolygon();
        }

        private void GeofenceEditorBlockShape_OnBearingChanged(int bearing)
        {
            this.bearing = bearing;
            LatLonCoord unrotatedHandleCoord = LatLonCoord.FindPointAtOffSet(center, this.blockWidth / 2, this.blockHeight / 2);
            LatLonCoord newCornerCoord = LatLonCoord.RotatePoint(unrotatedHandleCoord, this.center, -this.bearing);
            shapeCornerPoint.SetPosition(newCornerCoord, false);
            RedrawPolygon();
        }

        public override void SetSelected(bool selected)
        {            
            base.SetSelected(selected);
            RedrawPolygon();
        }

        public override void OnPolygonClick(GMapPolygon item, MouseEventArgs e)
        {
            // check if this object is clicked
            if(item.Name == this.id + "Rect")
            {
                this.InvokeOnShapeClick(e);
                Console.Out.WriteLine("OnPolygonClick " + this.id + " Rect");
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
            shapeCenterPoint.SetBearing(this.bearing);
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
                LatLonCoord.CalcWidthHeightFromRotatedSquare(center, thisPoint.GetCoordinate(), -this.bearing, out this.blockWidth, out this.blockHeight);
                RedrawPolygon();
            }
            else if (thisPoint.GetShapePointType() == EditableShapePoint.EditableShapePointType.ShapeCenter) // thing we can move
            {
                this.center = thisPoint.GetCoordinate();
                LatLonCoord unrotatedHandleCoord = LatLonCoord.FindPointAtOffSet(center, this.blockWidth / 2, this.blockHeight / 2);
                LatLonCoord newCornerCoord = LatLonCoord.RotatePoint(unrotatedHandleCoord, this.center, -this.bearing);
                shapeCornerPoint.SetPosition(newCornerCoord, false);
                RedrawPolygon();
            }
        }

        private void RedrawPolygon()
        {
            List<PointLatLng> points = new List<PointLatLng>
            {
                LatLonCoord.RotatePoint(LatLonCoord.FindPointAtOffSet(center, this.blockWidth / 2, this.blockHeight / 2), center, -this.bearing).ToPointLatLng(),
                LatLonCoord.RotatePoint(LatLonCoord.FindPointAtOffSet(center, -this.blockWidth / 2, this.blockHeight / 2), center, -this.bearing).ToPointLatLng(),
                LatLonCoord.RotatePoint(LatLonCoord.FindPointAtOffSet(center, -this.blockWidth / 2, -this.blockHeight / 2), center, -this.bearing).ToPointLatLng(),
                LatLonCoord.RotatePoint(LatLonCoord.FindPointAtOffSet(center, this.blockWidth / 2, -this.blockHeight / 2), center, -this.bearing).ToPointLatLng()
            };

            // set maps object, new or existing
            if (mapPolygonObject == null) 
            {
                mapPolygonObject = new GMapPolygon(points, this.id + "Rect")
                {
                    IsHitTestVisible = true
                };
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
            if (!IsValid())
            {
                mapPolygonObject.Stroke = new Pen(Brushes.Red, 5);
            } 
            else if (this.selected)
            {
                mapPolygonObject.Stroke = new Pen(Brushes.Blue, 5);
            }
            else
            {
                mapPolygonObject.Stroke = new Pen(Brushes.Gray, 5);
            }
        }

        public override bool IsValid()
        {
            return ((this.blockWidth > 0) && (this.blockHeight > 0));
        }

        internal GeofenceBlock ToGeoFenceBlock()
        {
            GeofenceBlock item = new GeofenceBlock
            {
                Latitude = LatLonCoord.LatLonPartToUInt32(this.center.Latitude),
                Longitude = LatLonCoord.LatLonPartToUInt32(this.center.Longitude),
                Length = (UInt32)Math.Round(this.blockHeight),
                Width = (UInt32)Math.Round(this.blockWidth),
                Heading = (UInt32)this.bearing,
                Type = (UInt32)this.areaType
            };
            if (item.Length < 1) { item.Length = 1; }
            if (item.Width < 1) { item.Width = 1; }
            if (item.Length > 255) { item.Length = 255; }
            if (item.Width > 255) { item.Width = 255; }
            return item;
        }
    }
}
