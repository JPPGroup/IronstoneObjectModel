using System;
using System.Drawing;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.Autocad;

namespace Jpp.Ironstone.DocumentManagement.ObjectModel
{
    class ViewportDrawingObject : DrawingObject
    {
        protected ViewportDrawingObject() : base()
        { }

        public ViewportDrawingObject(Viewport baseObject) : base()
        {
            BaseObject = baseObject.Id;
        }

        public static ViewportDrawingObject Create(Layout layout, double Bottom, double Top, double Left, double Right)
        {
            Transaction trans = layout.Database.TransactionManager.TopTransaction;
            Viewport vp = new Viewport();
            var btr = (BlockTableRecord)trans.GetObject(layout.BlockTableRecordId, OpenMode.ForWrite);

            btr.AppendEntity(vp);
            trans.AddNewlyCreatedDBObject(vp, true);

            ViewportDrawingObject result = new ViewportDrawingObject(vp);
            result.SetDimensions(Bottom, Top, Left, Right);
            return result;
        }

        protected override void ObjectModified(object sender, EventArgs e)
        {
        }

        protected override void ObjectErased(object sender, ObjectErasedEventArgs e)
        {
        }

        public override Point3d Location { get; set; }

        public override void Generate()
        {
        }

        public override double Rotation { get; set; }

        public override void Erase()
        {
            throw new NotImplementedException();
        }

        public override Rectangle GetBoundingBox()
        {
            throw new NotImplementedException();
        }

        public void SetDimensions(double Bottom, double Top, double Left, double Right)
        {
            Transaction trans = _database.TransactionManager.TopTransaction;
            Viewport vp = trans.GetObject(BaseObject, OpenMode.ForWrite) as Viewport;

            vp.Width = Right - Left;
            vp.Height = Top - Bottom;
            vp.CenterPoint = new Point3d(Left + vp.Width / 2, Bottom + vp.Height / 2, 0);
        }

        public void SetScale(double scale)
        {
            Transaction trans = _database.TransactionManager.TopTransaction;
            Viewport vp = trans.GetObject(BaseObject, OpenMode.ForWrite) as Viewport;
            vp.CustomScale = scale;
        }
    }
}
