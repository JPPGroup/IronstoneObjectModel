using Autodesk.AutoCAD.Geometry;

namespace Jpp.Ironstone.Drainage.ObjectModel.Objects
{
    public class PipeConnection
    {
        public double Diameter { get; set; }
        public Point3d Location { get; set; }
        public double Angle { get; set; }
        public string Code { get; set; }
    }
}
