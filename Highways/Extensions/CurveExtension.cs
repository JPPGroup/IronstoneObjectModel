using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Highways.ObjectModel.Helpers;

namespace Jpp.Ironstone.Highways.ObjectModel.Extensions
{
    public static class CurveExtension
    {
        public static Curve CreateOffset(this Curve curve, SidesOfCentre side, double dist)
        {
            Curve offset;

            switch (side)
            {
                case SidesOfCentre.Left:
                    offset = curve.CreateLeftOffset(dist);
                    break;
                case SidesOfCentre.Right:
                    offset = curve.CreateRightOffset(dist);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }

            return offset;
        }

        public static DBObjectCollection TrySplit(this Curve curve, Point3d point)
        {
            try
            {
                if (curve.StartPoint == point || curve.EndPoint == point) return null;
                var intPts = new Point3dCollection { point };

                return curve.GetSplitCurves(intPts);
            }
            catch (Exception)
            {
                return null;
            }
        }

        #region Private methods
        private static Curve CreateLeftOffset(this Curve curve, double offset)
        {
            Curve offsetObj = null;

            var posLOffset = curve.TryCreateOffset(offset);
            if (curve.IsOffSetValidForSide(posLOffset, SidesOfCentre.Left)) offsetObj = posLOffset;

            var negLOffset = curve.TryCreateOffset(-offset);
            if (curve.IsOffSetValidForSide(negLOffset, SidesOfCentre.Left)) offsetObj = negLOffset;

            return offsetObj;
        }

        private static Curve CreateRightOffset(this Curve curve, double offset)
        {
            Curve offsetObj = null;

            var posROffset = curve.TryCreateOffset(offset);
            if (curve.IsOffSetValidForSide(posROffset, SidesOfCentre.Right)) offsetObj = posROffset;

            var negROffset = curve.TryCreateOffset(-offset);
            if (curve.IsOffSetValidForSide(negROffset, SidesOfCentre.Right)) offsetObj = negROffset;

            return offsetObj;
        }

        private static Curve TryCreateOffset(this Curve curve, double offset)
        {
            try
            {
                return curve.GetOffsetCurves(offset)[0] as Curve;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static bool IsOffSetValidForSide(this Curve curve, Curve offSet, SidesOfCentre side)
        {
            if (offSet == null) return false;
            var start = new Point2d(curve.StartPoint.X, curve.StartPoint.Y);
            var offSetVector = start.GetVectorTo(new Point2d(offSet.StartPoint.X, offSet.StartPoint.Y));

            return Math.Abs(curve.AngleFromCurveToOffsetForSide(side) - offSetVector.Angle) < RadiansHelper.ANGLE_TOLERANCE;
        }

        private static double AngleFromCurveToOffsetForSide(this Curve curve, SidesOfCentre side)
        {
            double curveAngle;
            double returnAngle;
            switch (curve)
            {
                case Line line:
                    curveAngle = line.Angle;
                    break;
                case Arc arc:
                    var startPoint = new Point2d(arc.StartPoint.X, arc.StartPoint.Y);
                    var arcCentre = new Point2d(arc.Center.X, arc.Center.Y);
                    var startVector = arcCentre.GetVectorTo(startPoint);
                    curveAngle = arc.Clockwise() ? startVector.Angle - RadiansHelper.DEGREES_90 : startVector.Angle + RadiansHelper.DEGREES_90;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(curve), curve, null);
            }

            switch (side)
            {
                case SidesOfCentre.Right:
                    returnAngle = RadiansHelper.AngleForRightSide(curveAngle);
                    break;
                case SidesOfCentre.Left:
                    returnAngle = RadiansHelper.AngleForLeftSide(curveAngle);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }

            return returnAngle;
        }
        #endregion
    }
}
