using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.Autocad;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Jpp.Ironstone.Highways.ObjectModel.Roads
{
    public class RoadBuilder : DrawingObjectBuilder<Road>
    {
        private readonly Database _database;

        public RoadBuilder(Database database)
        {
            _database = database;
        }

        protected override Road CreateDrawingObject(IEnumerable<ObjectId> objectIds)
        {
            var idList = objectIds.ToList();
            var acTrans = _database.TransactionManager.TopTransaction;

            var entities = new List<Entity>();

            foreach (var objectId in idList)
            {
                var entity = (Entity) acTrans.GetObject(objectId, OpenMode.ForWrite);
                entities.Add(entity);
            }

            if(!entities.Any()) throw new ArgumentException(@"No entities found",nameof(entities));

            var first = entities.First();
            var acBlkTblRec = (BlockTableRecord)acTrans.GetObject(first.BlockId, OpenMode.ForWrite);

            var pLine = first.ToPolyline();
            entities.RemoveAt(0);
            first.Erase(true);

            pLine.Layer = Constants.LAYER_JPP_CENTRE_LINE;

            if (entities.Count > 0)
            {
                var integerCollection = pLine.JoinEntities(entities.ToArray());
                if (integerCollection.Count != entities.Count) throw new ArgumentException(@"Incorrect number of entities joined", nameof(entities));

                entities.ForEach(obj => obj.Erase(true));
            }

            var pLineId = acBlkTblRec.AppendEntity(pLine);
            acTrans.AddNewlyCreatedDBObject(pLine, true);

            return Road.CreateRoad(pLineId);
        }
        
        protected override bool IsConnected(ObjectId firstObjectId, ObjectId secondObjectId)
        {
            var acTrans = _database.TransactionManager.TopTransaction;
            var firstEntity = (Curve) acTrans.GetObject(firstObjectId, OpenMode.ForRead);
            var secondEntity = (Curve) acTrans.GetObject(secondObjectId, OpenMode.ForRead);

            switch (firstEntity)
            {
                case Line firstLine:
                    return secondEntity switch
                    {
                        Line _ => false,
                        Arc secondArc => LineConnectedToArc(firstLine, secondArc),
                        _ => throw new ArgumentOutOfRangeException(nameof(secondEntity), secondEntity, @"Type not handled"),
                    };
                case Arc firstArc:
                {
                        return secondEntity switch
                        {
                            Line secondLine => ArcConnectedToLine(firstArc, secondLine),
                            Arc secondArc => ArcConnectedToArc(firstArc, secondArc),
                            _ => throw new ArgumentOutOfRangeException(nameof(secondEntity), secondEntity, @"Type not handled"),
                        };
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(firstEntity), firstEntity, @"Type not handled");
            }
        }

        private static bool LineConnectedToArc(Line line, Arc arc)
        {
            Vector3d? lineVector = null;
            Vector3d? arcVector = null;

            if (line.StartPoint.IsEqualTo(arc.StartPoint))
            {
                lineVector = line.StartPoint.GetVectorTo(line.EndPoint);
                arcVector = arc.Center.GetVectorTo(arc.StartPoint);
            }
            if (line.StartPoint.IsEqualTo(arc.EndPoint))
            {
                lineVector = line.StartPoint.GetVectorTo(line.EndPoint);
                arcVector = arc.Center.GetVectorTo(arc.EndPoint);
            }
            if (line.EndPoint.IsEqualTo(arc.StartPoint))
            {
                lineVector = line.EndPoint.GetVectorTo(line.StartPoint);
                arcVector = arc.Center.GetVectorTo(arc.StartPoint);
            }
            if (line.EndPoint.IsEqualTo(arc.EndPoint))
            {
                lineVector = line.EndPoint.GetVectorTo(line.StartPoint);
                arcVector = arc.Center.GetVectorTo(arc.EndPoint);
            }

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (!lineVector.HasValue || !arcVector.HasValue) return false;

            var angleBetween = lineVector.Value.GetAngleTo(arcVector.Value);
            return Math.Abs(angleBetween - Constants.ANGLE_RADIANS_90_DEGREES) < Constants.ANGLE_TOLERANCE;
        }

        private static bool ArcConnectedToArc(Arc firstArc, Arc secondArc)
        {
            Vector3d? firstArcVector = null;
            Vector3d? secondArcVector = null;

            if (firstArc.StartPoint.IsEqualTo(secondArc.StartPoint))
            {
                firstArcVector = firstArc.Center.GetVectorTo(firstArc.StartPoint);
                secondArcVector = secondArc.Center.GetVectorTo(firstArc.StartPoint);
            }
            if (firstArc.StartPoint.IsEqualTo(secondArc.EndPoint))
            {
                firstArcVector = firstArc.Center.GetVectorTo(firstArc.StartPoint);
                secondArcVector = secondArc.Center.GetVectorTo(firstArc.EndPoint);
            }
            if (firstArc.EndPoint.IsEqualTo(secondArc.StartPoint))
            {
                firstArcVector = firstArc.Center.GetVectorTo(firstArc.EndPoint);
                secondArcVector = secondArc.Center.GetVectorTo(firstArc.StartPoint);
            }
            if (firstArc.EndPoint.IsEqualTo(secondArc.EndPoint))
            {
                firstArcVector = firstArc.Center.GetVectorTo(firstArc.EndPoint);
                secondArcVector = secondArc.Center.GetVectorTo(firstArc.EndPoint);
            }

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (!firstArcVector.HasValue || !secondArcVector.HasValue) return false;

            var angleBetween = firstArcVector.Value.GetAngleTo(secondArcVector.Value);
            return Math.Abs(angleBetween - Constants.ANGLE_RADIANS_0_DEGREES) < Constants.ANGLE_TOLERANCE || Math.Abs(angleBetween - Constants.ANGLE_RADIANS_180_DEGREES) < Constants.ANGLE_TOLERANCE;
        }

        private static bool ArcConnectedToLine(Arc arc, Line line)
        {
            Vector3d? arcVector = null;
            Vector3d? lineVector = null;
            
            if (arc.StartPoint.IsEqualTo(line.StartPoint))
            {
                arcVector = arc.Center.GetVectorTo(arc.StartPoint);
                lineVector = line.StartPoint.GetVectorTo(line.EndPoint);
            }
            if (arc.StartPoint.IsEqualTo(line.EndPoint))
            {
                arcVector = arc.Center.GetVectorTo(arc.StartPoint);
                lineVector = line.EndPoint.GetVectorTo(line.StartPoint);
            }
            if (arc.EndPoint.IsEqualTo(line.StartPoint))
            {
                arcVector = arc.Center.GetVectorTo(arc.EndPoint);
                lineVector = line.StartPoint.GetVectorTo(line.EndPoint);
            }
            if (arc.EndPoint.IsEqualTo(line.EndPoint))
            {
                arcVector = arc.Center.GetVectorTo(arc.EndPoint);
                lineVector = line.EndPoint.GetVectorTo(line.StartPoint);
            }

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (!arcVector.HasValue || !lineVector.HasValue) return false;

            var angleBetween = arcVector.Value.GetAngleTo(lineVector.Value);
            return Math.Abs(angleBetween - Constants.ANGLE_RADIANS_90_DEGREES) < Constants.ANGLE_TOLERANCE;
        }
    }
}
