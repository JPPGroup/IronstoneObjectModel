using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.Autocad.DrawingObjects;
using Jpp.Ironstone.Core.ServiceInterfaces;

namespace Jpp.Ironstone.Structures.ObjectModel.Foundations
{
    public class FoundationCentreLine : CurveDrawingObject
    {
        public Guid PartitionId { get; set; }

        private FoundationNode Node1;
        private FoundationNode Node2;

        private PersistentObjectIdCollection _offsets;

        public double UnfactoredLineLoad { get; set; }

        private SoilProperties _soilProperties;

        public Entity LeftOffset { get; set; }
        public Entity RightOffset { get; set; }

        public FoundationCentreLine(SoilProperties soilProperties)
        {
            _soilProperties = soilProperties;
            _offsets = new PersistentObjectIdCollection();
        }

        protected override void ObjectModified(object sender, EventArgs e)
        {
            
        }

        protected override void ObjectErased(object sender, ObjectErasedEventArgs e)
        {
            
        }

        public override void Generate()
        {
            double requiredWidth = UnfactoredLineLoad / _soilProperties.AllowableGroundBearingPressure;

            //To a minimum of 300
            //TODO: Confirm that this should be presnt, needs a more elegant solution
            if (double.IsNaN(requiredWidth) || requiredWidth < 0.3)
                requiredWidth = 0.3;

            // Get the current document and database
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            Transaction acTrans = acCurDb.TransactionManager.TopTransaction;
            
            LeftOffset = this.CreateLeftOffset(requiredWidth / 2);
            RightOffset = this.CreateRightOffset(requiredWidth / 2);

            BlockTableRecord modelSpace = acCurDb.GetModelSpace();
            modelSpace.UpgradeOpen();

            modelSpace.AppendEntity(LeftOffset);
            modelSpace.AppendEntity(RightOffset);

            acTrans.AddNewlyCreatedDBObject(LeftOffset, true);
            acTrans.AddNewlyCreatedDBObject(RightOffset, true);

        }

        public void AttachNodes(List<FoundationNode> nodes)
        {
            Node1 = null;
            Node2 = null;

            foreach (FoundationNode fn in nodes)
            {
                if (fn.Location.IsEqualTo(StartPoint))
                {
                    Node1 = fn;
                }
            }

            foreach (FoundationNode fn in nodes)
            {
                if (fn.Location.IsEqualTo(EndPoint))
                {
                    Node2 = fn;
                }
            }

            if (Node1 == null)
            {
                Node1 = new FoundationNode();
                Node1.Location = StartPoint;
                nodes.Add(Node1);
            }

            if (Node2 == null)
            {
                Node2 = new FoundationNode();
                Node2.Location = EndPoint;
                nodes.Add(Node2);
            }

            /*double startAngle = Math.Atan((EndPoint.X - StartPoint.X) / (EndPoint.Y - StartPoint.Y)) * 180 / Math.PI;
            double endAngle = Math.Atan((StartPoint.X - EndPoint.X) / (StartPoint.Y - EndPoint.Y)) * 180 / Math.PI;*/

            Vector3d startVector = StartPoint.GetVectorTo(EndPoint);
            double startAngle = startVector.GetAngleTo(Vector3d.YAxis, Vector3d.ZAxis) * 180 / Math.PI;
            Vector3d endVector = EndPoint.GetVectorTo(StartPoint);
            double endAngle = endVector.GetAngleTo(Vector3d.YAxis, Vector3d.ZAxis) * 180 / Math.PI;

            Node1.AddFoundation(this, startAngle);
            Node2.AddFoundation(this, endAngle);
        }
    }
}
