using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.DocumentManagement.Objectmodel.DrawingTypes;
using Path = System.IO.Path;

namespace Jpp.Ironstone.DocumentManagement.ObjectModel.DrawingTypes
{
    public abstract class AbstractXrefDrawingType : AbstractDrawingType
    {
        public abstract void SetDrawing(Document acDoc);

        public BlockReference AddToDrawingAsXref(Document targetDrawing)
        {
            Transaction trans = targetDrawing.Database.TransactionManager.TopTransaction;

            string xrefPath = GetPath();

            var xId = targetDrawing.Database.AttachXref(xrefPath, Path.GetFileNameWithoutExtension(GetPath()));

            BlockTableRecord modelSpace = targetDrawing.Database.GetModelSpace(true);
            BlockReference xrefBlock = new BlockReference(Point3d.Origin, xId);
            modelSpace.AppendEntity(xrefBlock);
            trans.AddNewlyCreatedDBObject(xrefBlock, true);

            //Correct path to make relative. Not sure why autocad does an incorrect path in the first place...
            BlockTableRecord record = (BlockTableRecord) trans.GetObject(xId, OpenMode.ForWrite);
            string path = record.PathName;
            path = path.Substring(path.IndexOf("xref"));
            record.PathName = path;

            return xrefBlock;
        }

        public override string GetPath()
        {
            return ParentController.GetPath(DefaultFilename, true);
        }
    }
}
