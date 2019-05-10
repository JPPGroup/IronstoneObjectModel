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
using Jpp.Ironstone.Structures.ObjectModel.TreeRings;

namespace Jpp.Ironstone.Structures.ObjectModel.Foundations
{
    public class FoundationManager : AbstractDrawingObjectManager
    {
        public List<FoundationCentreLine> Foundations { get; set; }
        private List<Guid> _partitionIds;
        private List<FoundationNode> _nodes;

        public FoundationManager(Document document) : base(document)
        {
            Foundations = new List<FoundationCentreLine>();
            _partitionIds = new List<Guid>();
            _nodes = new List<FoundationNode>();
        }

        public FoundationManager() : base()
        {
            Foundations = new List<FoundationCentreLine>();
        }

        public override void UpdateDirty()
        {
            //TODO: Optimise
            UpdateAll();
        }

        public override void UpdateAll()
        {
            RemoveErased();
            GenerateNodes();
            Partition();
            foreach (Guid guid in _partitionIds)
            {
                UpdatePartition(guid);
            }
        }

        public override void Clear()
        {
            throw new NotImplementedException();
        }

        public override void ActivateObjects()
        {
            // Get the current document and database
            Database acCurDb = HostDocument.Database;

            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                foreach (FoundationCentreLine fcl in Foundations)
                {
                    fcl.CreateActiveObject();
                }
            }
        }

        private void RemoveErased()
        {
            for (int i = Foundations.Count - 1; i >= 0; i--)
            {
                if (Foundations[i].DirtyRemoved)
                {
                    Foundations.RemoveAt(i);
                }
            }
        }

        public override void AllDirty()
        {
            foreach (FoundationCentreLine fcl in Foundations)
            {
                fcl.DirtyModified = true;
            }
        }

        public void Add(FoundationCentreLine fcl)
        {
            Foundations.Add(fcl);
            fcl.DirtyAdded = true;
        }

        public void SpliteCentrelines(FoundationCentreLine fcl)
        {
            /*Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Transaction acTrans = acCurDb.TransactionManager.TopTransaction;

            Curve newLine = acTrans.GetObject(fcl.BaseObject, OpenMode.ForRead) as Curve;

            List<int> removed = new List<int>();
            for (int i = 0; i < Foundations.Count; i++)
            {
                Curve toBeChecked = acTrans.GetObject(Foundations[i].BaseObject, OpenMode.ForRead) as Curve;
                Point3dCollection intersectionPoints = new Point3dCollection();
                newLine.IntersectWith(toBeChecked, Intersect.OnBothOperands, intersectionPoints, IntPtr.Zero,
                    IntPtr.Zero);
            }*/
        }

        /// <summary>
        /// Run through all foundation lines and create common nodes at intersections
        /// </summary>
        private void GenerateNodes()
        {
            bool complete = false;

            while (!complete)
            {
                _nodes.Clear();
                complete = true;

                foreach (FoundationCentreLine fcl in Foundations)
                {
                    fcl.AttachNodes(_nodes);
                }

                Document acDoc = Application.DocumentManager.MdiActiveDocument;
                Database acCurDb = acDoc.Database;
                Transaction acTrans = acCurDb.TransactionManager.TopTransaction;

                //Iterate over full collection, identifying any nodes that intersect part way through the lines and adjusting to suit
                SoilProperties sp = DataService.Current.GetStore<StructureDocumentStore>(acDoc.Name).SoilProperties;
                BlockTableRecord modelSpace = acCurDb.GetModelSpace();
                modelSpace.UpgradeOpen();

                foreach (FoundationNode foundationNode in _nodes)
                {
                    for (int i = 0; i < Foundations.Count; i++)
                    {
                        Curve toBeChecked = acTrans.GetObject(Foundations[i].BaseObject, OpenMode.ForRead) as Curve;
                        Point3d closest = toBeChecked.GetClosestPointTo(foundationNode.Location, false);
                        if (!closest.IsEqualTo(toBeChecked.StartPoint) && !closest.IsEqualTo(toBeChecked.EndPoint))
                        {
                            //TODO: Will global tolerance work??
                            if ((foundationNode.Location - closest).Length <= 0.0001)//Tolerance.Global.EqualPoint)
                            {
                                Point3dCollection splitPoint = new Point3dCollection();
                                splitPoint.Add(closest);
                                //Node is one line
                                DBObjectCollection splitSegments = toBeChecked.GetSplitCurves(splitPoint);

                                foreach (DBObject splitSegment in splitSegments)
                                {
                                    FoundationCentreLine fcl = new FoundationCentreLine(sp);
                                    fcl.BaseObject = modelSpace.AppendEntity(splitSegment as Entity);
                                    acTrans.AddNewlyCreatedDBObject(splitSegment, true);
                                    Foundations.Add(fcl);
                                }

                                toBeChecked.Erase();
                                Foundations.RemoveAt(i);
                                complete = false;
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void Partition()
        {
            Guid id = Guid.NewGuid();
            //TODO: Add optimisation partition code here
            _partitionIds.Add(id);

            foreach(FoundationCentreLine fcl in Foundations)
            {
                fcl.PartitionId = id;
            }

            foreach (FoundationNode n in _nodes)
            {
                n.PartitionId = id;
            }
        }

        private void UpdatePartition(Guid partitionId)
        {
            IEnumerable<FoundationCentreLine> updateSet =
                Foundations.Where(fcl => fcl.PartitionId == partitionId).Select(fcl => fcl);

            foreach (FoundationCentreLine fcl in updateSet)
            {
                fcl.Generate();
                if (fcl.LeftOffset == null || fcl.RightOffset == null)
                {
                    fcl.Generate();
                }
            }

            IEnumerable<FoundationNode> nodeUpdateSet =
                _nodes.Where(n => n.PartitionId == partitionId).Select(n => n);

            foreach (FoundationNode n in nodeUpdateSet)
            {
                n.TrimFoundations();
            }
        }
    }
}
