using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Common;
using Jpp.Ironstone.Core.Autocad;
using Jpp.jHub;

namespace Jpp.Ironstone.DocumentManagement.Objectmodel
{
    public class LayoutSheetController
    {
        public SerializibleDictionary<string, LayoutSheet> Sheets;

        public LayoutSheetController()
        {
            Sheets = new SerializibleDictionary<string, LayoutSheet>();
        }

        public void Scan()
        {
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            Transaction acTrans = acCurDb.TransactionManager.TopTransaction;
            DBDictionary layouts = acTrans.GetObject(acCurDb.LayoutDictionaryId, OpenMode.ForRead) as DBDictionary;

            // Step through and list each named layout and Model
            foreach (DBDictionaryEntry item in layouts)
            {
                if (!Sheets.ContainsKey(item.Key))
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
