using System;
using System.ComponentModel;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Jpp.Ironstone.Core;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Structures.ObjectModel.TreeRings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Jpp.Ironstone.Structures.ObjectModel
{
    public class StructureDocumentStore : DocumentStore
    {
        public SoilProperties SoilProperties { get; set; }

        public StructureDocumentStore(Document doc, Type[] managerTypes, ILogger<CoreExtensionApplication> log, LayerManager layerManager, IConfiguration settings) : base(doc, managerTypes, log, layerManager, settings) { }

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
