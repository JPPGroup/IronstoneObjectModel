namespace Jpp.Ironstone.Drainage.ObjectModel
{
    public class Constants
    {
        public const string LAYER_DEF_POINTS_NAME = "Defpoints";

        #region JPP Layers
        public const string LAYER_PIPE_CENTRE_LINE_NAME = "JPP_Civil_Drainage_PipeCentreline";
        public const short LAYER_PIPE_CENTRE_LINE_COLOR = 7;
        public const string LAYER_PIPE_CENTRE_LINE_TYPE = "DASHED2";

        public const string LAYER_PIPE_WALLS_NAME = "JPP_Civil_Drainage_PipeWall";
        public const short LAYER_PIPE_WALLS_COLOR = 30;

        public const string LAYER_MANHOLE_WALL_NAME = "JPP_Civil_Drainage_ManholeWall";
        public const short LAYER_MANHOLE_WALL_COLOR = 5;

        public const string LAYER_MANHOLE_FURNITURE_NAME = "JPP_Civil_Drainage_Furniture";
        public const short LAYER_MANHOLE_FURNITURE_COLOR = 30;
        #endregion

        #region Prompts
        public const string PROMPT_START_POSITION = "\nPlease click start point:";
        public const string PROMPT_END_POSITION = "\nPlease click end point:";
        public const string PROMPT_COVER_LEVEL = "\nPlease enter cover Level (m):";
        public const string PROMPT_COVER = "\nPlease enter cover (m):";
        public const string PROMPT_PIPE = "\nPlease enter pipe diameter (mm):";
        public const string PROMPT_INITIAL_INVERT_LEVEL = "\nPlease enter initial invert level (m):";
        public const string PROMPT_GRADIENT = "\nPlease enter gradient (1 in):";
        #endregion

        #region Defaults
        public const double DEFAULT_COVER_LEVEL = 100;
        public const double DEFAULT_COVER = 1.2;
        public const double DEFAULT_PIPE_DIAMETER = 150;
        public const double DEFAULT_GRADIENT = 150;
        #endregion
    }
}
