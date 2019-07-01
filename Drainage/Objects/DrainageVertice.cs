using Autodesk.AutoCAD.Geometry;

namespace Jpp.Ironstone.Drainage.ObjectModel.Objects
{
    public class DrainageVertex
    {
        public Point2d StartPoint { get; set; }
        public Point2d EndPoint { get; set; }
        public double Diameter { get; set; }
        public double Cover { get; set; }
        public double Gradient { get; set; }
    }
}
