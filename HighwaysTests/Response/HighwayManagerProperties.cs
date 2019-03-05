using System;

namespace Jpp.Ironstone.Highways.ObjectModel.Tests.Response
{
    [Serializable]
    public class HighwayManagerProperties
    {
        public int CentreLineCount { get; set; }
        public int RoadCount { get; set; }
        public int JunctionCount { get; set; }
        public int JunctionRightCount { get; set; }
        public int JunctionLeftCount { get; set; }
    }
}
