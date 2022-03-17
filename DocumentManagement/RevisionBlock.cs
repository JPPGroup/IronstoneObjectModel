using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using EnumsNET;
using Jpp.Ironstone.Core.Autocad;
using System.ComponentModel;

namespace Jpp.Ironstone.DocumentManagement.ObjectModel
{
    public class RevisionBlock : BlockRefDrawingObject
    {
        public RevisionBlock(BlockRefDrawingObject reference) : base()
        {
            this._document = reference.Document;
            this._database = reference.Database;
            this.BaseObject = reference.BaseObject;

            GetProperties();
        }

        public string Revision
        {
            get { return GetProperty<string>("REV"); }
            set { SetProperty("REV", value); }
        }

        public string Description
        {
            get { return GetProperty<string>("DESCRIPTION"); }
            set { SetProperty("DESCRIPTION", value); }
        }

        public string DrawnBy
        {
            get { return GetProperty<string>("DRAWN"); }
            set { SetProperty("DRAWN", value); }
        }

        public string CheckedBy
        {
            get { return GetProperty<string>("CHECKED"); }
            set { SetProperty("CHECKED", value); }
        }

        public string Date
        {
            get { return GetProperty<string>("DATE"); }
            set { SetProperty("DATE", value); }
        }

        public static RevisionBlock Create(Database target, Point3d insertionPoint, string blockName)
        {
            var refObj = BlockRefDrawingObject.Create(target, insertionPoint, BlockDrawingObject.Get(target, blockName));
            return new RevisionBlock(refObj);
        }
    }
}
