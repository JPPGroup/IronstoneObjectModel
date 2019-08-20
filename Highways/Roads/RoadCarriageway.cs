using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Core.Autocad;
using System;

namespace Jpp.Ironstone.Highways.ObjectModel.Roads
{
    [Serializable]
    public class RoadCarriageway
    {
        public PersistentObjectIdCollection RightObjects { get; set; }
        public PersistentObjectIdCollection LeftObjects { get; set; }

        public RoadCarriageway()
        {
            RightObjects = new PersistentObjectIdCollection();
            LeftObjects = new PersistentObjectIdCollection();
        }

        public void Generate(Road road)
        {
            var acTrans = road.BaseObject.Database.TransactionManager.TopTransaction;
            var acBlkTblRec = (BlockTableRecord)acTrans.GetObject(road.CentreLine.BlockId, OpenMode.ForWrite);

            var centreLine = road.CentreLine;
            var segments = road.Segments.Collection;

            for (var i = 0; i < segments.Count; i++)
            {
                var segment = segments[i];

                var sp = centreLine.GetPointAtDist(segment.Chainage);
                var ep = centreLine.EndPoint;

                if (segments.Count != 1 && i + 1 < segments.Count)
                {
                    ep = centreLine.GetPointAtDist(segments[i + 1].Chainage);
                }

                var centreSegment = centreLine.GetSectionBetween(sp, ep);
                var leftCurve = (Polyline) centreSegment.CreateOffset(Side.Left, segment.Properties.LeftCarriagewayWidth);
                var rightCurve = (Polyline) centreSegment.CreateOffset(Side.Right, segment.Properties.RightCarriagewayWidth);

                if (leftCurve != null)
                {
                    leftCurve.Layer = Constants.LAYER_DEF_POINTS;
                    LeftObjects.Add(acBlkTblRec.AppendEntity(leftCurve));
                    acTrans.AddNewlyCreatedDBObject(leftCurve, true);
                }

                if (rightCurve != null)
                {
                    rightCurve.Layer = Constants.LAYER_DEF_POINTS;
                    RightObjects.Add(acBlkTblRec.AppendEntity(rightCurve));
                    acTrans.AddNewlyCreatedDBObject(rightCurve, true);
                }
            }
        }

        public void Clear(Road road)
        {
            var acTrans = road.BaseObject.Database.TransactionManager.TopTransaction;

            foreach (ObjectId obj in RightObjects.Collection)
            {
                if (!obj.IsErased) acTrans.GetObject(obj, OpenMode.ForWrite).Erase();
            }

            foreach (ObjectId obj in LeftObjects.Collection)
            {
                if (!obj.IsErased) acTrans.GetObject(obj, OpenMode.ForWrite).Erase();
            }

            RightObjects.Clear();
            LeftObjects.Clear();
        }
        
        public static Polyline GetCarriageway(Road road, Side side)
        {
            var segments = road.Segments;
            var centreLine = road.CentreLine;

            Polyline returnCurve = null;

            for (var i = 0; i < segments.Count; i++)
            {
                var segment = segments[i];

                var sp = centreLine.GetPointAtDist(segment.Chainage);
                var ep = centreLine.EndPoint;

                if (segments.Count != 1 && i + 1 < segments.Count)
                {
                    ep = centreLine.GetPointAtDist(segments[i + 1].Chainage);
                }

                var centreSegment = centreLine.GetSectionBetween(sp, ep);
                Polyline curve = null;
                switch (side)
                {
                    case Side.Left:
                        curve = (Polyline) centreSegment.CreateOffset(Side.Left, segment.Properties.LeftCarriagewayWidth);
                        break;
                    case Side.Right:
                        curve = (Polyline) centreSegment.CreateOffset(Side.Right, segment.Properties.RightCarriagewayWidth);
                        break;
                }

                if (curve == null) throw new ArgumentException(@"Side not valid", nameof(side));

                if (returnCurve != null && i > 0)
                {
                    returnCurve.JoinEntity(curve);
                }
                else
                {
                    returnCurve = curve;
                }
            }

            return returnCurve;
        }
    }
}
