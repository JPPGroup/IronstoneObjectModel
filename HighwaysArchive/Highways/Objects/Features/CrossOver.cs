using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Highways.ObjectModel.Old.Abstract;
using Jpp.Ironstone.Highways.ObjectModel.Old.Extensions;
using Jpp.Ironstone.Highways.ObjectModel.Old.Factories;

namespace Jpp.Ironstone.Highways.ObjectModel.Old.Objects.Features
{
    //TODO: Consider adding to core a serializable Point3d?
    public class CrossOver : RoadFeature
    {
        private double[] _startFootwayPointArray;
        private double[] _endFootwayPointArray;

        public long StartObjectPtr { get; set; }
        public long EndObjectPtr { get; set; }
        public double[] StartFootwayPointArray
        {
            get => _startFootwayPointArray;
            set
            {
                if (value == _startFootwayPointArray) return;
                if(value.Length != 3) throw new ArgumentException(nameof(value));

                _startFootwayPointArray = value;
            }
        }
        public double[] EndFootwayPointArray
        {
            get => _endFootwayPointArray;
            set
            {
                if (value == _endFootwayPointArray) return;
                if (value.Length != 3) throw new ArgumentException(nameof(value));

                _endFootwayPointArray = value;
            }
        }

        [XmlIgnore] public Point3d StartFootwayPoint {
            get => new Point3d(StartFootwayPointArray[0], StartFootwayPointArray[1], StartFootwayPointArray[2]);
            set
            {
                StartFootwayPointArray[0] = value.X;
                StartFootwayPointArray[1] = value.Y;
                StartFootwayPointArray[2] = value.Z;
            }
        }
        [XmlIgnore] public Point3d EndFootwayPoint
        {
            get => new Point3d(EndFootwayPointArray[0], EndFootwayPointArray[1], EndFootwayPointArray[2]);
            set
            {
                EndFootwayPointArray[0] = value.X;
                EndFootwayPointArray[1] = value.Y;
                EndFootwayPointArray[2] = value.Z;
            }
        }
        [XmlIgnore] public Point3d StartRoadPoint { get; private set; }
        [XmlIgnore] public ObjectId StartObjectId
        {
            get
            {
                if (StartObjectPtr == 0) return ObjectId.Null;

                var acCurDb = Application.DocumentManager.MdiActiveDocument.Database;
                return acCurDb.GetObjectId(false, new Handle(StartObjectPtr), 0);
            }
            set => StartObjectPtr = value.Handle.Value;
        }
        [XmlIgnore] public Point3d EndRoadPoint { get; private set; }
        [XmlIgnore] public ObjectId EndObjectId
        {
            get
            {
                if (EndObjectPtr == 0) return ObjectId.Null;

                var acCurDb = Application.DocumentManager.MdiActiveDocument.Database;
                return acCurDb.GetObjectId(false, new Handle(EndObjectPtr), 0);
            }
            set => EndObjectPtr = value.Handle.Value;
        }

        public CrossOver() : base(RoadFeatureTypes.CrossOver)
        {
            StartFootwayPointArray = new double[3];
            EndFootwayPointArray = new double[3];
        }

        public override bool Generate(Road road)
        {
            Clear();

            var start = new CrossOverConstruction(StartFootwayPoint, road);
            var end = new CrossOverConstruction(EndFootwayPoint, road);
            
            if (!start.IsValid || !end.IsValid) return false;

            StartRoadPoint = start.GetFurthestPoint(end.RoadPoints);
            EndRoadPoint = end.GetFurthestPoint(start.RoadPoints);

            DrawCrossLines();

            RoadFeatureErased += road.Feature_Erased;

            return true;
        }

        private void DrawCrossLines()
        {
            var acCurDb = Application.DocumentManager.MdiActiveDocument.Database;
            using (var acTrans = TransactionFactory.CreateFromNew())
            {
                var blockTable = (BlockTable)acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead);
                var blockTableRecord = (BlockTableRecord)acTrans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                var startLine = new Line(StartFootwayPoint, StartRoadPoint);
                var endLine = new Line(EndFootwayPoint, EndRoadPoint);

                StartObjectId = blockTableRecord.AppendEntity(startLine);
                EndObjectId = blockTableRecord.AppendEntity(endLine);

                acTrans.AddNewlyCreatedDBObject(startLine, true);
                acTrans.AddNewlyCreatedDBObject(endLine, true);

                startLine.Erased += CrossOver_Erased;
                endLine.Erased += CrossOver_Erased;

                acTrans.Commit();
            }
        }

