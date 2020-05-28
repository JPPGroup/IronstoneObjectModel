using System.Xml.Serialization;
using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Structures.ObjectModel;
using Unity;
using CivSurface = Autodesk.Civil.DatabaseServices.Surface;

namespace Jpp.Ironstone.Housing.ObjectModel.Detail
{
    // TODO: Review the entire class as this has been copied wholesale
    public class DetailPlotMasterManager : AbstractDrawingObjectManager<DetailPlotMaster>
    {
        private ILogger _logger;

        public DetailPlotMasterManager(Document document, ILogger log) : base(document, log)
        {
            _logger = CoreExtensionApplication._current.Container.Resolve<ILogger>();
        }

        public DetailPlotMasterManager() : base()
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
            /*if (ExistingLevels == null || ProposedLevels == null)
                GetSurfaces();


            if (ExistingLevels != null)
            {
                // TODO: Check if this is required for when deserializing
                foreach (DetailPlotMaster detailPlot in ManagedObjects)
                {
                    if (conceptualPlot.FoundationsEnabled)
                    {
                        #if !DEBUG
                        if (!SharedUIHelper.StructuresAvailable)
                        {
                            _logger.Entry("Foundations cannot be updated while the structures modules is not present",
                                Severity.Error);

                            continue;
                        }
                        #endif

                        if(conceptualPlot.EstimateFoundationLevel(ExistingLevels, ProposedLevels, Properties))
                            conceptualPlot.RenderFoundations(Properties.DepthBands, _logger);
                    }
                }
            }*/

            base.UpdateAll();
        }
    }
}
