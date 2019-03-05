﻿namespace Jpp.Ironstone.Highways.Objectmodel.Abstact
{
    public abstract class CentreLineOffset
    {
        public abstract SidesOfCentre Side { get; }
        public abstract OffsetTypes OffsetType { get; }
        public abstract double Distance { get; }
        //public abstract List<JunctionIntersection> Intersects { get; }
        //public abstract bool Ignore { get; set; }
    }
}