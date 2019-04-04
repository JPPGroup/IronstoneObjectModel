using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Jpp.Ironstone.Core.Autocad;

namespace Jpp.Ironstone.DocumentManagement.Objectmodel.DrawingTypes
{
    public class CivilXrefDrawingType : AbstractDrawingType
    {
        public override void SetDrawing(Document acDoc)
        {
            //Make all the drawing changes
            using (DocumentLock dl = acDoc.LockDocument())
            {
                using (Transaction tr = acDoc.Database.TransactionManager.StartTransaction())
                {
                    //Get or create the survey text style
                    TextStyleTable tst = tr.GetObject(acDoc.Database.TextStyleTableId, OpenMode.ForRead) as TextStyleTable;
                    if (!tst.Has("JPP_Survey"))
                    {
                        tst.UpgradeOpen();
                        TextStyleTableRecord tstr = new TextStyleTableRecord();
                        tstr.FileName = "romans.shx";
                        tstr.Name = "JPP_Survey";
                        tst.Add(tstr);
                        tr.AddNewlyCreatedDBObject(tstr, true);
                    }

                    ObjectId surveyTextStyle = tst["JPP_Survey"];

                    Byte alpha = (Byte)(255 * (1));
                    Transparency trans = new Transparency(alpha);

                    //Iterate over all layer and set them to color 8, 0 transparency and continuous linetype
                    // Open the Layer table for read
                    LayerTable acLyrTbl = tr.GetObject(acDoc.Database.LayerTableId, OpenMode.ForRead) as LayerTable;
                    int layerCount = 0;
                    foreach (ObjectId id in acLyrTbl)
                    {
                        layerCount++;
                    }

                    foreach (ObjectId id in acLyrTbl)
                    {
                        LayerTableRecord ltr = tr.GetObject(id, OpenMode.ForWrite) as LayerTableRecord;
                        ltr.IsLocked = false;
                        ltr.Color = Autodesk.AutoCAD.Colors.Color.FromColorIndex(Autodesk.AutoCAD.Colors.ColorMethod.ByColor, 8);
                        ltr.LinetypeObjectId = acDoc.Database.ContinuousLinetype;
                        ltr.LineWeight = acDoc.Database.Celweight;
                        ltr.Transparency = trans;
                    }


                    //Get all model space drawing objects
                    TypedValue[] tv = new TypedValue[1];
                    tv.SetValue(new TypedValue(67, 0), 0);
                    SelectionFilter sf = new SelectionFilter(tv);
                    PromptSelectionResult psr = acDoc.Editor.SelectAll(sf);

                    foreach (SelectedObject so in psr.Value)
                    {
                        //For each object set its color, transparency, lineweight and linetype to ByLayer
                        Entity obj = tr.GetObject(so.ObjectId, OpenMode.ForWrite) as Entity;
                        obj.ColorIndex = 256;
                        obj.LinetypeId = acDoc.Database.Celtype;
                        obj.LineWeight = acDoc.Database.Celweight;

                        //Flatten
                        obj.Flatten();

                        //Change all text to Romans
                        if (obj is DBText)
                        {
                            DBText text = obj as DBText;
                            text.Position = new Point3d(text.Position.X, text.Position.Y, 0);
                            text.Height = 0.4;
                            text.TextStyleId = surveyTextStyle;
                        }
                        if (obj is MText)
                        {
                            MText text = obj as MText;
                            text.Location = new Point3d(text.Location.X, text.Location.Y, 0);
                            text.Height = 0.4;
                            text.TextStyleId = surveyTextStyle;
                        }

                    }
                    
                    //Iterate over all blocks
                    BlockTable blkTable = (BlockTable)tr.GetObject(acDoc.Database.BlockTableId, OpenMode.ForRead);
                    foreach (ObjectId id in blkTable)
                    {
                        BlockTableRecord btRecord = (BlockTableRecord)tr.GetObject(id, OpenMode.ForRead);
                        if (!btRecord.IsLayout)
                        {
                            foreach (ObjectId childId in btRecord)
                            {
                                //For each object set its color, transparency, lineweight and linetype to ByLayer
                                Entity obj = tr.GetObject(childId, OpenMode.ForWrite) as Entity;
                                obj.ColorIndex = 256;
                                obj.LinetypeId = acDoc.Database.Celtype;
                                obj.LineWeight = acDoc.Database.Celweight;

                                //Adjust Z values
                                obj.Flatten();
                            }
                        }
                    }

                    //Run the cleanup commands
                    acDoc.Database.PurgeAll();
                    acDoc.Database.Audit(true, false);

                    tr.Commit();
                }
            }
        }
    }
}
