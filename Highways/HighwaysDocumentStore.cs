using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core.Autocad;

namespace Jpp.Ironstone.Highways.Objectmodel
{
    class HighwaysDocumentStore : DocumentStore
    {
        //public SoilProperties SoilProperties { get; set; }

        public HighwaysDocumentStore(Document doc, Type[] managerTypes) : base(doc, managerTypes) { }

        //protected override void Save()
        //{
        //    SaveBinary("SoilProperties", SoilProperties);
        //    base.Save();
        //}

        //protected override void Load()
        //{
        //    SoilProperties = LoadBinary<SoilProperties>("SoilProperties");
        //    base.Load();
        //}

    }
}
