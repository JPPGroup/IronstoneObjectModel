using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.Autocad.DrawingObjects;
using Jpp.Ironstone.Highways.ObjectModel.Factories;
using Jpp.Ironstone.Highways.ObjectModel.Objects;

namespace Jpp.Ironstone.Highways.ObjectModel
{
    [Serializable]
    public class HighwaysManager : AbstractDrawingObjectManager
    {
        private bool _finalized;

        public List<Road> Roads { get; set; }
        public PersistentObjectIdCollection JunctionOffsetCollection { get; private set; }
        [XmlIgnore] public List<Junction> Junctions { get; set; }

        public HighwaysManager(Document document) : base(document) 
        {
            JunctionOffsetCollection = new PersistentObjectIdCollection();
        }

        private HighwaysManager()
        {
            JunctionOffsetCollection = new PersistentObjectIdCollection();
        }

        public void FinalizeLayout()
        {
            _finalized = true;
            Clear();
        }

        public void Highlight()
        {
            foreach (var road in Roads)
            {
                road.Highlight();
            }
        }

        public void Unhighlight()
        {
            foreach (var road in Roads)
            {
                road.Unhighlight();
            }
        }

        public void InitialiseFromCurves(IEnumerable<Curve> curves, bool generateLayout = false)
        {
            Clear();

            var curveList = curves.ToList();
            if (!curveList.Any()) return;

            Roads = BuildRoadsFromCurves(curveList);            
            Junctions = BuildJunctionsFromRoads(Roads);

            if (generateLayout) GenerateLayout();
        }

        public void GenerateLayout()
        {
            RemoveOffsets();
            
            GenerateJunctionsCarriageWay();
            GenerateRoads();
        }
       
        private void GenerateRoads()
        {        
            if (Roads == null || Roads.Count == 0) return;

            using (var acTrans = TransactionFactory.CreateFromNew())
            {
                foreach (var road in Roads) road.Generate();
                
                acTrans.Commit();
            }
        }

        private void GenerateJunctionsCarriageWay()
        {
            if (Junctions == null || Junctions.Count == 0) return;

            var allJunctionArcs = new List<Curve>();
            foreach (var junction in Junctions)
            {
                var arcs = junction.GenerateCarriageWayArcs();
                if (arcs == null) continue;

                allJunctionArcs.AddRange(arcs);
            }

            if (allJunctionArcs.Count == 0) return;

            var db = Application.DocumentManager.MdiActiveDocument.Database;

            using (var acTrans = TransactionFactory.CreateFromNew())
            {
                var blockTable = (BlockTable)acTrans.GetObject(db.BlockTableId, OpenMode.ForRead);
                var blockTableRecord = (BlockTableRecord)acTrans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                foreach (var arc in allJunctionArcs)
                {
                    arc.Layer = Constants.LAYER_DEF_POINTS;

                    JunctionOffsetCollection.Add(blockTableRecord.AppendEntity(arc));
                    acTrans.AddNewlyCreatedDBObject(arc, true);
                }

                acTrans.Commit();
            }
        }

        private void RemoveOffsets()
        {
            using (var acTrans = TransactionFactory.CreateFromNew())
            {
                foreach (ObjectId obj in JunctionOffsetCollection.Collection)
                {
                    if (!obj.IsErased)
                    {
                        acTrans.GetObject(obj, OpenMode.ForWrite,true).Erase();
                    }
                }

                JunctionOffsetCollection.Clear();

                if (Roads != null && Roads.Count > 0) Roads.ForEach(r => r.Reset());
                acTrans.Commit();
            }            
        }

        private static List<Road> BuildRoadsFromCurves(IEnumerable<Curve> curves)
        {
            var curveList = curves.ToList();
            if (!curveList.Any()) return null;

            var roads = new List<Road>();
            var road = new Road();
            var initCurve = curveList.FirstOrDefault();
            var connection = true;

            road.CentreLines.Add(initCurve);
            curveList.Remove(initCurve);

            while (connection)
            {
                connection = false;
                foreach (var curve in curveList)
                {
                    if (!road.CentreLines.IsConnected(curve)) continue;

                    connection = true;
                    road.CentreLines.Add(curve);
                    curveList.Remove(curve);

                    break;
                }
            }

            roads.Add(road);
            if (curveList.Count != 0) roads.AddRange(BuildRoadsFromCurves(curveList));

            var count = 1;
            foreach (var nameRoad in roads)
            {
                nameRoad.Name = $"Road {count}";
                count++;
            }

            return roads;
        }

        private static List<Junction> BuildJunctionsFromRoads(IEnumerable<Road> roads)
        {
            var roadList = roads.ToList();
            if (!roadList.Any()) return null;

            var junctions = new List<Junction>();

            foreach (var road in roadList)
            {
                var roadJunctions = road.CreateJunctions(roadList);
                if (roadJunctions != null) junctions.AddRange(roadJunctions);
            }

            return junctions;
        }

        private bool ValidateRoads()
        {
            return Roads.All(road => road.Valid);
        }

        public override void UpdateDirty()
        {
            throw new NotImplementedException();
        }

        public override void UpdateAll()
        {
            if (ValidateRoads())
            {

            }
            else
            {
                FinalizeLayout();
            }
        }

        public override void Clear()
        {
            if (!_finalized) RemoveOffsets();

            JunctionOffsetCollection = new PersistentObjectIdCollection();
            Junctions = null;
            Roads = null;
        }

        public override void AllDirty()
        {
            throw new NotImplementedException();
        }

        public override void ActivateObjects()
        {
            //TODO: Needs reviewing...
            if (_finalized) return;

            Roads.ForEach(delegate (Road road)
                {
                    var centreLines = road.CentreLines.ToList();
                    centreLines.ForEach(c => c.CreateActiveObject());
                }
            );

            if (ValidateRoads())
            {
                Junctions = BuildJunctionsFromRoads(Roads);
            }
            else
            {
                FinalizeLayout();
            }          
        }
    }
}
