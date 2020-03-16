using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Common;
using Jpp.Ironstone.DocumentManagement.ObjectModel;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Jpp.Ironstone.DocumentManagement.Objectmodel.DrawingTypes
{
    public abstract class AbstractDrawingType : DisposableManagedObject
    {
        public string DefaultFilename { get; protected set; }

        public ProjectController ParentController { get; set; }

        private Document _document;

        public abstract void Initialise(Document doc);

        /*public Database Database { get; private set; }
        
        protected void Load(string path)
        {
            Database = new Database(false, true);
            Database.ReadDwgFile(path, FileOpenMode.OpenForReadAndAllShare, false, null);
        }*/

        protected override void DisposeManagedResources()
        {
            if (_document != null)
            {
                _document.CloseAndDiscard();
            }
        }

        public Document GetDocument()
        {
            if(_document == null)
                _document = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.Open(GetPath());

            return _document;
        }

        public virtual string GetPath()
        {
            return ParentController.GetPath(DefaultFilename, false);
        }
    }
}

