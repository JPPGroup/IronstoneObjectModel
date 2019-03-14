using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Highways.ObjectModel.Abstract;
using Jpp.Ironstone.Highways.ObjectModel.Helpers;

namespace Jpp.Ironstone.Highways.ObjectModel.Objects
{
    [Serializable]
    public class Road : IParentObject
    {        
        public Guid Id { get; }
        public string Name { get; set; }
        public List<CentreLine> CentreLines { get; }
        public double LeftCarriageWay { get; private set; } = Constants.DEFAULT_CARRIAGE_WAY;
        public double RightCarriageWay { get; private set; } = Constants.DEFAULT_CARRIAGE_WAY;
        public double LeftPavement { get; private set; } = Constants.DEFAULT_PAVEMENT;
        public double RightPavement { get; private set; } = Constants.DEFAULT_PAVEMENT;
        [XmlIgnore] public Point2d StartPoint => CentreLines.First().StartPoint;
        [XmlIgnore] public SegmentType StartType => CentreLines.First().Type;
        [XmlIgnore] public CentreLine StartLine => CentreLines.First();
        [XmlIgnore] public Point2d EndPoint => CentreLines.Last().EndPoint;
        [XmlIgnore] public SegmentType EndType => CentreLines.Last().Type;
        [XmlIgnore] public CentreLine EndLine => CentreLines.Last();
        [XmlIgnore] public CentreLine this[int i] => CentreLines[i];
        [XmlIgnore] public bool Valid => IsValidRoad();
 
        public Road()
        {
            Id = Guid.NewGuid();
            CentreLines = new List<CentreLine>();
        }

        public void Generate()
        {
            if (!Valid) return;

            CentreLines.ForEach(c => c.Generate());           
        }

        public bool IsConnected(CentreLine centreLine)
        {
            if (CentreLines.Count == 0) return true;

            if (StartPoint == centreLine.EndPoint) return StartType != SegmentType.Line || centreLine.Type != SegmentType.Line;
            if (StartPoint == centreLine.StartPoint) return StartType != SegmentType.Line || centreLine.Type != SegmentType.Line;

            if (EndPoint == centreLine.EndPoint) return EndType != SegmentType.Line || centreLine.Type != SegmentType.Line;
            if (EndPoint == centreLine.StartPoint) return EndType != SegmentType.Line || centreLine.Type != SegmentType.Line;

            return false;
        }

