using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.Autocad.DrawingObjects;
using Jpp.Ironstone.Highways.ObjectModel.Objects;

namespace Jpp.Ironstone.Highways.ObjectModel
{
    [Serializable]
    public class HighwaysManager : AbstractDrawingObjectManager
    {
        public bool Finalized { get; set; }
        public List<Road> Roads { get; set; }
        public PersistentObjectIdCollection OffsetCollection { get; set; }
        [XmlIgnore] public List<Junction> Junctions { get; set; }

        public HighwaysManager(Document document) : base(document) 
        {
            OffsetCollection = new PersistentObjectIdCollection();
        }

        private HighwaysManager() { }

        public void FinalizeLayout()
        {
            Finalized = true;
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

        public void InitialiseFromCentreLines(IEnumerable<CentreLine> centreLines)
        {
            Clear();

            var centreList = centreLines.ToList();
            if (!centreList.Any()) return;

            Roads = BuildRoadsFromCentreLines(centreList);            
            Junctions = BuildJunctionsFromRoads(Roads);

            GenerateCarriageWayOffset();
        }

        public void SetLeftCarriageWayOffset(Guid roadGuid, double left)
        {

            var match = Roads.Find(r => r.Id == roadGuid);

            if (match.SetOffsets(left, match.RightCarriageWay)) GenerateCarriageWayOffset();
        }

        public void SetRightCarriageWayOffset(Guid roadGuid, double right)
        {
            var match = Roads.Find(r => r.Id == roadGuid);

            if (match.SetOffsets(match.LeftCarriageWay, right)) GenerateCarriageWayOffset();
        }

        private void GenerateCarriageWayOffset()
        {
            RemoveOffsets();      
            GenerateJunctionsCarriageWay();
            GenerateRoadsCarriageWay();
        }
       
        private void GenerateRoadsCarriageWay()
        {
            if (Roads == null || Roads.Count == 0) return;

            var allRoadOffset = new List<Curve>();
            foreach (var road in Roads)
            {
                var curves = road.GenerateCarriageWay();
                if (curves == null) continue;

                allRoadOffset.AddRange(curves);
            }

            if (allRoadOffset.Count == 0) return;

            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;

            using (var acTrans = db.TransactionManager.StartTransaction())
            {
                var blockTable = (BlockTable)acTrans.GetObject(db.BlockTableId, OpenMode.ForRead);
                var blockTableRecord = (BlockTableRecord)acTrans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                foreach (var curve in allRoadOffset)
                {
                    curve.Layer = Constants.LAYER_DEF_POINTS;

                    OffsetCollection.Add(blockTableRecord.AppendEntity(curve));
                    acTrans.AddNewlyCreatedDBObject(curve, true);
                }

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

            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;

            using (var acTrans = db.TransactionManager.StartTransaction())
            {
                var blockTable = (BlockTable)acTrans.GetObject(db.BlockTableId, OpenMode.ForRead);
                var blockTableRecord = (BlockTableRecord)acTrans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                foreach (var arc in allJunctionArcs)
                {
                    arc.Layer = Constants.LAYER_DEF_POINTS;

                    OffsetCollection.Add(blockTableRecord.AppendEntity(arc));
                    acTrans.AddNewlyCreatedDBObject(arc, true);
                }

                acTrans.Commit();
            }
        }

        private void RemoveOffsets()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;

            using (var acTrans = db.TransactionManager.StartTransaction())
            {
                foreach (ObjectId obj in OffsetCollection.Collection)
                {
                    if (!obj.IsErased)
                    {
                        acTrans.GetObject(obj, OpenMode.ForWrite,true).Erase();
                    }
                }

                OffsetCollection.Clear();

                if (Roads != null && Roads.Count > 0) Roads.ForEach(r => r.ResetOffsets());
                acTrans.Commit();
            }            
        }

        private static List<Road> BuildRoadsFromCentreLines(IEnumerable<CentreLine> centreLines)
        {
            var centreList = centreLines.ToList();
            if (!centreList.Any()) return null;

            var roads = new List<Road>();
            var road = new Road();
            var initCentre = centreList.FirstOrDefault();
            var connection = true;

            road.AddCentreLine(initCentre);
            centreList.Remove(initCentre);

            while (connection)
            {
                connection = false;
                foreach (var centre in centreList)
                {
                    if (!road.IsConnected(centre)) continue;

                    connection = true;
                    road.AddCentreLine(centre);
                    centreList.Remove(centre);

                    break;
                }
            }

            roads.Add(road);
            if (centreList.Count != 0) roads.AddRange(BuildRoadsFromCentreLines(centreList));

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
            if (!Finalized) RemoveOffsets();

            OffsetCollection = new PersistentObjectIdCollection();
            Junctions = null;
            Roads = null;
        }

        public override void AllDirty()
        {
            throw new NotImplementedException();
        }

        public override void ActivateObjects()
        {
            if (Finalized) return;

            foreach (var road in Roads)
            {
                foreach (var centreLine in road.CentreLines)
                {
                    centreLine.Road = road;
                    centreLine.CreateActiveObject();
                }                
            }

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
