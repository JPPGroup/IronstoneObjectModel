using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;
using System;

namespace Jpp.Ironstone.Housing.ObjectModel
{
    public class HousingDocumentStore : DocumentStore
    {
        public HousingDocumentStore(Document doc, Type[] managerTypes, ILogger log, LayerManager layerManager) : base(doc, managerTypes, log, layerManager) { }
    }
}
