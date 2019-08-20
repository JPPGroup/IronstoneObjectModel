using System;
using Autodesk.AutoCAD.Geometry;

namespace Jpp.Ironstone.Highways.ObjectModel.Old.Objects
{
    [Serializable]
    public class OffsetIntersect
    {
        public Point3d Point { get; }
        public bool Before { get; }

        private OffsetIntersect() { }

        public OffsetIntersect(Point3d point, bool before)
        {
            Point = point;
            Before = before;
        }
    }
}
