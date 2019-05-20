using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Highways.ObjectModel.Extensions;
using Jpp.Ironstone.Highways.ObjectModel.Factories;
using Jpp.Ironstone.Highways.ObjectModel.Helpers;

namespace Jpp.Ironstone.Highways.ObjectModel.Objects
{
    public class CrossOver
    {
        private const double POINT_TOLERANCE = 0.0001;

        public long LinePtr { get; set; }
        public double BaseAngle { get; set; }
        public double InitialLength { get; set; }
        public Point3d StartPoint { get; set; }
        [XmlIgnore] public ObjectId LineId
        {
            get
            {
                if (LinePtr == 0) return ObjectId.Null;

                var acCurDb = Application.DocumentManager.MdiActiveDocument.Database;
                return acCurDb.GetObjectId(false, new Handle(LinePtr), 0);
            }
            set => LinePtr = value.Handle.Value;
        }

        public void Clear()
        {
            var acTrans = TransactionFactory.CreateFromTop();

            if (!LineId.IsErased) acTrans.GetObject(LineId, OpenMode.ForWrite, true).Erase();

            LineId = ObjectId.Null;
        }

        public bool Generate(Point3d startPoint, Point3d endPoint, List<Road> roads)
        {
            foreach (var road in roads)
            {

            }

            return true;
        }

        private static Line DrawLine(Point3d startPoint, double angle, Road road, double initialLength)
        {
            var endX = initialLength * Math.Cos(angle) + startPoint.X;
            var endY = initialLength * Math.Sin(angle) + startPoint.Y;
            var end = new Point3d(endX, endY, 0);

            return TrimSplay(road, new Line(startPoint, end));
        }

        public static Line[] LinesPointOnRoad(Point3d point, Road road, bool isStart)
        {
            using (var acTrans = TransactionFactory.CreateFromNew())
            {
                foreach (var centre in road.CentreLines)
                {
                    //Do right...
                    foreach (ObjectId obj in centre.CarriageWayRight.Pavement.Curves.Collection)
                    {
                        var curve = (Curve)acTrans.GetObject(obj, OpenMode.ForWrite, true);
                        var p = curve.GetClosestPointTo(point, false);

                        if (point.DistanceTo(p) < POINT_TOLERANCE)
                        {
                            var angle = curve.AngleFromCurveToForSide(SidesOfCentre.Left, point);
                            var dist = centre.CarriageWayRight.Pavement.DistanceFromCentre;
                            var lineAngle = CalcAngle(angle, Constants.DEFAULT_CROSSOVER_ANGLE, point, road.CentreLines.First().GetCurve().StartPoint, isStart);

                            return new[] { DrawLine(point, lineAngle, road, dist) };

                            //return new[] { lineBas, linePlus, lineMinus };
                        }
                    }

                    //Do left...
                    foreach (ObjectId obj in centre.CarriageWayLeft.Pavement.Curves.Collection)
                    {
                        var curve = (Curve)acTrans.GetObject(obj, OpenMode.ForWrite, true);
                        var p = curve.GetClosestPointTo(point, false);

                        if (point.DistanceTo(p) < POINT_TOLERANCE)
                        {
                            var angle = curve.AngleFromCurveToForSide(SidesOfCentre.Right, point);
                            var dist = centre.CarriageWayLeft.Pavement.DistanceFromCentre;
                            var lineAngle = CalcAngle(angle, Constants.DEFAULT_CROSSOVER_ANGLE, point, road.CentreLines.First().GetCurve().StartPoint, isStart);

                            return new[] { DrawLine(point, lineAngle, road, dist) };

                           // return new[] { lineBas, linePlus, lineMinus };
                        }
                    }
                }

                //Do End...
                if (road.RoadClosureEnd.Active)
                {
                    //left pad
                    var lPad = (Curve)acTrans.GetObject(road.RoadClosureEnd.PadPavementLeftLineId, OpenMode.ForWrite, true);
                    var lPadPoint = lPad.GetClosestPointTo(point, false);
                    //right pad 
                    var rPad = (Curve)acTrans.GetObject(road.RoadClosureEnd.PadPavementRightLineId, OpenMode.ForWrite, true);
                    var rPadPoint = rPad.GetClosestPointTo(point, false);

                    //end 
                    var end = (Curve)acTrans.GetObject(road.RoadClosureEnd.EndPavementLineId, OpenMode.ForWrite, true);
                    var endPoint = end.GetClosestPointTo(point, false);

                    double angle = 0;
                    double dist = 0;
                    bool onEnd = false;

                    if (point.DistanceTo(lPadPoint) < POINT_TOLERANCE)
                    {
                        angle = lPad.AngleFromCurveToForSide(SidesOfCentre.Right, point);
                        dist = road.CentreLines.Last().CarriageWayLeft.Pavement.DistanceFromCentre;
                        onEnd = true;
                    }

                    if (point.DistanceTo(rPadPoint) < POINT_TOLERANCE)
                    {
                        angle = rPad.AngleFromCurveToForSide(SidesOfCentre.Left, point);
                        dist = road.CentreLines.Last().CarriageWayRight.Pavement.DistanceFromCentre;
                        onEnd = true;
                    }

                    if (point.DistanceTo(endPoint) < POINT_TOLERANCE)
                    {
                        angle = end.AngleFromCurveToForSide(SidesOfCentre.Right, point);
                        dist = road.CentreLines.Last().CarriageWayRight.Pavement.DistanceFromCentre;
                        onEnd = true;
                    }

                    if (onEnd)
                    {
                        var lineAngle = CalcAngle(angle, Constants.DEFAULT_CROSSOVER_ANGLE, point, road.CentreLines.First().GetCurve().StartPoint,isStart);

                        return new [] { DrawLine(point, lineAngle, road, dist)};
                    }

                }
                //Do Start...

                return null;
            }
        }

