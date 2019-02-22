using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Structures.Objectmodel.TreeRings;

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
            SoilProperties.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args)
            {
                this.GetManager<TreeRingManager>().AllDirty();
                Application.DocumentManager.MdiActiveDocument.SendStringToExecute("_regen ", false, false, false);
            };
            base.Load();
        }
    }
}
