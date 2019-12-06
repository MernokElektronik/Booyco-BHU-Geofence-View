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
    public delegate void GeofenceEditorNotification(GeofenceEditorNotificationSeverity severity, String message);

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

        internal void LoadGeoFenceObject(GeoFenceObject editableGeoFenceData)
        {
            this.Clear(); // first clear
            foreach (GeofenceCircle circle in editableGeoFenceData.geofenceCircles)
            {
                if (circle.Type != (UInt32)GeoFenceAreaType.None)
                {
                    AddShape(new GeofenceEditorCircleShape(this.map, new LatLonCoord(circle.Latitude, circle.Longitude), circle.Radius, (GeoFenceAreaType)circle.Type, (int)circle.Heading));
                }
            }
            foreach (GeofenceBlock block in editableGeoFenceData.geofenceBlocks)
            {
                if (block.Type != (UInt32)GeoFenceAreaType.None)
                {
                    AddShape(new GeofenceEditorBlockShape(this.map, new LatLonCoord(block.Latitude, block.Longitude), block.Width, block.Length, (int)block.Heading, (GeoFenceAreaType)block.Type));
                }
            }

            List<LatLonPolygon> polygons = LatLonCoord.Triangulator.TrianglesToPolygons(editableGeoFenceData.geofenceTriangles);
            foreach (LatLonPolygon polygon in polygons)
            {
                AddShape(new GeofenceEditorPolygonShape(this.map, polygon.Points, polygon.Bearing, polygon.areaType));
            }
        }

        private void Clear()
        {
            foreach (GeofenceEditorShape shape in shapes)
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
            if (OnShapeSelectionChanged != null)
            {
                OnShapeSelectionChanged.Invoke(this.selectedShape);
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
            this.Clear();
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
                    if ((shapeType == GeofenceEditorShapeType.Polygon) && (shapePoint.GetShapePointType() == EditableShapePoint.EditableShapePointType.PolygonPoint))
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

        internal bool TrySave()
        {
            bool valid = true;
            GeoFenceObject result = new GeoFenceObject(); // build result object
            int maxTriangeCount = GlobalSharedData.GeoFenceData.geofenceTriangles.Length;
            int maxCircleCount = GlobalSharedData.GeoFenceData.geofenceCircles.Length;
            int maxBlockCount = GlobalSharedData.GeoFenceData.geofenceBlocks.Length;
            int triangeCount = 0;
            int circleCount = 0;
            int blockCount = 0;
            // check all valid
            foreach (GeofenceEditorShape shape in shapes)
            {
                if (!shape.IsValid())
                {
                    ShowError(GeofenceEditorNotificationSeverity.Error, "Not all shapes are valid, correct red shapes.");
                    valid = false;
                }
                else
                {
                    // count
                    if (valid)
                    {
                        switch (shape.GetShapeType())
                        {
                            case GeofenceEditorShapeType.Circle: { circleCount++; break; }
                            case GeofenceEditorShapeType.Polygon: { triangeCount += ((GeofenceEditorPolygonShape)shape).CountTriangles(); break; }
                            case GeofenceEditorShapeType.Rectangle: { blockCount++; break; }
                            default: { valid = false; ShowError(GeofenceEditorNotificationSeverity.Error, "System error TrySave unknown type"); break; }
                        }
                    }
                }
            }
            // check counts
            if (valid && (circleCount > maxCircleCount))
            {
                valid = false;
                ShowError(GeofenceEditorNotificationSeverity.Error, "This device only allows " + maxCircleCount + " circles, found " + circleCount);
            }
            if (valid && (triangeCount > maxTriangeCount))
            {
                valid = false;
                ShowError(GeofenceEditorNotificationSeverity.Error, "This device only allows " + maxTriangeCount + " polygon triangles, found "+triangeCount);
            }
            if (valid && (blockCount > maxBlockCount))
            {
                valid = false;
                ShowError(GeofenceEditorNotificationSeverity.Error, "This device only allows " + maxBlockCount + " blocks, found " + blockCount);
            }
            // if still valid save
            int circleIndex = 0;
            int triangleIndex = 0;
            int blockIndex = 0;
            foreach (GeofenceEditorShape shape in shapes)
            {
                switch (shape.GetShapeType())
                {
                    case GeofenceEditorShapeType.Circle: { 
                            result.geofenceCircles[circleIndex] = ((GeofenceEditorCircleShape)shape).ToGeoFenceCircle(); 
                            circleIndex++; 
                            break; 
                    }
                    case GeofenceEditorShapeType.Polygon: {
                            GeofenceEditorPolygonShape polygon = ((GeofenceEditorPolygonShape)shape);
                            List<LatLonTriangle> triangles = polygon.ToGeoFenceTriangles();
                            foreach (LatLonTriangle triangle in triangles) {
                                result.geofenceTriangles[triangleIndex] = triangle.ToGeoFenceTriangle(polygon);
                                triangleIndex++;
                            }
                            break; 
                    }
                    case GeofenceEditorShapeType.Rectangle: { 
                            result.geofenceBlocks[blockIndex] = ((GeofenceEditorBlockShape)shape).ToGeoFenceBlock(); 
                            blockIndex++; 
                            break; 
                    }
                    default: { valid = false; ShowError(GeofenceEditorNotificationSeverity.Error, "System error TrySave unknown type"); break; }
                }
            }
            // pad arrays with empty objects
            while (circleIndex < maxCircleCount - 1)
            {
                result.geofenceCircles[circleIndex] = GeofenceCircle.GetEmpty();
                circleIndex++;
            }
            while (triangleIndex < maxCircleCount - 1)
            {
                result.geofenceTriangles[triangleIndex] = GeofenceTriangle.GetEmpty();
                triangleIndex++;
            }
            while (blockIndex < maxCircleCount - 1)
            {
                result.geofenceBlocks[blockIndex] = GeofenceBlock.GetEmpty();
                blockIndex++;
            }

            return valid;
        }

        internal void SetSelectedShapeAreaType(GeoFenceAreaType areaType)
        {
            if (this.selectedShape != null)
            {
                this.selectedShape.SetAreaType(areaType);
            }
        }
    }
}
