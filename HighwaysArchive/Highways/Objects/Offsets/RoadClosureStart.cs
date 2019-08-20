using System;
using Jpp.Ironstone.Highways.ObjectModel.Old.Abstract;

namespace Jpp.Ironstone.Highways.ObjectModel.Old.Objects.Offsets
{
    [Serializable]
    public class RoadClosureStart : RoadClosure
    {
        public RoadClosureStart() : base(ClosureTypes.Start) { }
    }
}
