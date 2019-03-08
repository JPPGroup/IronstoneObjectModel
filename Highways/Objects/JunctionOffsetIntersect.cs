using System;
using Autodesk.AutoCAD.Geometry;

namespace Jpp.Ironstone.Highways.ObjectModel.Objects
{
    [Serializable]
    public class OffsetIntersect
    {
        public Point3d Point { get; }
        public bool Before { get; }

        public OffsetIntersect(Point3d point, bool before)
        {
            Point = point;
            Before = before;
        }
    }
}
