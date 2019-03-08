using System;
using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core.Autocad;

namespace Jpp.Ironstone.Highways.ObjectModel
{
    public class HighwaysDocumentStore : DocumentStore
    {
        public HighwaysDocumentStore(Document doc, Type[] managerTypes) : base(doc, managerTypes) { }
    }
}
