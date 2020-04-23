using System;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Core.Autocad.DrawingObjects.Primitives;

namespace Jpp.Ironstone.Structures.ObjectModel.Appraisal.Elements
{
    public abstract class StructuralSupportLine : PolylineDrawingObject
    {
        protected override void ObjectModified(object sender, EventArgs e)
        {
        }

        protected override void ObjectErased(object sender, ObjectErasedEventArgs e)
        {
        }
    }
}
