using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Highways.ObjectModel.Abstract;
using Jpp.Ironstone.Highways.ObjectModel.Extensions;

namespace Jpp.Ironstone.Highways.ObjectModel.Objects
{
    [Serializable]
    public class CentreLine : Segment2d
    {
        private CarriageWayLeft _carriageWayLeft;
        private CarriageWayRight _carriageWayRight;

        public CarriageWayLeft CarriageWayLeft
        {
            get => _carriageWayLeft;
            set
            {
                if (IsValid(value)) _carriageWayLeft = value;
            }
        }
        public CarriageWayRight CarriageWayRight
        {
            get => _carriageWayRight;
            set
            {
                if (IsValid(value)) _carriageWayRight = value;
            }
        }
        [XmlIgnore] public Road Road { get; set; }

        public void Reverse()
        {
            var acCurDb = Application.DocumentManager.MdiActiveDocument.Database;
            using (var trans = acCurDb.TransactionManager.StartTransaction())
            {
                var curve = GetCurve(true);

                curve.ReverseCurve();
                trans.Commit();
            }
        }

        public Curve GetCurve(bool forWrite = false)
        {
            var mode = forWrite ? OpenMode.ForWrite : OpenMode.ForRead;
            var acCurDb = Application.DocumentManager.MdiActiveDocument.Database;
            var trans = acCurDb.TransactionManager.TopTransaction;
            
            return trans.GetObject(BaseObject, mode) as Curve;            
        }

        public void Highlight()
        {
            GetCurve().Highlight();
        }

        public void Unhighlight()
        {
            GetCurve().Unhighlight();
        }

        public CentreLine Previous()
        {
            if (!Road.CentreLines.Contains(this)) return null;

            var prevIdx = Road.PositionInRoad(this) - 1;
            return prevIdx < 0 ? null : Road[prevIdx];
        }

        public CentreLine Next()
        {
            if (!Road.CentreLines.Contains(this)) return null;

            var nextIdx = Road.PositionInRoad(this) + 1;
            return nextIdx > Road.CentreLines.Count() - 1 ? null : Road[nextIdx];
        }

        public bool Equals(CentreLine centreLine)
        {
            if (centreLine == null) return false;

            return StartPoint == centreLine.StartPoint && EndPoint == centreLine.EndPoint;
        }

        public Curve GenerateCarriageWayOffset(SidesOfCentre side)
        {
            switch (side)
            {
                case SidesOfCentre.Left:
                    if (CarriageWayLeft != null) return GetCurve().CreateOffset(side, CarriageWayLeft.Distance);
                    break;
                case SidesOfCentre.Right:
                    if (CarriageWayRight != null) return GetCurve().CreateOffset(side, CarriageWayRight.Distance);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }

            return null;
        }

        public Curve GenerateCarriageWayOffset(SidesOfCentre side, double extra)
        {
            switch (side)
            {
                case SidesOfCentre.Left:
                    if (CarriageWayLeft != null) return GetCurve().CreateOffset(side, CarriageWayLeft.Distance + extra);
                    break;
                case SidesOfCentre.Right:
                    if (CarriageWayRight != null) return GetCurve().CreateOffset(side, CarriageWayRight.Distance + extra);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }

            return null;
        }

        public CentreLine ConnectingCentreLine(IEnumerable<Road> roads, bool isStart)
        {
            const int dp = 3;
            var curve = GetCurve();

            var roadList = roads.ToList();
            if (!roadList.Any()) return null;

            foreach (var road in roadList)
            {
                foreach (var rCentreLine in road.CentreLines)
                {
                    var next = rCentreLine.Next();
                    var previous = rCentreLine.Previous();

                    if (next != null && next.Equals(this)) continue;
                    if (previous != null && previous.Equals(this)) continue;

                    if (rCentreLine.Equals(this)) continue;

                    var pts = new Point3dCollection();
                    curve.IntersectWith(rCentreLine.GetCurve(), Intersect.OnBothOperands, pts, IntPtr.Zero, IntPtr.Zero);
                    if (pts.Count <= 0) continue;

                    var intPointRounded = new Point2d(Math.Round(pts[0].X, dp), Math.Round(pts[0].Y, dp));
                    var centrePointRounded = isStart
                        ? new Point2d(Math.Round(StartPoint.X, dp), Math.Round(StartPoint.Y, dp))
                        : new Point2d(Math.Round(EndPoint.X, dp), Math.Round(EndPoint.Y, dp));

                    if (intPointRounded == centrePointRounded) return rCentreLine;
                }
            }

            return null;
        }

        public void SetOffsetIgnored(SidesOfCentre side)
        {
            switch (side)
            {
                case SidesOfCentre.Left:
                    if (CarriageWayLeft != null) CarriageWayLeft.Ignore = true;
                    break;
                case SidesOfCentre.Right:
                    if (CarriageWayRight != null) CarriageWayRight.Ignore = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }

        public void AddCarriageWayIntersection(SidesOfCentre side, Point3d arcPoint, bool before)
        {
            switch (side)
            {
                case SidesOfCentre.Left:
                    CarriageWayLeft?.Intersection.Add(new OffsetIntersect(arcPoint, before));
                    break;
                case SidesOfCentre.Right:
                    CarriageWayRight?.Intersection.Add(new OffsetIntersect(arcPoint, before));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }


        private bool IsValid(ICentreLineOffset offset)
        {
            if (!(GetCurve() is Arc arc)) return true;

            return arc.Radius > offset.Distance;
        }
    }
}
