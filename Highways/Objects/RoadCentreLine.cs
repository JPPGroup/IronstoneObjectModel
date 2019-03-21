using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Highways.ObjectModel.Abstract;
using Jpp.Ironstone.Highways.ObjectModel.Factories;
using Jpp.Ironstone.Highways.ObjectModel.Objects.Offsets;

namespace Jpp.Ironstone.Highways.ObjectModel.Objects
{
    [Serializable]
    public class RoadCentreLine : Segment2d, IRoadCentreLine
    {
        public CarriageWayLeft CarriageWayLeft { get; set; }
        public CarriageWayRight CarriageWayRight { get; set; }
        [XmlIgnore] public Road Road { get; internal set; }
        Road IRoadCentreLine.Road
        {
            get => Road;
            set => Road = value;
        }

        public RoadCentreLine()
        {
            CarriageWayLeft = new CarriageWayLeft();
            CarriageWayRight = new CarriageWayRight();
        }

        public override void Generate()
        {
            CarriageWayLeft.Create(this);
            CarriageWayRight.Create(this);
        }

        public void Reset()
        {
            CarriageWayLeft.Clear();
            CarriageWayRight.Clear();
        }

        public void Reverse()
        {
            //TODO: Reverse offset or mark as dirty to rebuild layout
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
            var trans = TransactionFactory.CreateFromTop();

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

        public RoadCentreLine Previous()
        {
            if (!Road.CentreLines.Contains(this)) return null;

            var prevIdx = Road.CentreLines.IndexOf(this) - 1;
            return prevIdx < 0 ? null : Road.CentreLines[prevIdx];
        }

        public RoadCentreLine Next()
        {
            if (!Road.CentreLines.Contains(this)) return null;

            var nextIdx = Road.CentreLines.IndexOf(this) + 1;
            return nextIdx > Road.CentreLines.Count() - 1 ? null : Road.CentreLines[nextIdx];
        }

        public bool Equals(RoadCentreLine centreLine)
        {
            if (centreLine == null) return false;
            if (Type != centreLine.Type) return false;

            return StartVector == centreLine.StartVector && EndVector == centreLine.EndVector;
        }
      
        public RoadCentreLine ConnectingCentreLine(IEnumerable<Road> roads, bool isStart)
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

        public void SetAllOffsets(double leftCarriageWay, double rightCarriageWay, double leftPavement, double rightPavement)
        {
            //TODO: Mark as dirty to rebuild layout
            CarriageWayLeft.DistanceFromCentre = leftCarriageWay;
            CarriageWayLeft.Pavement.DistanceFromCentre = leftCarriageWay + leftPavement;
            CarriageWayRight.DistanceFromCentre = rightCarriageWay;
            CarriageWayRight.Pavement.DistanceFromCentre = rightCarriageWay +rightPavement;
        }
     
        public void SetCarriageWayOffsetIgnored(SidesOfCentre side)
        {
            switch (side)
            {
                case SidesOfCentre.Left:
                    CarriageWayLeft.Ignore = true;
                    break;
                case SidesOfCentre.Right:
                    CarriageWayRight.Ignore = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }

        public void AddCarriageWayOffsetIntersectPoint(SidesOfCentre side, Point3d arcPoint, bool before)
        {
            switch (side)
            {
                case SidesOfCentre.Left:
                    CarriageWayLeft.Intersections.Add(new OffsetIntersect(arcPoint, before));
                    break;
                case SidesOfCentre.Right:
                    CarriageWayRight.Intersections.Add(new OffsetIntersect(arcPoint, before));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }

        public double GetCarriageWayDistance(SidesOfCentre side)
        {
            switch (side)
            {
                case SidesOfCentre.Left:
                    return CarriageWayLeft.DistanceFromCentre;
                case SidesOfCentre.Right:
                    return CarriageWayRight.DistanceFromCentre;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }

        public double GetPavementDistance(SidesOfCentre side)
        {
            switch (side)
            {
                case SidesOfCentre.Left:
                    return Road.LeftPavementActive ? CarriageWayLeft.Pavement.DistanceFromCentre - CarriageWayLeft.DistanceFromCentre : 0; 
                case SidesOfCentre.Right:
                    return Road.RightPavementActive ? CarriageWayRight.Pavement.DistanceFromCentre - CarriageWayRight.DistanceFromCentre : 0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }
    }
}
