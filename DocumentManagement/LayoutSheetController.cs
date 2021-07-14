using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Common;
using Jpp.Ironstone.Core;
using Jpp.Ironstone.Core.Autocad;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Jpp.Ironstone.DocumentManagement.ObjectModel
{
    public class LayoutSheetController
    {
        public SerializableDictionary<string, LayoutSheet> Sheets;
        private Database _document;
        private ILogger<CoreExtensionApplication> _logger;
        private IConfiguration _settings;

        public LayoutSheetController(ILogger<CoreExtensionApplication> logger, Database doc, IConfiguration settings)
        {
            Sheets = new SerializableDictionary<string, LayoutSheet>();
            _document = doc;
            _logger = logger;
            _settings = settings;
        }

        public void Scan()
        {
            Database acCurDb = _document;

            Transaction acTrans = acCurDb.TransactionManager.TopTransaction;
            DBDictionary layouts = acTrans.GetObject(acCurDb.LayoutDictionaryId, OpenMode.ForRead) as DBDictionary;

            // Step through and list each named layout and Model
            foreach (DBDictionaryEntry item in layouts)
            {
                if (!Sheets.ContainsKey(item.Key) && item.Key != "Model")
                {
                    Layout acLayout = acTrans.GetObject(item.Value, OpenMode.ForRead) as Layout;
                    LayoutSheet ls = new LayoutSheet(_logger, acLayout);

                    Sheets.Add(item.Key, ls);
                }
            }
        }

        public LayoutSheet AddLayout(string layoutName, PaperSize size)
        {
            using (Database template = new Database(false, true))
            {
                Transaction destTransaction = _document.TransactionManager.TopTransaction;
                SideLoad(template);
                //Database old = Application.DocumentManager.MdiActiveDocument.Database;

                Layout destinationLayout = destTransaction.GetObject(LayoutManager.Current.CreateLayout(layoutName), OpenMode.ForWrite) as Layout;
                //HostApplicationServices.WorkingDatabase = template;

                using (Transaction sourceTrans = template.TransactionManager.StartTransaction())
                {
                    //Layout layout = _document.Database.GetLayout(GetLayoutName(size));
                    Layout layout = template.GetLayout(GetLayoutName(size));
                    destinationLayout.CopyFrom(layout);
                    BlockTableRecord sourceBlockTableRecord =
                        sourceTrans.GetObject(layout.BlockTableRecordId, OpenMode.ForRead) as BlockTableRecord;
                    ObjectIdCollection sourceObjects = new ObjectIdCollection();
                    foreach (ObjectId objectId in sourceBlockTableRecord)
                    {
                        sourceObjects.Add(objectId);
                    }
                    IdMapping mapping = new IdMapping();
                    // TODO: Confirm ignore is correct option
                    _document.WblockCloneObjects(sourceObjects, destinationLayout.BlockTableRecordId, mapping, DuplicateRecordCloning.Ignore, false);
                }

                LayoutSheet resultSheet = new LayoutSheet(_logger, destinationLayout);
                Sheets.Add(resultSheet.Name, resultSheet);

                LayoutManager.Current.CurrentLayout = resultSheet.Name;
                Object acadObject = Application.AcadApplication;
                //Will this break in coreconsole?
                acadObject.GetType().InvokeMember("ZoomExtents",BindingFlags.InvokeMethod, null, acadObject, null);

                return resultSheet;
            }
        }

        public void RemoveDefaultLayouts()
        {
            Transaction trans = _document.TransactionManager.TopTransaction;

            DBDictionary layoutDic = trans.GetObject(_document.LayoutDictionaryId, OpenMode.ForRead, false) as DBDictionary;

            string pattern = @"\d+ - .+";

            LayoutManager acLayoutMgr = LayoutManager.Current;

            foreach (DBDictionaryEntry entry in layoutDic)
            {
                ObjectId layoutId = entry.Value;
                Layout layout = trans.GetObject(layoutId, OpenMode.ForRead) as Layout;

                if (!layout.LayoutName.StartsWith("Model"))
                {
                    Match m = Regex.Match(layout.LayoutName, pattern);
                    if (!m.Success)
                    {
                        acLayoutMgr.DeleteLayout(layout.LayoutName);
                        if(Sheets.ContainsKey(layout.LayoutName))
                            Sheets.Remove(layout.LayoutName);
                    }
                }
            }
        }

        private void SideLoad(Database template)
        {
            string templatePath = _settings["documentmanagement:defaultTemplateFile"];
            bool cleanup = false;
            if (templatePath.Equals("embedded", StringComparison.CurrentCultureIgnoreCase))
            {
                templatePath = Path.GetTempFileName();
                using (Stream s = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("Jpp.Ironstone.DocumentManagement.ObjectModel.Resources.CivilTemplate.dwg"))
                {
                    using (FileStream outStream = File.OpenWrite(templatePath))
                    {
                        // TODO: Add null checks/testing here
                        s.CopyTo(outStream);
                    }
                }

                cleanup = true;
            }

            template.ReadDwgFile(templatePath, FileOpenMode.OpenForReadAndAllShare, false, null);
            template.CloseInput(true);

            if (cleanup)
            {
                File.Delete(templatePath);
            }
        }

        private string GetLayoutName(PaperSize size)
        {
            switch (size)
            {
                case PaperSize.A0Landscape:
                    return "Civ_A0L";

                case PaperSize.A1Landscape:
                    return "Civ_A1L";

                case PaperSize.A2Landscape:
                    return "Civ_A2L";

                case PaperSize.A3Landscape:
                    return "Civ_A3L";

                case PaperSize.A0Portrait:
                    return "Civ_A0P";

                case PaperSize.A1Portrait:
                    return "Civ_A1P";

                case PaperSize.A2Portrait:
                    return "Civ_A2P";

                case PaperSize.A3Portrait:
                    return "Civ_A3P";

                case PaperSize.A4Portrait:
                    return "Civ_A4P";

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
