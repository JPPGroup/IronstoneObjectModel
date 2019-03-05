using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.Autocad.DrawingObjects;
using Jpp.Ironstone.Highways.ObjectModel.Objects;

namespace Jpp.Ironstone.Highways.ObjectModel
{
    public class HighwayManager : AbstractDrawingObjectManager
    {
        private bool _finalized;

        public bool Finalized {
            get => _finalized;
            set
            {
                if (value) Clear();
                _finalized = value;
            }
        }    
        public ICollection<CentreLine> CentreLines { get; private set; }
        public ICollection<Road> Roads { get; private set; }
        public PersistentObjectIdCollection OffsetCollection { get; private set; }

        [XmlIgnore] public ICollection<Junction> Junctions { get; private set; }

        
        public HighwayManager()
        {
            CentreLines = new List<CentreLine>();
        }

        public void InitialiseFromCentreLines(IEnumerable<CentreLine> centreLines)
        {
            Clear();

            var centreList = centreLines.ToList();
            if (!centreList.Any()) return;

            CentreLines = centreList;

            Roads = BuildRoadsFromCentreLines(centreList);
            Junctions = BuildJunctionsFromRoads(Roads);
            //Generate offsets...
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
                //rebuild junctions...
                //recreate offsets...
            }
            else
            {
                Finalized = true;
            }
        }

        public override void Clear()
        {
            //if (!Finalized) then delete all offsets...

            Junctions = null;
            Roads = null;

            CentreLines = new List<CentreLine>();
        }

        public override void AllDirty()
        {
            throw new NotImplementedException();
        }

        public override void ActivateObjects()
        {
            if (Finalized) return;

            foreach (var centreLine in CentreLines)
            {
                centreLine.CreateActiveObject();
            }
        }
    }
}
