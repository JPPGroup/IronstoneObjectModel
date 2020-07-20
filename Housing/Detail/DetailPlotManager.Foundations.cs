using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Structures.ObjectModel.Foundations;
using Jpp.Ironstone.Structures.ObjectModel.Ground;

namespace Jpp.Ironstone.Housing.ObjectModel.Detail
{
    // TODO: Review the entire class as this has been copied wholesale
    [Layer(Name = FoundationCentreLine.FOUNDATION_LAYER)]
    public partial class DetailPlotManager : AbstractDrawingObjectManager<DetailPlot>
    {
        private List<FoundationGroup> _foundationGroups;

        public void UpdateFoundations(IReadOnlyCollection<string> PlotIds)
        {
            throw new NotImplementedException();
        }

        private string _foundationLayerName;

        public void UpdateAllFoundations()
        {
            SoilSurfaceContainer soilSurfaceContainer = new SoilSurfaceContainer(this.HostDocument);

            LayerManager layerManager = DataService.Current.GetStore<DocumentStore>(HostDocument.Name).LayerManager;
            _foundationLayerName = layerManager.GetLayerName(FoundationCentreLine.FOUNDATION_LAYER);

            foreach (FoundationGroup group in _foundationGroups)
            {
                group.Delete();
            }

            _foundationGroups.Clear();

            ICollection<FoundationCentreLine> centrelines = GetCentrelines();
            centrelines = DetermineOverlayedLines(centrelines);
            GroupCentrelines(centrelines);

            foreach (FoundationGroup foundationGroup in _foundationGroups)
            {
                foundationGroup.Rebuild(soilSurfaceContainer);
            }

            RemoveCentrelines(centrelines);
        }

        private ICollection<FoundationCentreLine> GetCentrelines()
        {
            // Explode plots into foundation centrelines 
            // TODO: Implement dirty
            List<FoundationCentreLine> centrelines = new List<FoundationCentreLine>();

            foreach (DetailPlot detailPlot in ManagedObjects)
            {
                IReadOnlyCollection<LineDrawingObject> plotCentres = detailPlot.GetFoundationCentrelines();
                foreach (LineDrawingObject lineDrawingObject in plotCentres)
                {
                    FoundationCentreLine foundationCentreLine = FoundationCentreLine.CreateFromLine(lineDrawingObject, _soilProperties);
                    foundationCentreLine.PlotIds.Add(detailPlot.PlotId);
                    centrelines.Add(foundationCentreLine);
                }
            }

            return centrelines;
        }

        private ICollection<FoundationCentreLine> DetermineOverlayedLines(ICollection<FoundationCentreLine> centrelines)
        {
            bool unchanged = true;
            do
            {
                unchanged = OverlayedIteration(centrelines);
            } while (!unchanged);

            return centrelines;
        }

        private bool OverlayedIteration(ICollection<FoundationCentreLine> centrelines)
        {
            bool unchanged = true;

            //Iterate overall intersecting lines and group them
            for (int i = 0; i < centrelines.Count; i++)
            {
                FoundationCentreLine subject = centrelines.ElementAt(i);

                for (int j = 0; j < centrelines.Count; j++)
                {
                    if(i == j)
                        continue;

                    FoundationCentreLine target = centrelines.ElementAt(j);
                    if (subject.IsTargetSegmentOf(target))
                    {
                        unchanged = false;

                        //Merge/split lines as necessary

                        //Remove original lines
                        centrelines.Remove(target);
                        centrelines.Remove(subject);

                        target.Erase();
                        subject.Erase();
                    }
                }
            }

            return unchanged;
        }

        private void GroupCentrelines(ICollection<FoundationCentreLine> centrelines)
        {
            // Group foundations that touch
            List<FoundationNode> nodes = new List<FoundationNode>();

            GenerateNodes(nodes, centrelines);
            Partition(nodes, centrelines);
        }

        private void GenerateNodes(List<FoundationNode> nodes, ICollection<FoundationCentreLine> centrelines)
        {
            bool complete = false;

            while (!complete)
            {
                nodes.Clear();
                complete = true;

                foreach (FoundationCentreLine fcl in centrelines)
                {
                    fcl.AttachNodes(nodes);
                }

                Document acDoc = Application.DocumentManager.MdiActiveDocument;
                Database acCurDb = acDoc.Database;
                Transaction acTrans = acCurDb.TransactionManager.TopTransaction;

                // TODO: This section needs checking
                //Iterate over full collection, identifying any nodes that intersect part way through the lines and adjusting to suit
                BlockTableRecord modelSpace = HostDocument.Database.GetModelSpace(true);
                foreach (FoundationNode foundationNode in nodes)
                {
                    for (int i = 0; i < centrelines.Count; i++)
                    {
                        Curve toBeChecked = acTrans.GetObject(centrelines.ElementAt(i).BaseObject, OpenMode.ForRead) as Curve;
                        Point3d closest = toBeChecked.GetClosestPointTo(foundationNode.Location, false);
                        if (!closest.IsEqualTo(toBeChecked.StartPoint) && !closest.IsEqualTo(toBeChecked.EndPoint))
                        {
                            //TODO: Will global tolerance work??
                            if ((foundationNode.Location - closest).Length <= 0.0001) //Tolerance.Global.EqualPoint)
                            {
                                Point3dCollection splitPoint = new Point3dCollection();
                                splitPoint.Add(closest);
                                //Node is one line
                                DBObjectCollection splitSegments = toBeChecked.GetSplitCurves(splitPoint);

                                foreach (DBObject splitSegment in splitSegments)
                                {
                                    
                                    FoundationCentreLine fcl = new FoundationCentreLine(acDoc, _soilProperties, _foundationLayerName);
                                    fcl.BaseObject = modelSpace.AppendEntity(splitSegment as Entity);
                                    acTrans.AddNewlyCreatedDBObject(splitSegment, true);
                                    centrelines.Add(fcl);
                                }

                                toBeChecked.Erase();
                                //centrelines.RemoveAt(i);
                                centrelines.Remove(centrelines.ElementAt(i));
                                complete = false;
                                break;
                            }
                        }
                    }
                }
            }
        }


        private void Partition(List<FoundationNode> nodes, ICollection<FoundationCentreLine> centrelines)
        {
            /*Guid id = Guid.NewGuid();
            //TODO: Add optimisation partition code here
            partitionIds.Clear();
            partitionIds.Add(id);

            foreach(FoundationCentreLine fcl in centrelines)
            {
                fcl.PartitionId = id;
            }

            foreach (FoundationNode n in nodes)
            {
                n.PartitionId = id;
            }*/
            FoundationGroup foundationGroup = new FoundationGroup();
            foreach (FoundationCentreLine foundationCentreLine in centrelines)
            {
                foundationGroup.Centrelines.Add(foundationCentreLine);
            }

            foreach (FoundationNode foundationNode in nodes)
            {
                foundationGroup.Nodes.Add(foundationNode);
            }

            _foundationGroups.Add(foundationGroup);
        }

        private void RemoveCentrelines(ICollection<FoundationCentreLine> centrelines)
        {
            // TODO: Implemenmt centreline clearup
        }

    }
}
