using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;
using System;
using Jpp.Ironstone.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Jpp.Ironstone.Housing.ObjectModel
{
    [Layer(Name = Constants.FOR_REVIEW_LEVEL_LAYER)]
    [Layer(Name = Constants.FOR_REVIEW_GRADIENT_LAYER)]
    [Layer(Name = Constants.PLOT_BOUNDARY_LAYER)]
    public class HousingDocumentStore : DocumentStore
    {
        public HousingDocumentStore(Document doc, Type[] managerTypes, ILogger<CoreExtensionApplication> log, LayerManager layerManager, IConfiguration settings) : base(doc, managerTypes, log, layerManager, settings) { }
    }
}
