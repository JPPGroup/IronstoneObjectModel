using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Highways.Objectmodel.Helpers;

namespace Jpp.Ironstone.Highways.Objectmodel
{
    public class Road
    {
        private readonly List<CentreLine> _centreLines;
        private CarriageWayLeft _carriageWayLeft;
        private CarriageWayRight _carriageWayRight;

        public Network Network { get; set; }

        public Point2d StartPoint => _centreLines.First().StartPoint;
        public SegmentType StartType => _centreLines.First().Type;
        public CentreLine StartLine => _centreLines.First();
        public Point2d EndPoint => _centreLines.Last().EndPoint;
        public SegmentType EndType => _centreLines.Last().Type;
        public CentreLine EndLine => _centreLines.Last();
        public IEnumerable<CentreLine> CentreLines => _centreLines;
        public CentreLine this[int i] => _centreLines[i];
        public CarriageWayLeft CarriageWayLeft
        {
            get => _carriageWayLeft;
            set
            {
                if (value == null) return;

                _carriageWayLeft = value;
                ValidCentreLines();
            }
        }
        public CarriageWayRight CarriageWayRight
        {
            get => _carriageWayRight;
            set
            {
                if (value == null) return;

                _carriageWayRight = value;
                ValidCentreLines();
            }
        }

        public Road()
        {
            _centreLines = new List<CentreLine>();
        }

        public bool IsConnected(CentreLine centreLine)
        {
            if (_centreLines.Count == 0) return true;

            if (StartPoint == centreLine.EndPoint) return StartType != SegmentType.Line || centreLine.Type != SegmentType.Line;
            if (StartPoint == centreLine.StartPoint) return StartType != SegmentType.Line || centreLine.Type != SegmentType.Line;

            if (EndPoint == centreLine.EndPoint) return EndType != SegmentType.Line || centreLine.Type != SegmentType.Line;
            if (EndPoint == centreLine.StartPoint) return EndType != SegmentType.Line || centreLine.Type != SegmentType.Line;

            return false;
        }

        public void AddCentreLine(CentreLine centreLine)
        {
            if (!IsConnected(centreLine)) return;          
            if (_centreLines.Contains(centreLine)) return;
            
            if (AddCentreLineInitial(centreLine)) return;
            if (AddCentreLineEndToStart(centreLine)) return;
            if (AddCentreLineStartToEnd(centreLine)) return;
            if (AddCentreLineEndToEnd(centreLine)) return;
            if (AddCentreLineStartToStart(centreLine)) return;

            throw new ArgumentException("Invalid centre line");            
        }

        public void Highlight()
        {
            foreach (var centreLine in _centreLines)
            {
                centreLine.Highlight();
            }
        }

        public void Unhighlight()
        {
            foreach (var centreLine in _centreLines)
            {
                centreLine.Unhighlight();
            }
        }

        public int PositionInRoad(CentreLine centreLine)
        {
            return _centreLines.IndexOf(centreLine);
        }

        private bool AddCentreLineInitial(CentreLine centreLine)
        {
            if (_centreLines.Count != 0) return false;

            centreLine.Road = this;
            _centreLines.Add(centreLine);

            return true;
        }

        private bool AddCentreLineEndToStart(CentreLine centreLine)
        {
            if (EndPoint != centreLine.StartPoint) return false;

            var connection = _centreLines.Last();
            var angleBetween = centreLine.StartVector.GetAngleTo(connection.EndVector);

            if (!ValidConnection(centreLine, connection, angleBetween)) return false;

            centreLine.Road = this;
            _centreLines.Add(centreLine);

            return true;
        }

        private bool AddCentreLineEndToEnd(CentreLine centreLine)
        {
            if (EndPoint != centreLine.EndPoint) return false;

            centreLine.Reverse();
            var connection = _centreLines.Last();
            var angleBetween = centreLine.StartVector.GetAngleTo(connection.EndVector);

            if (!ValidConnection(centreLine, connection, angleBetween)) return false;

            centreLine.Road = this;
            _centreLines.Add(centreLine);

            return true;
        }

        private bool AddCentreLineStartToEnd(CentreLine centreLine)
        {
            if (StartPoint != centreLine.EndPoint) return false;

            var connection = _centreLines.First();
            var angleBetween = centreLine.EndVector.GetAngleTo(connection.StartVector);

            if (!ValidConnection(centreLine, connection, angleBetween)) return false;

            centreLine.Road = this;
            _centreLines.Insert(0, centreLine);

            return true;
        }

        private bool AddCentreLineStartToStart(CentreLine centreLine)
        {
            if (StartPoint != centreLine.StartPoint) return false;

            centreLine.Reverse();
            var connection = _centreLines.First();
            var angleBetween = centreLine.EndVector.GetAngleTo(connection.StartVector);

            if (!ValidConnection(centreLine, connection, angleBetween)) return false;

            centreLine.Road = this;
            _centreLines.Insert(0, centreLine);

            return true;
        }

        private static bool ValidConnection(CentreLine centreToAdd, CentreLine centreToConnect, double angleBetween)
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

        private void ValidCentreLines()
        {
            foreach (var centre in _centreLines)
            {
                if (!(centre.GetCurve() is Arc arc)) continue;

                if (arc.Radius <= CarriageWayLeft?.Distance || arc.Radius <= CarriageWayRight?.Distance)  throw new ArgumentException("Invalid centre line");
            }
        }
    }
}
