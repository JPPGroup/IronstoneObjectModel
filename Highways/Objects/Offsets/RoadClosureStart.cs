using System;
using Jpp.Ironstone.Highways.ObjectModel.Abstract;

namespace Jpp.Ironstone.Highways.ObjectModel.Objects.Offsets
{
    [Serializable]
    public class RoadClosureStart : RoadClosure
    {
        public RoadClosureStart() : base(ClosureTypes.Start) { }
    }
}
