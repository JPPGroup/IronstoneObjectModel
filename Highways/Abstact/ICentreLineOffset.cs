using System.Collections.Generic;
using Jpp.Ironstone.Highways.ObjectModel.Objects;

namespace Jpp.Ironstone.Highways.ObjectModel.Abstract
{
    interface ICentreLineOffset
    {
        SidesOfCentre Side { get; }
        OffsetTypes OffsetType { get; }
        double Distance { get; }
        List<OffsetIntersect> Intersection { get; }
        bool Ignore { get; set; }
    }
}
