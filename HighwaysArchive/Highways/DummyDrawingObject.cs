using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.Autocad;

namespace Jpp.Ironstone.Highways.ObjectModel.Old
{
    public class DummyDrawingObject : DrawingObject
    {
        protected override void ObjectModified(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override void ObjectErased(object sender, ObjectErasedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public override void Generate()
        {
            throw new NotImplementedException();
        }

        public override void Erase()
        {
            throw new NotImplementedException();
        }

        public override Point3d Location { get; set; }
        public override double Rotation { get; set; }
    }
}
