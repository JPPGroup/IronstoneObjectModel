using System.Xml.Serialization;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Highways.ObjectModel.Extensions;
using Jpp.Ironstone.Highways.ObjectModel.Factories;
using Jpp.Ironstone.Highways.ObjectModel.Objects;

namespace Jpp.Ironstone.Highways.ObjectModel.Abstract
{
    public abstract class RoadClosure
    {
        public long EndCarriageWayLinePtr { get; set; }
        public long EndPavementLinePtr { get; set; }
        public long PadPavementLeftLinePtr { get; set; }
        public long PadPavementRightLinePtr { get; set; }
        public bool Active { get; set; }
        public ClosureTypes Type { get; set; }
        public double Distance { get; set; }
        [XmlIgnore] public ObjectId EndCarriageWayLineId
        {
            get
            {
                if (EndCarriageWayLinePtr == 0) return ObjectId.Null;

                var acCurDb = Application.DocumentManager.MdiActiveDocument.Database;
                return acCurDb.GetObjectId(false, new Handle(EndCarriageWayLinePtr), 0);
            }
            set => EndCarriageWayLinePtr = value.Handle.Value;
        }
        [XmlIgnore] public ObjectId EndPavementLineId
        {
            get
            {
                if (EndPavementLinePtr == 0) return ObjectId.Null;

                var acCurDb = Application.DocumentManager.MdiActiveDocument.Database;
                return acCurDb.GetObjectId(false, new Handle(EndPavementLinePtr), 0);
            }
            set => EndPavementLinePtr = value.Handle.Value;
        }
        [XmlIgnore] public ObjectId PadPavementLeftLineId
        {
            get
            {
                if (PadPavementLeftLinePtr == 0) return ObjectId.Null;

                var acCurDb = Application.DocumentManager.MdiActiveDocument.Database;
                return acCurDb.GetObjectId(false, new Handle(PadPavementLeftLinePtr), 0);
            }
            set => PadPavementLeftLinePtr = value.Handle.Value;
        }
        [XmlIgnore] public ObjectId PadPavementRightLineId
        {
            get
            {
                if (PadPavementRightLinePtr == 0) return ObjectId.Null;

                var acCurDb = Application.DocumentManager.MdiActiveDocument.Database;
                return acCurDb.GetObjectId(false, new Handle(PadPavementRightLinePtr), 0);
            }
            set => PadPavementRightLinePtr = value.Handle.Value;
        }

        protected RoadClosure(ClosureTypes type)
        {
            Active = false;
            Type = type;
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
