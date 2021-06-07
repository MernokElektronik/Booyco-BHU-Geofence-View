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
    /// <summary>
    /// A basic triangle structure that holds the three vertices that make up a given triangle.
    /// </summary>
    public struct LatLonTriangle
    {
        public LatLonVertex A;
        public LatLonVertex B;
        public LatLonVertex C;

        public LatLonVertex this[int index] {
            get
            {
                switch (index)
                {
                    case 0: return this.A;
                    case 1: return this.B;
                    case 2: return this.C;
                    default: throw new Exception("Tried to access out of bound index");
                }
            }
            set
            {
                switch (index)
                {
                    case 0: { this.A = value; break; };
                    case 1: { this.B = value; break; };
                    case 2: { this.C = value; break; };
                    default: throw new Exception("Tried to access out of bound index");
                }
            }
        }

        public LatLonTriangle(LatLonVertex a, LatLonVertex b, LatLonVertex c)
        {
            A = a;
            B = b;
            C = c;
        }

        public bool ContainsPoint(LatLonVertex point)
        {
            //return true if the point to test is one of the vertices
            if (point.Equals(A) || point.Equals(B) || point.Equals(C))
                return true;

            bool oddNodes = false;

            if (checkPointToSegment(C, A, point))
                oddNodes = !oddNodes;
            if (checkPointToSegment(A, B, point))
                oddNodes = !oddNodes;
            if (checkPointToSegment(B, C, point))
                oddNodes = !oddNodes;

            return oddNodes;
        }

        public static bool ContainsPoint(LatLonVertex a, LatLonVertex b, LatLonVertex c, LatLonVertex point)
        {
            return new LatLonTriangle(a, b, c).ContainsPoint(point);
        }

        static bool checkPointToSegment(LatLonVertex sA, LatLonVertex sB, LatLonVertex point)
        {
            if ((sA.Position.Latitude < point.Position.Latitude && sB.Position.Latitude >= point.Position.Latitude) ||
                (sB.Position.Latitude < point.Position.Latitude && sA.Position.Latitude >= point.Position.Latitude))
            {
                double x =
                    sA.Position.Longitude +
                    (point.Position.Latitude - sA.Position.Latitude) /
                    (sB.Position.Latitude - sA.Position.Latitude) *
                    (sB.Position.Longitude - sA.Position.Longitude);

                if (x < point.Position.Longitude)
                    return true;
            }

            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(LatLonTriangle))
                return false;
            return Equals((LatLonTriangle)obj);
        }

        public bool Equals(LatLonTriangle obj)
        {
            return obj.A.Equals(A) && obj.B.Equals(B) && obj.C.Equals(C);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = A.GetHashCode();
                result = (result * 397) ^ B.GetHashCode();
                result = (result * 397) ^ C.GetHashCode();
                return result;
            }
        }

        internal GeofenceTriangle ToGeoFenceTriangle(GeofenceEditorPolygonShape polygon)
        {
            GeofenceTriangle item = new GeofenceTriangle
            {
                LatitudePoint1 = LatLonCoord.LatLonPartToUInt32(this.A.Position.Latitude),
                LongitudePoint1 = LatLonCoord.LatLonPartToUInt32(this.A.Position.Longitude),
                LatitudePoint2 = LatLonCoord.LatLonPartToUInt32(this.B.Position.Latitude),
                LongitudePoint2 = LatLonCoord.LatLonPartToUInt32(this.B.Position.Longitude),
                LatitudePoint3 = LatLonCoord.LatLonPartToUInt32(this.C.Position.Latitude),
                LongitudePoint3 = LatLonCoord.LatLonPartToUInt32(this.C.Position.Longitude),
                Heading = (UInt32)polygon.GetBearing(),
                Type = (UInt32)polygon.GetAreaType(),
                Overspeed = (UInt32)polygon.GetOverspeed(),
                WarningSpeed = (UInt32)polygon.GetWarningSpeed()
            };
            return item;
        }
    }
}
