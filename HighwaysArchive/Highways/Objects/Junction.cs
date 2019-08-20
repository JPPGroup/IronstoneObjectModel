using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Highways.ObjectModel.Old.Extensions;
using Jpp.Ironstone.Highways.ObjectModel.Old.Factories;
using Jpp.Ironstone.Highways.ObjectModel.Old.Helpers;

namespace Jpp.Ironstone.Highways.ObjectModel.Old.Objects
{
    public class Junction
    {
        private double _leftRadius = Constants.DEFAULT_RADIUS_JUNCTION;
        private double _rightRadius = Constants.DEFAULT_RADIUS_JUNCTION;

        public Guid Id { get; }
        public ContinuationTypes LeftContinuation { get; set; } = ContinuationTypes.Fillet;
        public ContinuationTypes RightContinuation { get; set; } = ContinuationTypes.Fillet;
        public double LeftRadius
        {
            get => _leftRadius;
            set
            {
                if (_leftRadius.Equals(value)) return;

                if (!IsValidRadius(SidesOfCentre.Left, value))
                    throw new ArgumentException("Invalid radius for junction.");

                _leftRadius = value;
            }
        }
        public double RightRadius
        {
            get => _rightRadius;
            set
            {
                if (_rightRadius.Equals(value)) return;

                if (!IsValidRadius(SidesOfCentre.Right, value))
                    throw new ArgumentException("Invalid radius for junction.");

                _rightRadius = value;
            }
        } 
        public JunctionPart PrimaryRoad { get; set; }
        public JunctionPart SecondaryRoad { get; set; }
        public TurnTypes Turn
        {
            get
            {
                var right = RadiansHelper.AngleForRightSide(PrimaryRoad.AngleAtIntersection);
                var left = RadiansHelper.AngleForLeftSide(PrimaryRoad.AngleAtIntersection);

                if (RadiansHelper.AnglesAreEqual(SecondaryRoad.AngleAtIntersection, right)) return TurnTypes.Right;
                if (RadiansHelper.AnglesAreEqual(SecondaryRoad.AngleAtIntersection, left)) return TurnTypes.Left;

                throw new ArgumentOutOfRangeException();
            }  
        }

        public Junction()
        {
            Id = Guid.NewGuid();
        }

        public void Highlight()
        {
            PrimaryRoad.CentreLine.Road.Highlight();
            SecondaryRoad.CentreLine.Road.Highlight();
        }

        public void Unhighlight()
        {
            PrimaryRoad.CentreLine.Unhighlight();
            SecondaryRoad.CentreLine.Unhighlight();
        }
       
        public ICollection<Curve> Generate()
        {
            var curveList = new List<Curve>();
            if (!PrimaryRoad.CentreLine.Road.Valid || !SecondaryRoad.CentreLine.Road.Valid) return curveList;

            var afterCurves = GenerateAfter();
            var beforeArc = GenerateBefore();

            if (afterCurves != null && afterCurves.Count > 0) curveList.AddRange(afterCurves);
            if (beforeArc != null && beforeArc.Count > 0) curveList.AddRange(beforeArc);

            return curveList;
        }

