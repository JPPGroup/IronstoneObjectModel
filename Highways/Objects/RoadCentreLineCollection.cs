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
    public class RoadCentreLineCollection : IList<RoadCentreLine>
    {
        private readonly Road _road;
        private readonly IList<RoadCentreLine> _centreLines;

        public int Count => _centreLines.Count;
        public bool IsReadOnly => _centreLines.IsReadOnly;
        public RoadCentreLine this[int index]
        {
            get => _centreLines[index];
            set
            {
                var oldItem = _centreLines[index];
                if (value != null) value.Road = _road;

                _centreLines[index] = value;
                if (oldItem != null) oldItem.Road = null;
            }
        }
        [XmlIgnore] public Point2d StartPoint => _centreLines.First().StartPoint;
        [XmlIgnore] public SegmentType StartType => _centreLines.First().Type;
        [XmlIgnore] public RoadCentreLine StartLine => _centreLines.First();
        [XmlIgnore] public Point2d EndPoint => _centreLines.Last().EndPoint;
        [XmlIgnore] public SegmentType EndType => _centreLines.Last().Type;
        [XmlIgnore] public RoadCentreLine EndLine => _centreLines.Last();
        [XmlIgnore] public bool Valid => IsValid();

        public RoadCentreLineCollection(Road road)
        {
            _road = road;
            _centreLines = new List<RoadCentreLine>();
        }

        public RoadCentreLineCollection(Road road, IList<RoadCentreLine> centreLines)
        {
            _road = road;
            _centreLines = centreLines;
        }

        public int IndexOf(RoadCentreLine item)
        {
            return _centreLines.IndexOf(item);
        }

        public void Insert(int index, RoadCentreLine item)
        {
            throw new NotImplementedException();

            //if (item != null) item.Road = _road;
            //_centreLines.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();

            //var oldItem = _centreLines[index];
            //_centreLines.RemoveAt(index);
            //if (oldItem != null) oldItem.Road = null;
        }

        public void Add(RoadCentreLine item)
        {
            if(Contains(item)) return;

            if (AddCentreLineInitial(item)) return;
            if (AddCentreLineEndToStart(item)) return;
            if (AddCentreLineStartToEnd(item)) return;
            if (AddCentreLineEndToEnd(item)) return;
            if (AddCentreLineStartToStart(item)) return;

            throw new ArgumentException("Invalid centre line");
        }

        public void Add(Curve curve)
        {
            Add(new RoadCentreLine { BaseObject = curve.Id });                     
        }

        private bool AddCentreLineInitial(RoadCentreLine item)
        {
            if (Count != 0) return false;

            if (item != null) item = InitializeItem(item);
            _centreLines.Add(item);

            return true;
        }

        private bool AddCentreLineEndToStart(RoadCentreLine item)
        {
            if (EndPoint != item.StartPoint) return false;

            var angleBetween = item.StartVector.GetAngleTo(EndLine.EndVector);

            if (!IsValidConnection(item, EndLine, angleBetween)) return false;

            item = InitializeItem(item);
            _centreLines.Add(item);

            return true;
        }

        private bool AddCentreLineStartToEnd(RoadCentreLine item)
        {
            if (StartPoint != item.EndPoint) return false;

            var angleBetween = item.EndVector.GetAngleTo(StartLine.StartVector);

            if (!IsValidConnection(item, StartLine, angleBetween)) return false;

            item = InitializeItem(item);
            _centreLines.Insert(0, item);

            return true;
        }

        private bool AddCentreLineEndToEnd(RoadCentreLine item)
        {
            if (EndPoint != item.EndPoint) return false;

            item.Reverse();
            var angleBetween = item.StartVector.GetAngleTo(EndLine.EndVector);

            if (!IsValidConnection(item, EndLine, angleBetween)) return false;

            item = InitializeItem(item);
            _centreLines.Add(item);

            return true;
        }

        private bool AddCentreLineStartToStart(RoadCentreLine item)
        {
            if (StartPoint != item.StartPoint) return false;

            item.Reverse();
            var angleBetween = item.EndVector.GetAngleTo(StartLine.StartVector);

            if (!IsValidConnection(item, StartLine, angleBetween)) return false;

            item = InitializeItem(item);
            _centreLines.Insert(0, item);

            return true;
        }

        private RoadCentreLine InitializeItem(RoadCentreLine item)
        {
            item.Road = _road;

            item.CarriageWayLeft.DistanceFromCentre = _road.LeftCarriageWay;
            item.CarriageWayLeft.Pavement.DistanceFromCentre = _road.LeftCarriageWay + _road.LeftPavement;

            item.CarriageWayRight.DistanceFromCentre = _road.RightCarriageWay;
            item.CarriageWayRight.Pavement.DistanceFromCentre = _road.RightCarriageWay + _road.RightPavement;

            return item;
        }

        public void Clear()
        {
            foreach (var item in _centreLines)
            {
                if (item != null) item.Road = null;
            }
            _centreLines.Clear();
        }

        public bool Contains(RoadCentreLine item)
        {
            return _centreLines.Contains(item);
        }

        public void CopyTo(RoadCentreLine[] array, int arrayIndex)
        {
            _centreLines.CopyTo(array, arrayIndex);
        }

        public bool Remove(RoadCentreLine item)
        {
            throw new NotImplementedException();

            //var b = _centreLines.Remove(item);
            //if (item != null) item.Road = null;
            //return b;
        }

        public IEnumerator<RoadCentreLine> GetEnumerator()
        {
            return _centreLines.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return (_centreLines as System.Collections.IEnumerable).GetEnumerator();
        }

        public bool IsConnected(RoadCentreLine item)
        {
            if (Count == 0) return true;

            if (StartPoint == item.EndPoint) return StartType != SegmentType.Line || item.Type != SegmentType.Line;
            if (StartPoint == item.StartPoint) return StartType != SegmentType.Line || item.Type != SegmentType.Line;

            if (EndPoint == item.EndPoint) return EndType != SegmentType.Line || item.Type != SegmentType.Line;
            if (EndPoint == item.StartPoint) return EndType != SegmentType.Line || item.Type != SegmentType.Line;

            return false;
        }


        public bool IsConnected(Curve item)
        {
            var centre = new RoadCentreLine { BaseObject = item.Id };

            return IsConnected(centre);
        }


        private bool IsValid()
        {
            //TODO: Needs reviewing...
            if (Count == 1) return true;

            for (var i = 1; i < Count - 1; i++)
            {
                var previous = this[i - 1];
                var current = this[i];
                var connected = current.StartPoint.IsEqualTo(previous.EndPoint);

                if (!connected) return false;

                var angleBetween = current.StartVector.GetAngleTo(previous.EndVector);
                var validConnection = IsValidConnection(current, previous, angleBetween);

                if (!validConnection) return false;
            }

            return true;
        }

        private static bool IsValidConnection(Segment2d itemToAdd, Segment2d itemToConnect, double angleBetween)
        {
            switch (itemToAdd.Type)
            {
                case SegmentType.Line:
                    if (itemToConnect.Type == SegmentType.Arc)
                    {
                        if (RadiansHelper.AnglesAreEqual(angleBetween, RadiansHelper.DEGREES_90)) return true;
                    }
                    break;
                case SegmentType.Arc:
                    if (itemToConnect.Type == SegmentType.Line)
                    {
                        if (RadiansHelper.AnglesAreEqual(angleBetween, RadiansHelper.DEGREES_90)) return true;
                    }
                    else if (itemToConnect.Type == SegmentType.Arc)
                    {
                        if (RadiansHelper.AnglesAreEqual(angleBetween, 0) || RadiansHelper.AnglesAreEqual(angleBetween, RadiansHelper.DEGREES_180)) return true;
                    }
                    break;
            }
            return false;
        }
    }
}
