using System;
using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;

namespace Jpp.Ironstone.Highways.ObjectModel
{
    public class HighwaysDocumentStore : DocumentStore
    {      
        public HighwaysDocumentStore(Document doc, Type[] managerTypes, ILogger log) : base(doc, managerTypes, log) { }
    }
}
