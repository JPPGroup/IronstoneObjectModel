using Autodesk.AutoCAD.Geometry;

namespace Jpp.Ironstone.Drainage.ObjectModel.Objects.Paper
{
    //MOVE: To Core...
    public class SpaceView
    {
        public int Rows { get; set; }
        public int Columns { get; set; }
        public int Area => Rows * Columns;
        public SpaceRegion Position { get; set; }
        public Point3d ModelTarget { get; set; }
        public double Scale { get; set; }
    }
}
