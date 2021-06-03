using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Core;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Structures.ObjectModel;
using Jpp.Ironstone.Structures.ObjectModel.Foundations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jpp.Ironstone.Housing.ObjectModel.Detail
{
    // TODO: Review the entire class as this has been copied wholesale
    public partial class DetailPlotManager : AbstractDrawingObjectManager<DetailPlot>
    {
        private ILogger<CoreExtensionApplication> _logger;
        private SoilProperties _soilProperties;

        public DetailPlotManager(Document document, ILogger<CoreExtensionApplication> log, IConfiguration settings) : base(document, log, settings)
        {
            CommonConstructor();
        }

        public DetailPlotManager() : base()
        {
            CommonConstructor();
        }

        private void CommonConstructor()
        {
            _logger = CoreExtensionApplication._current.Container.GetRequiredService<ILogger<CoreExtensionApplication>>();
            _foundationGroups = new List<FoundationGroup>();
            _soilProperties = DataService.Current.GetStore<StructureDocumentStore>(HostDocument.Name).SoilProperties;
        }

        public override void UpdateDirty()
        {
            //TODO: Optimize
            base.UpdateDirty();
            UpdateAll();
        }

        public override void UpdateAll()
        {
            using (Transaction trans = HostDocument.Database.TransactionManager.StartTransaction())
            {
                base.UpdateAll();

                UpdateAllFoundations();
                foreach (DetailPlot detailPlot in ManagedObjects)
                {
                    detailPlot.DrawOnTop();
                }

                trans.Commit();
            }
        }
    }
}
