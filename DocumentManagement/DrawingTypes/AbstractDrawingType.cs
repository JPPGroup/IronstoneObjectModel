using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;

namespace Jpp.Ironstone.DocumentManagement.Objectmodel.DrawingTypes
{
    public abstract class AbstractDrawingType
    {
        public abstract void SetDrawing(Document acDoc);
    }
}
