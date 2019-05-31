using System;
using System.ComponentModel;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Structures.ObjectModel.TreeRings;

namespace Jpp.Ironstone.Structures.ObjectModel
{
    public class StructureDocumentStore : DocumentStore
    {
        public SoilProperties SoilProperties { get; set; }

        public StructureDocumentStore(Document doc, Type[] managerTypes, ILogger log) : base(doc, managerTypes, log) { }

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
