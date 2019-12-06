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
    public class GeofenceEditorCircleShape : GeofenceEditorShape
    {
        public Double radiusMeters; // meters
        private EditableShapePoint shapeCenterPoint;
        private EditableShapePoint shapeRadiusPoint;

        public GeofenceEditorCircleShape(GMapControl map, LatLonCoord center, Double radiusMeters, GeoFenceAreaType areaType, int bearing) : base(map, GeofenceEditorShapeType.Circle)
        {
            // set vars
            this.center = center;
            this.radiusMeters = radiusMeters;
            this.polygonOverlay = this.map.Overlays.Where((o) => { return o.Id == "polygons"; }).FirstOrDefault();
            this.SetBearing(bearing);
            this.SetAreaType(areaType);
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
            if(item.Name == this.Id + "Circle")
            {
                this.InvokeOnShapeClick(e);
                Console.Out.WriteLine("OnPolygonClick " + this.Id + " Circle");
            }
        }

        public override List<EditableShapePoint> BuildEditableShapePoints()
        {
            List<EditableShapePoint> result = new List<EditableShapePoint>();
            GMapOverlay overlay = this.map.Overlays.Where((o) => { return o.Id == "objects"; }).FirstOrDefault();
            if (overlay == null)
            {
                throw new Exception("GeofenceEditorCircleShape: No overlay 'objects' found on map");
            }
            // add the center
            var coord = center;
            shapeCenterPoint = new EditableShapePoint(EditableShapePoint.EditableShapePointType.ShapeCenter, coord, overlay);
            shapeCenterPoint.OnPositionChanged += OnVertexPositionChanged;
            result.Add(shapeCenterPoint);
            // add the radius
            coord = LatLonCoord.FindPointAtDistanceFrom(center, 0, radiusMeters);
            shapeRadiusPoint = new EditableShapePoint(EditableShapePoint.EditableShapePointType.CircleRadius, coord, overlay);
            shapeRadiusPoint.OnPositionChanged += OnVertexPositionChanged;
            result.Add(shapeRadiusPoint);

            return result;
        }

        private void OnVertexPositionChanged(EditableShapePoint thisPoint)
        {
            if (thisPoint.GetShapePointType() == EditableShapePoint.EditableShapePointType.CircleRadius) // only thing we can move
            {
                var movingPoint = thisPoint.GetCoordinate();
                this.radiusMeters = LatLonCoord.Distance(center, movingPoint);
                RedrawPolygon();
            }
            else if (thisPoint.GetShapePointType() == EditableShapePoint.EditableShapePointType.ShapeCenter) // only thing we can move
            {
                this.center = thisPoint.GetCoordinate();
                shapeRadiusPoint.SetPosition(LatLonCoord.FindPointAtDistanceFrom(center, 0, radiusMeters), false);
                RedrawPolygon();
            }
        }

        private EditableShapePoint GetShapePointbyTypeAndSourceIndex(EditableShapePoint.EditableShapePointType type, int sourceIndex)
        {
            foreach (EditableShapePoint p in editableShapePoints)
            {
                if( (type == p.GetShapePointType()) && (p.sourceIndex == sourceIndex))
                {
                    return p;
                }
            }
            return null;
        }

        private List<PointLatLng> CreateCircle(LatLonCoord coord, double radiusMeters)
        {
            int segmentCount = 50;
            double radStep = (Math.PI * 2) / segmentCount;
            List<PointLatLng> gPointList = new List<PointLatLng>();
            for (int i = 0; i < segmentCount; i++)
            {
                gPointList.Add(LatLonCoord.FindPointAtDistanceFrom(coord, i * radStep, radiusMeters).ToPointLatLng());
            }
            return gPointList;
        }

        private void RedrawPolygon()
        {
            List<PointLatLng> points = CreateCircle(this.center, radiusMeters);
            // set maps object, new or existing
            if (mapPolygonObject == null) 
            {
                mapPolygonObject = new GMapPolygon(points, this.Id + "Circle");
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
            if(!this.IsValid())
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
            return (this.radiusMeters > 0);
        }

        internal GeofenceCircle ToGeoFenceCircle()
        {
            GeofenceCircle item = new GeofenceCircle
            {
                Latitude = LatLonCoord.LatLonPartToUInt32(this.center.Latitude),
                Longitude = LatLonCoord.LatLonPartToUInt32(this.center.Longitude),
                Heading = (UInt32)this.bearing,
                Radius = (UInt32)Math.Round(this.radiusMeters),
                Type = (UInt32)this.areaType
            };
            if (item.Radius < 1) { item.Radius = 1; }
            return item;
        }
    }
}
