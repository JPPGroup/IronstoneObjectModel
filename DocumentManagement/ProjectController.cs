using System;
using System.IO;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.DocumentManagement.Objectmodel.DrawingTypes;
using Jpp.Ironstone.DocumentManagement.ObjectModel.DrawingTypes;
using Microsoft.Extensions.DependencyInjection;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Jpp.Ironstone.DocumentManagement.ObjectModel
{
    public class ProjectController
    {
        private string _workingDirectory;
        private string _xrefDirectory;
        private FileSystemWatcher _watcher;
        private IServiceProvider _container;

        public string ProjectNumber { get; set; }
        public string ProjectName { get; set; }
        public string Client { get; set; }

        public ProjectController(IServiceProvider container, string workingDirectory)
        {
            _container = container;

            _workingDirectory = workingDirectory;
            _xrefDirectory = Path.Combine(_workingDirectory, "xref");
            if (!Directory.Exists(_xrefDirectory))
            {
                Directory.CreateDirectory(_xrefDirectory);
            }

            // TODO: Optimise the scan to only modified objects for perofmance
            _watcher = new FileSystemWatcher(_workingDirectory);
            _watcher.Created += (sender, args) => ScanFolder();
            _watcher.Deleted += (sender, args) => ScanFolder();
            _watcher.Renamed += (sender, args) => ScanFolder();
            _watcher.EnableRaisingEvents = true;
        }

        public T CreateDrawing<T>(string job, string filename = null) where T : AbstractDrawingType
        {
            //T newDrawing = (T) Activator.CreateInstance(typeof(T));
            T newDrawing = _container.GetRequiredService<T>();
            newDrawing.ParentController = this;
            string targetFilename = $"{job} - {newDrawing.DefaultFilename}";
            if(!string.IsNullOrEmpty(filename))
                targetFilename = $"{job} - {filename}";

            using (Document newDoc = CreateDrawing(targetFilename))
            {
                newDrawing.Initialise(newDoc);
                newDoc.CloseAndSave(GetPath(targetFilename, false));
            }
            return newDrawing;
        }

        public T GetXref<T>() where T : AbstractXrefDrawingType
        {
            T newDrawing = _container.GetRequiredService<T>();
            newDrawing.ParentController = this;

            string path = Path.Combine(_xrefDirectory, newDrawing.DefaultFilename);

            if (!File.Exists(path))
            {
                // TODO: Consider this for optimisation, does it need to create??
                using (Document newDoc = CreateXref(newDrawing.DefaultFilename))
                {
                    newDrawing.Initialise(newDoc);
                    newDoc.CloseAndSave(path);
                }
            }

            return newDrawing;
        }

        public Document CreateDrawing(string filename)
        {
            return Create(GetPath(filename, false));
        }

        public Document CreateXref(string filename)
        {
            
            return Create(GetPath(filename, true));
        }

        public string GetPath(string filename, bool xref)
        {
            if (xref)
            {
                return Path.Combine(_xrefDirectory, filename);
            }
            else
            {
                return Path.Combine(_workingDirectory, filename);
            }
        }

        private Document Create(string path)
        {
            _watcher.EnableRaisingEvents = false;
            //Create a new foundation xref
            DocumentCollection acDocMgr = Application.DocumentManager;
            Document acDoc = acDocMgr.Add(null);
            
            acDoc.Database.SaveAs(path, DwgVersion.Current);
            _watcher.EnableRaisingEvents = true;
            return acDoc;
        }

        private void ScanFolder()
        {

        }
    }
}
