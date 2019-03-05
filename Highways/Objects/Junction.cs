using System;
using System.Xml.Serialization;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Highways.Objectmodel.Helpers;

namespace Jpp.Ironstone.Highways.Objectmodel.Objects
{
    public class Junction
    {
        public const string LAYER_NAME_TURNING_HEAD = "JPP_TurningHead";
        public const double DEFAULT_RADIUS_JUNCTION = 6;
        public const double DEFAULT_RADIUS_TURNING = 7.5;
       
        public JunctionPart PrimaryRoad { get; set; }
        public JunctionPart SecondaryRoad { get; set; }
        [XmlIgnore] public bool TurningHead {
            get
            {
                var acTrans = Application.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
                var pCurve = acTrans.GetObject(PrimaryRoad.CentreLine.BaseObject, OpenMode.ForRead) as Curve;
                var sCurve = acTrans.GetObject(SecondaryRoad.CentreLine.BaseObject, OpenMode.ForRead) as Curve;

                return pCurve?.Layer == LAYER_NAME_TURNING_HEAD || sCurve?.Layer == LAYER_NAME_TURNING_HEAD;
            }
        }
        [XmlIgnore] public TurnTypes Turn
        {
            get
            {
                var right = RadiansHelper.AngleForRightSide(SecondaryRoad.AngleAtIntersection);
                var left = RadiansHelper.AngleForLeftSide(SecondaryRoad.AngleAtIntersection);

                if (RadiansHelper.AnglesAreEqual(PrimaryRoad.AngleAtIntersection, right)) return TurnTypes.Right;
                if (RadiansHelper.AnglesAreEqual(PrimaryRoad.AngleAtIntersection, left)) return TurnTypes.Left;

                throw new ArgumentOutOfRangeException();
            }  
        }
        [XmlIgnore] public double Radius => TurningHead ? DEFAULT_RADIUS_TURNING : DEFAULT_RADIUS_JUNCTION;

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

        public void GenerateCarriageWay()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;

            using (var acTrans = db.TransactionManager.StartTransaction())
            {
                var blockTable = (BlockTable)acTrans.GetObject(db.BlockTableId, OpenMode.ForRead);
                var blockTableRecord = (BlockTableRecord)acTrans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                var beforeArc = CreateCarriageBefore();
                if (beforeArc != null)
                {
                    //leftArc.Layer = LayerHelper.LAYER_DEF_POINTS;

                    blockTableRecord.AppendEntity(beforeArc);
                    acTrans.AddNewlyCreatedDBObject(beforeArc, true);
                }

                var afterArc = CreateCarriageAfter();
                if (afterArc != null)
                {
                    //rightArc.Layer = LayerHelper.LAYER_DEF_POINTS;

                    blockTableRecord.AppendEntity(afterArc);
                    acTrans.AddNewlyCreatedDBObject(afterArc, true);
                }

                acTrans.Commit();
            }
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

            var pCentreLine = PrimaryRoad.CentreLine;
            var sCentreLine = SecondaryRoad.CentreLine;

            while (sCentreLine != null)
            {
                var sNextCentreLine = SecondaryRoad.Type == JunctionPartTypes.Start 
                    ? sCentreLine.Next() 
                    : sCentreLine.Previous();

                while (pCentreLine != null)
                {
                    var pNextCentreLine = pCentreLine.Previous();
                    if (pCentreLine.BaseObject == sNextCentreLine?.BaseObject) break;
  
                    var arc = CarriageArc(pCentreLine, primaryOffset, sCentreLine, secondaryOffset, reverseArc);
                    if (arc != null) return arc;

                    pCentreLine = pNextCentreLine;
                }
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

            var pCentreLine = PrimaryRoad.CentreLine;
            var sCentreLine = SecondaryRoad.CentreLine;

            while (sCentreLine != null)
            {
                var sNextCentreLine = SecondaryRoad.Type == JunctionPartTypes.End
                    ? sCentreLine.Previous()
                    : sCentreLine.Next();

                while (pCentreLine != null)
                {
                    var pNextCentreLine = pCentreLine.Next();
                    if (pCentreLine.BaseObject == sNextCentreLine?.BaseObject) break;

                    var arc = CarriageArc(pCentreLine, primaryOffset, sCentreLine, secondaryOffset, reverseArc);
                    if (arc != null) return arc;

                    pCentreLine = pNextCentreLine;
                }
                sCentreLine = sNextCentreLine;
            }

            return null;
        }

        private Arc CarriageArc(CentreLine pCentreLine, SidesOfCentre primaryOffset, CentreLine sCentreLine, SidesOfCentre secondaryOffset, bool reverseArc)
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
            var startAngle = pts[0].Convert2d(plane).GetVectorTo(pIntersectPt[0].Convert2d(plane)).Angle;

            var sIntersectPt = new Point3dCollection();
            sCentreLine.GenerateCarriageWayOffset(secondaryOffset).IntersectWith(circle, Intersect.OnBothOperands, plane, sIntersectPt, IntPtr.Zero, IntPtr.Zero);
            var endAngle = pts[0].Convert2d(plane).GetVectorTo(sIntersectPt[0].Convert2d(plane)).Angle;

            return reverseArc
                ? new Arc(pts[0], Radius, endAngle, startAngle)
                : new Arc(pts[0], Radius, startAngle, endAngle);
        }
    }
}
