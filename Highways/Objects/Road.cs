using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Jpp.Ironstone.Highways.ObjectModel.Abstract;
using Jpp.Ironstone.Highways.ObjectModel.Factories;
using Jpp.Ironstone.Highways.ObjectModel.Objects.Offsets;

namespace Jpp.Ironstone.Highways.ObjectModel.Objects
{
    [Serializable]
    public class Road
    {  
        private double _leftCarriageWay = Constants.DEFAULT_CARRIAGE_WAY;
        private double _rightCarriageWay = Constants.DEFAULT_CARRIAGE_WAY;
        private double _leftPavement = Constants.DEFAULT_PAVEMENT - Constants.DEFAULT_CARRIAGE_WAY;
        private double _rightPavement = Constants.DEFAULT_PAVEMENT - Constants.DEFAULT_CARRIAGE_WAY;

        public Guid Id { get; }
        public string Name { get; set; }
        public RoadCentreLineCollection CentreLines { get; }
        public bool HasStartJunction { get; set; }
        public bool HasEndJunction { get; set; }
        public double LeftCarriageWay
        {
            get => _leftCarriageWay;
            set
            {
                if (_leftCarriageWay.Equals(value)) return;
                var centreList = CentreLines.ToList();
                using (var trans = TransactionFactory.CreateFromNew())
                {
                    if (centreList.Any(c => c.CarriageWayLeft.IsValid(c, value) == false || c.CarriageWayLeft.Pavement.IsValid(c, value + _leftPavement) == false))
                        throw new ArgumentException("Invalid radius for junction.");
                }

                centreList.ForEach(c =>
                {
                    c.CarriageWayLeft.Pavement.DistanceFromCentre = value + _leftPavement;
                    c.CarriageWayLeft.DistanceFromCentre = value;
                });

                _leftCarriageWay = value;
            }
        } 
        public double RightCarriageWay
        {
            get => _rightCarriageWay;
            set
            {
                if (_rightCarriageWay.Equals(value)) return;
                var centreList = CentreLines.ToList();
                using (var trans = TransactionFactory.CreateFromNew())
                {
                    if (centreList.Any(c => c.CarriageWayRight.IsValid(c, value) == false || c.CarriageWayRight.Pavement.IsValid(c, value + _leftPavement) == false))
                        throw new ArgumentException("Invalid radius for junction.");
                }

                centreList.ForEach(c =>
                {
                    c.CarriageWayRight.Pavement.DistanceFromCentre = value +_rightPavement;
                    c.CarriageWayRight.DistanceFromCentre = value;
                });

                _rightCarriageWay = value;
            }
        }
        public double LeftPavement
        {
            get => _leftPavement;
            set
            {
                if (_leftPavement.Equals(value)) return;
                var centreList = CentreLines.ToList();
                using (var trans = TransactionFactory.CreateFromNew())
                {
                    if (centreList.Any(c => c.CarriageWayLeft.Pavement.IsValid(c, _leftCarriageWay + value) == false))
                        throw new ArgumentException("Invalid radius for junction.");
                }


                centreList.ForEach(c =>
                {
                    c.CarriageWayLeft.Pavement.DistanceFromCentre = _leftCarriageWay + value;
                });

                _leftPavement = value;
            }
        }
        [XmlIgnore] public PavementTypes LeftPavementType => PavementType(SidesOfCentre.Left);
        public bool LeftPavementActive { get; set; } = true;
        public double RightPavement
        {
            get => _rightPavement;
            set
            {
                if (_rightPavement.Equals(value)) return;
                var centreList = CentreLines.ToList();
                using (var trans = TransactionFactory.CreateFromNew())
                {
                    if (centreList.Any(c => c.CarriageWayRight.Pavement.IsValid(c, _rightCarriageWay + value) == false))
                        throw new ArgumentException("Invalid radius for junction.");
                }

                centreList.ForEach(c =>
                {
                    c.CarriageWayRight.Pavement.DistanceFromCentre = _rightCarriageWay + value;
                });

                _rightPavement = value;
            }
        }
        [XmlIgnore] public PavementTypes RightPavementType => PavementType(SidesOfCentre.Right);
        public bool RightPavementActive { get; set; } = true;
        [XmlIgnore] public bool Valid => CentreLines.Valid;
        public RoadClosureStart RoadClosureStart { get; set; }
        public RoadClosureEnd RoadClosureEnd { get; set; }
        public List<RoadFeature> Features { get; }

        public Road()
        {
            Id = Guid.NewGuid();
            CentreLines = new RoadCentreLineCollection(this);
            RoadClosureStart = new RoadClosureStart();
            RoadClosureEnd = new RoadClosureEnd();
            Features = new List<RoadFeature>();
        }

        public void Generate()
        {
            if (!Valid) return;
            var centreList = CentreLines.ToList();
            centreList.ForEach(c => c.Generate());
            RoadClosureStart.Create(CentreLines.StartLine);
            RoadClosureEnd.Create(CentreLines.EndLine);

            var featureList = Features.ToList();
            featureList.ForEach(f =>
            {
                if (!f.Generate(this))
                {
                    f.RoadFeatureErased -= Feature_Erased;
                    Features.Remove(f);
                }
            });
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
                HasStartJunction = true;
                junctions.Add(new Junction {
                    PrimaryRoad = new JunctionPart { CentreLine = startConnected, Type = JunctionPartTypes.Mid, IntersectionPoint = startCentreLine.StartPoint },
                    SecondaryRoad = new JunctionPart { CentreLine = startCentreLine, Type = JunctionPartTypes.Start, IntersectionPoint = startCentreLine.StartPoint }
                });
            }

            var endConnected = endCentreLine.ConnectingCentreLine(roadList, false);
            if (endConnected != null)
            {
                HasEndJunction = true;
                junctions.Add(new Junction {
                    PrimaryRoad = new JunctionPart { CentreLine = endConnected, Type = JunctionPartTypes.Mid, IntersectionPoint = endCentreLine.EndPoint },
                    SecondaryRoad = new JunctionPart { CentreLine = endCentreLine, Type = JunctionPartTypes.End, IntersectionPoint = endCentreLine.EndPoint }
                });
            }

            return junctions.Any() ? junctions : null;
        }
      
        public void Reset()
        {
            var centreList = CentreLines.ToList();
            centreList.ForEach(c => c.Reset());
            RoadClosureStart.Clear();
            RoadClosureEnd.Clear();
            Features.ForEach(f =>
            {
                f.RoadFeatureErased -= Feature_Erased;
                f.Clear();
            });
        }

        public double GetPavementDistance(SidesOfCentre side)
        {
            switch (side)
            {
                case SidesOfCentre.Left:
                    return LeftPavement;
                case SidesOfCentre.Right:
                    return RightPavement;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }

        public PavementTypes PavementType(SidesOfCentre side)
        {
            return GetPavementDistance(side) >= Constants.MINIMUM_PAVEMENT ? PavementTypes.Footway : PavementTypes.Service;
        }

        public void Feature_Erased(object sender, EventArgs e)
        {
            if (sender is RoadFeature feature)
            {
                feature.RoadFeatureErased -= Feature_Erased;
                Features.Remove(feature);
            }
        }
    }
}
