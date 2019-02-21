using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core.Autocad;
using Jpp.jHub;

namespace Jpp.Ironstone.DocumentManagement.Objectmodel
{
    public class DocumentManagementDocumentStore : DocumentStore
    {
        private string ProjectFileLocation;

        public ProjectContainer container { get; private set; }

        public LayoutSheetController LayoutSheetController { get; private set; }

        public DocumentManagementDocumentStore(Document doc, Type[] ManagerTypes) : base(doc, ManagerTypes)
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
