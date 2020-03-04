using System.Collections.Generic;
using System.Xml.Serialization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Jpp.Ironstone.Core;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Structures.ObjectModel;
using Unity;
using CivSurface = Autodesk.Civil.DatabaseServices.Surface;

namespace Jpp.Ironstone.Housing.ObjectModel.Concept
{
    public class ConceptualPlotManager : AbstractDrawingObjectManager<ConceptualPlot>
    {
        [XmlIgnore]
        public CivSurface ProposedLevels { get; set; }

        [XmlIgnore]
        public CivSurface ExistingLevels { get; set; }

        [XmlIgnore]
        public SoilProperties Properties
        {
            get
            {
                if (_properties == null)
                {
                    _properties = DataService.Current.GetStore<StructureDocumentStore>(this.HostDocument.Name).SoilProperties;
                }

                return _properties;
            }
        }

        private SoilProperties _properties;
        private ILogger _logger;

        public ConceptualPlotManager(Document document, ILogger log) : base(document, log)
        {
            _logger = CoreExtensionApplication._current.Container.Resolve<ILogger>();
        }

        public ConceptualPlotManager() : base()
        {
            _logger = CoreExtensionApplication._current.Container.Resolve<ILogger>();
        }

        public override void UpdateDirty()
        {
            //TODO: Optimize
            base.UpdateDirty();
            UpdateAll();
        }

        public override void UpdateAll()
        {
            if (ExistingLevels == null || ProposedLevels == null)
                GetSurfaces();


            if (ExistingLevels != null)
            {
                // TODO: Check if this is required for when deserializing
                foreach (ConceptualPlot conceptualPlot in ManagedObjects)
                {
                    if (conceptualPlot.FoundationsEnabled)
                    {
                        if(!SharedUIHelper.StructuresAvailable)
                            _logger.Entry("Foundations cannot be updated while the structures modules is not present", Severity.Error);

                        if(conceptualPlot.EstimateFoundationLevel(ExistingLevels, ProposedLevels, Properties))
                            conceptualPlot.RenderFoundations(Properties.DepthBands);
                    }
                }
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
