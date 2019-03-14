using System;
using System.Xml.Serialization;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Highways.ObjectModel.Abstract;
using Jpp.Ironstone.Highways.ObjectModel.Extensions;
using Jpp.Ironstone.Highways.ObjectModel.Factories;

namespace Jpp.Ironstone.Highways.ObjectModel.Objects.Offsets
{
    [Serializable]
    public class Pavement : CentreLineOffset
    {
        [XmlIgnore] public CarriageWay CarriageWay { get; set; }

        public Pavement(double distance, SidesOfCentre side, CarriageWay carriageWay) : base(distance, side, OffsetTypes.Pavement, carriageWay.CentreLine)
        {
            CarriageWay = carriageWay;
        }

        public override void Create()
        {
            base.Clear();

            var db = Application.DocumentManager.MdiActiveDocument.Database;
            var acTrans = TransactionFactory.CreateFromTop();
            var blockTable = (BlockTable)acTrans.GetObject(db.BlockTableId, OpenMode.ForRead);
            var blockTableRecord = (BlockTableRecord)acTrans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            foreach (ObjectId obj in CarriageWay.Curves.Collection)
            {
                var entity = acTrans.GetObject(obj, OpenMode.ForRead) as Entity;
                if (entity is Curve curve)
                {
                    var curveOffset = curve.CreateOffset(Side, CarriageWay.PavementWidth);
                    if (curveOffset != null)
                    {
                        curveOffset.Layer = Constants.LAYER_DEF_POINTS;

                        Curves.Add(blockTableRecord.AppendEntity(curveOffset));
                        acTrans.AddNewlyCreatedDBObject(curveOffset, true);
                    }
                }                
            }
        }
    }
}
