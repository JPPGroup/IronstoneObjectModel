using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jpp.Ironstone.Highways.ObjectModel.Tests.Response
{
    [Serializable]
    public class CarriageWayProperties
    {
        public SidesOfCentre Side { get; set; }
        public OffsetTypes Type { get; set; }
        public double CarriageDistance { get; set; }
        public bool Ignored { get; set; }
        public int Intersections { get; set; }
        public SidesOfCentre PavementSide { get; set; }
        public OffsetTypes PavementType { get; set; }
        public double PavementDistance { get; set; }       
    }
}
