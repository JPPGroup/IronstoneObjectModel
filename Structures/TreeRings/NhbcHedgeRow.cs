using System;
using System.Xml.Serialization;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.Autocad;

namespace Jpp.Ironstone.Structures.ObjectModel.TreeRings
{
    //JAb: Need to review - Tree base is circle?!?
    public class NhbcHedgeRow : NHBCTree
    {
        [XmlIgnore]
        public override Point3d Location
        {
            get
            {
                var acTrans = Application.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
                using (var c = acTrans.GetObject(BaseObject, OpenMode.ForRead) as Curve)
                {
                    if (c == null) throw new NullReferenceException();

                    var vector = c.EndPoint.GetAsVector() - c.StartPoint.GetAsVector();
                    return c.StartPoint + vector * 0.5;
                }
            }
            //JAb: Not SOLID, violates Liskov Substitution & Interface Segregation principles!
            set { }
        }

        //JAb: Not SOLID, violates Liskov Substitution & Interface Segregation principles!
        protected override void GenerateBase() { }

        public override DBObjectCollection DrawRings(Shrinkage shrinkage, double startDepth, double step)
        {
            var collection = new DBObjectCollection();
            var currentDepth = startDepth;

            while (true)
            {
                var shape = DrawShape(currentDepth, shrinkage);
                if (shape == null) return collection;

                collection.Add(shape);
                currentDepth += step;
            }
        }

        public override Curve DrawShape(double depth, Shrinkage shrinkage)
        {
            var plane = new Plane();
            var acTrans = Application.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
            var c = acTrans.GetObject(BaseObject, OpenMode.ForWrite) as Curve;
            if (c == null) return null;

            var radius = GetRingRadius(depth, shrinkage);
            if (radius <= 0) return null;

            var plus = c.GetOffsetCurves(radius)[0] as Polyline;
            var minus = c.GetOffsetCurves(-radius)[0] as Polyline;

            if (plus == null || minus == null) return null;

            if (c is Polyline pLine)
            {
                var vn = pLine.NumberOfVertices - 1;
                for (var i = vn; i > 0; i--)
                {
                    var circle = new Circle { Center = pLine.GetPoint3dAt(i), Radius = radius };
                    if (DoesIntersect(plus, circle)) plus.FilletAt(i, radius);
                    if (DoesIntersect(minus, circle)) minus.FilletAt(i, radius);
                }
            }

            var endAngleStart = plus.EndPoint.Convert2d(plane).GetVectorTo(minus.EndPoint.Convert2d(plane));
            var endAngleEnd = minus.EndPoint.Convert2d(plane).GetVectorTo(plus.EndPoint.Convert2d(plane));

            var startAngleStart = plus.StartPoint.Convert2d(plane).GetVectorTo(minus.StartPoint.Convert2d(plane));
            var startAngleEnd = minus.StartPoint.Convert2d(plane).GetVectorTo(plus.StartPoint.Convert2d(plane));

            var endCurve = new Arc(c.EndPoint, radius, endAngleEnd.Angle, endAngleStart.Angle);
            var startCurve = new Arc(c.StartPoint, radius, startAngleStart.Angle, startAngleEnd.Angle);

            plus.JoinEntities(new Entity[] { startCurve, endCurve, minus });
            plus.Closed = true;

            return plus;
        }

        private static bool DoesIntersect(Entity firstCurve, Entity secondCurve)
        {
            var pts = new Point3dCollection();
            firstCurve.IntersectWith(secondCurve, Intersect.OnBothOperands, new Plane(), pts, IntPtr.Zero, IntPtr.Zero);
            return pts.Count > 0;
        }
    }
}
