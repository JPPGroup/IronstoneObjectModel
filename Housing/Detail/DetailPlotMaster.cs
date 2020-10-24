using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;

namespace Jpp.Ironstone.Housing.ObjectModel.Detail
{
    public class DetailPlotMaster : TemplateDrawingObject
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
            BlockDrawingObject blockDrawingObject = BlockDrawingObject.Create(doc, plotTypeName);

            DetailPlotMaster newPlotMaster = new DetailPlotMaster(doc);
            newPlotMaster.BaseObject = blockDrawingObject.BaseObject;
            newPlotMaster.PlotTypeName = plotTypeName;
            
            //Add origin circle
            Circle circ = new Circle(new Point3d(0,0,0), Vector3d.ZAxis, 0.1 );
            circ.Layer = "DEFPOINTS";
            Line x = new Line(new Point3d(-0.15,0,0), new Point3d(0.15,0,0));
            x.Layer = "DEFPOINTS";
            Line y = new Line(new Point3d(0, -0.15,0), new Point3d(0, 0.15,0));
            y.Layer = "DEFPOINTS";

            newPlotMaster.AddEntity(circ);
            newPlotMaster.AddEntity(x);
            newPlotMaster.AddEntity(y);

            DataService.Current.GetStore<HousingDocumentStore>(doc.Name).GetManager<DetailPlotMasterManager>().Add(newPlotMaster);
            return newPlotMaster;
        }

        public override void TransferDrawingObject(Document destination, ObjectId newId)
        {
            DetailPlotMaster newMaster = new DetailPlotMaster(destination);
            newMaster.PlotTypeName = this.PlotTypeName;
            newMaster.BaseObject = newId;

            DataService.Current.GetStore<HousingDocumentStore>(destination.Name).GetManager<DetailPlotMasterManager>().Add(newMaster);
            
        }
    }
}
