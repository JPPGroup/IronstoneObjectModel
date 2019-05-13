using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Jpp.Ironstone.Drainage.ObjectModel.Helpers
{
    public static class LeaderHelper
    {
        public static ObjectId GenerateLeader(string contents, Point3d leaderPosition, Point3d textPosition)
        {
            var leaderId = ObjectId.Null;
            var acCurDb = Application.DocumentManager.MdiActiveDocument.Database;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                if (acBlkTbl != null)
                {
                    var acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    if (acBlkTblRec != null)
                    {
                        using (var leader = new MLeader())
                        {
                            var mText = new MText
                            {
                                Width = 20,
                                Contents = contents,
                                Location = textPosition
                            };

                            mText.SetDatabaseDefaults();

                            leader.SetDatabaseDefaults();
                            leader.ContentType = ContentType.MTextContent;
                            leader.MText = mText;
                            leader.TextHeight = 2;
                            leader.AddLeaderLine(leaderPosition);
                            leader.Layer = Constants.LAYER_DEF_POINTS_NAME;

                            leaderId = acBlkTblRec.AppendEntity(leader);
                            acTrans.AddNewlyCreatedDBObject(leader, true);
                        }
                    }
                }

                acTrans.Commit();
            }

            return leaderId;
        }
    }
}
