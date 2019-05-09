using System;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Core.Autocad.DrawingObjects;
using Jpp.Ironstone.Highways.ObjectModel.Extensions;
using Jpp.Ironstone.Highways.ObjectModel.Factories;
using Jpp.Ironstone.Highways.ObjectModel.Objects;

namespace Jpp.Ironstone.Highways.ObjectModel.Abstract
{
    public abstract class RoadClosure
    {
        public bool Active { get; set; }
        public ClosureTypes Type { get; set; }
        public ObjectId EndCarriageWayLineId { get; private set; }
        public ObjectId EndPavementLineId { get; private set; }
        public ObjectId PadPavementLeftLineId { get; private set; }
        public ObjectId PadPavementRightLineId { get; private set; }

        public double Distance { get; set; }

        protected RoadClosure(ClosureTypes type)
        {
            Active = false;
            Type = type;
            EndCarriageWayLineId = new ObjectId();
            EndPavementLineId = new ObjectId();

            PadPavementLeftLineId = new ObjectId();
            PadPavementRightLineId = new ObjectId();

            Distance = Constants.DEFAULT_PAVEMENT_WIDTH;
        }

        public void Clear()
        {
            var acTrans = TransactionFactory.CreateFromTop();

            if (!EndCarriageWayLineId.IsErased) acTrans.GetObject(EndCarriageWayLineId, OpenMode.ForWrite, true).Erase();
            if (!EndPavementLineId.IsErased) acTrans.GetObject(EndPavementLineId, OpenMode.ForWrite, true).Erase();
            if (!PadPavementLeftLineId.IsErased) acTrans.GetObject(PadPavementLeftLineId, OpenMode.ForWrite, true).Erase();
            if (!PadPavementRightLineId.IsErased) acTrans.GetObject(PadPavementRightLineId, OpenMode.ForWrite, true).Erase();

            EndCarriageWayLineId = ObjectId.Null;
            EndPavementLineId = ObjectId.Null;
            PadPavementLeftLineId = ObjectId.Null;
            PadPavementRightLineId = ObjectId.Null;
        }

        public void Create(RoadCentreLine centreLine)
        {
            if (!Active) return;

            var carriageLeftOffset = centreLine.GetCurve().CreateOffset(SidesOfCentre.Left, centreLine.CarriageWayLeft.DistanceFromCentre);
            var carriageRightOffset = centreLine.GetCurve().CreateOffset(SidesOfCentre.Right, centreLine.CarriageWayRight.DistanceFromCentre);
            var pavementLeftOffset = centreLine.GetCurve().CreateOffset(SidesOfCentre.Left, centreLine.CarriageWayLeft.Pavement.DistanceFromCentre);
            var pavementRightOffset = centreLine.GetCurve().CreateOffset(SidesOfCentre.Right, centreLine.CarriageWayRight.Pavement.DistanceFromCentre);

            Line carriageLine = null;
            Line pavementLine = null;
            Line extraLeft = null;
            Line extraRight = null;
              
            switch (Type)
            {
                case ClosureTypes.Start:
                    carriageLine = new Line(carriageLeftOffset.StartPoint, carriageRightOffset.StartPoint);
                    pavementLine = new Line(pavementLeftOffset.StartPoint, pavementRightOffset.StartPoint).CreateOffset(SidesOfCentre.Right, Distance) as Line;
                    if (pavementLine != null)
                    {
                        extraLeft = new Line(pavementLeftOffset.StartPoint, pavementLine.StartPoint);
                        extraRight = new Line(pavementRightOffset.StartPoint, pavementLine.EndPoint);
                    }
                    
                    break;
                case ClosureTypes.End:
                    carriageLine = new Line(carriageLeftOffset.EndPoint, carriageRightOffset.EndPoint);
                    pavementLine = new Line(pavementLeftOffset.EndPoint, pavementRightOffset.EndPoint).CreateOffset(SidesOfCentre.Left, Distance) as Line;
                    if (pavementLine != null)
                    {
                        extraLeft = new Line(pavementLeftOffset.EndPoint, pavementLine.StartPoint);
                        extraRight = new Line(pavementRightOffset.EndPoint, pavementLine.EndPoint);
                    }
                    break;
            }

            if (carriageLine != null & pavementLine != null)
            {
                var db = Application.DocumentManager.MdiActiveDocument.Database;
                var acTrans = TransactionFactory.CreateFromTop();
                var blockTable = (BlockTable)acTrans.GetObject(db.BlockTableId, OpenMode.ForRead);
                var blockTableRecord = (BlockTableRecord)acTrans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                carriageLine.Layer = Constants.LAYER_DEF_POINTS;
                pavementLine.Layer = Constants.LAYER_DEF_POINTS;
                extraLeft.Layer = Constants.LAYER_DEF_POINTS;
                extraRight.Layer = Constants.LAYER_DEF_POINTS;

                EndCarriageWayLineId = blockTableRecord.AppendEntity(carriageLine);
                EndPavementLineId = blockTableRecord.AppendEntity(pavementLine);

                PadPavementLeftLineId = blockTableRecord.AppendEntity(extraLeft);
                PadPavementRightLineId = blockTableRecord.AppendEntity(extraRight);

                acTrans.AddNewlyCreatedDBObject(carriageLine, true);
                acTrans.AddNewlyCreatedDBObject(pavementLine, true);
                acTrans.AddNewlyCreatedDBObject(extraLeft, true);
                acTrans.AddNewlyCreatedDBObject(extraRight, true);
            }            
        }
    }
}
