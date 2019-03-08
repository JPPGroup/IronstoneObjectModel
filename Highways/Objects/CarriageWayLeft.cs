using System;
using System.Collections.Generic;
using Jpp.Ironstone.Highways.ObjectModel.Abstract;

namespace Jpp.Ironstone.Highways.ObjectModel.Objects
{
    [Serializable]
    public class CarriageWayLeft : ICentreLineOffset
    {
        public SidesOfCentre Side { get; }
        public OffsetTypes OffsetType { get; }
        public double Distance { get; }
        public List<OffsetIntersect> Intersection { get; }
        public bool Ignore { get; set; }

        public CarriageWayLeft(double distance)
        {
            Distance = distance;
            Side = SidesOfCentre.Left;
            OffsetType = OffsetTypes.CarriageWay;
            Intersection = new List<OffsetIntersect>();
        }
    }
}
