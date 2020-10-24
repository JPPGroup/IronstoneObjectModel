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
using DBObject = Autodesk.AutoCAD.DatabaseServices.DBObject;
using Entity = Autodesk.AutoCAD.DatabaseServices.Entity;

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
                    foundationCentreLine.UnfactoredLineLoad = double.Parse(foundationCentreLine[FoundationGroup.FOUNDATION_CENTRE_LOAD_KEY]);
                    foundationCentreLine.UnfactoredOverlapLineLoad = double.Parse(foundationCentreLine[FoundationGroup.FOUNDATION_CENTRE_OVERLAPLOAD_KEY]);
                    centrelines.Add(foundationCentreLine);
                }
            }

            return centrelines;
        }

        private ICollection<FoundationCentreLine> DetermineOverlayedLines(ICollection<FoundationCentreLine> centrelines)
        {
            bool unchanged = true;
            int i = 0;
            do
            {
                // This is here as a failsafe to prevent the porgram locking
                if(i > 100)
                    throw new InvalidOperationException("Overlayed line loop failed to terminate.");

                unchanged = OverlayedIteration(centrelines);
                i++;
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
                        //Merge/split lines as necessary
                        ICollection<FoundationCentreLine> newLines = CalculateNewLines(subject, target);
                        foreach (FoundationCentreLine foundationCentreLine in newLines)
                        {
                            centrelines.Add(foundationCentreLine);
                        }

                        //Remove original lines
                        centrelines.Remove(target);
                        centrelines.Remove(subject);

                        target.Erase();
                        subject.Erase();

                        return false;
                    }
                }
            }

            return unchanged;
        }

        private ICollection<FoundationCentreLine> CalculateNewLines(FoundationCentreLine line1, FoundationCentreLine line2)
        {
            List<FoundationCentreLine> newLines = new List<FoundationCentreLine>();

            List<PointDistance> points = new List<PointDistance>();
            Point2d referencePoint = new Point2d(line1.StartPoint.X, line1.StartPoint.Y);
            points.Add(new PointDistance() { Point = referencePoint, Distance = 0});

            Point2d point1 = new Point2d(line1.EndPoint.X, line1.EndPoint.Y);
            points.Add(new PointDistance() { Point = point1, Distance = point1.GetDistanceTo(referencePoint) });

            Point2d point2 = new Point2d(line2.StartPoint.X, line2.StartPoint.Y);
            points.Add(new PointDistance() { Point = point2, Distance = point2.GetDistanceTo(referencePoint)});

            Point2d point3 = new Point2d(line2.EndPoint.X, line2.EndPoint.Y);
            points.Add(new PointDistance() { Point = point3, Distance = point3.GetDistanceTo(referencePoint)});

            var orderedPoints = points.OrderBy(pd => pd.Distance).Distinct();

            for (int i = 0; i < orderedPoints.Count() - 1; i++)
            {
                Point2d startPoint2d = orderedPoints.ElementAt(i).Point;
                Point3d start = new Point3d(startPoint2d.X, startPoint2d.Y, 0);
                Point2d endPoint2d = orderedPoints.ElementAt(i + 1).Point;
                Point3d end = new Point3d(endPoint2d.X, endPoint2d.Y, 0);
                LineDrawingObject line = LineDrawingObject.Create(HostDocument.Database, start, end);
                FoundationCentreLine centreLine = FoundationCentreLine.CreateFromLine(line, _soilProperties);

                //Determine line loads
                if (line1.IsTargetSegmentOf(centreLine))
                    centreLine.UnfactoredLineLoad += line1.UnfactoredOverlapLineLoad;

                if (line2.IsTargetSegmentOf(centreLine))
                    centreLine.UnfactoredLineLoad += line2.UnfactoredOverlapLineLoad;

                newLines.Add(centreLine);
            }

            return newLines;
        }

        private class PointDistance : IEquatable<PointDistance>
        {
            public const double DELTA = 0.001;
            public const int  PLACES = 3;

            public double Distance { get; set; }
            public Point2d Point { get; set; }

            // Autogenerated
            public bool Equals(PointDistance other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;

                bool distance = Math.Abs(other.Distance - Distance) < DELTA;
                bool x = Math.Abs(other.Point.X - Point.X) < DELTA;
                bool y = Math.Abs(other.Point.Y - Point.Y) < DELTA;

                return distance && x && y;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((PointDistance) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (int)Math.Round(Distance, PLACES) ^ (int)Math.Round(Point.X, PLACES) ^ (int)Math.Round(Point.Y, PLACES);
                }
            }
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
