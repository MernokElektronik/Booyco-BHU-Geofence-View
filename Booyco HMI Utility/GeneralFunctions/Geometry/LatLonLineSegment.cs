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
    public struct LatLonLineSegment
    {
        public LatLonVertex A;
        public LatLonVertex B;

        public LatLonLineSegment(LatLonVertex a, LatLonVertex b)
        {
            A = a;
            B = b;
        }

        public LatLonLineSegment orientateLeftUp() {
            bool bSwap = false;
            if (this.A.Position.Longitude > this.B.Position.Longitude) {
                bSwap = true;
            }
            else if ((MathHelper.DoubleEqual(this.A.Position.Longitude, this.B.Position.Longitude)) && (this.A.Position.Latitude > this.B.Position.Latitude))
            {
                bSwap = true;
            }
            if (bSwap)
            {
                return this.swapCoordinates();
            }
            return this;
        }

        public LatLonLineSegment swapCoordinates()
        {
            double t;
            t = this.A.Position.Longitude; this.A.Position.Longitude = this.B.Position.Longitude;  this.B.Position.Longitude = t;
            t = this.A.Position.Latitude; this.A.Position.Latitude = this.B.Position.Latitude; this.B.Position.Latitude = t;
            return this;
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
            pl.Clear();
            for (int i = 0; i < pCount; i++) { pl.Add(new LatLonCoord(0, 0)); }
        }

        internal LatLonLineSegment Clone()
        {
            return new LatLonLineSegment(
                new LatLonVertex(new LatLonCoord(this.A.Position.Latitude, this.A.Position.Longitude), this.A.Index),
                new LatLonVertex(new LatLonCoord(this.B.Position.Latitude, this.B.Position.Longitude), this.B.Index)
            );
        }
    }
}
