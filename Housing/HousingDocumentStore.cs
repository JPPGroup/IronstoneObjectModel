using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;
using System;

namespace Jpp.Ironstone.Housing.ObjectModel
{
    [Layer(Name = Constants.FOR_REVIEW_LEVEL_LAYER)]
    [Layer(Name = Constants.FOR_REVIEW_GRADIENT_LAYER)]
    public class HousingDocumentStore : DocumentStore
    {
        public HousingDocumentStore(Document doc, Type[] managerTypes, ILogger log, LayerManager layerManager, IUserSettings settings) : base(doc, managerTypes, log, layerManager, settings) { }
    }
}
