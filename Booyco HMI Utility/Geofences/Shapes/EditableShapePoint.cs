using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Booyco_HMI_Utility.Geofences.Shapes
{
    public class EditableShapePoint
    {
        public delegate void EditableShapePointPositionChanged(EditableShapePoint item);
        public delegate void EditableShapePointClicked(EditableShapePoint item, MouseEventArgs e);
        public enum EditableShapePointType { PolygonPoint, PolygonEdgeButton, ShapeCenter, CircleRadius, RectangleCorner };

        private readonly EditableShapePointType type;
        private LatLonCoord coordinate;
        private bool selected = false;
        private bool shapeSelected = false;
        public int sourceIndex = -1; // variable used to keep track of where in the source this point is used

        private readonly GMapMarker marker = null;

        public event EditableShapePointPositionChanged OnPositionChanged;
        public event EditableShapePointClicked OnClicked;

        public EditableShapePoint(EditableShapePointType type, LatLonCoord coordinate, GMapOverlay overlay)
        {
            this.type = type;
            this.coordinate = coordinate;
            this.selected = false;

            if (type == EditableShapePointType.PolygonEdgeButton)
            {
                marker = new GMarkerEdgeButton(this.coordinate.ToPointLatLng());
            }
            else if (type == EditableShapePointType.PolygonPoint)
            {
                marker = new GMarkerMovablePoint(this.coordinate.ToPointLatLng());
            }
            else if (type == EditableShapePointType.CircleRadius)
            {
                marker = new GMarkerMovablePoint(this.coordinate.ToPointLatLng());
            }
            else if (type == EditableShapePointType.ShapeCenter)
            {
                marker = new GMarkerShapeCenter(this.coordinate.ToPointLatLng());
            }
            else if (type == EditableShapePointType.RectangleCorner)
            {
                marker = new GMarkerMovablePoint(this.coordinate.ToPointLatLng());
            }
            else
            {
                marker = new GMarkerGoogle(this.coordinate.ToPointLatLng(), GMarkerGoogleType.blue_dot);
            }
            marker.IsHitTestVisible = true;

            overlay.Markers.Add(marker);
            this.CheckMarkerSelected();
        }

        public void SetSelected(bool selected)
        {
            this.selected = selected;
            this.CheckMarkerSelected();
        }

        public void SetShapeSelected(bool selected)
        {
            this.shapeSelected = selected;
            this.CheckMarkerSelected();
        }

        #pragma warning disable IDE0060 // Remove unused parameter
        internal void OnMouseMove(GMapControl map, bool mouseDown, GMapMarker markerUnderMouse, object sender, MouseEventArgs e)
        #pragma warning restore IDE0060 // Remove unused parameter
        {
            if ( ((type == EditableShapePointType.ShapeCenter) ||  (type == EditableShapePointType.PolygonPoint) || (type == EditableShapePointType.CircleRadius) || (type == EditableShapePointType.RectangleCorner)) && (marker != null))
            {
                bool isInDragMode = (marker.Equals(markerUnderMouse) && this.selected); // we can only drag points once we have selected them
                if (isInDragMode)
                {
                    this.SetPosition(LatLonCoord.FromPointLatLng(map.FromLocalToLatLng(e.X, e.Y))); // move marker to cursor latlng
                }
            }
        }

        public void SetPosition(LatLonCoord coord, bool invokeOnChanged = true)
        {
            this.coordinate = coord;
            if(this.marker != null)
            {
                this.marker.Position = coord.ToPointLatLng();
                if ((OnPositionChanged != null) && invokeOnChanged)
                {
                    OnPositionChanged.Invoke(this);
                }
            }
        }

        public void Clear(GMapOverlay overlay)
        {
            GMapMarker m = GetMarker();
            if (m != null)
            {
                overlay.Markers.Remove(m);
            }
        }

        public LatLonCoord GetCoordinate()
        {
            return coordinate;
        }

        public GMapMarker GetMarker()
        {
            return marker;
        }

        #pragma warning disable IDE0060 // Remove unused parameter
        internal void MarkerClicked(GMapMarker item, MouseEventArgs e)
        #pragma warning restore IDE0060 // Remove unused parameter
        {
            if(OnClicked != null)
            {
                OnClicked.Invoke(this, e);
            }
        }

        public EditableShapePointType GetShapePointType()
        {
            return type;
        }

        private void CheckMarkerSelected()
        {
            if(marker != null)
            {
                if (this.type == EditableShapePointType.PolygonEdgeButton) // edge buttons only visible when the shape is selected
                {
                    marker.IsVisible = this.shapeSelected;
                }
                else if ((this.type == EditableShapePointType.PolygonPoint) || (this.type == EditableShapePointType.CircleRadius) || (this.type == EditableShapePointType.RectangleCorner)) // movable points can be clicked
                {
                    ((GMarkerMovablePoint)marker).SetSelected(this.selected);
                }
                else if ((this.type == EditableShapePointType.ShapeCenter) ) 
                {
                    ((GMarkerShapeCenter)marker).SetSelected(this.selected);
                }
            }
        }

        internal bool GetSelected()
        {
            return this.selected;
        }
    }
}
