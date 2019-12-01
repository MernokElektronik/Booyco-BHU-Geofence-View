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
        public enum EditableShapePointType { PolygonPoint, PolygonEdgeButton, ShapeCenter, CircleRadius, RectangleCorner };

        private EditableShapePointType type;
        private LatLonCoord coordinate;
        private bool selected = false;
        private bool shapeSelected = false;

        private GMapMarker marker = null;

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

        internal void OnMouseMove(GMapControl map, bool mouseDown, object sender, MouseEventArgs e)
        {
            if (type == EditableShapePointType.PolygonPoint)
            {
                bool isInDragMode = ((GMarkerMovablePoint)marker).GetDragMode();
                if (isInDragMode)
                {
                    this.SetPosition(map.FromLocalToLatLng(e.X, e.Y));
                }
            }
        }

        public void Clear()
        {

        }

        public LatLonCoord GetCoordinate()
        {
            return coordinate;
        }

        public GMapMarker GetMarker()
        {
            return marker;
        }

        internal void MarkerClicked(GMapMarker item, MouseEventArgs e)
        {
            // marker has been clicked
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
    }
}