        private static Line TrimSplay(Road road, Line splay)
        {
            var acTrans = TransactionFactory.CreateFromTop();
            var possLines = new List<Line>();

            foreach (var centre in road.CentreLines)
            {
                foreach (ObjectId obj in centre.CarriageWayRight.Curves.Collection)
                {
                    var curve = (Curve)acTrans.GetObject(obj, OpenMode.ForWrite, true);
                    var pts = new Point3dCollection();
                    var plane = new Plane();
                    curve.IntersectWith(splay, Intersect.OnBothOperands, plane, pts, IntPtr.Zero, IntPtr.Zero);

                    if (pts.Count > 0) possLines.Add(new Line(splay.StartPoint, pts[0]));
                }

                foreach (ObjectId obj in centre.CarriageWayLeft.Curves.Collection)
                {
                    var curve = (Curve)acTrans.GetObject(obj, OpenMode.ForWrite, true);
                    var pts = new Point3dCollection();
                    var plane = new Plane();
                    curve.IntersectWith(splay, Intersect.OnBothOperands, plane, pts, IntPtr.Zero, IntPtr.Zero);

                    if (pts.Count > 0) possLines.Add(new Line(splay.StartPoint, pts[0]));
                }
            }

            if (road.RoadClosureEnd.Active)
            {
                var curve = (Curve)acTrans.GetObject(road.RoadClosureEnd.EndCarriageWayLineId, OpenMode.ForWrite, true);
                var pts = new Point3dCollection();
                var plane = new Plane();
                curve.IntersectWith(splay, Intersect.OnBothOperands, plane, pts, IntPtr.Zero, IntPtr.Zero);

                if (pts.Count > 0) possLines.Add(new Line(splay.StartPoint, pts[0]));
            }

            if (road.RoadClosureStart.Active)
            {
                var curve = (Curve)acTrans.GetObject(road.RoadClosureStart.EndCarriageWayLineId, OpenMode.ForWrite, true);
                var pts = new Point3dCollection();
                var plane = new Plane();
                curve.IntersectWith(splay, Intersect.OnBothOperands, plane, pts, IntPtr.Zero, IntPtr.Zero);

                if (pts.Count > 0) possLines.Add(new Line(splay.StartPoint, pts[0]));
            }

            //TODO: Trim with Junction...

            return possLines.Count == 0 ? null : possLines.OrderBy(l => l.Length).First();
        }

        private static double CalcAngle(double baseAngle, double crossAngle, Point3d crossPoint1, Point3d crossPoint2, bool isStart)
        {
            var xAngle = RadiansHelper.DegreesToRadians(crossAngle);
            var anglePlus = baseAngle + xAngle;
            var angleMinus = baseAngle - xAngle;
            var angleToStart = new Line(crossPoint1, crossPoint2).Angle;

            var diffBase = Math.Abs(baseAngle - angleToStart);
            var diffPlus = Math.Abs(anglePlus - angleToStart);
            var diffMinus = Math.Abs(angleMinus - angleToStart);

            var flip = diffBase > 180;

            if (isStart)
            {
                if (flip)
                {
                    return diffPlus < diffBase ? anglePlus : angleMinus;
                }

                return diffPlus > diffBase ? anglePlus : angleMinus;

            }

            if (flip)
            {
                return diffMinus > diffBase ? anglePlus : angleMinus;
            }

            return diffMinus < diffBase ? anglePlus : angleMinus;
        }

    }
}
