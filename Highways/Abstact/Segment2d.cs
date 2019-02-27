using System;
using System.Xml.Serialization;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Highways.Objectmodel.Extensions;
using Jpp.Ironstone.Highways.Objectmodel.Helpers;

namespace Jpp.Ironstone.Highways.Objectmodel
{
    public abstract class Segment2d : DrawingObject
    {        
        [XmlIgnore]
        public override double Rotation
        {
            get => 0;
            set { return; }
        }
        [XmlIgnore]
        public override Point3d Location
        {
            get => default(Point3d);
            set { return; }
        } 
        [XmlIgnore]
        public SegmentType Type {
            get
            {
                var acTrans = Application.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
                using (var curve = acTrans.GetObject(BaseObject, OpenMode.ForRead) as Curve)
                {
                    switch (curve)
                    {
                        case Line _:
                            return SegmentType.Line;
                        case Arc _:
                            return SegmentType.Arc;
                        default:
                            throw new ArgumentException("Invalid segment type.");
                    }
                }
            }
        }
        [XmlIgnore]
        public Point2d StartPoint
        {
            get
            {
                var acTrans = Application.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
                using (var curve = acTrans.GetObject(BaseObject, OpenMode.ForRead) as Curve)
                {
                    if (curve != null && (curve is Line || curve is Arc)) return new Point2d(curve.StartPoint.X, curve.StartPoint.Y);
                }

                throw new ArgumentException("Invalid segment type.");
            }
        }
        [XmlIgnore]
        public Point2d EndPoint
        {
            get
            {
                var acTrans = Application.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
                using (var curve = acTrans.GetObject(BaseObject, OpenMode.ForRead) as Curve)
                {
                    if (curve != null && (curve is Line || curve is Arc)) return new Point2d(curve.EndPoint.X, curve.EndPoint.Y);
                }

                throw new ArgumentException("Invalid segment type.");
            }
        }
        [XmlIgnore]
        public Vector2d StartVector
        {
            get
            {
                var acTrans = Application.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
                using (var curve = acTrans.GetObject(BaseObject, OpenMode.ForRead) as Curve)
                {
                    switch (curve)
                    {
                        case Line _:
                            return StartPoint.GetVectorTo(EndPoint);
                        case Arc arc:
                            var arcCentre = new Point2d(arc.Center.X, arc.Center.Y);
                            return arcCentre.GetVectorTo(StartPoint); 
                        default:
                            throw new ArgumentException("Invalid segment type.");
                    }
                }
            }
        }
        [XmlIgnore]
        public Vector2d EndVector
        {
            get
            {
                var acTrans = Application.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
                using (var curve = acTrans.GetObject(BaseObject, OpenMode.ForRead) as Curve)
                {
                    switch (curve)
                    {
                        case Line _:
                            return EndPoint.GetVectorTo(StartPoint);
                        case Arc arc:
                            var arcCentre = new Point2d(arc.Center.X, arc.Center.Y);
                            return arcCentre.GetVectorTo(EndPoint);
                        default:
                            throw new ArgumentException("Invalid segment type.");
                    }
                }
            }
        }
        [XmlIgnore]
        public double Angle
        {
            get
            {
                var acTrans = Application.DocumentManager.MdiActiveDocument.TransactionManager.TopTransaction;
                using (var curve = acTrans.GetObject(BaseObject, OpenMode.ForRead) as Curve)
                {
                    switch (curve)
                    {
                        case Line line:
                            return line.Angle;
                        case Arc arc:                            
                            return arc.Clockwise() ? RadiansHelper.AngleForRightSide(StartVector.Angle) : RadiansHelper.AngleForLeftSide(StartVector.Angle);
                        default:
                            throw new ArgumentException("Invalid segment type.");
                    }
                }
            }
        }

        public override void Generate() { }

        protected override void ObjectModified(object sender, EventArgs e) { }
        protected override void ObjectErased(object sender, ObjectErasedEventArgs e) { }
    }
}
