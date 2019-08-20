namespace Jpp.Ironstone.Highways.ObjectModel
{
    public static class Constants
    {
        public const string DEFAULT_ROAD_NAME = "UNNAMED ROAD";
        public const double DEFAULT_FOOTWAY_WIDTH = 2;
        public const double DEFAULT_CARRIAGEWAY_WIDTH = 2.4;
        public const int DEFAULT_CHAINAGE_MARKER = 5;
        public const double MINIMUM_FOOTWAY_WIDTH = 1.5;

        public const string LAYER_DEF_POINTS = "Defpoints";
        public const string LAYER_JPP_CENTRE_LINE = "JPP_Centreline";

        public const double DEFAULT_RADIUS_JUNCTION = 6;
        public const double DEFAULT_CROSSOVER_RADIUS = 1.35;
        public const double MINIMUM_PAVEMENT = 1.5;
        public const double POINT_TOLERANCE = 0.0001;
    }

    public enum FootwayTypes { Footway, ServiceStrip }
}