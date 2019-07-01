using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.Autocad;

namespace Jpp.Ironstone.Structures.ObjectModel.TreeRings
{
    //JAb: Need to review - Tree base is circle?!?
    public class HedgeRow : Tree
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

            if (!(c is Polyline) && !(c is Polyline2d) && !(c is Polyline3d)) return null;

            Polyline poly = null;
            switch (c)
            {
                case Polyline polyline:
                    poly = polyline;
                    break;
                case Polyline2d polyline2d:
                    poly = new Polyline();
                    poly.ConvertFrom(polyline2d, false);
                    break;
                case Polyline3d polyline3d:
                    poly = new Polyline();
                    polyline3d.Flatten();

                    poly.AddVertexAt(0, new Point2d(0, 0), 0, 0, 0);
                    poly.AddVertexAt(1, polyline3d.StartPoint.Convert2d(plane),0,0,0);
                    
                    var objCol = new DBObjectCollection();
                    polyline3d.Explode(objCol);

                    var entList = new List<Entity>();
                    foreach (Curve obj in objCol)
                    {

                        entList.Add(obj);
                    }

                    poly.JoinEntities(entList.ToArray());
                    poly.RemoveVertexAt(0);
                    break;
            }

            if (poly == null) return null;
            var vn = poly.NumberOfVertices - 1;

            var pOffPlus = poly.GetOffsetCurves(radius)[0] as Polyline;
            var pOffMinus = poly.GetOffsetCurves(-radius)[0] as Polyline;

            for (var i = vn; i > 0; i--)
            {
                var circle = new Circle { Center = poly.GetPoint3dAt(i), Radius = radius };
                if (DoesIntersect(pOffPlus, circle)) pOffPlus.FilletAt(i, radius);
                if (DoesIntersect(pOffMinus, circle)) pOffMinus.FilletAt(i, radius);
            }

            if (pOffMinus == null || pOffPlus == null) return null;

            var endAngleStart = pOffPlus.EndPoint.Convert2d(plane).GetVectorTo(pOffMinus.EndPoint.Convert2d(plane));
            var endAngleEnd = pOffMinus.EndPoint.Convert2d(plane).GetVectorTo(pOffPlus.EndPoint.Convert2d(plane));

            var startAngleStart = pOffPlus.StartPoint.Convert2d(plane).GetVectorTo(pOffMinus.StartPoint.Convert2d(plane));
            var startAngleEnd = pOffMinus.StartPoint.Convert2d(plane).GetVectorTo(pOffPlus.StartPoint.Convert2d(plane));

            var endCurve = new Arc(c.EndPoint, radius, endAngleEnd.Angle, endAngleStart.Angle);
            var startCurve = new Arc(c.StartPoint, radius, startAngleStart.Angle, startAngleEnd.Angle);

            pOffPlus.JoinEntities(new Entity[] { startCurve, endCurve, pOffMinus });
            pOffPlus.Closed = true;

            return pOffPlus;
        }
        
        private static bool DoesIntersect(Entity firstCurve, Entity secondCurve)
        {
            var pts = new Point3dCollection();
            firstCurve.IntersectWith(secondCurve, Intersect.OnBothOperands, new Plane(), pts, IntPtr.Zero, IntPtr.Zero);
            return pts.Count > 0;
        }
    }
}
