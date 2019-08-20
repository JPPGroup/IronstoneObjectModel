using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Highways.ObjectModel.Roads;
using System;
using System.Collections.Generic;

namespace Jpp.Ironstone.Highways.ObjectModel.Junctions
{
    //These routines are horrible on the eye, but work. 

    [Serializable]
    public class JunctionController
    {
        public PersistentObjectIdCollection Fillets { get; set; }

        public JunctionController()
        {
            Fillets = new PersistentObjectIdCollection();
        }

        public void BuildJunctions(IReadOnlyList<Road> roads, Database database)
        {
            var acTrans = database.TransactionManager.TopTransaction;

            foreach (ObjectId obj in Fillets.Collection)
            {
                if (!obj.IsErased) acTrans.GetObject(obj, OpenMode.ForWrite).Erase();
            }

            Fillets.Clear();


            foreach (var road in roads)
            {
                road.Segments.Reset();
            }

            var _ = InitialJunctions(roads);
        }

        private IEnumerable<Junction> InitialJunctions(IReadOnlyList<Road> roads)
        {
            var junctions = new List<Junction>();

            foreach (var secondaryRoad in roads)
            {
                var centreLine = secondaryRoad.CentreLine;
                var sp = centreLine.StartPoint;
                var ep = centreLine.EndPoint;

                using var pts = new Point3dCollection();
                if (centreLine.IntersectWithSelf(pts))
                {
                    foreach (Point3d point in pts)
                    {
                        if (sp.IsEqualTo(point))
                        {
                            var curve = centreLine.GetSectionBetween(sp, point);
                            var pAngle = curve.GetTangentialAngleAtEnd(); 
                            var sAngle = curve.GetTangentialAngleAtStart();
                            var turn = TurnForAngles(pAngle, sAngle);

                            if (turn.HasValue)
                            {
                                DrawJunctionCircle(secondaryRoad, secondaryRoad, turn.Value, true);

                                junctions.Add(new Junction
                                {
                                    PrimaryRoadId = secondaryRoad.Id,
                                    SecondaryRoadId = secondaryRoad.Id,
                                    IntersectionPoint = new SerializablePoint { Point3d = point },
                                    Chainage = curve.Length,
                                    Side = turn.Value
                                });
                            }
                            
                            break;
                        }

                        if (ep.IsEqualTo(point))
                        {
                            var curve = centreLine.GetSectionBetween(point, ep);
                            var pAngle = curve.GetTangentialAngleAtStart(); 
                            var sAngle = curve.GetTangentialAngleAtEnd();
                            var turn = TurnForAngles(pAngle, sAngle);

                            if (turn.HasValue)
                            {
                                DrawJunctionCircle(secondaryRoad, secondaryRoad, turn.Value, false);

                                junctions.Add(new Junction
                                {
                                    PrimaryRoadId = secondaryRoad.Id,
                                    SecondaryRoadId = secondaryRoad.Id,
                                    IntersectionPoint = new SerializablePoint { Point3d = point },
                                    Chainage = curve.Length,
                                    Side = turn.Value
                                });
                            }
                            
                            break;
                        }
                    }
                }

                foreach (var primaryRoad in roads)
                {
                    if (secondaryRoad.Id == primaryRoad.Id) continue;

                    var joiningCentreLine = primaryRoad.CentreLine;

                    var chainageAtStartPoint = joiningCentreLine.TryGetDistAtPoint(sp);
                    if (chainageAtStartPoint > 0)
                    {
                        var curve = joiningCentreLine.GetSectionBetween(joiningCentreLine.StartPoint, sp);
                        var pAngle = curve.GetTangentialAngleAtEnd(); 
                        var sAngle = centreLine.GetTangentialAngleAtStart();
                        var turn = TurnForAngles(pAngle, sAngle);

                        if (turn.HasValue)
                        {
                            DrawJunctionCircle(primaryRoad, secondaryRoad, turn.Value, true);

                            junctions.Add(new Junction
                            {
                                PrimaryRoadId = primaryRoad.Id,
                                SecondaryRoadId = secondaryRoad.Id,
                                IntersectionPoint = new SerializablePoint { Point3d = sp },
                                Chainage = curve.Length,
                                Side = turn.Value
                            });
                        }
                        
                    }

                    var chainageAtEndPoint = joiningCentreLine.TryGetDistAtPoint(ep);
                    if (chainageAtEndPoint > 0)
                    {
                        var curve = joiningCentreLine.GetSectionBetween(joiningCentreLine.StartPoint, ep);
                        var pAngle = curve.GetTangentialAngleAtEnd();
                        var sAngle = centreLine.GetTangentialAngleAtEnd();
                        var turn = TurnForAngles(pAngle, sAngle);

                        if (turn.HasValue)
                        {
                            DrawJunctionCircle(primaryRoad, secondaryRoad, turn.Value, false);

                            junctions.Add(new Junction
                            {
                                PrimaryRoadId = primaryRoad.Id,
                                SecondaryRoadId = secondaryRoad.Id,
                                IntersectionPoint = new SerializablePoint { Point3d = ep },
                                Chainage = curve.Length,
                                Side = turn.Value
                            });
                        }
                    }
                }
            }

            return junctions;
        }

