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

        public static double LatitudeMetersPerDegree(double ElevationMetersAboveSeaLevel = 0)
        {
            long EarthEquatorialRadius = 6378137; // m
            long EarthPolarRadius = 6356752; // m
            return ((EarthPolarRadius + ElevationMetersAboveSeaLevel) * Math.PI) / 180; // Half PolarCircumference + Elevation div Latitude degrees
        }

        public static double CircumferenceAtLatitude(double cLat, double ElevationMetersAboveSeaLevel = 0)
        {
            long EarthEquatorialRadius = 6378137; // m
            long EarthPolarRadius = 6356752; // m
            double r1, r2, lr;
            if (cLat < -90) cLat = -90;
            if (cLat > +90) cLat = +90;
            lr = Math.Abs(cLat) / 90;
            r1 = EarthEquatorialRadius + ElevationMetersAboveSeaLevel;
            r2 = EarthPolarRadius + ElevationMetersAboveSeaLevel;
            r1 = (r1 * 2 * Math.PI);                       // Circumference of Earth at Equator (Larger Diameter, more accurate at Equator)
            r2 = (r2 * 2 * Math.PI);                      // Circumference of Earth at Poles (Smaller Diameter, more accurate at high Latitudes)
            r1 = (r1 * (1 - lr)) + (r2 * lr);       // Circumference Base correction (decide which circumference to use more)
            return r1 * Math.Cos(cLat * (Math.PI / 180));    // Corrected Circumference at given Latitude
        }

        public static double LongitudeMetersPerDegree(double cLat, double ElevationMetersAboveSeaLevel = 0)
        {
            return (CircumferenceAtLatitude(cLat, ElevationMetersAboveSeaLevel) / 360); // Meters per Degree of Longitude at this Latitude
        }

        public static double LatitudeDeltaToMeters(double cLat1, double cLat2, double ElevationMetersAboveSeaLevel = 0)
        {
            double r, res;
            if (cLat1 < -90) cLat1 = -90;
            if (cLat1 > +90) cLat1 = +90;
            if (cLat2 < -90) cLat2 = -90;
            if (cLat2 > +90) cLat2 = +90;
            res = 0;
            if (cLat2 != cLat1)
            {
                r = LatitudeMetersPerDegree(ElevationMetersAboveSeaLevel);
                res = (cLat2 * r) - (cLat1 * r);      // Distance Delta in Meters
            }
            return res;
        }

        public static double LongitudeDeltaToMeters(double cLon1, double cLon2, double AtLattitude = 0, double ElevationMetersAboveSeaLevel = 0)
        {
            double r = 0;
            if (AtLattitude < -90) AtLattitude = -90;
            if (AtLattitude > +90) AtLattitude = +90;
            r = LongitudeMetersPerDegree(AtLattitude, ElevationMetersAboveSeaLevel);
            return (cLon2 * r) - (cLon1 * r);      // Distance Delta in Meters
        }

        public static LatLonCoord RotatePoint(LatLonCoord cPointToRotate, LatLonCoord cAroundThisPoint, double fRotateDegrees)
        {
            double pOriginLat, pOriginLon;
            double sina, cosa;
            sina = Math.Sin(fRotateDegrees * (Math.PI / 180));
            cosa = Math.Cos(fRotateDegrees * (Math.PI / 180));
            // Translate PointToRotate to the same offset from the 0,0 Origin
            pOriginLon = (cPointToRotate.Longitude - cAroundThisPoint.Longitude);
            pOriginLat = (cPointToRotate.Latitude - cAroundThisPoint.Latitude);
            // Rotate to a new Point rotated by fRotateDegrees around the Origin
            double newPOriginLon = ((pOriginLon * cosa) - (pOriginLat * sina));
            double newPOriginLat = ((pOriginLon * sina) + (pOriginLat * cosa));
            // Translate the new Origin offset back to the real map points
            return new LatLonCoord(newPOriginLat + cAroundThisPoint.Latitude, newPOriginLon + cAroundThisPoint.Longitude);
        }

        public static void CalcUnrotatedSquareCorners(LatLonCoord fCentre, LatLonCoord oldHandlePosition, double iRotateDegrees, out LatLonCoord c1, out LatLonCoord c2, out LatLonCoord c3, out LatLonCoord c4)
        {
            double halfLat, halfLon;
            // Unrotate the handle here... Is this even needed? Do we draw the handle with rotation or is the rotation independent of the handle sizing?
            LatLonCoord fHandle = RotatePoint(oldHandlePosition, fCentre, -iRotateDegrees); // Rotate by minus the rotation degrees to find the original unrotated box points

            halfLat = Math.Abs(fCentre.Latitude - fHandle.Latitude);
            halfLon = Math.Abs(fCentre.Longitude - fHandle.Longitude);
            //         ---->           Pos
            c1.Latitude = (fCentre.Latitude + halfLat);     //   1  ___________  2
            c1.Longitude = (fCentre.Longitude - halfLon);   //     |           |       /|
            c2.Latitude = (fCentre.Latitude + halfLat);     //     |           |        |
            c2.Longitude = (fCentre.Longitude + halfLon);   //     |     c     |        |
            c3.Latitude = (fCentre.Latitude - halfLat);     //     |           |
            c3.Longitude = (fCentre.Longitude + halfLon);   //     |___________|h
            c4.Latitude = (fCentre.Latitude - halfLat);     //   4               3
            c4.Longitude = (fCentre.Longitude - halfLon);   //  Neg
        }

        public static void CalcWidthHeightFromRotatedSquare(LatLonCoord cCentre, LatLonCoord cHandle, double iRotateDegrees, out double fWidth, out double fHeight)
        {
            int metersAboveSeaLevel = 1000;
            LatLonCoord c1, c2, c3, c4;
            CalcUnrotatedSquareCorners(cCentre, cHandle, iRotateDegrees, out c1, out c2, out c3, out c4);
            fWidth = Math.Abs(LongitudeDeltaToMeters(c1.Longitude, c2.Longitude, c1.Latitude, metersAboveSeaLevel));
            fHeight = Math.Abs(LatitudeDeltaToMeters(c2.Latitude, c3.Latitude, metersAboveSeaLevel));
        }
    }
}
