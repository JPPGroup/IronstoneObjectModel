using Autodesk.AutoCAD.ApplicationServices;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Highways.ObjectModel.Junctions;
using System;

namespace Jpp.Ironstone.Highways.ObjectModel.Roads
{
    [Serializable]
    [Layer(Name = Constants.LAYER_JPP_CENTRE_LINE)]
    [Layer(Name = Constants.LAYER_DEF_POINTS)]
    public class RoadManager : AbstractDrawingObjectManager<Road>
    {
        public JunctionController Junctions { get; set; }

        private RoadManager()
        {
            Junctions = new JunctionController();
        }

        public RoadManager(Document document, ILogger log) : base(document, log)
        {
            Junctions = new JunctionController();
        }
        
        public override void UpdateDirty()
        {
            base.UpdateDirty();
            GenerateLayout();
        }

        public override void UpdateAll()
        {
            base.UpdateAll();
            GenerateLayout();
        }

        private void GenerateLayout()
        {
            using var clearTrans = HostDocument.Database.TransactionManager.StartTransaction();
            try
            {
                foreach (var road in ManagedObjects) road.Clear();
                clearTrans.Commit();
            }
            catch (Exception e)
            {
                Log.LogException(e);
                clearTrans.Abort();
                return;
            }

            if (ActiveObjects.Count == 0) return;

            using var buildTrans = HostDocument.Database.TransactionManager.StartTransaction();
            try
            {
                Junctions.BuildJunctions(ActiveObjects, HostDocument.Database);

                foreach (var road in ActiveObjects)
                {
                    try
                    {
                        road.HasErrors = false;
                        road.Generate();
                    }
                    catch (Exception)
                    {
                        road.HasErrors = true;
                        throw;
                    }
                }

                buildTrans.Commit();
            }
            catch (Exception e)
            {
                Log.LogException(e);
                buildTrans.Abort();
            }
        }
    }
}
