using System;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;

namespace Jpp.Ironstone.Highways.ObjectModel.Extensions
{
    //MOVE: To Core
    public static class PolylineExtension
    {
        public static DBObjectCollection ExplodeAndErase(this Polyline pLine)
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;
            var acDbObjColl = new DBObjectCollection();

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var actualPolyline = acTrans.GetObject(pLine.ObjectId, OpenMode.ForWrite) as Polyline;
                var acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                if (acBlkTbl != null)
                {
                    var acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    if (acBlkTblRec != null)
                    {
                        actualPolyline?.Explode(acDbObjColl);

                        foreach (Entity acEnt in acDbObjColl)
                        {
                            acBlkTblRec.AppendEntity(acEnt);
                            acTrans.AddNewlyCreatedDBObject(acEnt, true);
                        }

                        actualPolyline?.Erase();
                    }
                }
                acTrans.Commit();
            }

            return acDbObjColl;
        }
    }
}
