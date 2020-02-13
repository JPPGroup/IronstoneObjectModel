using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Common;
using Jpp.Ironstone.Core.Autocad;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Jpp.Ironstone.DocumentManagement.ObjectModel
{
    public class LayoutSheetController
    {
        public SerializableDictionary<string, LayoutSheet> Sheets;

        public LayoutSheetController()
        {
            Sheets = new SerializableDictionary<string, LayoutSheet>();
        }   

        public void Scan(Document acDoc = null)
        {
            if(acDoc == null)
                acDoc = Application.DocumentManager.MdiActiveDocument;

            Database acCurDb = acDoc.Database;

            Transaction acTrans = acCurDb.TransactionManager.TopTransaction;
            DBDictionary layouts = acTrans.GetObject(acCurDb.LayoutDictionaryId, OpenMode.ForRead) as DBDictionary;

            // Step through and list each named layout and Model
            foreach (DBDictionaryEntry item in layouts)
            {
                if (!Sheets.ContainsKey(item.Key) && item.Key != "Model")
                {
                    Layout acLayout = acTrans.GetObject(item.Value, OpenMode.ForRead) as Layout;
                    LayoutSheet ls = new LayoutSheet();
                    ls.Name = item.Key;

                    Sheets.Add(item.Key, ls);
                }
            }

            foreach (LayoutSheet layoutSheet in Sheets.Values)
            {
                ParseTitleBlock(layoutSheet);
            }
        }

        private void ParseTitleBlock(LayoutSheet sheet)
        {
            Transaction acTrans = Application.DocumentManager.MdiActiveDocument.Database.TransactionManager.TopTransaction;

            Layout acLayout = Application.DocumentManager.MdiActiveDocument.Database.GetLayout(sheet.Name);
            BlockTableRecord btr = (BlockTableRecord)acTrans.GetObject(acLayout.BlockTableRecordId, OpenMode.ForRead);

            foreach (ObjectId objectId in btr)
            {
                Entity ent = (Entity)acTrans.GetObject(objectId, OpenMode.ForRead);
                if (ent.GetType() == typeof(BlockReference))
                {
                    BlockReference blkRef = (BlockReference)ent;
                    /*if (blkRef.IsDynamicBlock)
                    {
                        // Here you have a DynamicBlock reference.
                    }*/
                    string name = blkRef.Name;
                    bool dynamic = blkRef.IsDynamicBlock;

                    AttributeCollection attCol = blkRef.AttributeCollection;

                    foreach (ObjectId objID in attCol)
                    {
                        DBObject dbObj = acTrans.GetObject(objID, OpenMode.ForRead) as DBObject;

                        AttributeReference acAttRef = dbObj as AttributeReference;
                    }

                }
            }
        }
    }
}
