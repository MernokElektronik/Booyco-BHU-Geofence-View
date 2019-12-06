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
    public enum GeoFenceAreaType
    {
        [Description("None")]
        None = 0,
        [Description("Low Speed")]
        LowSpeed = 1,
        [Description("High Speed")]
        HighSpeed = 2,
        [Description("Med Speed")]
        MedSpeed = 3,
        [Description("No Go Zone")]
        NoGo = 6,
        [Description("Geofence Active")]
        GeofenceActive = 255
    }

    /// <summary>
	/// Specifies a desired winding order for the shape vertices.
	/// </summary>
	public enum WindingOrder
    {
        Clockwise,
        CounterClockwise
    }

    public interface IGeofenceShape { }

    public class GeofenceCircle: IGeofenceShape
    {
        public UInt32 Latitude;
        public UInt32 Longitude;
        public UInt32 Radius;
        public UInt32 Heading;
        public UInt32 Type;

        internal static GeofenceCircle GetEmpty()
        {
            return new GeofenceCircle
            {
                Latitude = 0,
                Longitude = 0,
                Radius = 0,
                Heading = 0,
                Type = (UInt32)GeoFenceAreaType.None
            };
        }
    }

    public class GeofenceTriangle: IGeofenceShape
    {
        public UInt32 LatitudePoint1;
        public UInt32 LongitudePoint1;
        public UInt32 LatitudePoint2;
        public UInt32 LongitudePoint2;
        public UInt32 LatitudePoint3;
        public UInt32 LongitudePoint3;
        public UInt32 Heading;
        public UInt32 Type;
        internal static GeofenceTriangle GetEmpty()
        {
            return new GeofenceTriangle
            {
                LatitudePoint1 = 0,
                LongitudePoint1 = 0,
                LatitudePoint2 = 0,
                LongitudePoint2 = 0,
                LatitudePoint3 = 0,
                LongitudePoint3 = 0,
                Heading = 0,
                Type = (UInt32)GeoFenceAreaType.None
            };
        }
    }

    public class GeofenceBlock: IGeofenceShape
    {
        public UInt32 Latitude;
        public UInt32 Longitude;
        public UInt32 Width;
        public UInt32 Length;
        public UInt32 Heading;
        public UInt32 Type;
        internal static GeofenceBlock GetEmpty()
        {
            return new GeofenceBlock
            {
                Latitude = 0,
                Longitude = 0,
                Length = 0,
                Width = 0,
                Heading = 0,
                Type = (UInt32)GeoFenceAreaType.None
            };
        }
    }

    public class GeoFenceObject
    {
        public GeofenceCircle[] geofenceCircles = new GeofenceCircle[30]; // 30 Circles
        public GeofenceTriangle[] geofenceTriangles = new GeofenceTriangle[30]; // 20 Triangles
        public GeofenceBlock[] geofenceBlocks = new GeofenceBlock[30]; // 1 Block
        public double StartLatitude;
        public double StartLongitude;

        /// <summary>
        /// Makes a deep copy of this object
        /// </summary>
        /// <returns></returns>
        public GeoFenceObject Clone()
        {
            var o = new GeoFenceObject
            {
                StartLatitude = this.StartLatitude,
                StartLongitude = this.StartLongitude
            };
            return o;
        }

        public void CalculateStartLatitudeAndLongitude()
        {
            double latSum = 0;
            double longSum = 0;
            int objCount = 0;
            foreach (GeofenceCircle circle in geofenceCircles)
            {
                if (circle != null)
                {
                    latSum += circle.Latitude;
                    longSum += circle.Longitude;
                    objCount++;
                }
            }
            foreach (GeofenceTriangle triangle in geofenceTriangles)
            {
                if (triangle != null)
                {
                    latSum += triangle.LatitudePoint1 + triangle.LatitudePoint2 + triangle.LatitudePoint3;
                    longSum += triangle.LongitudePoint1 + triangle.LongitudePoint2 + triangle.LongitudePoint3;
                    objCount += 3;
                }
            }
            foreach (GeofenceBlock block in geofenceBlocks)
            {
                if (block != null)
                {
                    latSum += block.Latitude;
                    longSum += block.Longitude;
                    objCount++;
                }
            }
            // average
            if(objCount > 0)
            {
                this.StartLatitude = latSum / objCount;
                this.StartLongitude = longSum / objCount;
            }
            else // otherwise revert to default
            {
                this.StartLatitude = GeofenceMapView.StartLat;
                this.StartLongitude = GeofenceMapView.StartLon;
            }
        } 
    }

    public static class MathHelper
    {
        /// <summary>
        /// Clamping a value to be sure it lies between two values
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="aValue"></param>
        /// <param name="aMax"></param>
        /// <param name="aMin"></param>
        /// <returns></returns>
        public static T Clamp<T>(T aValue, T aMin, T aMax) where T : IComparable<T>
        {
            var _Result = aValue;
            if (aValue.CompareTo(aMax) > 0)
                _Result = aMax;
            else if (aValue.CompareTo(aMin) < 0)
                _Result = aMin;
            return _Result;
        }

    }

    class IndexableCyclicalLinkedList<T> : LinkedList<T>
    {
        /// <summary>
        /// Gets the LinkedListNode at a particular index.
        /// </summary>
        /// <param name="index">The index of the node to retrieve.</param>
        /// <returns>The LinkedListNode found at the index given.</returns>
        public LinkedListNode<T> this[int index]
        {
            get
            {
                //perform the index wrapping
                while (index < 0)
                    index = Count + index;
                if (index >= Count)
                    index %= Count;

                //find the proper node
                LinkedListNode<T> node = First;
                for (int i = 0; i < index; i++)
                    node = node.Next;

                return node;
            }
        }
        /// <summary>
        /// Removes the node at a given index.
        /// </summary>
        /// <param name="index">The index of the node to remove.</param>
        public void RemoveAt(int index)
        {
            Remove(this[index]);
        }
        /// <summary>
        /// Finds the index of a given item.
        /// </summary>
        /// <param name="item">The item to find.</param>
        /// <returns>The index of the item if found; -1 if the item is not found.</returns>
        public int IndexOf(T item)
        {
            for (int i = 0; i < Count; i++)
                if (this[i].Value.Equals(item))
                    return i;

            return -1;
        }
    }

    /// <summary>
	/// Implements a List structure as a cyclical list where indices are wrapped.
	/// </summary>
	/// <typeparam name="T">The Type to hold in the list.</typeparam>
	class CyclicalList<T> : List<T>
    {
        public new T this[int index]
        {
            get
            {
                //perform the index wrapping
                while (index < 0)
                    index = Count + index;
                if (index >= Count)
                    index %= Count;
                return base[index];
            }
            set
            {
                //perform the index wrapping
                while (index < 0)
                    index = Count + index;
                if (index >= Count)
                    index %= Count;
                base[index] = value;
            }
        }

        public CyclicalList() { }

        public CyclicalList(IEnumerable<T> collection) : base(collection){ }

        public new void RemoveAt(int index)
        {
            Remove(this[index]);
        }
    }

    public class LatLonPolygon
    {
        public List<LatLonCoord> Points = new List<LatLonCoord>();
        public int Bearing = 0;
        public GeoFenceAreaType areaType = GeoFenceAreaType.None;
        private List<LatLonLineSegment> cpLines;

        public LatLonPolygon(List<LatLonLineSegment> cpLines, int bearing, GeoFenceAreaType areaType)
        {
            this.cpLines = new List<LatLonLineSegment>();
            foreach (LatLonLineSegment line in cpLines)
            {
                this.cpLines.Add(new LatLonLineSegment(
                    new LatLonVertex(new LatLonCoord(line.A.Position.Latitude, line.A.Position.Longitude), line.A.Index),
                    new LatLonVertex(new LatLonCoord(line.B.Position.Latitude, line.B.Position.Longitude), line.B.Index)
                ));
            }
        }
    }

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
                Type = (UInt32)polygon.GetAreaType()
            };
            return item;
        }
    }

    public struct LatLonVertex
    {
        public LatLonCoord Position;
        public int Index;

        public LatLonVertex(LatLonCoord position, int index)
        {
            Position = position;
            Index = index;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != typeof(LatLonVertex))
                return false;
            return Equals((LatLonVertex)obj);
        }

        public bool Equals(LatLonVertex obj)
        {
            return obj.Position.Equals(Position) && obj.Index == Index;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Position.GetHashCode() * 397) ^ Index;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", Position, Index);
        }
    }

    public struct LatLonLineSegment
    {
        public LatLonVertex A;
        public LatLonVertex B;

        public LatLonLineSegment(LatLonVertex a, LatLonVertex b)
        {
            A = a;
            B = b;
        }

        public void orientateLeftUp() {
            bool bSwap = false;
            if (this.A.Position.Longitude > this.B.Position.Longitude) {
                bSwap = true;
            }
            else if ((this.A.Position.Longitude == this.B.Position.Longitude) && (this.A.Position.Latitude > this.B.Position.Latitude))
            {
                bSwap = true;
            }
            if (bSwap)
            {
                this.swapCoordinates();
            }
        }

        public void swapCoordinates()
        {
            double t;
            t = this.A.Position.Longitude; this.A.Position.Longitude = this.B.Position.Longitude;  this.B.Position.Longitude = t;
            t = this.A.Position.Latitude; this.A.Position.Latitude = this.B.Position.Longitude; this.B.Position.Latitude = t;
        }

        public double? IntersectsWithRay(LatLonCoord origin, LatLonCoord direction)
        {
            double largestDistance = Math.Max(A.Position.Longitude - origin.Longitude, B.Position.Longitude - origin.Longitude) * 2f;
            LatLonLineSegment raySegment = new LatLonLineSegment(new LatLonVertex(origin, 0), new LatLonVertex(origin.Add(direction.Multiply(largestDistance)), 0));

            LatLonCoord? intersection = FindIntersection(this, raySegment);
            double? value = null;

            if (intersection != null)
                value = LatLonCoord.Distance(origin, (LatLonCoord)intersection);

            return value;
        }

        public static LatLonCoord? FindIntersection(LatLonLineSegment a, LatLonLineSegment b)
        {
            double x1 = a.A.Position.Longitude;
            double y1 = a.A.Position.Latitude;
            double x2 = a.B.Position.Longitude;
            double y2 = a.B.Position.Latitude;
            double x3 = b.A.Position.Longitude;
            double y3 = b.A.Position.Latitude;
            double x4 = b.B.Position.Longitude;
            double y4 = b.B.Position.Latitude;

            double denom = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);

            double uaNum = (x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3);
            double ubNum = (x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3);

            double ua = uaNum / denom;
            double ub = ubNum / denom;

            if (MathHelper.Clamp(ua, 0f, 1f) != ua || MathHelper.Clamp(ub, 0f, 1f) != ub)
                return null;

            return a.A.Position.Add((a.B.Position.Substract(a.A.Position)).Multiply(ua));
        }

        internal bool Matches(LatLonLineSegment lineToCheck)
        {
            List<LatLonCoord> pl = new List<LatLonCoord>();
            pl.Add(new LatLonCoord(this.A.Position.Latitude, this.A.Position.Longitude));
            pl.Add(new LatLonCoord(this.B.Position.Latitude, this.B.Position.Longitude));
            pl.Add(new LatLonCoord(lineToCheck.A.Position.Latitude, lineToCheck.A.Position.Longitude));
            pl.Add(new LatLonCoord(lineToCheck.B.Position.Latitude, lineToCheck.B.Position.Longitude));
            return (
                (pl[0].Matches(pl[2]) && pl[1].Matches(pl[3]))
                    ||
                (pl[0].Matches(pl[3]) && pl[1].Matches(pl[2]))
            );
        }

        public static void MakePoints(ref List<LatLonCoord> pl, int pCount)
        {
            int n;
            pl.Clear();
            for(int i=0; i<pCount; i++)
            {
                pl.Add(new LatLonCoord(0,0));
            }
        }

        internal LatLonLineSegment Clone()
        {
            return new LatLonLineSegment(
                new LatLonVertex(new LatLonCoord(this.A.Position.Latitude, this.A.Position.Longitude), this.A.Index),
                new LatLonVertex(new LatLonCoord(this.B.Position.Latitude, this.B.Position.Longitude), this.B.Index)
            );
        }
    }

    public struct LatLonCoord
    {
        public double Latitude;
        public double Longitude;

        public static UInt32 LatLonPartToUInt32(double LatOrLon)
        {
            return (UInt32)Math.Round(LatOrLon * 10e7); // multiply by 10e7, round, cast
        }

        public static double LatLonPartFromUInt32(UInt32 LatOrLon)
        {
            return LatOrLon / 10e7; // divide by 10e7
        }

        public LatLonCoord(double Latitude, double Longitude)
        {
            this.Latitude = Latitude;
            this.Longitude = Longitude;
        }

        public LatLonCoord average(LatLonCoord otherPoint)
        {
            return new LatLonCoord((this.Latitude + otherPoint.Latitude) / 2, (this.Longitude + otherPoint.Longitude) / 2);
        }

        public PointLatLng ToPointLatLng()
        {
            return new PointLatLng(this.Latitude, this.Longitude);
        }

        public static LatLonCoord FromPointLatLng(PointLatLng coord)
        {
            return new LatLonCoord(coord.Lat, coord.Lng);
        }

        public LatLonCoord Add(LatLonCoord offset)
        {
            return new LatLonCoord(this.Latitude + offset.Latitude, this.Longitude + offset.Longitude);
        }

        public LatLonCoord Substract(LatLonCoord coord)
        {
            return new LatLonCoord(this.Latitude - coord.Latitude, this.Longitude - coord.Longitude);
        }

        public LatLonCoord Multiply(double scale)
        {
            return new LatLonCoord(this.Latitude * scale, this.Longitude * scale);
        }

        public double GetLatitude()
        {
            return this.Latitude;
        }

        public double GetLongitude()
        {
            return this.Longitude;
        }

        public static double Distance(LatLonCoord coord1, LatLonCoord coord2)
        {
            if ((coord1.Latitude == coord2.Latitude) && (coord1.Longitude == coord2.Longitude))
            {
                return 0;
            }
            else
            {
                double theta = coord1.Longitude - coord2.Longitude;
                double dist = Math.Sin(DegreesToRadians(coord1.Latitude)) * Math.Sin(DegreesToRadians(coord2.Latitude)) + Math.Cos(DegreesToRadians(coord1.Latitude)) * Math.Cos(DegreesToRadians(coord2.Latitude)) * Math.Cos(DegreesToRadians(theta));
                dist = Math.Acos(dist);
                dist = RadiansToDegrees(dist);
                dist = dist * 60 * 1.1515;
                dist = dist * 1609.344; // convert to meters
                return (dist);
            }
        }

        public static double DegreesToRadians(double degrees)
        {
            const double degToRadFactor = Math.PI / 180;
            return degrees * degToRadFactor;
        }

        public static double RadiansToDegrees(double radians)
        {
            const double radToDegFactor = 180 / Math.PI;
            return radians * radToDegFactor;
        }

        public static LatLonCoord FindPointAtDistanceFrom(LatLonCoord startPoint, double initialBearingRadians, double distanceMeters)
        {
            const double radiusEarthKilometres = 6371.01;
            var distRatio = (distanceMeters / 1000) / radiusEarthKilometres;
            var distRatioSine = Math.Sin(distRatio);
            var distRatioCosine = Math.Cos(distRatio);
            var startLatRad = LatLonCoord.DegreesToRadians(startPoint.Latitude);
            var startLonRad = LatLonCoord.DegreesToRadians(startPoint.Longitude);
            var startLatCos = Math.Cos(startLatRad);
            var startLatSin = Math.Sin(startLatRad);
            var endLatRads = Math.Asin((startLatSin * distRatioCosine) + (startLatCos * distRatioSine * Math.Cos(initialBearingRadians)));
            var endLonRads = startLonRad + Math.Atan2(Math.Sin(initialBearingRadians) * distRatioSine * startLatCos, distRatioCosine - startLatSin * Math.Sin(endLatRads));
            return new LatLonCoord(LatLonCoord.RadiansToDegrees(endLatRads), LatLonCoord.RadiansToDegrees(endLonRads));
        }

        public static class Triangulator
        {

            static readonly IndexableCyclicalLinkedList<LatLonVertex> polygonVertices = new IndexableCyclicalLinkedList<LatLonVertex>();
            static readonly IndexableCyclicalLinkedList<LatLonVertex> earVertices = new IndexableCyclicalLinkedList<LatLonVertex>();
            static readonly CyclicalList<LatLonVertex> convexVertices = new CyclicalList<LatLonVertex>();
            static readonly CyclicalList<LatLonVertex> reflexVertices = new CyclicalList<LatLonVertex>();

            public static LatLonLineSegment SideToLine(List<LatLonTriangle> tls, int triangleIdx, int SidePoint1, int Sidepoint2)
            {
                LatLonLineSegment sideLine = new LatLonLineSegment(tls[triangleIdx][SidePoint1], tls[triangleIdx][Sidepoint2]);
                sideLine.orientateLeftUp();
                return sideLine;
            }

            public static void MakeLines(List<LatLonLineSegment> ll, int pCount)
            {
                ll.Clear();
                for(var i=0; i<pCount; i++)
                {
                    ll.Add(new LatLonLineSegment());
                }
            }

            public static bool AddTriangleToPolyLines(ref List<LatLonLineSegment> plns, List<LatLonLineSegment> tlns)
            {
                int atPL, atTL, atn, ati, atf;
                bool bHasCommonSide = false;
                bool result = false;
                atTL = tlns.Count;
                atPL = plns.Count;
                int[] aCommonSides = new int[3];
                if (atTL == 3)
                {
                    for (ati = 0; ati < atTL; ati++)
                    {
                        atf = -1;
                        if (atPL > 0) // If there are any sides in this polygon yet,
                        {
                            atn = 0; // Find if there exists a line in the polygon that is the exact same as the current Triangle side (ati),
                            while (atn < atPL)
                            {
                                if (plns[atn].Matches(tlns[ati])) { atf = atn; atn = atPL; } else { atn++; }
                            }
                        }
                        if (atf < 0) {       // If no such side exists yet, this side must be added to either this, or the next polygon.
                            aCommonSides[ati] = 0;
                        }
                        else
                        { // else the line inside the polygon must be removed (duplicate triangle sides are inner lines and
                            atPL--; // when all inner lines are removed, what remains is the outer polygon).
                            aCommonSides[ati] = 1;     // Mark this Side as being common (to avoid adding it later)
                            bHasCommonSide = true;
                            plns.RemoveAt(atf); // remove in between
                        }
                    }
                    if ((bHasCommonSide) || (atPL < 3))
                    {
                        result = true;  // If this is a new poly, or the triangle shares a line, mark to add triangle here.
                    }
                    if (result)
                    {                  // If Triangle should be aded here (minus any common sides)...
                        for (ati = 0; ati < atTL; ati++)
                        { // Walk the lines/sides list...
                            if (aCommonSides[ati] == 0)
                            { // If this is not a common side, add it...
                                LatLonLineSegment line = tlns[ati].Clone();
                                plns.Add(line);
                            }
                        }
                    }
                }
                else
                {
                    throw new Exception("tlns not a triangle");
                }
                return result;
            }

            public static List<LatLonLineSegment> TriangleToLines(List<LatLonTriangle> tls, int triangleIdx)
            {
                List<LatLonLineSegment> tlns = new List<LatLonLineSegment>();
                int ttlL = 3;
                MakeLines(tlns, 3);
                tlns[0] = SideToLine(tls, triangleIdx, 0, 1);
                tlns[1] = SideToLine(tls, triangleIdx, 1, 2);
                tlns[2] = SideToLine(tls, triangleIdx, 2, 0);
                return tlns;
            }

            public static List<LatLonPolygon> TrianglesToPolygons(GeofenceTriangle[] geoFenceTriangleArray)
            {
                // convert to list
                List<LatLonTriangle> tls = new List<LatLonTriangle>();
                for (int ti=0; ti<geoFenceTriangleArray.Length; ti++)
                {
                    if (((GeoFenceAreaType)geoFenceTriangleArray[ti].Type) != GeoFenceAreaType.None)
                    {
                        tls.Add(new LatLonTriangle(
                           new LatLonVertex(new LatLonCoord(LatLonPartFromUInt32(geoFenceTriangleArray[ti].LatitudePoint1), LatLonPartFromUInt32(geoFenceTriangleArray[ti].LongitudePoint1)), 0),
                           new LatLonVertex(new LatLonCoord(LatLonPartFromUInt32(geoFenceTriangleArray[ti].LatitudePoint2), LatLonPartFromUInt32(geoFenceTriangleArray[ti].LongitudePoint2)), 0),
                           new LatLonVertex(new LatLonCoord(LatLonPartFromUInt32(geoFenceTriangleArray[ti].LatitudePoint3), LatLonPartFromUInt32(geoFenceTriangleArray[ti].LongitudePoint3)), 0)
                        ));
                    }
                }
                
                int L = tls.Count;
                int i = 0;
                List<LatLonLineSegment> cpLines, ctLines;
                List<LatLonPolygon> pls = new List<LatLonPolygon>();

                cpLines = new List<LatLonLineSegment>();
                if (L > 0)
                {
                    for (int n = 0; n < L; n++)// Walk the Triangle List...
                    {
                        ctLines = TriangleToLines(tls, n);
                        if (!AddTriangleToPolyLines(ref cpLines, ctLines)) {  // If the adder return false, we should start a new Polygon...
                            if (cpLines.Count > 0) {  // If there are any valid lines added,
                                pls.Add(new LatLonPolygon(cpLines, (int)geoFenceTriangleArray[n].Heading, (GeoFenceAreaType)geoFenceTriangleArray[n].Type));       // Create a new Polygon from the lines and push it onto the list.
                                cpLines.Clear();
                                AddTriangleToPolyLines(ref cpLines, ctLines);   // Add the current triangle now to the new Polygon. (Nvm, the return value, this must be new)
                            }
                        }
                        if(cpLines.Count > 0){  // If there are any valid lines still, the final Polygon must be closed off...
                            pls.Add(new LatLonPolygon(cpLines, (int)geoFenceTriangleArray[n].Heading, (GeoFenceAreaType)geoFenceTriangleArray[n].Type)); // Create a new Polygon from the lines and push it onto the list.
                        }
                    }
                }
                return pls;
            }                                  

            /// <summary>
            /// Triangulates a 2D polygon produced the indexes required to render the points as a triangle list.
            /// </summary>
            /// <param name="inputVertices">The polygon vertices in counter-clockwise winding order.</param>
            /// <param name="desiredWindingOrder">The desired output winding order.</param>
            public static List<LatLonTriangle> Triangulate(
                LatLonCoord[] inputVertices,
                WindingOrder desiredWindingOrder)
            {
                //Log("\nBeginning triangulation...");
                LatLonCoord[] outputVertices;
                int[] indices;

                List<LatLonTriangle> triangles = new List<LatLonTriangle>();

                //make sure we have our vertices wound properly
                if (DetermineWindingOrder(inputVertices) == WindingOrder.Clockwise)
                    outputVertices = ReverseWindingOrder(inputVertices);
                else
                    outputVertices = (LatLonCoord[])inputVertices.Clone();

                //clear all of the lists
                polygonVertices.Clear();
                earVertices.Clear();
                convexVertices.Clear();
                reflexVertices.Clear();

                //generate the cyclical list of vertices in the polygon
                for (int i = 0; i < outputVertices.Length; i++)
                    polygonVertices.AddLast(new LatLonVertex(outputVertices[i], i));

                //categorize all of the vertices as convex, reflex, and ear
                FindConvexAndReflexVertices();
                FindEarVertices();

                //clip all the ear vertices
                while (polygonVertices.Count > 3 && earVertices.Count > 0)
                    ClipNextEar(triangles);

                //if there are still three points, use that for the last triangle
                if (polygonVertices.Count == 3)
                    triangles.Add(new LatLonTriangle(
                        polygonVertices[0].Value,
                        polygonVertices[1].Value,
                        polygonVertices[2].Value));

                //add all of the triangle indices to the output array
                indices = new int[triangles.Count * 3];

                //move the if statement out of the loop to prevent all the
                //redundant comparisons
                if (desiredWindingOrder == WindingOrder.CounterClockwise)
                {
                    for (int i = 0; i < triangles.Count; i++)
                    {
                        indices[(i * 3)] = triangles[i].A.Index;
                        indices[(i * 3) + 1] = triangles[i].B.Index;
                        indices[(i * 3) + 2] = triangles[i].C.Index;
                    }
                }
                else
                {
                    for (int i = 0; i < triangles.Count; i++)
                    {
                        indices[(i * 3)] = triangles[i].C.Index;
                        indices[(i * 3) + 1] = triangles[i].B.Index;
                        indices[(i * 3) + 2] = triangles[i].A.Index;
                    }
                }

                return triangles;
            }

            /// <summary>
            /// Reverses the winding order for a set of vertices.
            /// </summary>
            /// <param name="vertices">The vertices of the polygon.</param>
            /// <returns>The new vertices for the polygon with the opposite winding order.</returns>
            public static LatLonCoord[] ReverseWindingOrder(LatLonCoord[] vertices)
            {
                // Log("\nReversing winding order...");
                LatLonCoord[] newVerts = new LatLonCoord[vertices.Length];
                newVerts[0] = vertices[0];
                for (int i = 1; i < newVerts.Length; i++)
                    newVerts[i] = vertices[vertices.Length - i];
                return newVerts;
            }


            /// <summary>
            /// Determines the winding order of a polygon given a set of vertices.
            /// </summary>
            /// <param name="vertices">The vertices of the polygon.</param>
            /// <returns>The calculated winding order of the polygon.</returns>
            public static WindingOrder DetermineWindingOrder(LatLonCoord[] vertices)
            {
                double sum = 0.0;
                for (int i = 0; i < vertices.Length; i++)
                {
                    LatLonCoord v1 = vertices[i];
                    LatLonCoord v2 = vertices[(i + 1) % vertices.Length];
                    sum += (v2.Longitude - v1.Longitude) * (v2.Latitude + v1.Latitude);
                }
                return (sum > 0.0)?WindingOrder.Clockwise:WindingOrder.CounterClockwise;
            }

            private static void ClipNextEar(ICollection<LatLonTriangle> triangles)
            {
                //find the triangle
                LatLonVertex ear = earVertices[0].Value;
                LatLonVertex prev = polygonVertices[polygonVertices.IndexOf(ear) - 1].Value;
                LatLonVertex next = polygonVertices[polygonVertices.IndexOf(ear) + 1].Value;
                triangles.Add(new LatLonTriangle(ear, next, prev));

                //remove the ear from the shape
                earVertices.RemoveAt(0);
                polygonVertices.RemoveAt(polygonVertices.IndexOf(ear));
                // Log("\nRemoved Ear: {0}", ear);

                //validate the neighboring vertices
                ValidateAdjacentVertex(prev);
                ValidateAdjacentVertex(next);

                //write out the states of each of the lists
#if DEBUG
                StringBuilder rString = new StringBuilder();
                foreach (LatLonVertex v in reflexVertices)
                    rString.Append(string.Format("{0}, ", v.Index));
                // Log("Reflex Vertices: {0}", rString);
                StringBuilder cString = new StringBuilder();
                foreach (LatLonVertex v in convexVertices)
                    cString.Append(string.Format("{0}, ", v.Index));
                // Log("Convex Vertices: {0}", cString);
                StringBuilder eString = new StringBuilder();
                foreach (LatLonVertex v in earVertices)
                    eString.Append(string.Format("{0}, ", v.Index));
                // Log("Ear Vertices: {0}", eString);
#endif
            }

            private static void ValidateAdjacentVertex(LatLonVertex vertex)
            {
                // Log("Validating: {0}...", vertex);
                if (reflexVertices.Contains(vertex))
                {
                    if (IsConvex(vertex))
                    {
                        reflexVertices.Remove(vertex);
                        convexVertices.Add(vertex);
                        // Log("Vertex: {0} now convex", vertex);
                    }
                    else
                    {
                        // Log("Vertex: {0} still reflex", vertex);
                    }
                }
                if (convexVertices.Contains(vertex))
                {
                    bool wasEar = earVertices.Contains(vertex);
                    bool isEar = IsEar(vertex);

                    if (wasEar && !isEar)
                    {
                        earVertices.Remove(vertex);
                        // Log("Vertex: {0} no longer ear", vertex);
                    }
                    else if (!wasEar && isEar)
                    {
                        earVertices.AddFirst(vertex);
                        // Log("Vertex: {0} now ear", vertex);
                    }
                    else
                    {
                        // Log("Vertex: {0} still ear", vertex);
                    }
                }
            }

            private static void FindConvexAndReflexVertices()
            {
                for (int i = 0; i < polygonVertices.Count; i++)
                {
                    LatLonVertex v = polygonVertices[i].Value;
                    if (IsConvex(v))
                    {
                        convexVertices.Add(v);
                        // Log("Convex: {0}", v);
                    }
                    else
                    {
                        reflexVertices.Add(v);
                        // Log("Reflex: {0}", v);
                    }
                }
            }

            private static void FindEarVertices()
            {
                for (int i = 0; i < convexVertices.Count; i++)
                {
                    LatLonVertex c = convexVertices[i];
                    if (IsEar(c))
                    {
                        earVertices.AddLast(c);
                        // Log("Ear: {0}", c);
                    }
                }
            }

            private static bool IsEar(LatLonVertex c)
            {
                LatLonVertex p = polygonVertices[polygonVertices.IndexOf(c) - 1].Value;
                LatLonVertex n = polygonVertices[polygonVertices.IndexOf(c) + 1].Value;
                // Log("Testing vertex {0} as ear with triangle {1}, {0}, {2}...", c, p, n);
                foreach (LatLonVertex t in reflexVertices)
                {
                    if (t.Equals(p) || t.Equals(c) || t.Equals(n))
                        continue;
                    if (LatLonTriangle.ContainsPoint(p, c, n, t))
                    {
                        // Log("\tTriangle contains vertex {0}...", t);
                        return false;
                    }
                }

                return true;
            }

            private static bool IsConvex(LatLonVertex c)
            {
                LatLonVertex p = polygonVertices[polygonVertices.IndexOf(c) - 1].Value;
                LatLonVertex n = polygonVertices[polygonVertices.IndexOf(c) + 1].Value;
                LatLonCoord d1 = LatLonCoord.Normalize(c.Position.Substract(p.Position));
                LatLonCoord d2 = LatLonCoord.Normalize(n.Position.Substract(c.Position));
                LatLonCoord n2 = new LatLonCoord(d2.Longitude, -d2.Latitude);
                return (LatLonCoord.Dot(d1, n2) <= 0f);
            }

            private static bool IsReflex(LatLonVertex c)
            {
                return !IsConvex(c);
            }

        }

        private static double Dot(LatLonCoord d1, LatLonCoord n2)
        {
            return d1.Latitude * n2.Latitude + d1.Longitude * n2.Longitude;
        }

        private static LatLonCoord Normalize(LatLonCoord latLonCoord)
        {
            double mag = latLonCoord.Magnitude();
            if(mag > 0)
            {
                return latLonCoord.Divide(mag);
            }
            else
            {
                return latLonCoord;
            }
        }

        private LatLonCoord Divide(double m)
        {
            return new LatLonCoord(this.Latitude / m, this.Longitude / m);
        }

        private double Magnitude()
        {
            return Math.Sqrt(this.Longitude * this.Longitude + this.Latitude * this.Latitude);
        }

        public static LatLonCoord FindPointAtOffSet(LatLonCoord startPoint, double xMetersRight, double yMetersDown)
        {
            LatLonCoord xPoint = FindPointAtDistanceFrom(startPoint, Math.PI/2, xMetersRight);
            LatLonCoord yPoint = FindPointAtDistanceFrom(xPoint, Math.PI, yMetersDown);
            return yPoint;
        }

        internal LatLonCoord AddCoordinate(LatLonCoord difference)
        {
            return new LatLonCoord(Latitude + difference.Latitude, Longitude + difference.Longitude);
        }

        internal bool Matches(LatLonCoord pointToCheck)
        {
            return ((this.Longitude == pointToCheck.Longitude) && (this.Latitude == pointToCheck.Latitude));
        }
    }
}
