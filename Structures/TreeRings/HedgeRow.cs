﻿using System;
using System.Collections.Generic;
using System.Linq;
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

        public override void Generate()
        {
            base.Generate();
            ValidateCentreLine();
        }


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

        //TODO: Need to review drawing of initial offset...
        public override Curve DrawShape(double depth, Shrinkage shrinkage)
        {
            var radius = GetRingRadius(depth, shrinkage);
            if (radius <= 0) return null;

            var pLine = PolylineFromBase();
            if (pLine == null) return null;

            //TODO: Review this offset section as believe its a large maxinitialoffset result that is causing errors
            var initOffset = MaxInitialOffset(pLine);
            var initOffPlus = TryOffsetCurve(pLine, initOffset);
            var initOffMinus = TryOffsetCurve(pLine, -initOffset);

            var plusVert = new List<int>();
            var minusVert = new List<int>();

            for (var i = 1; i < pLine.NumberOfVertices; i++)
            {
                var circle = new Circle {Center = pLine.GetPoint3dAt(i), Radius = initOffset };
                if (DoesIntersect(initOffPlus, circle)) plusVert.Add(i);
                if (DoesIntersect(initOffMinus, circle)) minusVert.Add(i);
            }

            var plusVertOrdered = plusVert.OrderByDescending(val => val).ToList();
            var minusVertOrdered = minusVert.OrderByDescending(val => val).ToList();

            plusVertOrdered.ForEach(i => initOffPlus.FilletAt(i, initOffset));
            minusVertOrdered.ForEach(i => initOffMinus.FilletAt(i, initOffset));

            //Now we've built the base create the real thing....
            var adjust = radius - initOffset;
            var realOffPlus = TryOffsetCurve(initOffPlus, adjust);
            var realOffMinus = TryOffsetCurve(initOffMinus, -adjust);
            
            if (!pLine.Closed)
            {
                var start = StartArc(pLine, realOffPlus, realOffMinus, radius);
                var end = EndArc(pLine, realOffPlus, realOffMinus, radius);

                realOffPlus.JoinEntities(new Entity[] {start, end, realOffMinus});
                realOffPlus.Closed = true;
            }
            else
            {
                realOffPlus.Closed = true;
                realOffMinus.Closed = true;
                if (realOffMinus.Length > realOffPlus.Length)
                {
                    realOffPlus = realOffMinus;
                }
            }

            return realOffPlus;
        }

        //TODO: Consider moving to base extension
        private static Polyline TryOffsetCurve(Curve pLine, double offset)
        {
            var noPolyEx = new ArgumentException("No offset curve created.");
            try
            {
                var result = pLine.GetOffsetCurves(offset)[0] as Polyline;
                return result ?? throw noPolyEx;
            }
            catch (Exception)
            {
                throw noPolyEx;
            }
        }

        private Arc StartArc(Curve baseCurve, Curve plusCurve, Curve minusCurve, double radius)
        {
            var plane = new Plane();

            var start = plusCurve.StartPoint.Convert2d(plane).GetVectorTo(baseCurve.StartPoint.Convert2d(plane));
            var end = minusCurve.StartPoint.Convert2d(plane).GetVectorTo(baseCurve.StartPoint.Convert2d(plane));

            return new Arc(baseCurve.StartPoint, radius, start.Angle, end.Angle);
        }

        private Arc EndArc(Curve baseCurve, Curve plusCurve, Curve minusCurve, double radius)
        {
            var plane = new Plane();

            var start = plusCurve.EndPoint.Convert2d(plane).GetVectorTo(baseCurve.EndPoint.Convert2d(plane));
            var end = minusCurve.EndPoint.Convert2d(plane).GetVectorTo(baseCurve.EndPoint.Convert2d(plane));

            return new Arc(baseCurve.EndPoint, radius, end.Angle, start.Angle);
        }

        //TODO: Consider moving to base extension
        private Polyline PolylineFromBase()
        {
            var acTrans = Application.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
            var c = acTrans.GetObject(BaseObject, OpenMode.ForWrite) as Curve;
            
            if (c == null) return null;
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
                    var plane = new Plane();
                    poly = new Polyline();
                    polyline3d.Flatten();

                    poly.AddVertexAt(0, new Point2d(0, 0), 0, 0, 0);
                    poly.AddVertexAt(1, polyline3d.StartPoint.Convert2d(plane), 0, 0, 0);

                    var objCol = new DBObjectCollection();
                    polyline3d.Explode(objCol);

                    var entList = new List<Entity>();
                    foreach (Curve obj in objCol) entList.Add(obj);
                    poly.JoinEntities(entList.ToArray());
                    poly.RemoveVertexAt(0);
                    break;
            }

            if (poly == null) return null;

            //IMPORTANT: Flatten polyline, ensure coplanar objects are created for union of regions
            poly.Flatten();

            return poly;
        }

        private static double MaxInitialOffset(Polyline pLine)
        {
            var len = pLine.Length;
            for (var i = 0; i < pLine.NumberOfVertices; i++)
            {
                if (pLine.GetSegmentType(i) != SegmentType.Line) continue;

                var seg = pLine.GetLineSegment2dAt(i);
                if (seg.Length < len) len = seg.Length;
            }

            return len * 0.4;
        }

        //TODO: Consider moving to base extension
        private static bool DoesIntersect(Entity firstEntity, Entity secondEntity)
        {
            var pts = new Point3dCollection();
            firstEntity.IntersectWith(secondEntity, Intersect.OnBothOperands, new Plane(), pts, IntPtr.Zero, IntPtr.Zero);
            return pts.Count > 0;
        }

        private void ValidateCentreLine()
        {
            //Ensure joining vertices are not effectively straight - tolerance set at 0.1 degree
            const double tolerance = (Math.PI / 180) * 0.1;
            var basePolyLine = PolylineFromBase();

            for (var i = basePolyLine.NumberOfVertices - 2; i > 0; i--)
            {
                var l1 = basePolyLine.GetLineSegmentAt(i- 1);
                var l2 = basePolyLine.GetLineSegmentAt(i);

                var v1 = l1.EndPoint - l1.StartPoint;
                var v2 = l2.EndPoint - l2.StartPoint;
                var angle = v1.GetAngleTo(v2);

                if(angle < tolerance) 
                    basePolyLine.RemoveVertexAt(i);
            }
        }
    }
}
