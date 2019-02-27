using System;

namespace Jpp.Ironstone.Highways.Objectmodel.Helpers
{
    public static class RadiansHelper
    {
        public const double DEGREES_360 = 2 * Math.PI;
        public const double DEGREES_180 = Math.PI;
        public const double DEGREES_90 = Math.PI / 2;
        public const double ANGLE_TOLERANCE = Math.PI / 360;

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
    }
}
