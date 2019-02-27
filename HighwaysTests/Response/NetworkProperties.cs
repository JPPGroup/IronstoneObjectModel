using System;

namespace HighwaysTests.Response
{
    [Serializable]
    public class NetworkProperties
    {
        public int CentreLineCount { get; set; }
        public int RoadCount { get; set; }
        public int JunctionCount { get; set; }
        public int JunctionRightCount { get; set; }
        public int JunctionLeftCount { get; set; }
    }
}
