using System.Xml.Serialization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Structures.ObjectModel;
using CivSurface = Autodesk.Civil.DatabaseServices.Surface;

namespace Jpp.Ironstone.Housing.ObjectModel.Concept
{
    public class ConceptualPlotManager : AbstractDrawingObjectManager<ConceptualPlot>
    {
        [XmlIgnore]
        public CivSurface ProposedLevels { get; set; }

        [XmlIgnore]
        public CivSurface ExistingLevels { get; set; }

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
            SoilProperties soilProperties = DataService.Current.GetStore<StructureDocumentStore>(this.HostDocument.Name).SoilProperties;

            foreach (ObjectId surfaceId in SurfaceIds)
            {
                // Direct cast is safe as collection is filtered down to surfaces by Autocad
                CivSurface temp = (CivSurface)surfaceId.GetObject(OpenMode.ForRead);

                // Continue is not used, incase user has set the same surface as both
                if (temp.Name ==  soilProperties.ProposedGroundSurfaceName)
                {
                    ProposedLevels = temp;
                }
                if (temp.Name == soilProperties.ExistingGroundSurfaceName)
                {
                    ExistingLevels = temp;
                }
            }
        }
    }
}
