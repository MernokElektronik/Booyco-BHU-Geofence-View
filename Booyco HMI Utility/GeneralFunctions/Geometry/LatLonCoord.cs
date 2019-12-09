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
    public partial struct LatLonCoord
    {
        public double Latitude;
        public double Longitude;

        public static UInt32 LatLonPartToUInt32(double LatOrLon)
        {
            return (UInt32)((Int32)Math.Round(LatOrLon * 10e6)); // multiply by 10e6, round, cast
        }

        public static double LatLonPartFromUInt32(UInt32 LatOrLon)
        {
            return (((Int32)LatOrLon) / 10e6); // divide by 10e6
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
            if (MathHelper.DoubleEqual(coord1.Latitude, coord2.Latitude) && MathHelper.DoubleEqual(coord1.Longitude, coord2.Longitude))
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
            return (MathHelper.DoubleEqual(this.Longitude, pointToCheck.Longitude) && MathHelper.DoubleEqual(this.Latitude, pointToCheck.Latitude));
        }
    }
}