        private ICollection<Curve> GenerateBefore()
        {
            bool reverseArc;
            SidesOfCentre primarySide;
            SidesOfCentre secondarySide;
            double radius;
            ContinuationTypes continuation;
            switch (Turn)
            {
                case TurnTypes.Right:
                    reverseArc = true;
                    primarySide = SidesOfCentre.Right;

                    if (SecondaryRoad.Type == JunctionPartTypes.Start)
                    {
                        secondarySide = SidesOfCentre.Right;
                        radius = RightRadius;
                        continuation = RightContinuation;
                    }                  
                    else if (SecondaryRoad.Type == JunctionPartTypes.End)
                    {
                        secondarySide = SidesOfCentre.Left;
                        radius = LeftRadius;
                        continuation = LeftContinuation;
                    }
                    else
                        throw new ArgumentOutOfRangeException();
                    break;
                case TurnTypes.Left:
                    reverseArc = false;
                    primarySide = SidesOfCentre.Left;

                    if (SecondaryRoad.Type == JunctionPartTypes.Start)
                    { 
                        secondarySide = SidesOfCentre.Left;
                        radius = LeftRadius;
                        continuation = LeftContinuation;
                    }  
                    else if (SecondaryRoad.Type == JunctionPartTypes.End)
                    { 
                        secondarySide = SidesOfCentre.Right;
                        radius = RightRadius;
                        continuation = RightContinuation;
                    }  
                    else
                        throw new ArgumentOutOfRangeException();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            var sCentreLine = SecondaryRoad.CentreLine;

            while (sCentreLine != null)
            {
                var sNextCentreLine = SecondaryRoad.Type == JunctionPartTypes.End ? sCentreLine.Previous() : sCentreLine.Next();

                var pCentreLine = PrimaryRoad.CentreLine;
                var ignoredCentre = new List<RoadCentreLine>();

                while (pCentreLine != null)
                {
                    var pNextCentreLine = pCentreLine.Previous();
                    if (pCentreLine.BaseObject == sNextCentreLine?.BaseObject) break;
  
                    var arc = CarriageArc(radius, pCentreLine, primarySide, true, sCentreLine, secondarySide, SecondaryRoad.Type == JunctionPartTypes.End, reverseArc);
                    if (arc != null)
                    {
                        foreach (var centre in ignoredCentre) centre.SetCarriageWayOffsetIgnored(primarySide);

                        var junctionCurves = new List<Curve> { arc };
                        var pavementCurves = PavementCurves(arc, reverseArc, continuation, pCentreLine, primarySide, sCentreLine, secondarySide);
                        if (pavementCurves != null && pavementCurves.Count > 0) junctionCurves.AddRange(pavementCurves);

                        return junctionCurves;
                    }

                    ignoredCentre.Add(pCentreLine);
                    pCentreLine = pNextCentreLine;
                }

                sCentreLine.SetCarriageWayOffsetIgnored(secondarySide);
                sCentreLine = sNextCentreLine;
            }

            return null;
        }

        private ICollection<Curve> GenerateAfter()
        {
            bool reverseArc;
            SidesOfCentre primarySide;
            SidesOfCentre secondarySide;
            double radius;
            ContinuationTypes continuation;
            switch (Turn)
            {
                case TurnTypes.Right:
                    reverseArc = false;
                    primarySide = SidesOfCentre.Right;

                    if (SecondaryRoad.Type == JunctionPartTypes.Start)
                    {
                        secondarySide = SidesOfCentre.Left;
                        radius = LeftRadius;
                        continuation = LeftContinuation;
                    }                        
                    else if (SecondaryRoad.Type == JunctionPartTypes.End)
                    {
                        secondarySide = SidesOfCentre.Right;
                        radius = RightRadius;
                        continuation = RightContinuation;
                    }
                    else
                        throw new ArgumentOutOfRangeException();
                    break;
                case TurnTypes.Left:
                    reverseArc = true;
                    primarySide = SidesOfCentre.Left;

                    if (SecondaryRoad.Type == JunctionPartTypes.Start)
                    {
                        secondarySide = SidesOfCentre.Right;
                        radius = RightRadius;
                        continuation = RightContinuation;
                    }
                    else if (SecondaryRoad.Type == JunctionPartTypes.End)
                    {
                        secondarySide = SidesOfCentre.Left;
                        radius = LeftRadius;
                        continuation = LeftContinuation;
                    }                        
                    else
                        throw new ArgumentOutOfRangeException();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var sCentreLine = SecondaryRoad.CentreLine;
            while (sCentreLine != null)
            {              
                var sNextCentreLine = SecondaryRoad.Type == JunctionPartTypes.End ? sCentreLine.Previous() : sCentreLine.Next();

                var pCentreLine = PrimaryRoad.CentreLine;
                var ignoredCentre = new List<RoadCentreLine>();

                while (pCentreLine != null)
                {
                    var pNextCentreLine = pCentreLine.Next();
                    if (pCentreLine.BaseObject == sNextCentreLine?.BaseObject) break;

                    var arc = CarriageArc(radius, pCentreLine, primarySide, false, sCentreLine, secondarySide, SecondaryRoad.Type == JunctionPartTypes.End, reverseArc);
                    if (arc != null)
                    {
                        foreach (var centre in ignoredCentre) centre.SetCarriageWayOffsetIgnored(primarySide);

                        var junctionCurves = new List<Curve> {arc};

                        var pavementCurves = PavementCurves(arc, reverseArc, continuation, pCentreLine, primarySide, sCentreLine, secondarySide);
                        if (pavementCurves != null && pavementCurves.Count > 0) junctionCurves.AddRange(pavementCurves);

                        return junctionCurves;
                    }

                    ignoredCentre.Add(pCentreLine);
                    pCentreLine = pNextCentreLine;
                }

                sCentreLine.SetCarriageWayOffsetIgnored(secondarySide);
                sCentreLine = sNextCentreLine;
            }

            return null;
        }

        private List<Curve> PavementCurves(Arc arc, bool reverseArc, ContinuationTypes continuation, RoadCentreLine pCentreLine, SidesOfCentre primarySide, RoadCentreLine sCentreLine, SidesOfCentre secondarySide)
        {
            var pDist = pCentreLine.GetPavementDistance(primarySide);
            var sDist = sCentreLine.GetPavementDistance(secondarySide);

            if (continuation == ContinuationTypes.Fillet)
            {
                if (pDist > sDist)
                {
                    var pave = arc.GetOffsetCurves(-pDist)[0] as Curve;
                    if (pave != null)
                    {
                        Line close;
                        if (pCentreLine.Road.PavementType(primarySide) == sCentreLine.Road.PavementType(secondarySide))
                        {
                            var lineVector = reverseArc ? pave.StartPoint.GetVectorTo(arc.StartPoint) : pave.EndPoint.GetVectorTo(arc.EndPoint);
                            var diff = (pDist - sDist) / pDist;
                            var endPoint = reverseArc ? pave.StartPoint + (lineVector * diff) : pave.EndPoint + (lineVector * diff);
                            close = reverseArc ? new Line(pave.StartPoint, endPoint) : new Line(pave.EndPoint, endPoint);
                        }
                        else
                        {
                            close = reverseArc ? new Line(pave.StartPoint, arc.StartPoint) : new Line(pave.EndPoint, arc.EndPoint);
                        }

                        return new List<Curve> { pave, close };
                    }

                }
                else if (pDist < sDist)
                {
                    var pave = arc.GetOffsetCurves(-sDist)[0] as Curve;
                    if (pave != null)
                    {
                        Line close;
                        if (pCentreLine.Road.PavementType(primarySide) == sCentreLine.Road.PavementType(secondarySide))
                        {
                            var lineVector = reverseArc ? pave.EndPoint.GetVectorTo(arc.EndPoint) : pave.StartPoint.GetVectorTo(arc.StartPoint);
                            var diff = (sDist - pDist) / sDist;
                            var endPoint = reverseArc ? pave.EndPoint + (lineVector * diff) : pave.StartPoint + (lineVector * diff);
                            close = reverseArc ? new Line(pave.EndPoint, endPoint) : new Line(pave.StartPoint, endPoint);
                        }
                        else
                        {
                            close = reverseArc ? new Line(pave.EndPoint, arc.EndPoint) : new Line(pave.StartPoint, arc.StartPoint);
                        }
                        return new List<Curve> { pave, close };
                    }
                }
                else
                {
                    if (pDist > 0)
                    {
                        var pave = arc.GetOffsetCurves(-pDist)[0] as Curve;
                        return new List<Curve> { pave };
                    }
                }
            }
            else
            {
                var pCurve = pDist> 0 ? arc.GetOffsetCurves(-pDist)[0] as Curve : null;
                var sCurve = sDist > 0 ? arc.GetOffsetCurves(-sDist)[0] as Curve : null;
                Line pLine = null;
                Line sLine = null;

                if (pCurve != null)
                {
                    pLine = reverseArc ? new Line(pCurve.EndPoint, arc.EndPoint) : new Line(pCurve.StartPoint, arc.StartPoint);                    
                    pLine.TransformBy(Matrix3d.Rotation(RadiansHelper.DEGREES_90,Vector3d.ZAxis, pLine.StartPoint));
                    var pts = new Point3dCollection();
                    var plane = new Plane();
                    pLine.IntersectWith(arc, Intersect.ExtendThis, plane, pts, IntPtr.Zero, IntPtr.Zero);
                    if (pts.Count <= 0) return null;
                    pLine.EndPoint = pts[0];                    
                }

                if (sCurve != null)
                {
                    sLine = reverseArc ? new Line(sCurve.StartPoint, arc.StartPoint) : new Line(sCurve.EndPoint, arc.EndPoint);
                    sLine.TransformBy(Matrix3d.Rotation(RadiansHelper.DEGREES_90, Vector3d.ZAxis, sLine.StartPoint));
                    var pts = new Point3dCollection();
                    var plane = new Plane();
                    sLine.IntersectWith(arc, Intersect.ExtendThis, plane, pts, IntPtr.Zero, IntPtr.Zero);
                    if (pts.Count <= 0) return null;
                    sLine.EndPoint = pts[0];
                }

                if (pLine != null && sLine != null)
                {
                    var pts = new Point3dCollection();
                    var plane = new Plane();
                    pLine.IntersectWith(sLine, Intersect.OnBothOperands, plane, pts, IntPtr.Zero, IntPtr.Zero);

                    if (pts.Count <= 0) return new List<Curve> {pLine, sLine};

                    if (pCentreLine.Road.PavementType(primarySide) == sCentreLine.Road.PavementType(secondarySide))
                    {
                        pLine.EndPoint = pts[0];
                        sLine.EndPoint = pts[0];
                        return new List<Curve> {pLine, sLine};
                    }

                    if (pDist > sDist)
                    {
                        sLine.EndPoint = pts[0];
                        return new List<Curve> { pLine, sLine };
                    }

                    pLine.EndPoint = pts[0];
                    return new List<Curve> { pLine, sLine };

                }

                if (pLine != null) return new List<Curve> { pLine };
                if (sLine != null) return new List<Curve> { sLine };
            }

            return null;
        }

        private Arc CarriageArc(double radius, RoadCentreLine pCentreLine, SidesOfCentre primarySide, bool primaryBefore, RoadCentreLine sCentreLine, SidesOfCentre secondarySide, bool secondaryBefore, bool reverseArc)
        {
            var pDistance = pCentreLine.GetCarriageWayDistance(primarySide);
            var sDistance = sCentreLine.GetCarriageWayDistance(secondarySide);
            var pCurve = pCentreLine.GetCurve().CreateOffset(primarySide, pDistance + radius);
            var sCurve = sCentreLine.GetCurve().CreateOffset(secondarySide, sDistance + radius);

            if (pCurve == null || sCurve == null) return null;

            var pts = new Point3dCollection();
            var plane = new Plane();

            pCurve.IntersectWith(sCurve, Intersect.OnBothOperands, plane, pts, IntPtr.Zero, IntPtr.Zero);
            if (pts.Count <= 0) return null;

            var circle = new Circle(pts[0], Vector3d.ZAxis, radius);

            var pIntersectPt = new Point3dCollection();
            pCentreLine.GetCurve().CreateOffset(primarySide, pDistance).IntersectWith(circle, Intersect.OnBothOperands, plane, pIntersectPt, IntPtr.Zero, IntPtr.Zero);
            pCentreLine.AddCarriageWayOffsetIntersectPoint(primarySide, pIntersectPt[0], primaryBefore);
            var startAngle = pts[0].Convert2d(plane).GetVectorTo(pIntersectPt[0].Convert2d(plane)).Angle;

            var sIntersectPt = new Point3dCollection();
            sCentreLine.GetCurve().CreateOffset(secondarySide, sDistance).IntersectWith(circle, Intersect.OnBothOperands, plane, sIntersectPt, IntPtr.Zero, IntPtr.Zero);
            sCentreLine.AddCarriageWayOffsetIntersectPoint(secondarySide, sIntersectPt[0],secondaryBefore);
            var endAngle = pts[0].Convert2d(plane).GetVectorTo(sIntersectPt[0].Convert2d(plane)).Angle;

            return reverseArc
                ? new Arc(pts[0], radius, endAngle, startAngle)
                : new Arc(pts[0], radius, startAngle, endAngle);
        }


        private bool IsValidRadius(SidesOfCentre side, double value)
        {
            using (var trans = TransactionFactory.CreateFromNew())
            {
                var pSide = Turn == TurnTypes.Left ? SidesOfCentre.Left : SidesOfCentre.Right;

                var pDist = PrimaryRoad.CentreLine.GetPavementDistance(pSide);
                var sDist = SecondaryRoad.CentreLine.GetPavementDistance(side);

                return value > pDist && value > sDist;
            }                
        }
    }
}
