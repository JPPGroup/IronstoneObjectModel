using System;
using System.Xml.Serialization;
using Autodesk.AutoCAD.ApplicationServices;
using DesignCalculations.Engine;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;

namespace Jpp.Ironstone.Structures.ObjectModel.Appraisal
{
    [Layer(Name = "JPP_Structures_BeamCentreline")]
    [Layer(Name = "JPP_Structures_LoadbearingWall")]
    public class StructuralElementManager : AbstractDrawingObjectManager<DrawingObject>  
    {
        [XmlIgnore] 
        private CalculationSet _calculations;

        public StructuralElementManager(Document document, ILogger log) : base(document, log)
        {
        }

        public StructuralElementManager() : base()
        {
        }

        public override void Add(DrawingObject toBeManaged)
        {
            if (toBeManaged is IAppraisalObject ao)
            {
                _calculations.AddCalculation(ao.Calculation);
                base.Add(toBeManaged);
            }
            else
            {
                throw new InvalidOperationException("Object must be an appraisal object");
            }
        }
    }
}
