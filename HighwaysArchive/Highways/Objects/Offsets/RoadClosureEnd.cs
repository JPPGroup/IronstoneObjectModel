using System;
using Jpp.Ironstone.Highways.ObjectModel.Old.Abstract;

namespace Jpp.Ironstone.Highways.ObjectModel.Old.Objects.Offsets
{
    [Serializable]
    public class RoadClosureEnd : RoadClosure
    {
        public RoadClosureEnd() : base(ClosureTypes.End) { }
    }
}
