using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Jpp.Ironstone.Core;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Core.UI.Autocad;
using Jpp.Ironstone.Housing.ObjectModel.Detail;

namespace Jpp.Ironstone.Housing.ObjectModel
{
    public class SharedHouseCommands
    {
        [IronstoneCommand]
        [CommandMethod("H_PlotMaster_New")]
        public static void CreateDetailPlotMaster()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor editor = doc.Editor;

            string blockName = editor.PromptForString("Please enter plot type name:");

            if(string.IsNullOrEmpty(blockName))
                return;

            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                DetailPlotMaster newPlotMaster = DetailPlotMaster.Create(doc, blockName);
                trans.Commit();
            }

            doc.SendStringToExecute($"-bedit {blockName}\n", true, false, false);
        }

        [IronstoneCommand]
        [CommandMethod("H_Plot_Add")]
        public static void CreateDetailPlot()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor editor = doc.Editor;

            List<string> keywords = new List<string>();
            List<DetailPlotMaster> masters = DataService.Current.GetStore<HousingDocumentStore>(doc.Name).GetManager<DetailPlotMasterManager>().ManagedObjects;
            foreach (DetailPlotMaster master in masters)
            {
                keywords.Add(master.PlotTypeName);
            }

            string blockName = editor.PromptForKeywords("Please select plot master:", keywords.ToArray());

            if(string.IsNullOrEmpty(blockName))
                return;

            Point3d? basePoint = editor.PromptForPosition("Please select base point: ");

            if (!basePoint.HasValue)
                return;

            string plotId = editor.PromptForString("Please enter plot name:");
            if(string.IsNullOrEmpty(plotId))
                return;

            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                DetailPlotMaster dpm = masters.First(m => m.PlotTypeName.Equals(blockName));

                //BlockDrawingObject newPlotMaster = BlockDrawingObject.Create(doc.Database, blockName);
                DetailPlot.Create(doc, dpm, basePoint.Value, plotId);
                trans.Commit();
            }
        }
    }
}
