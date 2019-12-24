using System.Xml.Serialization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;
using CivSurface = Autodesk.Civil.DatabaseServices.Surface;

namespace Jpp.Ironstone.Housing.ObjectModel.Concept
{
    [Layer(Name = Constants.PLOT_BOUNDARY_LAYER)]
    public class ConceptualPlotManager : AbstractDrawingObjectManager<ConceptualPlot>
    {
        [XmlIgnore]
        public CivSurface ProposedLevels { get; set; } 

        public ConceptualPlotManager(Document document, ILogger log) : base(document, log)
        {
        }

        public ConceptualPlotManager() : base()
        {
        }

        public override void UpdateDirty()
        {
            //TODO: Optimize
            base.UpdateDirty();
            UpdateAll();
        }

        public override void UpdateAll()
        {
            if(ProposedLevels == null)
                GetSurfaces();

            // TODO: Check if this is required for when deserializing
            foreach (ConceptualPlot conceptualPlot in ManagedObjects)
            {
                conceptualPlot.Manager = this;
            }
            base.UpdateAll();
        }

        public void GetSurfaces()
        {
            //Get the target surface
            ObjectIdCollection SurfaceIds = CivilApplication.ActiveDocument.GetSurfaceIds();

            foreach (ObjectId surfaceId in SurfaceIds)
            {
                CivSurface temp = surfaceId.GetObject(OpenMode.ForRead) as CivSurface;
                if (temp.Name == "Proposed Ground")
                {
                    ProposedLevels = temp;
                }
            }
        }

        public void Add(ConceptualPlot plot)
        {
            ManagedObjects.Add(plot);
            plot.DirtyAdded = true;
        }
    }
}
