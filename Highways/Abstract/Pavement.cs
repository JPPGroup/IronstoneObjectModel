using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Highways.ObjectModel.Extensions;
using Jpp.Ironstone.Highways.ObjectModel.Factories;
using Jpp.Ironstone.Highways.ObjectModel.Objects;

namespace Jpp.Ironstone.Highways.ObjectModel.Abstract
{
    public abstract class Pavement : CentreLineOffset
    {
        protected Pavement(double distance, SidesOfCentre side) : base(distance, side, OffsetTypes.Pavement) { }

        protected virtual void Create(CarriageWay carriageWay, RoadCentreLine centreLine)
        {
            var db = Application.DocumentManager.MdiActiveDocument.Database;
            var acTrans = TransactionFactory.CreateFromTop();
            var blockTable = (BlockTable)acTrans.GetObject(db.BlockTableId, OpenMode.ForRead);
            var blockTableRecord = (BlockTableRecord)acTrans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);
            var offsetDist = DistanceFrom(carriageWay);

            foreach (ObjectId obj in carriageWay.Curves.Collection)
            {
                var entity = acTrans.GetObject(obj, OpenMode.ForRead) as Entity;
                if (entity is Curve curve)
                {
                    var curveOffset = curve.CreateOffset(Side, offsetDist);
                    if (curveOffset != null)
                    {
                        curveOffset.Layer = Constants.LAYER_DEF_POINTS;

                        Curves.Add(blockTableRecord.AppendEntity(curveOffset));
                        acTrans.AddNewlyCreatedDBObject(curveOffset, true);
                    }
                }                
            }
        }

        private double DistanceFrom(CentreLineOffset carriageWay)
        {
            return DistanceFromCentre - carriageWay.DistanceFromCentre;
        }
    }
}
