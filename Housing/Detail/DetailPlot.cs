using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Structures.ObjectModel.Foundations;

namespace Jpp.Ironstone.Housing.ObjectModel.Detail
{
    public class DetailPlot : BlockRefDrawingObject
    {
        public string PlotTypeName { get; set; }
        public string PlotId { get; set; }

        public DetailPlot(Document doc, BlockReference reference) : base(doc, reference)
        {
        }

        private DetailPlot()
        {
        }

        public static DetailPlot Create(Document doc, DetailPlotMaster master, Point3d basePoint, string PlotId)
        {
            BlockRefDrawingObject refDrawingObject = BlockRefDrawingObject.Create(doc.Database, basePoint, master);
            Transaction trans = doc.TransactionManager.TopTransaction;
            BlockReference reference = (BlockReference)trans.GetObject(refDrawingObject.BaseObject, OpenMode.ForRead);

            DetailPlot newDetailPlot = new DetailPlot(doc, reference);
            DataService.Current.GetStore<HousingDocumentStore>(doc.Name).GetManager<DetailPlotManager>().Add(newDetailPlot);
            newDetailPlot.PlotId = PlotId;
            newDetailPlot.PlotTypeName = master.PlotTypeName;
            return newDetailPlot;
        }

        public IReadOnlyCollection<LineDrawingObject> GetFoundationCentrelines()
        {
            DBObjectCollection collection = Explode(true);
            List<LineDrawingObject> lineDrawing = new List<LineDrawingObject>();

            foreach (DBObject dbObject in collection)
            {
                if (dbObject is Line)
                {
                    LineDrawingObject ldo = new LineDrawingObject(this._document);
                    ldo.BaseObject = dbObject.ObjectId;

                    if (ldo.HasKey(FoundationGroup.FOUNDATION_CENTRE_LOAD_KEY))
                    {
                        lineDrawing.Add(ldo);
                        continue;
                    }
                }

                dbObject.Erase();
            }

            return lineDrawing;
        }
    }
}
