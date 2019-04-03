using System;
using Jpp.Ironstone.Highways.ObjectModel.Abstract;

namespace Jpp.Ironstone.Highways.ObjectModel.Objects.Offsets
{
    [Serializable]
    public class RoadClosureEnd : RoadClosure
    {
        public RoadClosureEnd() : base(ClosureTypes.End) { }
    }
}
