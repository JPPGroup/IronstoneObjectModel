using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;

namespace Jpp.Ironstone.Highways.Objectmodel.Extensions
{
    public static class PolylineExtension
    {
        public static DBObjectCollection ExplodeAndErase(this Polyline pLine)
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;
            var acDbObjColl = new DBObjectCollection();

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                if (acBlkTbl != null)
                {
                    var acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    if (acBlkTblRec != null)
                    {
                        pLine.Explode(acDbObjColl);

                        foreach (Entity acEnt in acDbObjColl)
                        {
                            acBlkTblRec.AppendEntity(acEnt);
                            acTrans.AddNewlyCreatedDBObject(acEnt, true);
                        }

                        pLine.Erase(true);
                    }
                }
                acTrans.Commit();
            }

            return acDbObjColl;
        }
    }
}
