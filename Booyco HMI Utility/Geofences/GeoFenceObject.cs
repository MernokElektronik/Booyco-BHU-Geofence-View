using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Booyco_HMI_Utility.Geofences
{
    public class GeoFenceObject
    {
        public double Latitude;
        public double Longitude;

        /// <summary>
        /// Makes a deep copy of this object
        /// </summary>
        /// <returns></returns>
        public GeoFenceObject Clone()
        {
            var o = new GeoFenceObject();
            o.Latitude = this.Latitude;
            o.Longitude = this.Longitude;
            return o;
        }
    }
}
