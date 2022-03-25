using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Microsoft.Extensions.Configuration;

namespace Jpp.Ironstone.DocumentManagement.ObjectModel
{
    public class ViewportDrawingObject : DrawingObject
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

        public void SetStandardScale(IConfiguration settings, double scale)
        {
            double[] settingScales = settings.GetSection("standardScales").Get<double[]>();
            List<double> scaleValue = new List<double>();
            foreach (double setting in settingScales)
            {
                scaleValue.Add(1000 / setting);
            }

            scaleValue = scaleValue.OrderByDescending(d => d).ToList();

            double standardScale = 0;
            foreach (double d in scaleValue)
            {
                if (d < scale)
                {
                    standardScale = d;
                    break;
                }
            }

            if (standardScale == 0)
                throw new ArgumentOutOfRangeException($"No appropriate scale found, requested {scale} equates to {1/scale}");
            
            SetScale(standardScale);
        }

        public void FocusOn(IConfiguration settings, Extents3d extents)
        {
            Transaction trans = _database.TransactionManager.TopTransaction;
            Viewport vp = trans.GetObject(BaseObject, OpenMode.ForWrite) as Viewport;
            Point3d center = (extents.MinPoint + ((extents.MaxPoint - extents.MinPoint) * 0.5));
            vp.ViewCenter = new Point2d(center.X, center.Y);

            double requiredHeight = extents.MaxPoint.Y - extents.MinPoint.Y;
            double requiredHeightScale = vp.Height / requiredHeight;

            double requiredWidth = extents.MaxPoint.X - extents.MinPoint.X;
            double requiredWidthScale = vp.Width / requiredWidth;

            SetStandardScale(settings, Math.Min(requiredHeightScale, requiredWidthScale));
        }
    }
}
