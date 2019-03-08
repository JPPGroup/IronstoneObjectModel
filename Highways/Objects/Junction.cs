using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Highways.ObjectModel.Helpers;

namespace Jpp.Ironstone.Highways.ObjectModel.Objects
{
    public class Junction
    {
        public JunctionPart PrimaryRoad { get; set; }
        public JunctionPart SecondaryRoad { get; set; }
        public bool TurningHead {
            get
            {
                var acTrans = Application.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
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
            SidesOfCentre primaryOffset;
            SidesOfCentre secondaryOffset;
            
            switch (Turn)
            {
                case TurnTypes.Right:
                    reverseArc = true;
                    primaryOffset = SidesOfCentre.Right;

                    if (SecondaryRoad.Type == JunctionPartTypes.Start)
                        secondaryOffset = SidesOfCentre.Right;
                    else if (SecondaryRoad.Type == JunctionPartTypes.End)
                        secondaryOffset = SidesOfCentre.Left;
                    else
                        throw new ArgumentOutOfRangeException();
                    break;
                case TurnTypes.Left:
                    reverseArc = false;
                    primaryOffset = SidesOfCentre.Left;

                    if (SecondaryRoad.Type == JunctionPartTypes.Start)
                        secondaryOffset = SidesOfCentre.Left;
                    else if (SecondaryRoad.Type == JunctionPartTypes.End)
                        secondaryOffset = SidesOfCentre.Right;
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
  
                    var arc = CarriageArc(pCentreLine, primaryOffset,true, sCentreLine, secondaryOffset, SecondaryRoad.Type == JunctionPartTypes.End, reverseArc);
                    if (arc != null)
                    {
                        foreach (var centre in ignoredCentre) centre.SetOffsetIgnored(primaryOffset);

                        return arc;
                    }

                    ignoredCentre.Add(pCentreLine);
                    pCentreLine = pNextCentreLine;
                }

                sCentreLine.SetOffsetIgnored(secondaryOffset);
                sCentreLine = sNextCentreLine;
            }

            return null;
        }

        private Arc CreateCarriageAfter()
        {
            bool reverseArc;
            SidesOfCentre primaryOffset;
            SidesOfCentre secondaryOffset;

            switch (Turn)
            {
                case TurnTypes.Right:
                    reverseArc = false;
                    primaryOffset = SidesOfCentre.Right;

                    if (SecondaryRoad.Type == JunctionPartTypes.Start)
                        secondaryOffset = SidesOfCentre.Left;
                    else if (SecondaryRoad.Type == JunctionPartTypes.End)
                        secondaryOffset = SidesOfCentre.Right;
                    else
                        throw new ArgumentOutOfRangeException();
                    break;
                case TurnTypes.Left:
                    reverseArc = true;
                    primaryOffset = SidesOfCentre.Left;

                    if (SecondaryRoad.Type == JunctionPartTypes.Start)
                        secondaryOffset = SidesOfCentre.Right;
                    else if (SecondaryRoad.Type == JunctionPartTypes.End)
                        secondaryOffset = SidesOfCentre.Left;
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

                    var arc = CarriageArc(pCentreLine, primaryOffset, false, sCentreLine, secondaryOffset, SecondaryRoad.Type == JunctionPartTypes.End, reverseArc);
                    if (arc != null)
                    {
                        foreach (var centre in ignoredCentre) centre.SetOffsetIgnored(primaryOffset);

                        return arc;
                    }

                    ignoredCentre.Add(pCentreLine);
                    pCentreLine = pNextCentreLine;
                }

                sCentreLine.SetOffsetIgnored(secondaryOffset);
                sCentreLine = sNextCentreLine;
            }

            return null;
        }

        private Arc CarriageArc(CentreLine pCentreLine, SidesOfCentre primaryOffset, bool primaryBefore, CentreLine sCentreLine, SidesOfCentre secondaryOffset, bool secondaryBefore, bool reverseArc)
        {
            var pCurve = pCentreLine.GenerateCarriageWayOffset(primaryOffset, Radius);
            var sCurve = sCentreLine.GenerateCarriageWayOffset(secondaryOffset, Radius);

            if (pCurve == null || sCurve == null) return null;

            var pts = new Point3dCollection();
            var plane = new Plane();

            pCurve.IntersectWith(sCurve, Intersect.OnBothOperands, plane, pts, IntPtr.Zero, IntPtr.Zero);
            if (pts.Count <= 0) return null;

            var circle = new Circle(pts[0], Vector3d.ZAxis, Radius);

            var pIntersectPt = new Point3dCollection();
            pCentreLine.GenerateCarriageWayOffset(primaryOffset).IntersectWith(circle, Intersect.OnBothOperands, plane, pIntersectPt, IntPtr.Zero, IntPtr.Zero);
            pCentreLine.AddCarriageWayIntersection(primaryOffset, pIntersectPt[0], primaryBefore);
            var startAngle = pts[0].Convert2d(plane).GetVectorTo(pIntersectPt[0].Convert2d(plane)).Angle;

            var sIntersectPt = new Point3dCollection();
            sCentreLine.GenerateCarriageWayOffset(secondaryOffset).IntersectWith(circle, Intersect.OnBothOperands, plane, sIntersectPt, IntPtr.Zero, IntPtr.Zero);
            sCentreLine.AddCarriageWayIntersection(secondaryOffset, sIntersectPt[0],secondaryBefore);
            var endAngle = pts[0].Convert2d(plane).GetVectorTo(sIntersectPt[0].Convert2d(plane)).Angle;

            return reverseArc
                ? new Arc(pts[0], Radius, endAngle, startAngle)
                : new Arc(pts[0], Radius, startAngle, endAngle);
        }
    }
}
