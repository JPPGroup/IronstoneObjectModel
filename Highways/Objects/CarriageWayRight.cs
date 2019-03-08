using System;
using System.Collections.Generic;
using Jpp.Ironstone.Highways.ObjectModel.Abstract;

namespace Jpp.Ironstone.Highways.ObjectModel.Objects
{
    [Serializable]
    public class CarriageWayRight : ICentreLineOffset
    {
        public SidesOfCentre Side { get; }
        public OffsetTypes OffsetType { get; }
        public double Distance { get; }
        public List<OffsetIntersect> Intersection { get; }
        public bool Ignore { get; set; }

        public CarriageWayRight(double distance)
        {
            Distance = distance;
            Side = SidesOfCentre.Right;
            OffsetType = OffsetTypes.CarriageWay;
            Intersection = new List<OffsetIntersect>();
        }
    }
}