        public override void Clear()
        {
            using (var acTrans = TransactionFactory.CreateFromNew())
            {
                if (!StartObjectId.IsErased)
                {
                    var line = acTrans.GetObject(StartObjectId,OpenMode.ForRead);

                    if (line.IsWriteEnabled == false) line.UpgradeOpen();

                    //line.Erased -= CrossOver_Erased;
                    line.Erase();
                }

                if (!EndObjectId.IsErased)
                {
                    var line = acTrans.GetObject(EndObjectId, OpenMode.ForRead);

                    if (line.IsWriteEnabled == false) line.UpgradeOpen();

                    //line.Erased -= CrossOver_Erased;
                    line.Erase();
                }
                
                acTrans.Commit();
            }
        }

        private void CrossOver_Erased(object sender, ObjectErasedEventArgs e)
        {
            Clear();
            OnRoadFeatureErased();
        }
    }

    internal class CrossOverConstruction
    {
        private readonly Point3d _footwayPoint;
        private readonly Road _road;

        private List<ObjectIdCollection> _roadObjectIdCollection;
        private Line _perpendicularLine;
        private Circle _constructionCircle;

        public Point3dCollection RoadPoints { get; }
        public bool IsValid => RoadPoints.Count > 0;

        public CrossOverConstruction(Point3d point, Road road)
        {
            _footwayPoint = point;
            _road = road;
            
            _roadObjectIdCollection = new List<ObjectIdCollection>();
            RoadPoints = new Point3dCollection();

            if (!SetLineAndRoadObjects()) return;

            SetConstructionCircle();
            SetPointsOnRoad();
        }

        public Point3d GetFurthestPoint(Point3dCollection comparedCollection)
        {
            var returnPt = RoadPoints[0];
            var distance = returnPt.DistanceTo(comparedCollection[0]);

            foreach (Point3d pt in RoadPoints)
            {
                foreach (Point3d compare in comparedCollection)
                {
                    var ptDistance = pt.DistanceTo(compare);
                    if (!(ptDistance > distance)) continue;

                    distance = ptDistance;
                    returnPt = pt;
                }
            }

            return returnPt;
        }

        private bool SetLineAndRoadObjects()
        {
            var acTrans = TransactionFactory.CreateFromTop();

            foreach (var centre in _road.CentreLines)
            {
                double angle;
                foreach (ObjectId obj in centre.CarriageWayRight.Pavement.Curves.Collection)
                {
                    var curve = (Curve)acTrans.GetObject(obj, OpenMode.ForWrite, true);
                    var p = curve.GetClosestPointTo(_footwayPoint, false);

                    if (!(p.DistanceTo(_footwayPoint) < Constants.POINT_TOLERANCE)) continue;

                    angle = curve.AngleFromCurveToForSide(SidesOfCentre.Left, _footwayPoint);

                    _perpendicularLine = LineFromPoint(_footwayPoint, _road.RightPavement, angle);
                    _roadObjectIdCollection = _road.CentreLines.Select(c => c.CarriageWayRight.Curves.Collection).ToList();

                    return true;
                }

                foreach (ObjectId obj in centre.CarriageWayLeft.Pavement.Curves.Collection)
                {
                    var curve = (Curve)acTrans.GetObject(obj, OpenMode.ForWrite, true);
                    var p = curve.GetClosestPointTo(_footwayPoint, false);

                    if (!(p.DistanceTo(_footwayPoint) < Constants.POINT_TOLERANCE)) continue;

                    angle = curve.AngleFromCurveToForSide(SidesOfCentre.Right, _footwayPoint);

                    _perpendicularLine = LineFromPoint(_footwayPoint, _road.LeftPavement, angle);
                    _roadObjectIdCollection = _road.CentreLines.Select(c => c.CarriageWayLeft.Curves.Collection).ToList();

                    return true;
                }
            }

            if (_road.RoadClosureEnd.Active)
            {
                if (SetLineAndRoadObjectsForClosure(_road.RoadClosureEnd)) return true;
            }

            if (_road.RoadClosureStart.Active)
            {
                if (SetLineAndRoadObjectsForClosure(_road.RoadClosureStart)) return true;
            }

            return false;
        }

        private bool SetLineAndRoadObjectsForClosure(RoadClosure roadRoadClosure)
        {
            var acTrans = TransactionFactory.CreateFromTop();
            double angle;

            //left
            var lPad = (Curve)acTrans.GetObject(roadRoadClosure.PadPavementLeftLineId, OpenMode.ForWrite, true);
            var lPoint = lPad.GetClosestPointTo(_footwayPoint, false);

            if (lPoint.DistanceTo(_footwayPoint) < Constants.POINT_TOLERANCE)
            {
                angle = lPad.AngleFromCurveToForSide(SidesOfCentre.Right, _footwayPoint);

                _perpendicularLine = LineFromPoint(_footwayPoint, _road.LeftPavement, angle);
                _roadObjectIdCollection = _road.CentreLines.Select(c => c.CarriageWayLeft.Curves.Collection).ToList();
                return true;
            }

            //right
            var rPad = (Curve)acTrans.GetObject(roadRoadClosure.PadPavementRightLineId, OpenMode.ForWrite, true);
            var rPoint = rPad.GetClosestPointTo(_footwayPoint, false);

            if (rPoint.DistanceTo(_footwayPoint) < Constants.POINT_TOLERANCE)
            {
                angle = rPad.AngleFromCurveToForSide(SidesOfCentre.Left, _footwayPoint);

                _perpendicularLine = LineFromPoint(_footwayPoint, _road.RightPavement, angle);
                _roadObjectIdCollection = _road.CentreLines.Select(c => c.CarriageWayRight.Curves.Collection).ToList();
                return true;
            }

            //end 
            var end = (Curve)acTrans.GetObject(roadRoadClosure.EndPavementLineId, OpenMode.ForWrite, true);
            var ePoint = end.GetClosestPointTo(_footwayPoint, false);

            if (ePoint.DistanceTo(_footwayPoint) < Constants.POINT_TOLERANCE)
            {
                var side = roadRoadClosure.Type == ClosureTypes.End ? SidesOfCentre.Right : SidesOfCentre.Left;
                angle = end.AngleFromCurveToForSide(side, _footwayPoint);
                _perpendicularLine = LineFromPoint(_footwayPoint, roadRoadClosure.Distance, angle);
                _roadObjectIdCollection.Add(new ObjectIdCollection { roadRoadClosure.EndCarriageWayLineId });

                return true;
            }

            return false;
        }

        private void SetConstructionCircle()
        {
            _constructionCircle = new Circle {Center = _perpendicularLine.EndPoint, Radius = Constants.DEFAULT_CROSSOVER_RADIUS};
        }

        private void SetPointsOnRoad()
        {
            RoadPoints.Clear();

            var acTrans = TransactionFactory.CreateFromTop();

            foreach (var objectCol in _roadObjectIdCollection)
            {
                foreach (ObjectId obj in objectCol)
                {
                    var roadObj = (Curve) acTrans.GetObject(obj, OpenMode.ForWrite, true);
                    var pts = new Point3dCollection();
                    var plane = new Plane();
                    _constructionCircle.IntersectWith(roadObj, Intersect.OnBothOperands, plane, pts, IntPtr.Zero, IntPtr.Zero);

                    if (pts.Count <= 0) continue;

                    foreach (Point3d pt in pts) RoadPoints.Add(pt);
                }
            }
        }

        private static Line LineFromPoint(Point3d point, double length, double angle)
        {
            var endX = length * Math.Cos(angle) + point.X;
            var endY = length * Math.Sin(angle) + point.Y;
            var end = new Point3d(endX, endY, point.X);

            return new Line(point, end);
        }
    }
}
