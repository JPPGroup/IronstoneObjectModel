using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core.Autocad;

namespace Jpp.Ironstone.Structures.Objectmodel
{
    public class StructureDocumentStore : DocumentStore
    {
        public SoilProperties SoilProperties { get; set; }

        public StructureDocumentStore(Document doc, Type[] ManagerTypes) : base(doc, ManagerTypes)
        {
        }

        protected override void Save()
        {
            SaveBinary("SoilProperties", SoilProperties);
            base.Save();
        }

        protected override void Load()
        {
            SoilProperties = LoadBinary<SoilProperties>("SoilProperties");
            base.Load();
        }
    }
}
