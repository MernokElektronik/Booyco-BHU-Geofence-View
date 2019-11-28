using System;
using System.Collections.Generic;
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
}
