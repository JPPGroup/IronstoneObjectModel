using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Highways.ObjectModel.Extensions;
using Jpp.Ironstone.Highways.ObjectModel.Factories;
using Jpp.Ironstone.Highways.ObjectModel.Helpers;

namespace Jpp.Ironstone.Highways.ObjectModel.Objects
{
    public class Junction
    {
        public JunctionPart PrimaryRoad { get; set; }
        public JunctionPart SecondaryRoad { get; set; }
        [XmlIgnore] public bool TurningHead {
            get
            {
                var acTrans = TransactionFactory.CreateFromTop();
                var pCurve = acTrans.GetObject(PrimaryRoad.CentreLine.BaseObject, OpenMode.ForRead) as Curve;
                var sCurve = acTrans.GetObject(SecondaryRoad.CentreLine.BaseObject, OpenMode.ForRead) as Curve;

                return pCurve?.Layer == Constants.LAYER_TURNING_HEAD || sCurve?.Layer == Constants.LAYER_TURNING_HEAD;
            }
        }
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
        public double Radius => TurningHead ? Constants.DEFAULT_RADIUS_TURNING : Constants.DEFAULT_RADIUS_JUNCTION;

        public void Highlight()
        {
            PrimaryRoad.CentreLine.Highlight();
            SecondaryRoad.CentreLine.Highlight();
        }

        public void Unhighlight()
        {
            PrimaryRoad.CentreLine.Unhighlight();
            SecondaryRoad.CentreLine.Unhighlight();
        }
       
        public ICollection<Arc> GenerateCarriageWayArcs()
        {
            var arcList = new List<Arc>();
            if (!PrimaryRoad.CentreLine.Road.Valid || !SecondaryRoad.CentreLine.Road.Valid) return arcList;

            var afterArc = CreateCarriageAfter();
            var beforeArc = CreateCarriageBefore();

            if (afterArc != null) arcList.Add(afterArc);
            if (beforeArc != null) arcList.Add(beforeArc);

            return arcList;
        }

        private Arc CreateCarriageBefore()
        {
            bool reverseArc;
            SidesOfCentre primarySide;
            SidesOfCentre secondarySide;
            
            switch (Turn)
            {
                case TurnTypes.Right:
                    reverseArc = true;
                    primarySide = SidesOfCentre.Right;

                    if (SecondaryRoad.Type == JunctionPartTypes.Start)
                        secondarySide = SidesOfCentre.Right;
                    else if (SecondaryRoad.Type == JunctionPartTypes.End)
                        secondarySide = SidesOfCentre.Left;
                    else
                        throw new ArgumentOutOfRangeException();
                    break;
                case TurnTypes.Left:
                    reverseArc = false;
                    primarySide = SidesOfCentre.Left;

                    if (SecondaryRoad.Type == JunctionPartTypes.Start)
                        secondarySide = SidesOfCentre.Left;
                    else if (SecondaryRoad.Type == JunctionPartTypes.End)
                        secondarySide = SidesOfCentre.Right;
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
                var ignoredCentre = new List<CentreLine>();

                while (pCentreLine != null)
                {
                    var pNextCentreLine = pCentreLine.Previous();
                    if (pCentreLine.BaseObject == sNextCentreLine?.BaseObject) break;
  
                    var arc = CarriageArc(pCentreLine, primarySide, true, sCentreLine, secondarySide, SecondaryRoad.Type == JunctionPartTypes.End, reverseArc);
                    if (arc != null)
                    {
                        foreach (var centre in ignoredCentre) centre.SetCarriageWayOffsetIgnored(primarySide);

                        return arc;
                    }

                    ignoredCentre.Add(pCentreLine);
                    pCentreLine = pNextCentreLine;
                }

                sCentreLine.SetCarriageWayOffsetIgnored(secondarySide);
                sCentreLine = sNextCentreLine;
            }

            return null;
        }

