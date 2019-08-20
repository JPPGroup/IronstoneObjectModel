using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.Autocad;
using System;
using System.Xml.Serialization;

namespace Jpp.Ironstone.Highways.ObjectModel.Roads
{
    [Serializable]
    public class Road : DrawingObject
    {
        private string _name = Constants.DEFAULT_ROAD_NAME;

        public Guid Id { get; set; }

        public string Name
        {
            get => _name;
            set
            {
                if (value == _name) return;

                DirtyModified = true;
                _name = value;
            }
        }

        public bool HasErrors { get; set; }
        public RoadLabel Label { get; set; }
        public PersistentObjectIdCollection ChainageMarkers { get; set; }
        public RoadCarriageway Carriageway { get; set; }
        public RoadSegmentCollection Segments { get; set; }

        [XmlIgnore]
        public override double Rotation
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        [XmlIgnore]
        public override Point3d Location
        {
            get
            {
                var acTrans = BaseObject.Database.TransactionManager.TopTransaction;
                var curve = (Curve) acTrans.GetObject(BaseObject, OpenMode.ForRead);
                return curve.StartPoint;
            }
            set => throw new NotImplementedException();
        }

        [XmlIgnore]
        public Polyline CentreLine
        {
            get
            {
                var acTrans = BaseObject.Database.TransactionManager.TopTransaction;
                return (Polyline) acTrans.GetObject(BaseObject, OpenMode.ForRead);
            }
        }

        private Road()
        {
            Id = Guid.NewGuid();
            Segments = new RoadSegmentCollection();
            Carriageway = new RoadCarriageway();
            Label = new RoadLabel();
            ChainageMarkers = new PersistentObjectIdCollection();
        }

        public static Road CreateRoad(ObjectId objectId)
        {
            var road = new Road {BaseObject = objectId};
            road.Segments.Add(new RoadSegment {Chainage = 0});
            return road;
        }

        protected override void ObjectModified(object sender, EventArgs e)
        {
        }

        protected override void ObjectErased(object sender, ObjectErasedEventArgs e)
        {
            if (!e.Erased) return;

            ClearChainageMarkers();
            Label.Erase();
        }

        public override void CreateActiveObject()
        {
            base.CreateActiveObject();
            Label.CreateActiveObject();
        }

        public override void Generate()
        {
            Carriageway.Generate(this);
            SetLabelDetails();
            DrawChainageMarkers();

            DirtyModified = false;
        }

        public void Clear()
        {
            ClearChainageMarkers();
            Carriageway.Clear(this);
        }


        public override void Erase()
        {
            throw new NotImplementedException();
        }

        private void ClearChainageMarkers()
        {
            using var acTrans = BaseObject.Database.TransactionManager.StartTransaction();

            foreach (ObjectId obj in ChainageMarkers.Collection)
            {
                if (!obj.IsErased) acTrans.GetObject(obj, OpenMode.ForWrite).Erase();
            }

            ChainageMarkers.Clear();

            acTrans.Commit();
        }

        private void DrawChainageMarkers()
        {
            using var acTrans = BaseObject.Database.TransactionManager.StartTransaction();
            var centre = CentreLine;

            var first = AddChainageMarkersAtDist(0);
            if (centre.Length % Constants.DEFAULT_CHAINAGE_MARKER > 0) AddChainageMarkersAtDist(centre.Length, first);

            for (var i = 5; i < centre.Length; i += Constants.DEFAULT_CHAINAGE_MARKER)
            {
                AddChainageMarkersAtDist(i, first);
            }

            acTrans.Commit();
        }

