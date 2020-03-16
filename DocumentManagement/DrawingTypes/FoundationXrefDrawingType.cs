using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.DocumentManagement.Objectmodel.DrawingTypes;

namespace Jpp.Ironstone.DocumentManagement.ObjectModel.DrawingTypes
{
    public class FoundationXrefDrawingType : AbstractXrefDrawingType
    {
        public FoundationXrefDrawingType()
        {
            DefaultFilename = "ConceptFoundations.dwg";
        }

        public override void SetDrawing(Document acDoc)
        {
        }

        public override void Initialise(Document doc)
        {
        }
    }
}
