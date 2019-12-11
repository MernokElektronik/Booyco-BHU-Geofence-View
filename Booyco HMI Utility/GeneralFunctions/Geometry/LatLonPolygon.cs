using Booyco_HMI_Utility.Geofences.Shapes;
using GMap.NET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Booyco_HMI_Utility
{
    public class LatLonPolygon
    {
        // public List<LatLonCoord> Points = new List<LatLonCoord>();
        public int Bearing = 0;
        public GeoFenceAreaType areaType = GeoFenceAreaType.None;
        private List<LatLonLineSegment> cpLines;

        public LatLonPolygon(List<LatLonLineSegment> cpLines, int bearing, GeoFenceAreaType areaType)
        {
            this.areaType = areaType;
            this.Bearing = bearing;
            this.cpLines = new List<LatLonLineSegment>();
            foreach (LatLonLineSegment line in cpLines)
            {
                this.cpLines.Add(new LatLonLineSegment(
                    new LatLonVertex(new LatLonCoord(line.A.Position.Latitude, line.A.Position.Longitude), line.A.Index),
                    new LatLonVertex(new LatLonCoord(line.B.Position.Latitude, line.B.Position.Longitude), line.B.Index)
                ));
            }
        }

        internal List<LatLonCoord> ToPoints()
        {
            return LatLonCoord.Triangulator.CreatePolyFromLines(cpLines);
        }
    }
}
