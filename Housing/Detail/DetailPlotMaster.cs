using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;

namespace Jpp.Ironstone.Housing.ObjectModel.Detail
{
    public class DetailPlotMaster : BlockDrawingObject
    {
        public string PlotTypeName { get; set; }

        public DetailPlotMaster() : base()
        {
        }

        public DetailPlotMaster(Database database) : base(database)
        {
        }
        
        public DetailPlotMaster(Document document) : base(document)
        {
        }

        public static DetailPlotMaster Create(Document doc, string plotTypeName)
        {
            BlockDrawingObject blockDrawingObject = BlockDrawingObject.Create(doc.Database, plotTypeName);

            DetailPlotMaster newPlotMaster = new DetailPlotMaster(doc);
            newPlotMaster.BaseObject = blockDrawingObject.BaseObject;
            newPlotMaster.PlotTypeName = plotTypeName;

            DataService.Current.GetStore<HousingDocumentStore>(doc.Name).GetManager<DetailPlotMasterManager>().Add(newPlotMaster);
            return newPlotMaster;
        }
    }
}
