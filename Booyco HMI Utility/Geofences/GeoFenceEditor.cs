using GMap.NET.WindowsForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace Booyco_HMI_Utility.Geofences
{
    public class GeoFenceEditor
    {
        private GeofenceEditorShape selectedShape;
        private List<GeofenceEditorShape> shapes;
        private GMapControl map;

        private GMapOverlay PolygonOverlay = new GMapOverlay("polygons");
        internal readonly GMapOverlay MarkerOverlay = new GMapOverlay("objects");

        public GeoFenceEditor(GMapControl map)
        {
            this.map = map;
            this.map.Overlays.Add(PolygonOverlay);
            this.map.Overlays.Add(MarkerOverlay);
            this.shapes = new List<GeofenceEditorShape>();
        }

        public void setSelectedShape(GeofenceEditorShape selectedShape) {
            this.selectedShape = selectedShape;
            // clear old stuff
            foreach(GeofenceEditorShape shape in shapes)
            {
                shape.setSelected(false);
            }
            selectedShape.setSelected(true);
        }

        public void addShape(GeofenceEditorShape geofenceEditorShape)
        {
            this.shapes.Add(geofenceEditorShape);
        }
    }
}
