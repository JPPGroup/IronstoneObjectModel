using System;
using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Jpp.Ironstone.DocumentManagement.ObjectModel
{
    public class DocumentManagementDocumentStore : DocumentStore
    {
        //private string _projectFileLocation;

       //public ProjectContainer Container { get; private set; }

        //public LayoutSheetController LayoutSheetController { get; private set; }

        public DocumentManagementDocumentStore(Document doc, Type[] managerTypes, ILogger<CoreExtensionApplication> log, LayerManager layerManager, IConfiguration settings) : base(doc, managerTypes, log, layerManager, settings) { }
        
        protected override void Save()
        {
            //SaveBinary("LayoutSheetController", LayoutSheetController);
            base.Save();
        }

        protected override void Load()
        {
            //LayoutSheetController = LoadBinary<LayoutSheetController>("LayoutSheetController");
            base.Load();
        }
    }
}
