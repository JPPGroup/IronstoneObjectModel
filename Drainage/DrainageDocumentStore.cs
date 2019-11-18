using System;
using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;

namespace Jpp.Ironstone.Drainage.ObjectModel
{
    public class DrainageDocumentStore : DocumentStore
    {
        public DrainageDocumentStore(Document doc, Type[] managerTypes, ILogger log, LayerManager layerManager, IUserSettings settings) : base(doc, managerTypes, log, layerManager, settings) { }
    }
}
