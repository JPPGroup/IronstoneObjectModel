using System;
using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;

namespace Jpp.Ironstone.DocumentManagement.ObjectModel
{
    public class DocumentManagementDocumentStore : DocumentStore
    {
        //private string _projectFileLocation;

       //public ProjectContainer Container { get; private set; }

        public LayoutSheetController LayoutSheetController { get; private set; }

        public DocumentManagementDocumentStore(Document doc, Type[] managerTypes, ILogger log, LayerManager layerManager) : base(doc, managerTypes, log, layerManager) { }
        
        protected override void Save()
        {
            SaveBinary("LayoutSheetController", LayoutSheetController);
            base.Save();
        }

        protected override void Load()
        {
            LayoutSheetController = LoadBinary<LayoutSheetController>("LayoutSheetController");
            base.Load();
        }
    }
}
