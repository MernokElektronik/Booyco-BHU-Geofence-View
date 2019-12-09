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

        // n+1 because b/a tends to 1 with n leading digits
        public static double DoubleEpsilon { get; } = Math.Pow(10, -(12 + 1));

        public static bool DoubleEqual(double a, double b)
        {
            if (Math.Abs(a) <= double.Epsilon || Math.Abs(b) <= double.Epsilon)
                return Math.Abs(a - b) <= double.Epsilon;
            return Math.Abs(1.0 - a / b) <= DoubleEpsilon;
        }

    }
}
