using System;
using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core.Autocad;

namespace Jpp.Ironstone.Drainage.ObjectModel
{
    public class DrainageDocumentStore : DocumentStore
    {
        public DrainageDocumentStore(Document doc, Type[] managerTypes) : base(doc, managerTypes) { }
    }
}
