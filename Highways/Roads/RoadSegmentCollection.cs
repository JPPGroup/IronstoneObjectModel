using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace Jpp.Ironstone.Highways.ObjectModel.Roads
{
    [Serializable, XmlInclude(typeof(RoadSegment))]
    public class RoadSegmentCollection : IEnumerable
    {
        private readonly List<RoadSegment> _features;
        private readonly List<RoadSegment> _junctions;

        public RoadSegmentCollection()
        {
            _features = new List<RoadSegment>();
            _junctions = new List<RoadSegment>();
        }

        [XmlIgnore]
        public int Count => _features.Count;

        [XmlIgnore] 
        public List<RoadSegment> Collection
        {
            get
            {
                var list = new List<RoadSegment>();
                list.AddRange(_junctions);
                foreach (var feature in _features)
                {
                    if (list.Any(l => l.Chainage.Equals(feature.Chainage))) continue;
                    list.Add(feature);
                }

                return list.OrderBy(s => s.Chainage).ToList();
            }
        }

        [XmlIgnore]
        public RoadSegment this[int i] => _features[i];


        public void Add(object segment)
        {
            Add((RoadSegment)segment);
        }

        public void Add(RoadSegment segment)
        {
            //TODO: Need to review...not sure this is 100% right
            for (var i = 1; i < _junctions.Count; i++)
            {
                if(segment.Chainage < _junctions[i].Chainage && segment.Chainage > _junctions[i - 1].Chainage) 
                    throw new ArgumentException(@"Segment cannot be between a junction.", nameof(segment));
            }

            _features.Add(segment);
        }

        public void Junction(Side side, double startChainage, double? endChainage = null)
        {
            startChainage = Math.Round(startChainage, 4);
            if (endChainage.HasValue) endChainage = Math.Round(endChainage.Value, 4);

            var midSegments = endChainage.HasValue
                ? _junctions.Where(j => j.Chainage > startChainage && j.Chainage < endChainage)
                : _junctions.Where(j => j.Chainage > startChainage);

            foreach (var segment in midSegments)
            {
                segment.Properties.LeftCarriagewayWidth = side == Side.Left ? 0 : segment.Properties.LeftCarriagewayWidth;
                segment.Properties.RightCarriagewayWidth = side == Side.Right ? 0 : segment.Properties.RightCarriagewayWidth;
            }

            AddStart(startChainage, side);
            if (endChainage.HasValue) AddEnd(endChainage.Value, side);
        }

        private void AddStart(double chainage, Side side)
        {
            var exists = _junctions.FirstOrDefault(j => j.Chainage.Equals(chainage));
            if (exists != null)
            {
                exists.Properties.LeftCarriagewayWidth = side == Side.Left ? 0 : exists.Properties.LeftCarriagewayWidth;
                exists.Properties.RightCarriagewayWidth = side == Side.Right ? 0 : exists.Properties.RightCarriagewayWidth;
                return;
            }

            var feature = _features.OrderByDescending(s => s.Chainage).First(j => j.Chainage <= chainage);
            var junction = _junctions.OrderByDescending(s => s.Chainage).FirstOrDefault(j => j.Chainage < chainage);

            var left = junction?.Properties.LeftCarriagewayWidth ?? feature.Properties.LeftCarriagewayWidth;
            var right = junction?.Properties.RightCarriagewayWidth ?? feature.Properties.RightCarriagewayWidth;

            _junctions.Add(new RoadSegment
            {
                Chainage = chainage,
                Properties =
                {
                    LeftCarriagewayWidth = side == Side.Left ? 0 : left,
                    RightCarriagewayWidth = side == Side.Right ? 0 : right
                }
            });
        }

        private void AddEnd(double chainage, Side side)
        {
            var exists = _junctions.FirstOrDefault(j => j.Chainage.Equals(chainage));
            var feature = _features.OrderByDescending(s => s.Chainage).First(j => j.Chainage <= chainage);

            if (exists != null)
            {
                exists.Properties.LeftCarriagewayWidth = side == Side.Left ? feature.Properties.LeftCarriagewayWidth : exists.Properties.LeftCarriagewayWidth;
                exists.Properties.RightCarriagewayWidth = side == Side.Right ? feature.Properties.RightCarriagewayWidth : exists.Properties.RightCarriagewayWidth;
                return;
            }

            var junction = _junctions.OrderByDescending(s => s.Chainage).FirstOrDefault(j => j.Chainage < chainage);
            var left = junction?.Properties.LeftCarriagewayWidth ?? feature.Properties.LeftCarriagewayWidth;
            var right = junction?.Properties.RightCarriagewayWidth ?? feature.Properties.RightCarriagewayWidth;

            _junctions.Add(new RoadSegment
            {
                Chainage = chainage,
                Properties =
                    {
                        LeftCarriagewayWidth = side == Side.Left ? feature.Properties.LeftCarriagewayWidth : left,
                        RightCarriagewayWidth = side == Side.Right ? feature.Properties.RightCarriagewayWidth : right
                    }
            });
        }

        public void Reset()
        {
            _junctions.Clear();
        }

        public IEnumerator GetEnumerator()
        {
            return _features.GetEnumerator();
        }
    }
}
