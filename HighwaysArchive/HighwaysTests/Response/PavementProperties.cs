using System;

namespace Jpp.Ironstone.Highways.ObjectModel.Tests.Response
{
    [Serializable]
    public class PavementProperties
    {
        public SidesOfCentre Side { get; set; }
        public OffsetTypes Type { get; set; }
        public double Distance { get; set; }
        public int Curves { get; set; }
    }
}
