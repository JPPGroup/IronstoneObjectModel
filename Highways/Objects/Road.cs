using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Jpp.Ironstone.Highways.ObjectModel.Objects
{
    [Serializable]
    public class Road
    {        
        public Guid Id { get; }
        public string Name { get; set; }
        public RoadCentreLineCollection CentreLines { get; }
        public double LeftCarriageWay { get; set; } = Constants.DEFAULT_CARRIAGE_WAY;
        public double RightCarriageWay { get; set; } = Constants.DEFAULT_CARRIAGE_WAY;
        public double LeftPavement { get; set; } = Constants.DEFAULT_PAVEMENT - Constants.DEFAULT_CARRIAGE_WAY;
        public double RightPavement { get; set; } = Constants.DEFAULT_PAVEMENT - Constants.DEFAULT_CARRIAGE_WAY;
        [XmlIgnore] public bool Valid => CentreLines.Valid;
 
        public Road()
        {
            Id = Guid.NewGuid();
            CentreLines = new RoadCentreLineCollection(this);
        }

        public void Generate()
        {
            if (!Valid) return;
            var centreList = CentreLines.ToList();
            centreList.ForEach(c => c.Generate());           
        }        
       
        public void Highlight()
        {
            foreach (var centreLine in CentreLines)
            {
                centreLine.Highlight();
            }
        }

        public void Unhighlight()
        {
            foreach (var centreLine in CentreLines)
            {
                centreLine.Unhighlight();
            }
        }

        public ICollection<Junction> CreateJunctions(IEnumerable<Road> roads)
        {
            var roadList = roads.ToList();
            if (!roadList.Any()) return null;

            var junctions = new List<Junction>();
            var startCentreLine = CentreLines.StartLine;
            var endCentreLine = CentreLines.EndLine;

            var startConnected = startCentreLine.ConnectingCentreLine(roadList, true);
            if (startConnected != null)
            {
                junctions.Add(new Junction {
                    PrimaryRoad = new JunctionPart { CentreLine = startConnected, Type = JunctionPartTypes.Mid, IntersectionPoint = startCentreLine.StartPoint },
                    SecondaryRoad = new JunctionPart { CentreLine = startCentreLine, Type = JunctionPartTypes.Start, IntersectionPoint = startCentreLine.StartPoint }
                });
            }

            var endConnected = endCentreLine.ConnectingCentreLine(roadList, false);
            if (endConnected != null)
            {
                junctions.Add(new Junction {
                    PrimaryRoad = new JunctionPart { CentreLine = endConnected, Type = JunctionPartTypes.Mid, IntersectionPoint = endCentreLine.EndPoint },
                    SecondaryRoad = new JunctionPart { CentreLine = endCentreLine, Type = JunctionPartTypes.End, IntersectionPoint = endCentreLine.EndPoint }
                });
            }

            return junctions.Any() ? junctions : null;
        }
      
        public void SetOffsets(double leftCarriageWay, double rightCarriageWay, double leftPavement, double rightPavement)
        {
            //TODO: Fix floating point precision..
            if (LeftCarriageWay == leftCarriageWay && RightCarriageWay == rightCarriageWay && LeftPavement == leftPavement && RightPavement == rightPavement) return;

            LeftCarriageWay = leftCarriageWay;
            RightCarriageWay = rightCarriageWay;

            LeftPavement = leftPavement;
            RightPavement = rightPavement;

            var centreList = CentreLines.ToList();
            centreList.ForEach(c => c.SetAllOffsets(leftCarriageWay, rightCarriageWay, leftPavement, rightPavement));            
        }

        public void Reset()
        {
            var centreList = CentreLines.ToList();
            centreList.ForEach(c => c.Reset());
        }
    }
}
