using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Common;
using Jpp.Ironstone.Core;
using Jpp.Ironstone.DocumentManagement.Objectmodel.DrawingTypes;
using Jpp.Ironstone.DocumentManagement.ObjectModel.DrawingTypes;
using Jpp.Ironstone.DocumentManagement.ObjectModel.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Jpp.Ironstone.DocumentManagement.ObjectModel
{
    public class ProjectController : BaseNotify
    {
        private string _workingDirectory;
        private string _xrefDirectory;
        private FileSystemWatcher _watcher;
        private IServiceProvider _container;
        private IConfiguration _settings;

        public string ProjectNumber {
            get
            {
                return _projectModel.ProjectNumber;
            }
            set
            {
                _projectModel.ProjectNumber = value;
                OnPropertyChanged(nameof(ProjectNumber));
                SaveModel();
            }
        }

        public string ProjectName {
            get
            {
                return _projectModel.ProjectName;
            }
            set
            {
                _projectModel.ProjectName = value;
                OnPropertyChanged(nameof(ProjectName));
                SaveModel();
            }
        }
        public string Client {
            get
            {
                return _projectModel.Client;
            }
            set
            {
                _projectModel.Client = value;
                OnPropertyChanged(nameof(Client));
                SaveModel();
            }
        }

        public Dictionary<string, LayoutSheetController> SheetControllers { get; private set; }

        private ProjectModel _projectModel;
        private ILogger<CoreExtensionApplication> _logger;

        public ProjectController(IServiceProvider container, ILogger<CoreExtensionApplication> logger, IConfiguration settings, string workingDirectory)
        {
            _container = container;
            _settings = settings;
            _logger = logger;

            _workingDirectory = workingDirectory;
            _xrefDirectory = Path.Combine(_workingDirectory, "Xrefs");
            if (!Directory.Exists(_xrefDirectory))
            {
                Directory.CreateDirectory(_xrefDirectory);
            }

            SheetControllers = new Dictionary<string, LayoutSheetController>();

            ScanFolder();

            // TODO: Optimise the scan to only modified objects for perofmance
            // TODO: Replace this with individual methods instead
            _watcher = new FileSystemWatcher(_workingDirectory);
            _watcher.Created += (sender, args) => ScanFolder();
            _watcher.Changed += (sender, args) => ScanFolder();
            _watcher.Deleted += (sender, args) => ScanFolder();
            _watcher.Renamed += (sender, args) => ScanFolder();
            _watcher.EnableRaisingEvents = true;
        }

        private void LoadModel()
        {
            //TODO: Test this setting is present, verify default value from embedded settings
            string filePath = Path.Combine(_workingDirectory, _settings["documentmanagement:configfile"]);
            if (File.Exists(filePath))
            {
                string text = File.ReadAllText(filePath);
                _projectModel = JsonSerializer.Deserialize<ProjectModel>(text);
            }

            OnPropertyChanged(nameof(ProjectNumber));
            OnPropertyChanged(nameof(ProjectName));
            OnPropertyChanged(nameof(Client));
        }

        private void SaveModel()
        {
            _watcher.EnableRaisingEvents = false;
            string json = JsonSerializer.Serialize(_projectModel);
            File.WriteAllText(Path.Combine(_workingDirectory, _settings["documentmanagement::configfile"]), json);
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
            LoadModel();

            foreach(string s in Directory.GetFiles(_workingDirectory, "*.dwg"))
            {
                Database db = new Database(false, true);
                db.ReadDwgFile(s, FileOpenMode.OpenForReadAndAllShare, true, null);
                db.CloseInput(true);

                LayoutSheetController lsc = new LayoutSheetController(_logger, db, _settings);
                SheetControllers.Add(Path.GetFileNameWithoutExtension(s), lsc);
            }
        }
    }
}
