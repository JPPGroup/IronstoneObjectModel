using Autodesk.AutoCAD.Geometry;

namespace Jpp.Ironstone.Drainage.ObjectModel.Layouts
{
    //MOVE: To Core...
    public class CivA0L : ILayout
    {
        private const double WIDTH = 865;
        private const double HEIGHT = 750;

        public string Name => "CIV_A0L";    
        public int Columns => 9;
        public int Row => 12;
        public double ColumnSize => WIDTH / Columns; 
        public double RowSize => HEIGHT / Row;
        public Point3d TopCorner => new Point3d(15, 810, 0);
    }
}