        private bool AddChainageMarkersAtDist(double distance, bool? firstMarkerLessThan180 = null)
        {
            var angleBelow180 = false;
            var centre = CentreLine;
            var acTrans = BaseObject.Database.TransactionManager.TopTransaction;
            var acBlkTblRec = (BlockTableRecord) acTrans.GetObject(centre.BlockId, OpenMode.ForWrite);

            var point = centre.GetPointAtDist(distance);
            var ang = centre.GetFirstDerivative(centre.GetParameterAtPoint(point));

            var lineAng = ang.GetNormal() * 0.5;
            lineAng = lineAng.TransformBy(Matrix3d.Rotation(Math.PI / 2.0, centre.Normal, Point3d.Origin));

            var line = new Line
            {
                StartPoint = point - lineAng,
                EndPoint = point + lineAng,
                Layer = Constants.LAYER_DEF_POINTS
            };

            var labelAng = ang.GetNormal() * 0.1;
            var label = new MText
            {
                Layer = Constants.LAYER_DEF_POINTS,
                TextHeight = 0.25,
                Contents = $"{distance:0.000}"
            };

            var vec = line.StartPoint.GetVectorTo(line.EndPoint);
            var angle = vec.GetAngleTo(Vector3d.YAxis, Vector3d.ZAxis) * 180 / Math.PI;
            if (angle < 180)
            {
                angleBelow180 = true;
                if (firstMarkerLessThan180.HasValue && !firstMarkerLessThan180.Value)
                {
                    label.Attachment = AttachmentPoint.TopCenter;
                    label.Location = point + labelAng;
                }
                else
                {
                    label.Attachment = AttachmentPoint.BottomCenter;
                    label.Location = point - labelAng;
                }

                label.Rotation = (90 - angle) * Math.PI / 180;

            }
            else
            {
                if (firstMarkerLessThan180.HasValue && firstMarkerLessThan180.Value)
                {
                    label.Attachment = AttachmentPoint.TopCenter;
                    label.Location = point - labelAng;
                }
                else
                {
                    label.Attachment = AttachmentPoint.BottomCenter;
                    label.Location = point + labelAng;
                }

                label.Rotation = (90 - angle + 180) * Math.PI / 180;

            }

            ChainageMarkers.Add(acBlkTblRec.AppendEntity(line));
            ChainageMarkers.Add(acBlkTblRec.AppendEntity(label));

            acTrans.AddNewlyCreatedDBObject(line, true);
            acTrans.AddNewlyCreatedDBObject(label, true);

            return angleBelow180;
        }

        private void SetLabelDetails()
        {
            var centre = CentreLine;
            var ang = centre.GetFirstDerivative(centre.GetParameterAtPoint(Location));
            ang = ang.GetNormal() * 0.75;

            Label.Contents = Name;

            using var line = new Line {StartPoint = Location, EndPoint = Location + ang};

            var vec = line.StartPoint.GetVectorTo(line.EndPoint);
            var angle = vec.GetAngleTo(Vector3d.YAxis, Vector3d.ZAxis) * 180 / Math.PI;
            if (angle < 180)
            {
                var locAng = ang.TransformBy(Matrix3d.Rotation(Math.PI * 0.25, centre.Normal, Point3d.Origin));
                Label.Location = Location + locAng;

                Label.Attachment = AttachmentPoint.BottomLeft;
                Label.Rotation = (90 - angle) * Math.PI / 180;
            }
            else
            {
                var locAng = ang.TransformBy(Matrix3d.Rotation(Math.PI * 0.75, centre.Normal, Point3d.Origin));
                Label.Location = Location - locAng;

                Label.Attachment = AttachmentPoint.BottomRight;
                Label.Rotation = (90 - angle + 180) * Math.PI / 180;

            }
        }

        public void Reverse()
        {
            //TODO: reverse chainage on segments too
            using var acTrans = BaseObject.Database.TransactionManager.StartTransaction();
            var obj = acTrans.GetObject(BaseObject, OpenMode.ForRead);
            
            if(!obj.IsWriteEnabled) obj.UpgradeOpen();
            
            var cur = obj as Curve;
            if (cur != null) cur.ReverseCurve();

            acTrans.Commit();

            DirtyModified = true;
        }
    }
}
