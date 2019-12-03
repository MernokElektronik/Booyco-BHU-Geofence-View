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
        private EditableShapePoint centerPoint = null;

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
                Console.Out.WriteLine("OnPolygonClick " + this.Id + " Polygon");
            }
        }

        public void OnEdgeButtonClicked(EditableShapePoint item, MouseEventArgs e)
        {
            GMapOverlay overlay = this.map.Overlays.Where((o) => { return o.Id == "objects"; }).FirstOrDefault();
            int polygonVertexIndex = item.sourceIndex;
            int aVertexIndex = this.WrapIndex(polygonVertexIndex - 1);
            int bVertexIndex = this.WrapIndex(polygonVertexIndex);
            EditableShapePoint aVertex = GetShapePointbyTypeAndSourceIndex(EditableShapePoint.EditableShapePointType.PolygonPoint, aVertexIndex);
            EditableShapePoint bVertex = GetShapePointbyTypeAndSourceIndex(EditableShapePoint.EditableShapePointType.PolygonPoint, bVertexIndex);
            if((aVertex != null) && (bVertex != null) && (overlay != null))
            {
                LatLonCoord coord = aVertex.GetCoordinate().average(bVertex.GetCoordinate());
                this.polygonCoordinates.Insert(bVertexIndex, coord);
                // rebuild and redraw
                this.Clear();
                this.editableShapePoints = this.BuildEditableShapePoints();
                this.SetSelected(true);
                RedrawPolygon();
                overlay.IsVisibile = false;
                overlay.IsVisibile = true;
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

            int l = polygonCoordinates.Count;
            // add the points
            for (var i = 0; i < l; i++)
            {
                LatLonCoord vertextCoord = polygonCoordinates[i];
                EditableShapePoint vertextPoint = new EditableShapePoint(EditableShapePoint.EditableShapePointType.PolygonPoint, vertextCoord, overlay);
                vertextPoint.sourceIndex = i;
                vertextPoint.OnPositionChanged += OnVertexPositionChanged;
                result.Add(vertextPoint);
            }
            // add the edge buttons            
            for (var i = 0; i < l; i++)
            {
                if (i == 0)
                {
                    // last and first
                    LatLonCoord a = polygonCoordinates[l - 1];
                    LatLonCoord b = polygonCoordinates[0];
                    EditableShapePoint edgePoint = new EditableShapePoint(EditableShapePoint.EditableShapePointType.PolygonEdgeButton, a.average(b), overlay); // middle between a and b
                    edgePoint.OnClicked += OnEdgeButtonClicked;
                    edgePoint.sourceIndex = i;
                    result.Add(edgePoint);
                }
                else
                {
                    // this and previous
                    LatLonCoord a = polygonCoordinates[i];
                    LatLonCoord b = polygonCoordinates[i - 1];
                    EditableShapePoint edgePoint = new EditableShapePoint(EditableShapePoint.EditableShapePointType.PolygonEdgeButton, a.average(b), overlay); // middle between a and b
                    edgePoint.OnClicked += OnEdgeButtonClicked;
                    edgePoint.sourceIndex = i;
                    result.Add(edgePoint);
                }
            }
            // add the center
            LatLonCoord centerCoord = GetPolygonCenter();
            centerPoint = new EditableShapePoint(EditableShapePoint.EditableShapePointType.ShapeCenter, centerCoord, overlay);
            centerPoint.OnPositionChanged += OnVertexPositionChanged;
            result.Add(centerPoint);

            return result;
        }

        public LatLonCoord GetPolygonCenter()
        {
            int l = this.polygonCoordinates.Count;
            double LatitudeSum = 0;
            double LongitudeSum = 0;
            for (var i = 0; i < l; i++)
            {
                LatitudeSum += polygonCoordinates[i].GetLatitude();
                LongitudeSum += polygonCoordinates[i].GetLongitude();
            }
            return new LatLonCoord(LatitudeSum/l, LongitudeSum/l);
        }

        internal void RemovePoint(EditableShapePoint shapePoint)
        {
            int i = shapePoint.sourceIndex;
            if ( (i >= 0) && (shapePoint.GetShapePointType() == EditableShapePoint.EditableShapePointType.PolygonPoint) ) // item that can be removed
            {
                GMapOverlay overlay = this.map.Overlays.Where((o) => { return o.Id == "objects"; }).FirstOrDefault();
                if (overlay != null)
                {
                    if (polygonCoordinates.Count > 3)
                    {
                        polygonCoordinates.RemoveAt(i);
                        // rebuild
                        this.Clear();
                        this.editableShapePoints = this.BuildEditableShapePoints();
                        overlay.IsVisibile = false;
                        overlay.IsVisibile = true;
                        RedrawPolygon();
                    }
                    else
                    {
                        throw new Exception("Can not remove a point when there are only 3 left");
                    }
                }
            }
        }

        private void OnVertexPositionChanged(EditableShapePoint thisPoint)
        {
            if (thisPoint.GetShapePointType() == EditableShapePoint.EditableShapePointType.PolygonPoint) // only thing we can move
            {
                // move point and connected edge points
                var l = this.polygonCoordinates.Count;
                var i = thisPoint.sourceIndex;
                if ((i >= 0) && (i < l))
                {
                    this.polygonCoordinates[i] = thisPoint.GetCoordinate(); // set proper coordinate in source object
                    EditableShapePoint previousEdge = GetShapePointbyTypeAndSourceIndex(EditableShapePoint.EditableShapePointType.PolygonEdgeButton, WrapIndex(i));
                    EditableShapePoint nextEdge = GetShapePointbyTypeAndSourceIndex(EditableShapePoint.EditableShapePointType.PolygonEdgeButton, WrapIndex(i + 1));
                    EditableShapePoint previousPoint = GetShapePointbyTypeAndSourceIndex(EditableShapePoint.EditableShapePointType.PolygonPoint, WrapIndex(i - 1));
                    EditableShapePoint nextPoint = GetShapePointbyTypeAndSourceIndex(EditableShapePoint.EditableShapePointType.PolygonPoint, WrapIndex(i + 1));
                    if ((previousEdge != null) && (nextEdge != null) && (previousPoint != null) && (nextPoint != null))
                    {
                        // move next edge
                        LatLonCoord coordNext = thisPoint.GetCoordinate().average(nextPoint.GetCoordinate()); // middle between a and b
                        nextEdge.SetPosition(coordNext);
                        // move previous edge
                        LatLonCoord coordPrev = thisPoint.GetCoordinate().average(previousPoint.GetCoordinate()); // middle between a and b
                        previousEdge.SetPosition(coordPrev);
                    }
                    // move center point
                    centerPoint.SetPosition(GetPolygonCenter(), false);
                    RedrawPolygon();
                }
            } 
            else if (thisPoint.GetShapePointType() == EditableShapePoint.EditableShapePointType.ShapeCenter)
            {
                LatLonCoord newCenter = thisPoint.GetCoordinate();
                LatLonCoord oldCenter = GetPolygonCenter();
                LatLonCoord difference = newCenter.Substract(oldCenter);
                foreach (EditableShapePoint p in editableShapePoints)
                {
                    if( (p.GetShapePointType() == EditableShapePoint.EditableShapePointType.PolygonEdgeButton) || (p.GetShapePointType() == EditableShapePoint.EditableShapePointType.PolygonPoint))
                    {
                        p.SetPosition(p.GetCoordinate().AddCoordinate(difference));
                    }
                }
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

        private int WrapIndex(int i)
        {
            int l = polygonCoordinates.Count;
            while (i < 0){ i += l; }
            return i % l;
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
