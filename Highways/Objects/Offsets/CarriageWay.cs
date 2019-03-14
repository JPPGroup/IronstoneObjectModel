using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Highways.ObjectModel.Abstract;
using Jpp.Ironstone.Highways.ObjectModel.Extensions;
using Jpp.Ironstone.Highways.ObjectModel.Factories;

namespace Jpp.Ironstone.Highways.ObjectModel.Objects.Offsets
{
    [Serializable]
    public class CarriageWay : CentreLineOffset, IParentObject
    {           
        [XmlIgnore] public double PavementWidth => GetPavementWidth();
        public List<OffsetIntersect> Intersections { get; private set; }
        public bool Ignore { get; set; }
        public Pavement Pavement { get; }

        public CarriageWay(double distance, double pavementWidth, SidesOfCentre side, CentreLine centreLine) : base(distance, side, OffsetTypes.CarriageWay, centreLine)
        {
            Intersections = new List<OffsetIntersect>();
            Ignore = false;
            Pavement = new Pavement(distance + pavementWidth, side, this);
        }

        public override void Clear()
        {
            base.Clear();
            Pavement.Clear();

            Intersections = new List<OffsetIntersect>();
            Ignore = false;
        }

        public override void Create()
        {
            base.Clear();

            var keepList = new List<Curve>();
            var wasteList = new List<Curve>();
            var offsetCurve = CentreLine.GetCurve().CreateOffset(Side, DistanceFromCentre);

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

            Pavement.Create();
        }

        private double GetPavementWidth()
        {
            return Pavement.DistanceFromCentre - DistanceFromCentre;
        }

        #region IParentObject Members

        void IParentObject.ResolveChildren()
        {
            Pavement.CarriageWay = this;
            Pavement.CentreLine = CentreLine;
        }

        #endregion
    }
}
