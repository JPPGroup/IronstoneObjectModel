using System;
using System.Xml.Serialization;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Highways.ObjectModel.Old.Helpers;

namespace Jpp.Ironstone.Highways.ObjectModel.Old.Objects
{
    public class JunctionPart
    {
        public JunctionPartTypes Type { get; set; }
        public RoadCentreLine CentreLine { get; set; }
        public Point2d IntersectionPoint { get; set; }
        [XmlIgnore] public double AngleAtIntersection
        {
            get
            {
                switch (Type)
                {
                    case JunctionPartTypes.Start:
                        if (CentreLine.Type != SegmentType.Arc) return CentreLine.StartVector.Angle;

                        var arcStart = (Arc)CentreLine.GetCurve();
                        return arcStart.IsClockwise() 
                            ? RadiansHelper.AngleForRightSide(CentreLine.StartVector.Angle)
                            : RadiansHelper.AngleForLeftSide(CentreLine.StartVector.Angle);
                    case JunctionPartTypes.End:
                        if (CentreLine.Type != SegmentType.Arc) return CentreLine.EndVector.Angle;

                        var arcEnd = (Arc)CentreLine.GetCurve();
                        return arcEnd.IsClockwise()
                            ? RadiansHelper.AngleForLeftSide(CentreLine.EndVector.Angle)
                            : RadiansHelper.AngleForRightSide(CentreLine.EndVector.Angle);
                    case JunctionPartTypes.Mid:
                        if (CentreLine.Type != SegmentType.Arc) return CentreLine.StartVector.Angle;

                        var arcMid = (Arc)CentreLine.GetCurve();
                        var centreMid = new Point2d(arcMid.Center.X, arcMid.Center.Y);
                        var vecMid = centreMid.GetVectorTo(IntersectionPoint);
                        return arcMid.IsClockwise()
                            ? RadiansHelper.AngleForRightSide(vecMid.Angle)
                            : RadiansHelper.AngleForLeftSide(vecMid.Angle);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
