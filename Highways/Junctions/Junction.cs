using Jpp.Ironstone.Core.Autocad;
using System;

namespace Jpp.Ironstone.Highways.ObjectModel.Junctions
{
    [Serializable]
    public class Junction
    {
        public Guid PrimaryRoadId { get; set; }
        public Guid SecondaryRoadId { get; set; }
        public SerializablePoint IntersectionPoint { get; set; }
        public double Chainage { get; set; }
        public Side Side { get; set; }
    }
}
