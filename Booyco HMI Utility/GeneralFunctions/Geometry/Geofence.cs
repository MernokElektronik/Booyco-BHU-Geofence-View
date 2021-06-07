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
        [Description("Speed Zone")]
        SpeedZone = 70,
        [Description("Geofence Active")]
        GeofenceActive = 255
    }

    public interface IGeofenceShape { }

    public struct GeofenceCircle: IGeofenceShape
    {
        public UInt32 Latitude;
        public UInt32 Longitude;
        public UInt32 Radius;
        public UInt32 Heading;
        public UInt32 Type;
        public UInt32 WarningSpeed;
        public UInt32 Overspeed;

        internal static GeofenceCircle GetEmpty()
        {
            return new GeofenceCircle
            {
                Latitude = 0,
                Longitude = 0,
                Radius = 0,
                Heading = 0,
                Type = (UInt32)GeoFenceAreaType.None,
                WarningSpeed = 0,
                Overspeed = 0


            };
        }
    }

    public struct GeofenceTriangle : IGeofenceShape
    {
        public UInt32 LatitudePoint1;
        public UInt32 LongitudePoint1;
        public UInt32 LatitudePoint2;
        public UInt32 LongitudePoint2;
        public UInt32 LatitudePoint3;
        public UInt32 LongitudePoint3;
        public UInt32 Heading;
        public UInt32 Type;
        public UInt32 WarningSpeed;
        public UInt32 Overspeed;

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
                Type = (UInt32)GeoFenceAreaType.None,
                WarningSpeed = 0,
                Overspeed = 0

            };
        }
    }

    public struct GeofenceBlock : IGeofenceShape
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

    /// <summary>
	/// Specifies a desired winding order for the shape vertices.
	/// </summary>
	public enum WindingOrder
    {
        Clockwise,
        CounterClockwise
    }

    public class GeoFenceObject
    {
        public GeofenceCircle[] geofenceCircles = new GeofenceCircle[100]; // 100 Circles
        public GeofenceTriangle[] geofenceTriangles = new GeofenceTriangle[33]; // 33 Triangles
        public GeofenceBlock[] geofenceBlocks = new GeofenceBlock[1]; // 1 Block
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
            for (int i = 0; i < geofenceCircles.Length; i++)
            {
                o.geofenceCircles[i] = this.geofenceCircles[i]; // a struct, deep enough
            }
            for (int i = 0; i < geofenceTriangles.Length; i++)
            {
                o.geofenceTriangles[i] = this.geofenceTriangles[i]; // a struct, deep enough
            }
            for (int i = 0; i < geofenceBlocks.Length; i++)
            {
                o.geofenceBlocks[i] = this.geofenceBlocks[i]; // a struct, deep enough
            }
            return o;
        }

        public void CalculateStartLatitudeAndLongitude()
        {
            double latSum = 0;
            double longSum = 0;
            int objCount = 0;
            foreach (GeofenceCircle circle in geofenceCircles)
            {
                if (circle.Type != 0)
                {
                    latSum += LatLonCoord.LatLonPartFromUInt32(circle.Latitude);
                    longSum += LatLonCoord.LatLonPartFromUInt32(circle.Longitude);
                    objCount++;
                }
            }
            foreach (GeofenceTriangle triangle in geofenceTriangles)
            {
                if (triangle.Type != 0)
                {
                    latSum += LatLonCoord.LatLonPartFromUInt32(triangle.LatitudePoint1 + triangle.LatitudePoint2 + triangle.LatitudePoint3);
                    longSum += LatLonCoord.LatLonPartFromUInt32(triangle.LongitudePoint1 + triangle.LongitudePoint2 + triangle.LongitudePoint3);
                    objCount += 3;
                }
            }
            foreach (GeofenceBlock block in geofenceBlocks)
            {
                if (block.Type != 0)
                {
                    latSum += LatLonCoord.LatLonPartFromUInt32(block.Latitude);
                    longSum += LatLonCoord.LatLonPartFromUInt32(block.Longitude);
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
}
