using GMap.NET;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booyco_HMI_Utility
{
    class GeofenceCircle
    {
        UInt32 Latitude;
        UInt32 Longtitude;
        UInt32 Radius;
        UInt32 Heading;
        UInt32 Type;
    }

    class Geofence3Point
    {
        UInt32 LatitudePoint1;
        UInt32 LongitudePoint1;
        UInt32 LatitudePoint2;
        UInt32 LongitudePoint2;
        UInt32 LatitudePoint3;
        UInt32 LongitudePoint3;
        UInt32 Heading;
        UInt32 Type;
    }

    class GeofenceBlock
    {
        UInt32 Latitude;
        UInt32 Longitude;
        UInt32 Width;
        UInt32 Length;
        UInt32 Heading;
        UInt32 Type;
    }

    public class LatLonCoord
    {
        double Latitude;
        double Longitude;

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

        public LatLonCoord Add(PointF pointF)
        {
            return new LatLonCoord(Latitude + pointF.Y, this.Longitude + pointF.X);
        }

        public LatLonCoord Substract(LatLonCoord coord)
        {
            return new LatLonCoord(Latitude - coord.Latitude, this.Longitude - coord.Longitude);
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
    }
}
