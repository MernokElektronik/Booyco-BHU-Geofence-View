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
    public delegate void OnMapClickAggregation(
            List<GeoFenceEditor.MapClickAggregator.PolgygonClickData> polygonClickEvents, 
            List<GeoFenceEditor.MapClickAggregator.MarkerClickData> markerClickEvents
    );
    public delegate void GeofenceEditorPointSelectionChanged(EditableShapePoint point);
    public delegate void GeofenceEditorNotification(GeofenceEditorNotificationSeverity severity, String message);

    public class GeoFenceEditor
    {
        public static GeoFenceEditor instance = null; // singeton

        private GeofenceEditorShape selectedShape;
        private readonly List<GeofenceEditorShape> shapes;
        private readonly GMapControl map;
        private bool mouseDown = false;
        private GMapMarker markerUnderMouse = null;
        private MapClickAggregator mapClickAggregator = null;

        private readonly GMapOverlay polygonOverlay = new GMapOverlay("polygons");
        private readonly GMapOverlay markerOverlay = new GMapOverlay("objects");

        public event GeofenceEditorShapeSelectionChanged OnShapeSelectionChanged;
        public event GeofenceEditorPointSelectionChanged OnShapePointSelectionChanged;
        public event GeofenceEditorNotification OnError;

        // little class to aggregate events so we can detect if overlapping shapes and markers have been clicked
        public class MapClickAggregator
        {
            public struct PolgygonClickData
            {
                public GMapPolygon gMapPolygon;
                public MouseEventArgs mouseEventArgs;
            }

            public struct MarkerClickData
            {
                public GMapMarker gMapMarker;
                public MouseEventArgs mouseEventArgs;
            }

            public event OnMapClickAggregation OnMapClickAggregation = null;
            private List<PolgygonClickData> polygonClickEvents = new List<PolgygonClickData>();
            private List<MarkerClickData> markerClickEvents = new List<MarkerClickData>();
            private Timer postClicktimer = null;

            public void AddPolygonClickEvent(GMapPolygon gMapPolygon, MouseEventArgs mouseEventArgs)
            {
                polygonClickEvents.Add(new PolgygonClickData
                {
                    gMapPolygon = gMapPolygon,
                    mouseEventArgs = mouseEventArgs
                });
                RefreshTimer();
            }

            public void AddMarkerClickEvent(GMapMarker gMapMarker, MouseEventArgs mouseEventArgs)
            {
                markerClickEvents.Add(new MarkerClickData
                {
                    gMapMarker = gMapMarker,
                    mouseEventArgs = mouseEventArgs
                });
                RefreshTimer();
            }

            private void RefreshTimer()
            {
                if(this.postClicktimer == null)
                {
                    this.postClicktimer = new Timer
                    {
                        Interval = 50 // 50 ms
                    };
                    this.postClicktimer.Tick += new EventHandler(OnMapclickAggregationTrigger);
                    this.postClicktimer.Start();
                }
            }

            private void OnMapclickAggregationTrigger(object sender, System.EventArgs args)
            {
                this.postClicktimer.Stop();
                this.postClicktimer = null;
                // trigger
                if(this.OnMapClickAggregation != null){ this.OnMapClickAggregation.Invoke(this.polygonClickEvents, this.markerClickEvents); }
                this.polygonClickEvents.Clear();
                this.markerClickEvents.Clear();
            }
        }

        public GeoFenceEditor(GMapControl map)
        {
            GeoFenceEditor.instance = this; // set instance, does mean only 1 can be made
            this.map = map;
            this.map.Overlays.Add(polygonOverlay);
            this.map.Overlays.Add(markerOverlay);
            this.shapes = new List<GeofenceEditorShape>();
            this.map.OnMarkerClick += OnMarkerClickUiEvent;
            this.map.OnMarkerEnter += OnMarkerEnter;
            this.map.OnMarkerLeave += OnMarkerLeave;
            this.map.MouseMove += OnMouseMove;
            this.map.MouseDown += OnMouseDown;
            this.map.MouseUp += OnMouseUp;
            this.map.OnPolygonClick += OnPolygonClickUiEvent;
            this.mapClickAggregator = new MapClickAggregator();
            this.mapClickAggregator.OnMapClickAggregation += MapClickAggregator_OnMapClickAggregation;
        }

        private void MapClickAggregator_OnMapClickAggregation(List<MapClickAggregator.PolgygonClickData> polygonClickEvents, List<MapClickAggregator.MarkerClickData> markerClickEvents)
        {
            bool handled = false;
            // see if there are any of my markers, if so only execute marker events
            if (markerClickEvents.Count > 0)
            {
                if (this.selectedShape != null)
                {
                    MapClickAggregator.MarkerClickData myMarkerClickData;
                    foreach (MapClickAggregator.MarkerClickData clickdata in markerClickEvents)
                    {
                        if (this.selectedShape.HasMarker(clickdata.gMapMarker))
                        {
                            OnMarkerClick(clickdata.gMapMarker, clickdata.mouseEventArgs); // found my marker, forward click
                            handled = true;
                            break;
                        }
                    }
                }
                else
                {
                    MapClickAggregator.MarkerClickData clickdata = markerClickEvents.FirstOrDefault();
                    OnMarkerClick(clickdata.gMapMarker, clickdata.mouseEventArgs); // found my marker, forward click
                    handled = true;
                }
            }

            // see if shapes are clicked
            if ((!handled) && (polygonClickEvents.Count > 0))
            {
                // select the next polygon (first on that isnt the selected shape)
                if (this.selectedShape != null)
                {
                    foreach (MapClickAggregator.PolgygonClickData clickdata in polygonClickEvents)
                    {
                        if (!this.selectedShape.HasPolygon(clickdata.gMapPolygon))
                        {
                            OnPolygonClick(clickdata.gMapPolygon, clickdata.mouseEventArgs); // found my marker, forward click
                            handled = true;
                            break;
                        }
                    }
                }
                else
                {
                    MapClickAggregator.PolgygonClickData clickdata = polygonClickEvents.FirstOrDefault();
                    OnPolygonClick(clickdata.gMapPolygon, clickdata.mouseEventArgs); // found my marker, forward click
                    handled = true;
                }
            }
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
                    AddShape(new GeofenceEditorCircleShape(this.map, new LatLonCoord(LatLonCoord.LatLonPartFromUInt32(circle.Latitude), LatLonCoord.LatLonPartFromUInt32(circle.Longitude)), circle.Radius, (GeoFenceAreaType)circle.Type, (int)circle.Heading, (int)circle.Overspeed, (int)circle.WarningSpeed));
                }
            }
            foreach (GeofenceBlock block in editableGeoFenceData.geofenceBlocks)
            {
                if (block.Type != (UInt32)GeoFenceAreaType.None)
                {
                    AddShape(new GeofenceEditorBlockShape(this.map, new LatLonCoord(LatLonCoord.LatLonPartFromUInt32(block.Latitude), LatLonCoord.LatLonPartFromUInt32(block.Longitude)), block.Width, block.Length, (int)block.Heading, (GeoFenceAreaType)block.Type));
                }
            }

            List<LatLonPolygon> polygons = LatLonCoord.Triangulator.TrianglesToPolygons(editableGeoFenceData.geofenceTriangles);
            foreach (LatLonPolygon polygon in polygons)
            {
               AddShape(new GeofenceEditorPolygonShape(this.map, polygon.ToPoints(), polygon.Bearing, polygon.Overspeed, polygon.WarningSpeed, polygon.areaType));
            }
        }

        private void ClearSelection()
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

        public void Clear()
        {
            foreach (GeofenceEditorShape shape in shapes)
            {
                shape.Clear();
            }
            shapes.Clear();
            if (selectedShape != null)
            {
                selectedShape.SetSelected(true);
            }
            // notify of change
            if (OnShapeSelectionChanged != null)
            {
                OnShapeSelectionChanged.Invoke(this.selectedShape);
            }
            // redraw
            if (polygonOverlay != null) { polygonOverlay.Clear(); }
            if (markerOverlay != null) { markerOverlay.Clear(); }
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

        void OnMarkerClickUiEvent(GMapMarker item, MouseEventArgs e)
        {
            this.mapClickAggregator.AddMarkerClickEvent(item, e);
        }

        void OnMarkerEnter(GMapMarker item)
        {
            if (selectedShape != null)
            {
                if (!selectedShape.HasMarker(markerUnderMouse)) // see if only marker under mouse still exists
                {
                    markerUnderMouse = null;
                }
                if (markerUnderMouse == null)
                {
                    markerUnderMouse = item;
                }
            }
            else
            {
                markerUnderMouse = item;
            }            
        }

        void OnMarkerLeave(GMapMarker item)
        {
            if (item.Equals(markerUnderMouse))
            {
                markerUnderMouse = null;
            }
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
            this.ClearSelection();
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

        private void OnPolygonClick(GMapPolygon item, MouseEventArgs e)
        {
            foreach(GeofenceEditorShape shape in shapes)
            {
                shape.OnPolygonClick(item, e);
            }
        }

        private void OnPolygonClickUiEvent(GMapPolygon item, MouseEventArgs e)
        {
            this.mapClickAggregator.AddPolygonClickEvent(item, e);
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
                this.selectedShape.Clear();
                this.shapes.Remove(this.selectedShape);
                this.SetSelectedShape(null);
            }
        }

        internal void SetSelectedShapeOverspeed(int overspeed)
        {
            if (this.selectedShape != null)
            {
                this.selectedShape.SetOverspeed(overspeed);
                // refresh marker overlay
                this.markerOverlay.IsVisibile = false;
                this.markerOverlay.IsVisibile = true;
            }
        }

        internal void SetSelectedShapeWarningSpeed(int warningSpeed)
        {
            if (this.selectedShape != null)
            {
                this.selectedShape.SetWarningSpeed(warningSpeed);
                // refresh marker overlay
                this.markerOverlay.IsVisibile = false;
                this.markerOverlay.IsVisibile = true;
            }
        }

        internal void SetSelectedShapeBearing(int bearing)
        {
            if (this.selectedShape != null)
            {
                this.selectedShape.SetBearing(bearing);
                // refresh marker overlay
                this.markerOverlay.IsVisibile = false;
                this.markerOverlay.IsVisibile = true;
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
            if (valid)
            {
                // if still valid save
                int circleIndex = 0;
                int triangleIndex = 0;
                int blockIndex = 0;
                foreach (GeofenceEditorShape shape in shapes)
                {
                    switch (shape.GetShapeType())
                    {
                        case GeofenceEditorShapeType.Circle:
                            {
                                result.geofenceCircles[circleIndex] = ((GeofenceEditorCircleShape)shape).ToGeoFenceCircle();
                                circleIndex++;
                                break;
                            }
                        case GeofenceEditorShapeType.Polygon:
                            {
                                GeofenceEditorPolygonShape polygon = ((GeofenceEditorPolygonShape)shape);
                                List<LatLonTriangle> triangles = polygon.ToGeoFenceTriangles();
                                foreach (LatLonTriangle triangle in triangles)
                                {
                                    result.geofenceTriangles[triangleIndex] = triangle.ToGeoFenceTriangle(polygon);
                                    triangleIndex++;
                                }
                                break;
                            }
                        case GeofenceEditorShapeType.Rectangle:
                            {
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
                while (triangleIndex < maxTriangeCount - 1)
                {
                    result.geofenceTriangles[triangleIndex] = GeofenceTriangle.GetEmpty();
                    triangleIndex++;
                }
                while (blockIndex < maxBlockCount - 1)
                {
                    result.geofenceBlocks[blockIndex] = GeofenceBlock.GetEmpty();
                    blockIndex++;
                }
            }            

            if (valid)
            {
                // finally push temp object clone into global
                GlobalSharedData.GeoFenceData = result.Clone();
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
