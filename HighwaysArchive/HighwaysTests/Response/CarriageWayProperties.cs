using System;

namespace Jpp.Ironstone.Highways.ObjectModel.Tests.Response
{
    [Serializable]
    public class CarriageWayProperties
    {
        public SidesOfCentre Side { get; set; }
        public OffsetTypes Type { get; set; }
        public double Distance { get; set; }
        public bool Ignored { get; set; }
        public int Intersections { get; set; }
        public int Curves { get; set; }
        public PavementProperties Pavement { get; set; }
    }
}
