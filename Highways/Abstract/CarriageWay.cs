using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Highways.ObjectModel.Exceptions;
using Jpp.Ironstone.Highways.ObjectModel.Extensions;
using Jpp.Ironstone.Highways.ObjectModel.Factories;
using Jpp.Ironstone.Highways.ObjectModel.Objects;

namespace Jpp.Ironstone.Highways.ObjectModel.Abstract
{
    public abstract class CarriageWay : CentreLineOffset
    {           
        public List<OffsetIntersect> Intersections { get; private set; }
        public bool Ignore { get; set; }

        protected CarriageWay(SidesOfCentre side) : base(Constants.DEFAULT_CARRIAGE_WAY, side, OffsetTypes.CarriageWay)
        {            
            Intersections = new List<OffsetIntersect>();
            Ignore = false;
        }

        protected CarriageWay(double distance, SidesOfCentre side) : base(distance, side, OffsetTypes.CarriageWay)
        {
            Intersections = new List<OffsetIntersect>();
            Ignore = false;
        }

        public new virtual void Clear()
        {
            base.Clear();

            Intersections = new List<OffsetIntersect>();
            Ignore = false;
        }

        public virtual void Create(RoadCentreLine centreLine)
        {
            if (!IsValid(centreLine)) throw new ObjectException("Invalid offset for centre line.", centreLine.BaseObject);

            base.Clear();

            var keepList = new List<Curve>();
            var wasteList = new List<Curve>();
            var offsetCurve = centreLine.GetCurve().CreateOffset(Side, DistanceFromCentre);

            if (Intersections.Count == 0 & Ignore) return;
            keepList.Add(offsetCurve);

            foreach (var intersection in Intersections)
            {
                var hasIntersected = false;

                foreach (var r in keepList.ToList())
                {
                    var splitSets = r.TrySplit(intersection.Point);
                    if (splitSets == null) continue;

                    hasIntersected = true;
                    keepList.Remove(r);

                    var beforeCurve = splitSets[0] as Curve;
                    var afterCurve = splitSets[1] as Curve;

                    if (intersection.Before)
                    {
                        if (beforeCurve != null) keepList.Add(beforeCurve);
                        if (afterCurve != null) wasteList.Add(afterCurve);
                    }
                    else
                    {
                        if (beforeCurve != null) wasteList.Add(beforeCurve);
                        if (afterCurve != null) keepList.Add(afterCurve);
                    }
                }

                if (hasIntersected) continue;

                foreach (var w in wasteList.ToList())
                {
                    var splitSets = w.TrySplit(intersection.Point);
                    if (splitSets == null) continue;

                    wasteList.Remove(w);

                    var beforeCurve = splitSets[0] as Curve;
                    var afterCurve = splitSets[1] as Curve;
                    if (intersection.Before)
                    {
                        if (beforeCurve != null) keepList.Add(beforeCurve);
                        if (afterCurve != null) wasteList.Add(afterCurve);
                    }
                    else
                    {
                        if (beforeCurve != null) wasteList.Add(beforeCurve);
                        if (afterCurve != null) keepList.Add(afterCurve);
                    }
                }
            }

            var db = Application.DocumentManager.MdiActiveDocument.Database;
            var acTrans = TransactionFactory.CreateFromTop();
            var blockTable = (BlockTable)acTrans.GetObject(db.BlockTableId, OpenMode.ForRead);
            var blockTableRecord = (BlockTableRecord)acTrans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            foreach (var curve in keepList)
            {
                curve.Layer = Constants.LAYER_DEF_POINTS;

                Curves.Add(blockTableRecord.AppendEntity(curve));
                acTrans.AddNewlyCreatedDBObject(curve, true);
            }
        }
    }
}
