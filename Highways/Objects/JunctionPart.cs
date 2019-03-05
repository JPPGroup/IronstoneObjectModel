using System;
using System.Xml.Serialization;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Highways.Objectmodel.Extensions;
using Jpp.Ironstone.Highways.Objectmodel.Helpers;

namespace Jpp.Ironstone.Highways.Objectmodel.Objects
{
    public class JunctionPart
    {
        public JunctionPartTypes Type { get; set; }
        public CentreLine CentreLine { get; set; }
        public Point2d IntersectionPoint { get; set; }
        [XmlIgnore]
        public double AngleAtIntersection
        {
            get
            {
                switch (Type)
                {
                    case JunctionPartTypes.Start:
                        if (CentreLine.Type != SegmentType.Arc) return CentreLine.StartVector.Angle;

                        var arcStart = (Arc)CentreLine.GetCurve();
                        return arcStart.Clockwise() 
                            ? RadiansHelper.AngleForRightSide(CentreLine.StartVector.Angle)
                            : RadiansHelper.AngleForLeftSide(CentreLine.StartVector.Angle);
                    case JunctionPartTypes.End:
                        if (CentreLine.Type != SegmentType.Arc) return CentreLine.EndVector.Angle;

                        var arcEnd = (Arc)CentreLine.GetCurve();
                        return arcEnd.Clockwise()
                            ? RadiansHelper.AngleForLeftSide(CentreLine.EndVector.Angle)
                            : RadiansHelper.AngleForRightSide(CentreLine.EndVector.Angle);
                    case JunctionPartTypes.Mid:
                        if (CentreLine.Type != SegmentType.Arc) return CentreLine.StartVector.Angle;

                        var arcMid = (Arc)CentreLine.GetCurve();
                        var centreMid = new Point2d(arcMid.Center.X, arcMid.Center.Y);
                        var vecMid = centreMid.GetVectorTo(IntersectionPoint);
                        return arcMid.Clockwise()
                            ? RadiansHelper.AngleForRightSide(vecMid.Angle)
                            : RadiansHelper.AngleForLeftSide(vecMid.Angle);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