        private Arc CreateCarriageAfter()
        {
            bool reverseArc;
            SidesOfCentre primarySide;
            SidesOfCentre secondarySide;

            switch (Turn)
            {
                case TurnTypes.Right:
                    reverseArc = false;
                    primarySide = SidesOfCentre.Right;

                    if (SecondaryRoad.Type == JunctionPartTypes.Start)
                        secondarySide = SidesOfCentre.Left;
                    else if (SecondaryRoad.Type == JunctionPartTypes.End)
                        secondarySide = SidesOfCentre.Right;
                    else
                        throw new ArgumentOutOfRangeException();
                    break;
                case TurnTypes.Left:
                    reverseArc = true;
                    primarySide = SidesOfCentre.Left;

                    if (SecondaryRoad.Type == JunctionPartTypes.Start)
                        secondarySide = SidesOfCentre.Right;
                    else if (SecondaryRoad.Type == JunctionPartTypes.End)
                        secondarySide = SidesOfCentre.Left;
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
                var ignoredCentre = new List<CentreLine>();

                while (pCentreLine != null)
                {
                    var pNextCentreLine = pCentreLine.Next();
                    if (pCentreLine.BaseObject == sNextCentreLine?.BaseObject) break;

                    var arc = CarriageArc(pCentreLine, primarySide, false, sCentreLine, secondarySide, SecondaryRoad.Type == JunctionPartTypes.End, reverseArc);
                    if (arc != null)
                    {
                        foreach (var centre in ignoredCentre) centre.SetCarriageWayOffsetIgnored(primarySide);

                        return arc;
                    }

                    ignoredCentre.Add(pCentreLine);
                    pCentreLine = pNextCentreLine;
                }

                sCentreLine.SetCarriageWayOffsetIgnored(secondarySide);
                sCentreLine = sNextCentreLine;
            }

            return null;
        }

        private Arc CarriageArc(CentreLine pCentreLine, SidesOfCentre primarySide, bool primaryBefore, CentreLine sCentreLine, SidesOfCentre secondarySide, bool secondaryBefore, bool reverseArc)
        {
            var pDistance = pCentreLine.GetCarriageWayDistance(primarySide);
            var sDistance = sCentreLine.GetCarriageWayDistance(secondarySide);
            var pCurve = pCentreLine.GetCurve().CreateOffset(primarySide, pDistance + Radius);
            var sCurve = sCentreLine.GetCurve().CreateOffset(secondarySide, sDistance + Radius);

            if (pCurve == null || sCurve == null) return null;

            var pts = new Point3dCollection();
            var plane = new Plane();

            pCurve.IntersectWith(sCurve, Intersect.OnBothOperands, plane, pts, IntPtr.Zero, IntPtr.Zero);
            if (pts.Count <= 0) return null;

            var circle = new Circle(pts[0], Vector3d.ZAxis, Radius);

            var pIntersectPt = new Point3dCollection();
            pCentreLine.GetCurve().CreateOffset(primarySide, pDistance).IntersectWith(circle, Intersect.OnBothOperands, plane, pIntersectPt, IntPtr.Zero, IntPtr.Zero);
            pCentreLine.AddCarriageWayOffsetIntersectPoint(primarySide, pIntersectPt[0], primaryBefore);
            var startAngle = pts[0].Convert2d(plane).GetVectorTo(pIntersectPt[0].Convert2d(plane)).Angle;

            var sIntersectPt = new Point3dCollection();
            sCentreLine.GetCurve().CreateOffset(secondarySide, sDistance).IntersectWith(circle, Intersect.OnBothOperands, plane, sIntersectPt, IntPtr.Zero, IntPtr.Zero);
            sCentreLine.AddCarriageWayOffsetIntersectPoint(secondarySide, sIntersectPt[0],secondaryBefore);
            var endAngle = pts[0].Convert2d(plane).GetVectorTo(sIntersectPt[0].Convert2d(plane)).Angle;

            return reverseArc
                ? new Arc(pts[0], Radius, endAngle, startAngle)
                : new Arc(pts[0], Radius, startAngle, endAngle);
        }
    }
}
