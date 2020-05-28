using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Jpp.Ironstone.Structures.ObjectModel.Foundations
{
    public class FoundationNode
    {
        public Guid PartitionId { get; set; }
        public Point3d Location;

        private List<FoundationConnection> _connected;

        public FoundationNode()
        {
            _connected = new List<FoundationConnection>();
        }

        public void AddFoundation(FoundationCentreLine fcl, double angle)
        {
            FoundationConnection con = new FoundationConnection()
            {
                Foundation =  fcl,
                Bearing = angle
            };

            if (fcl.StartPoint.IsEqualTo(Location))
            {
                con.ConnectionPoint = ConnectionPoint.Start;
            }
            else
            {
                con.ConnectionPoint = ConnectionPoint.End;
            }

            _connected.Add(con);
        }

        public void TrimFoundations()
        {
            if(_connected.Count > 1)
            { 
            var sortedConnections = _connected.OrderBy(fc => fc.Bearing).ToArray();
                for (int i = 0; i < sortedConnections.Count(); i++)
                {
                    FoundationCentreLine currentCentreLine = sortedConnections.ElementAt(i).Foundation;
                    ConnectionPoint cp = sortedConnections.ElementAt(i).ConnectionPoint;

                    FoundationConnection nextConnection = Next(i, sortedConnections);
                    FoundationCentreLine nextCentreLine = nextConnection.Foundation;
                    FoundationConnection previouConnection = Previous(i, sortedConnections);
                    FoundationCentreLine previousCentreLine = previouConnection.Foundation;

                    Curve nextSubject, previousSubject;
                    switch (cp)
                    {
                        case ConnectionPoint.Start:
                            nextSubject = currentCentreLine.RightOffsetCached;
                            previousSubject = currentCentreLine.LeftOffsetCached;
                            break;

                        case ConnectionPoint.End:
                            nextSubject = currentCentreLine.LeftOffsetCached;
                            previousSubject = currentCentreLine.RightOffsetCached;
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    Curve nextTarget, previousTarget;
                    switch (nextConnection.ConnectionPoint)
                    {
                        case ConnectionPoint.Start:
                            nextTarget = nextCentreLine.LeftOffsetCached;
                            break;

                        case ConnectionPoint.End:
                            nextTarget = nextCentreLine.RightOffsetCached;
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    switch (previouConnection.ConnectionPoint)
                    {
                        case ConnectionPoint.Start:
                            previousTarget = previousCentreLine.RightOffsetCached;
                            break;

                        case ConnectionPoint.End:
                            previousTarget = previousCentreLine.LeftOffsetCached;
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    TrimToIntersect(nextSubject, nextTarget, cp);
                    TrimToIntersect(previousSubject, previousTarget, cp);
                }
            }
        }

        private void TrimToIntersect(Curve subject, Curve target, ConnectionPoint startOrEnd)
        {
            Point3dCollection points = new Point3dCollection();
            subject.IntersectWith(target, Intersect.ExtendBoth, points, IntPtr.Zero, IntPtr.Zero);

            if (points.Count < 1)
            {
                return;
            }

            switch (subject)
            {
                case Line line:
                    switch (startOrEnd)
                    {
                        case ConnectionPoint.Start:
                            line.StartPoint = points[0];
                            break;

                        case ConnectionPoint.End:
                            line.EndPoint = points[0];
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;

                case Polyline polyline:
                    switch (startOrEnd)
                    {
                        case ConnectionPoint.Start:
                            polyline.AddVertexAt(0, new Point2d(points[0].X, points[0].Y), 0, polyline.ConstantWidth, polyline.ConstantWidth);
                            polyline.RemoveVertexAt(1);
                            break;

                        case ConnectionPoint.End:
                            polyline.AddVertexAt(polyline.NumberOfVertices - 1, new Point2d(points[0].X, points[0].Y), 0, polyline.ConstantWidth, polyline.ConstantWidth);
                            polyline.RemoveVertexAt(polyline.NumberOfVertices - 1);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private FoundationConnection Next(int i, IEnumerable<FoundationConnection> list)
        {
            if (i + 1 < list.Count())
            {
                return list.ElementAt(i + 1);
            }
            else
            {
                return list.ElementAt(i + 1 - list.Count());
            }
        }

        private FoundationConnection Previous(int i, IEnumerable<FoundationConnection> list)
        {
            if (i - 1 >= 0)
            {
                return list.ElementAt(i - 1);
            }
            else
            {
                return list.ElementAt(i - 1 + list.Count());
            }
        }
    }

    struct FoundationConnection
    {
        public FoundationCentreLine Foundation { get; set; }
        public double Bearing { get; set; }
        public ConnectionPoint ConnectionPoint { get; set; }
    }

    enum ConnectionPoint
    {
        Start,
        End
    }
}
