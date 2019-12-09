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
            return (MathHelper.DoubleEqual(Position.Longitude, obj.Position.Longitude)) && (MathHelper.DoubleEqual(Position.Latitude, obj.Position.Latitude)) && (obj.Index == Index);
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
}