        private void DrawJunctionCircle(Road primaryRoad, Road secondaryRoad, Side turn, bool isStart)
        {
            Point3d? point = null;

            double? primaryChainStart = null;
            double? primaryChainEnd = null;

            double? secondaryBeforeChain = null;
            double? secondaryAfterChain = null;

            Point3d? primaryBeforePoint = null;
            Point3d? primaryAfterPoint = null;

            Point3d? secondaryBeforePoint = null;
            Point3d? secondaryAfterPoint = null;

            Point3d? beforePoint = null;
            Point3d? afterPoint = null;

            var acTrans = primaryRoad.BaseObject.Database.TransactionManager.TopTransaction;
            var acBlkTblRec = (BlockTableRecord)acTrans.GetObject(primaryRoad.CentreLine.BlockId, OpenMode.ForWrite);

            using var primary = RoadCarriageway.GetCarriageway(primaryRoad, turn);

            var startInd = isStart ? 0 : secondaryRoad.CentreLine.NumberOfVertices - 2;
            var endIdx = isStart ? secondaryRoad.CentreLine.NumberOfVertices - 1 : 0;

            for (var i = startInd; LoopCheck(i, endIdx, isStart);)
            {
                var curve = secondaryRoad.CentreLine.GetCurveAt(i);
                var pts = new Point3dCollection();
                primary.IntersectWith(curve, Intersect.OnBothOperands, pts, IntPtr.Zero, IntPtr.Zero);

                if (pts.Count == 1)
                {
                    point = pts[0];
                    break;
                }

                i = isStart ? i + 1 : i - 1;
            }

            if (!point.HasValue) return;

            var index = primary.GetIndexAtPoint(point.Value);

            var beforeSide = isStart ? Side.Left : Side.Right;
            var afterSide = isStart ? Side.Right : Side.Left;

            if (turn == Side.Right)
            {
                beforeSide = isStart ? Side.Right : Side.Left;
                afterSide = isStart ? Side.Left : Side.Right;
            }

            using var before = RoadCarriageway.GetCarriageway(secondaryRoad, beforeSide);
            using var after = RoadCarriageway.GetCarriageway(secondaryRoad, afterSide);

            var beforeStartIdx = isStart ? 0 : before.NumberOfVertices - 2;
            var afterStartIdx = isStart ? 0 : after.NumberOfVertices - 2;
            var beforeEndIdx = isStart ? before.NumberOfVertices - 1 : 0;
            var afterEndIdx = isStart ? after.NumberOfVertices - 1 : 0;

            for (var i = index; i >= 0; i--)
            {
                if (beforePoint.HasValue) break;

                var primaryCurveAt = primary.GetOffsetCurveAt(i, turn, Constants.DEFAULT_RADIUS_JUNCTION);
                if (primaryCurveAt == null) continue;

                for (var j = beforeStartIdx; LoopCheck(j, beforeEndIdx, isStart);)
                {
                    if (beforePoint.HasValue) break;

                    var beforeCurveAt = before.GetOffsetCurveAt(j, beforeSide, Constants.DEFAULT_RADIUS_JUNCTION);

                    if (beforeCurveAt != null)
                    {
                        var pts = new Point3dCollection();
                        primaryCurveAt.IntersectWith(beforeCurveAt, Intersect.OnBothOperands, pts, IntPtr.Zero, IntPtr.Zero);

                        if (pts.Count > 0)
                        {
                            foreach (Point3d pt in pts)
                            {
                                if (pt != primaryCurveAt.StartPoint && pt != primaryCurveAt.EndPoint && pt != beforeCurveAt.StartPoint && pt != beforeCurveAt.EndPoint)
                                {
                                    beforePoint = pt;
                                    break;
                                }
                            }
                        }
                    }

                    j = isStart ? j + 1 : j - 1;
                }
            }

            for (var i = index; i < primary.NumberOfVertices - 1; i++)
            {
                if (afterPoint.HasValue) break;

                var primaryCurveAt = primary.GetOffsetCurveAt(i, turn, Constants.DEFAULT_RADIUS_JUNCTION);
                if (primaryCurveAt == null) continue;

                for (var j = afterStartIdx; LoopCheck(j, afterEndIdx, isStart);)
                {
                    if (afterPoint.HasValue) break;

                    var afterCurveAt = after.GetOffsetCurveAt(j, afterSide, Constants.DEFAULT_RADIUS_JUNCTION);

                    if (afterCurveAt != null)
                    {
                        var pts = new Point3dCollection();
                        primaryCurveAt.IntersectWith(afterCurveAt, Intersect.OnBothOperands, pts, IntPtr.Zero, IntPtr.Zero);

                        if (pts.Count > 0)
                        {
                            foreach (Point3d pt in pts)
                            {
                                if (pt != primaryCurveAt.StartPoint && pt != primaryCurveAt.EndPoint && pt != afterCurveAt.StartPoint && pt != afterCurveAt.EndPoint)
                                {
                                    afterPoint = pt;
                                    break;
                                }
                            }
                        }
                    }

                    j = isStart ? j + 1 : j - 1;
                }
            }

            if (!beforePoint.HasValue || !afterPoint.HasValue) return;

            using var beforeCircle = new Circle(beforePoint.Value, Vector3d.ZAxis, Constants.DEFAULT_RADIUS_JUNCTION);
            using var afterCircle = new Circle(afterPoint.Value, Vector3d.ZAxis, Constants.DEFAULT_RADIUS_JUNCTION);

            for (var i = index; i >= 0; i--)
            {
                var curve = primary.GetCurveAt(i);
                if (curve == null) continue;

                var pts = new Point3dCollection();
                beforeCircle.IntersectWith(curve, Intersect.OnBothOperands, pts, IntPtr.Zero, IntPtr.Zero);

                if (pts.Count == 1)
                {
                    primaryBeforePoint = pts[0];
                    var intersectPoint = primaryRoad.CentreLine.GetClosestPointTo(pts[0], false);
                    primaryChainStart = primaryRoad.CentreLine.GetDistAtPoint(intersectPoint);
                    break;
                }
            }

            for (var i = index; i < primary.NumberOfVertices - 1; i++)
            {
                var curve = primary.GetCurveAt(i);
                if (curve == null) continue;

                var pts = new Point3dCollection();
                afterCircle.IntersectWith(curve, Intersect.OnBothOperands, pts, IntPtr.Zero, IntPtr.Zero);

                if (pts.Count == 1)
                {
                    primaryAfterPoint = pts[0];
                    var intersectPoint = primaryRoad.CentreLine.GetClosestPointTo(pts[0], false);
                    primaryChainEnd = primaryRoad.CentreLine.GetDistAtPoint(intersectPoint);
                    break;
                }
            }

            for (var i = beforeStartIdx; LoopCheck(i, beforeEndIdx, isStart);)
            {
                var curve = before.GetCurveAt(i);
                if (curve == null) continue;

                var pts = new Point3dCollection();
                beforeCircle.IntersectWith(curve, Intersect.OnBothOperands, pts, IntPtr.Zero, IntPtr.Zero);

                if (pts.Count == 1)
                {
                    secondaryBeforePoint = pts[0];
                    var intersectPoint = secondaryRoad.CentreLine.GetClosestPointTo(pts[0], false);
                    secondaryBeforeChain = secondaryRoad.CentreLine.GetDistAtPoint(intersectPoint);
                    break;
                }

                i = isStart ? i + 1 : i - 1;
            }

            for (var i = afterStartIdx; LoopCheck(i, afterEndIdx, isStart);)
            {
                var curve = after.GetCurveAt(i);
                if (curve == null) continue;

                var pts = new Point3dCollection();
                afterCircle.IntersectWith(curve, Intersect.OnBothOperands, pts, IntPtr.Zero, IntPtr.Zero);

                if (pts.Count == 1)
                {
                    secondaryAfterPoint = pts[0];
                    var intersectPoint = secondaryRoad.CentreLine.GetClosestPointTo(pts[0], false);
                    secondaryAfterChain = secondaryRoad.CentreLine.GetDistAtPoint(intersectPoint);
                    break;
                }

                i = isStart ? i + 1 : i - 1;
            }


            if (primaryChainStart.HasValue && primaryChainEnd.HasValue && secondaryBeforeChain.HasValue && secondaryAfterChain.HasValue)
            {
                const int start = 0;

                primaryRoad.Segments.Junction(turn, primaryChainStart.Value, primaryChainEnd.Value);

                switch (turn)
                {
                    case Side.Left:
                        if (isStart)
                        {
                            secondaryRoad.Segments.Junction(Side.Left, start, secondaryBeforeChain.Value);
                            secondaryRoad.Segments.Junction(Side.Right, start, secondaryAfterChain.Value);
                        }
                        else
                        {
                            secondaryRoad.Segments.Junction(Side.Left, secondaryAfterChain.Value);
                            secondaryRoad.Segments.Junction(Side.Right, secondaryBeforeChain.Value);
                        }
                        break;
                    case Side.Right:
                        if (isStart)
                        {
                            secondaryRoad.Segments.Junction(Side.Left, start, secondaryAfterChain.Value);
                            secondaryRoad.Segments.Junction(Side.Right, start, secondaryBeforeChain.Value);
                        }
                        else
                        {
                            secondaryRoad.Segments.Junction(Side.Left, secondaryBeforeChain.Value);
                            secondaryRoad.Segments.Junction(Side.Right, secondaryAfterChain.Value);
                        }
                        break;
                }
            }

            if (primaryBeforePoint.HasValue && secondaryBeforePoint.HasValue && primaryAfterPoint.HasValue && secondaryAfterPoint.HasValue)
            {
                using var beforePrimaryLine = new Line(beforePoint.Value, primaryBeforePoint.Value);
                using var beforeSecondaryLine = new Line(beforePoint.Value, secondaryBeforePoint.Value);
                using var afterPrimaryLine = new Line(afterPoint.Value, primaryAfterPoint.Value);
                using var afterSecondaryLine = new Line(afterPoint.Value, secondaryAfterPoint.Value);

                var beforeStartAngle = turn == Side.Left ? beforePrimaryLine.Angle : beforeSecondaryLine.Angle;
                var beforeEndAngle = turn == Side.Left ? beforeSecondaryLine.Angle : beforePrimaryLine.Angle;
                var afterStartAngle = turn == Side.Left ? afterSecondaryLine.Angle : afterPrimaryLine.Angle;
                var afterEndAngle = turn == Side.Left ? afterPrimaryLine.Angle : afterSecondaryLine.Angle;


                var beforeArc = new Arc(beforePoint.Value, Constants.DEFAULT_RADIUS_JUNCTION, beforeStartAngle, beforeEndAngle)
                {
                    Layer = Constants.LAYER_DEF_POINTS
                };

                var afterArc = new Arc(afterPoint.Value, Constants.DEFAULT_RADIUS_JUNCTION, afterStartAngle, afterEndAngle)
                {
                    Layer = Constants.LAYER_DEF_POINTS
                };

                Fillets.Add(acBlkTblRec.AppendEntity(beforeArc));
                acTrans.AddNewlyCreatedDBObject(beforeArc, true);

                Fillets.Add(acBlkTblRec.AppendEntity(afterArc));
                acTrans.AddNewlyCreatedDBObject(afterArc, true);
            }

            static bool LoopCheck(int i, int e, bool forward)
            {
                return forward ? i < e : i >= e;
            }
        }

        private static Side? TurnForAngles(double primaryAngle, double secondaryAngle)
        {
            var right = AngleHelper.ForRightSide(secondaryAngle);
            var left = AngleHelper.ForLeftSide(secondaryAngle);

            if (Math.Abs(primaryAngle - right) < Constants.ANGLE_TOLERANCE) return Side.Right;
            if (Math.Abs(primaryAngle - left) < Constants.ANGLE_TOLERANCE) return Side.Left;

            return null;
        }
    }
}