        public void AddCentreLine(CentreLine centreLine)
        {
            if (!IsConnected(centreLine)) return;          
            if (CentreLines.Contains(centreLine)) return;

            centreLine.SetAllOffsets(LeftCarriageWay, RightCarriageWay, LeftPavement, RightPavement);

            if (AddCentreLineInitial(centreLine)) return;
            if (AddCentreLineEndToStart(centreLine)) return;
            if (AddCentreLineStartToEnd(centreLine)) return;
            if (AddCentreLineEndToEnd(centreLine)) return;
            if (AddCentreLineStartToStart(centreLine)) return;

            throw new ArgumentException("Invalid centre line");            
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

        public int PositionInRoad(CentreLine centreLine)
        {
            return CentreLines.IndexOf(centreLine);
        }

        public ICollection<Junction> CreateJunctions(IEnumerable<Road> roads)
        {
            var roadList = roads.ToList();
            if (!roadList.Any()) return null;

            var junctions = new List<Junction>();
            var startCentreLine = StartLine;
            var endCentreLine = EndLine;

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

        private bool AddCentreLineInitial(CentreLine centreLine)
        {
            if (CentreLines.Count != 0) return false;

            centreLine.Road = this;
            CentreLines.Add(centreLine);

            return true;
        }

        private bool AddCentreLineEndToStart(CentreLine centreLine)
        {
            if (EndPoint != centreLine.StartPoint) return false;

            var connection = CentreLines.Last();
            var angleBetween = centreLine.StartVector.GetAngleTo(connection.EndVector);

            if (!IsValidConnection(centreLine, connection, angleBetween)) return false;

            centreLine.Road = this;
            CentreLines.Add(centreLine);

            return true;
        }

        private bool AddCentreLineEndToEnd(CentreLine centreLine)
        {
            if (EndPoint != centreLine.EndPoint) return false;

            centreLine.Reverse();
            var connection = CentreLines.Last();
            var angleBetween = centreLine.StartVector.GetAngleTo(connection.EndVector);

            if (!IsValidConnection(centreLine, connection, angleBetween)) return false;

            centreLine.Road = this;
            CentreLines.Add(centreLine);

            return true;
        }

        private bool AddCentreLineStartToEnd(CentreLine centreLine)
        {
            if (StartPoint != centreLine.EndPoint) return false;

            var connection = CentreLines.First();
            var angleBetween = centreLine.EndVector.GetAngleTo(connection.StartVector);

            if (!IsValidConnection(centreLine, connection, angleBetween)) return false;

            centreLine.Road = this;
            CentreLines.Insert(0, centreLine);

            return true;
        }

        private bool AddCentreLineStartToStart(CentreLine centreLine)
        {
            if (StartPoint != centreLine.StartPoint) return false;

            centreLine.Reverse();
            var connection = CentreLines.First();
            var angleBetween = centreLine.EndVector.GetAngleTo(connection.StartVector);

            if (!IsValidConnection(centreLine, connection, angleBetween)) return false;

            centreLine.Road = this;
            CentreLines.Insert(0, centreLine);

            return true;
        }

        private static bool IsValidConnection(CentreLine centreToAdd, CentreLine centreToConnect, double angleBetween)
        {
            switch (centreToAdd.Type)
            {
                case SegmentType.Line:
                    if (centreToConnect.Type == SegmentType.Arc)
                    {
                        if (RadiansHelper.AnglesAreEqual(angleBetween, RadiansHelper.DEGREES_90)) return true;
                    }
                    break;
                case SegmentType.Arc:
                    if (centreToConnect.Type == SegmentType.Line)
                    {
                        if (RadiansHelper.AnglesAreEqual(angleBetween, RadiansHelper.DEGREES_90)) return true;
                    }
                    else if (centreToConnect.Type == SegmentType.Arc)
                    {
                        if (RadiansHelper.AnglesAreEqual(angleBetween, 0) || RadiansHelper.AnglesAreEqual(angleBetween, RadiansHelper.DEGREES_180)) return true;
                    }
                    break;
            }
            return false;
        }

        private bool IsValidRoad()
        {
            //TODO: Needs reviewing...
            if (CentreLines.Count == 1) return true;

            for (var i = 1; i < CentreLines.Count - 1; i++)
            {
                var previous = CentreLines[i - 1];
                var current = CentreLines[i];
                var connected = current.StartPoint.IsEqualTo(previous.EndPoint);

                if (!connected) return false;

                var angleBetween = current.StartVector.GetAngleTo(previous.EndVector);
                var validConnection = IsValidConnection(current, previous, angleBetween);

                if (!validConnection) return false;
            }

            return true;
        }

        public void SetOffsets(double leftCarriageWay, double rightCarriageWay, double leftPavement, double rightPavement)
        {
            //TODO: Fix floating point precision..
            if (LeftCarriageWay == leftCarriageWay && RightCarriageWay == rightCarriageWay && LeftPavement == leftPavement && RightPavement == rightPavement) return;

            LeftCarriageWay = leftCarriageWay;
            RightCarriageWay = rightCarriageWay;

            LeftPavement = leftPavement;
            RightPavement = rightPavement;

            CentreLines.ForEach(c => c.SetAllOffsets(leftCarriageWay, rightCarriageWay, leftPavement, rightPavement));            
        }

        public void Reset()
        {
            CentreLines.ForEach(c => c.Reset());
        }

        #region IParentObject Members

        void IParentObject.ResolveChildren()
        {
            CentreLines.ForEach(delegate(CentreLine centre)
                {
                    centre.Road = this;
                    (centre as IParentObject).ResolveChildren();
                }
            );
        }

        #endregion

    }
}
