using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Jpp.Ironstone.Highways.Objectmodel
{
    public class Network
    {        
        public ICollection<Road> Roads { get; private set; }
        public ICollection<Junction> Junctions { get; private set;}

        public Network()
        {
            Roads = new List<Road>();
            Junctions = new List<Junction>();
        }

        public void InitialiseNetworkFromCentreLines(ICollection<CentreLine> centreLines)
        {
            Roads = BuildRoadsFromCentreLines(centreLines);
            Junctions = BuildJunctionsFromRoads();
        }

        private List<Road> BuildRoadsFromCentreLines(ICollection<CentreLine> centreLines)
        {
            if (centreLines == null) return null;

            var roads = new List<Road>();
            var road = new Road {Network = this};
            var initCentre = centreLines.FirstOrDefault();
            var connection = true;

            road.AddCentreLine(initCentre);
            centreLines.Remove(initCentre);
            
            while (connection)
            {
                connection = false;
                foreach (var centre in centreLines.ToList())
                {
                    if (!road.IsConnected(centre)) continue;

                    connection = true;
                    road.AddCentreLine(centre);
                    centreLines.Remove(centre);

                    break;
                }
            }

            roads.Add(road);

            if (centreLines.Count != 0) roads.AddRange(BuildRoadsFromCentreLines(centreLines));

            return roads;
        }

        private List<Junction> BuildJunctionsFromRoads()
        {
            if (Roads == null || Roads.Count == 0) return null;

            var junctions = new List<Junction>();

            foreach (var road in Roads.ToList())
            {
                var startJunction = GetStartJunction(road, Roads);
                if (startJunction != null) junctions.Add(startJunction);

                var endJunction = GetEndJunction(road, Roads);
                if (endJunction != null) junctions.Add(endJunction);
            }

            return junctions;
        }

        private Junction GetStartJunction(Road road, ICollection<Road> roads)
        {
            if (road == null || roads == null || roads.Count == 0) return null;

            var startCentreLine = road.StartLine;
            var connected = ConnectingCentreLine(startCentreLine, true, roads);

            if (connected != null)
            {
                return new Junction
                {
                    Network = this,
                    PrimaryRoad = new JunctionPart{ CentreLine = connected, Road = connected.Road, Type = JunctionPartTypes.Mid, IntersectionPoint = startCentreLine.StartPoint },
                    SecondaryRoad = new JunctionPart { CentreLine = startCentreLine, Road = road, Type = JunctionPartTypes.Start, IntersectionPoint = startCentreLine.StartPoint }
                };
            }

            return null;
        }

        private Junction GetEndJunction(Road road, ICollection<Road> roads)
        {
            if (road == null || roads == null || roads.Count == 0) return null;

            var endCentreLine = road.EndLine;
            var connected = ConnectingCentreLine(endCentreLine, false, roads);

            if (connected != null)
            {
                return new Junction
                {
                    Network = this,
                    PrimaryRoad = new JunctionPart { CentreLine = connected, Road = connected.Road, Type = JunctionPartTypes.Mid, IntersectionPoint = endCentreLine.EndPoint },
                    SecondaryRoad = new JunctionPart { CentreLine = endCentreLine, Road = road, Type = JunctionPartTypes.End, IntersectionPoint = endCentreLine.EndPoint }
                };
            }

            return null;
        }

        private static CentreLine ConnectingCentreLine(CentreLine centre, bool isStart, IEnumerable<Road> roads)
        {
            const int dp = 3;
            var curve = centre.GetCurve();
            foreach (var road in roads.ToList())
            {
                foreach (var rCentreLine in road.CentreLines)
                {
                    var next = rCentreLine.Next();
                    var previous = rCentreLine.Previous();
                    
                    if (next != null && next.Equals(centre)) continue;
                    if (previous != null && previous.Equals(centre)) continue;

                    if (rCentreLine.Equals(centre)) continue;

                    var pts = new Point3dCollection();
                    curve.IntersectWith(rCentreLine.GetCurve(), Intersect.OnBothOperands, pts, IntPtr.Zero, IntPtr.Zero);
                    if (pts.Count <= 0) continue;
                    
                    var intPointRounded = new Point2d(Math.Round(pts[0].X, dp), Math.Round(pts[0].Y, dp));
                    var centrePointRounded = isStart 
                        ? new Point2d(Math.Round(centre.StartPoint.X, dp), Math.Round(centre.StartPoint.Y, dp)) 
                        : new Point2d(Math.Round(centre.EndPoint.X, dp), Math.Round(centre.EndPoint.Y, dp));

                    if (intPointRounded == centrePointRounded) return rCentreLine;
                }
            }

            return null;
        }
    }
}
