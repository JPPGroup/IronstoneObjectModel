using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Structures.ObjectModel;
using Jpp.Ironstone.Structures.ObjectModel.Foundations;
using Unity;

namespace Jpp.Ironstone.Housing.ObjectModel.Detail
{
    // TODO: Review the entire class as this has been copied wholesale
    public partial class DetailPlotManager : AbstractDrawingObjectManager<DetailPlot>
    {
        private ILogger _logger;
        private SoilProperties _soilProperties;

        public DetailPlotManager(Document document, ILogger log) : base(document, log)
        {
            CommonConstructor();
        }

        public DetailPlotManager() : base()
        {
            CommonConstructor();
        }

        private void CommonConstructor()
        {
            _logger = CoreExtensionApplication._current.Container.Resolve<ILogger>();
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
            base.UpdateAll();

            UpdateAllFoundations();
        }
    }
}
