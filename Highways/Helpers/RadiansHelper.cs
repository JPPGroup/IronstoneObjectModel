using System;
using System.Net.NetworkInformation;

namespace Jpp.Ironstone.Highways.ObjectModel.Helpers
{
    public static class RadiansHelper
    {
        public const double DEGREES_360 = 2 * Math.PI;
        public const double DEGREES_180 = Math.PI;
        public const double DEGREES_90 = Math.PI / 2;
        public const double ANGLE_TOLERANCE = Math.PI / 360;

        public static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        public static double AngleForSide(double angle, SidesOfCentre side)
        {
            switch (side)
            {
                case SidesOfCentre.Right:
                    return AngleForRightSide(angle);
                case SidesOfCentre.Left:
                    return AngleForLeftSide(angle);
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }
    
        public static double AngleForRightSide(double angle)
        {
            var output = angle - DEGREES_90;
            while (output < 0)
            {
                output += DEGREES_360;
            }
            return output;
        }

        public static double AngleForLeftSide(double angle)
        {
            var output = angle + DEGREES_90;
            while (output >= DEGREES_360)
            {
                output -= DEGREES_360;
            }
            return output;
        }

        public static bool AnglesAreEqual(double angle1, double angle2)
        {
            return Math.Abs(angle1 - angle2) < ANGLE_TOLERANCE;
        }

        public static double AngleBetween(double start, double end)
        {
            var output = start - end;

            while (output < 0)
            {
                output += DEGREES_360;
            }

            while (output >= DEGREES_360)
            {
                output -= DEGREES_360;
            }

            return output;
        }
    }
}
