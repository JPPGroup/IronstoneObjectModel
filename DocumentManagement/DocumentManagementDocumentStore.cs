using System;
using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.jHub;

namespace Jpp.Ironstone.DocumentManagement.ObjectModel
{
    public class DocumentManagementDocumentStore : DocumentStore
    {
        private string _projectFileLocation;

        public ProjectContainer Container { get; private set; }

        public LayoutSheetController LayoutSheetController { get; private set; }

        public DocumentManagementDocumentStore(Document doc, Type[] managerTypes, ILogger log) : base(doc, managerTypes, log)
        {
        }

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
