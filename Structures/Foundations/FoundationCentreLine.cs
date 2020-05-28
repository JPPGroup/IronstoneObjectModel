using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil;
using Autodesk.Civil.DatabaseServices;
using Jpp.DesignCalculations.Calculations.Design.Foundations;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Structures.ObjectModel.Ground;

namespace Jpp.Ironstone.Structures.ObjectModel.Foundations
{
    public class FoundationCentreLine : LineDrawingObject
    {
        public const string FOUNDATION_LAYER = "structures.foundations.layers.foundation";
        // TODO: Check layers can be found

        public Guid PartitionId { get; set; }

        public List<string> PlotIds { get; }

        private FoundationNode Node1;
        private FoundationNode Node2;

        private PersistentObjectIdCollection _offsets;

        public double UnfactoredLineLoad { get; set; }

        private SoilProperties _soilProperties;

        public ObjectId LeftOffset { get; set; }
        public ObjectId RightOffset { get; set; }

        public Curve LeftOffsetCached
        {
            get
            {
                if (_leftOffsetCached == null)
                {
                    Transaction trans = _document.TransactionManager.TopTransaction;
                    _leftOffsetCached = (Curve)trans.GetObject(LeftOffset, OpenMode.ForWrite);
                }

                return _leftOffsetCached;
            }
            private set { _leftOffsetCached = value; }
        }

        public Curve RightOffsetCached
        {
            get
            {
                if (_rightOffsetCached == null)
                {
                    Transaction trans = _document.TransactionManager.TopTransaction;
                    _rightOffsetCached = (Curve)trans.GetObject(RightOffset, OpenMode.ForWrite);
                }

                return _rightOffsetCached;
            }
            private set { _rightOffsetCached = value; }
        }

        private Curve _leftOffsetCached, _rightOffsetCached;

        public FoundationCentreLine(Document doc, SoilProperties soilProperties, string foundationLayerName) : base(doc)
        {
            _soilProperties = soilProperties;
            _offsets = new PersistentObjectIdCollection();
            PlotIds = new List<string>();
            _foundationLayerName = foundationLayerName;
        }

        private string _foundationLayerName;

        public void AddWidths(SoilSurfaceContainer soilSurfaceContainer)
        {
            RightOffsetCached = null;
            LeftOffsetCached = null;
            double requiredWidth = CalculateRequiredWidth(soilSurfaceContainer);
            
            Transaction trans = _document.TransactionManager.TopTransaction;
            
            LeftOffsetCached = this.CreateLeftOffset(requiredWidth / 2);
            LeftOffsetCached.Layer = _foundationLayerName;
            RightOffsetCached = this.CreateRightOffset(requiredWidth / 2);
            RightOffsetCached.Layer = _foundationLayerName;

            BlockTableRecord modelSpace = _document.Database.GetModelSpace(true);

            LeftOffset = modelSpace.AppendEntity(LeftOffsetCached);
            RightOffset = modelSpace.AppendEntity(RightOffsetCached);

            trans.AddNewlyCreatedDBObject(LeftOffsetCached, true);
            trans.AddNewlyCreatedDBObject(RightOffsetCached, true);
        }

        public override void Erase()
        {
            Transaction trans = _document.TransactionManager.TopTransaction;
            trans.GetObject(LeftOffset, OpenMode.ForWrite).Erase();
            trans.GetObject(RightOffset, OpenMode.ForWrite).Erase();
            
            base.Erase();
        }

        public double CalculateRequiredWidth(SoilSurfaceContainer soilSurfaceContainer)
        {
            double groundBearingPressure = soilSurfaceContainer.GetGroundBearingPressure(this.StartPoint, this.EndPoint);
            Transaction trans = _document.TransactionManager.TopTransaction;
            double appliedLoad = double.Parse(this[FoundationGroup.FOUNDATION_CENTRE_LOAD_KEY]);

            if(appliedLoad == 0)
                throw new ArgumentOutOfRangeException("No applied load has been set.");

            if(groundBearingPressure == 0)
                throw new ArgumentOutOfRangeException("No ground bearing pressure has been set.");

            FoundationWidth widthCalc = new FoundationWidth()
            {
                AppliedLoad = appliedLoad,
                GroundBearingPressure = groundBearingPressure,
                WallThickness = 0.3 // TODO: Add a way of specifying wall thickness
            };

            widthCalc.Run();

            if(!widthCalc.Calculated)
                throw new InvalidOperationException("Width calculation failed.");

            return widthCalc.RequiredWidth.Value;
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

            Vector3d startVector = StartPoint.GetVectorTo(EndPoint);
            double startAngle = startVector.GetAngleTo(Vector3d.YAxis, Vector3d.ZAxis) * 180 / Math.PI;
            Vector3d endVector = EndPoint.GetVectorTo(StartPoint);
            double endAngle = endVector.GetAngleTo(Vector3d.YAxis, Vector3d.ZAxis) * 180 / Math.PI;

            Node1.AddFoundation(this, startAngle);
            Node2.AddFoundation(this, endAngle);
        }

        public IReadOnlyList<DepthPoint> CalculateDepths(SoilSurfaceContainer soilSurfaceContainer)
        {
            // TODO: Add code for more than just the centre line
            // TODO: Add tree ring code

            FeatureLine existingLine = soilSurfaceContainer.GetFeatureLine(this.BaseObject, soilSurfaceContainer.ExistingGround);
            FeatureLine proposedLine = soilSurfaceContainer.GetFeatureLine(this.BaseObject, soilSurfaceContainer.ProposedGround);

            List<Point3d> elevationPoints = GetPointsOfElevationChange(soilSurfaceContainer, existingLine, proposedLine);
            List<DepthPoint> depthPoints = new List<DepthPoint>();

            foreach (Point3d elevationPoint in elevationPoints)
            {
                DepthPoint dp = new DepthPoint()
                {
                    DistanceParameter = existingLine.GetParameterAtPoint(elevationPoint),
                    RequiredDepth = soilSurfaceContainer.GetDepthAtPoint(elevationPoint, existingLine, proposedLine)
                };

                depthPoints.Add(dp);
            }

            return depthPoints.OrderBy(x => x.DistanceParameter).ToList();
        }

        private List<Point3d> GetPointsOfElevationChange(SoilSurfaceContainer soilSurfaceContainer, FeatureLine existingLine, FeatureLine proposedLine)
        {
            List<Point3d> elevationPoints = new List<Point3d>();

            elevationPoints.Add(StartPoint);
            
            foreach (Point3d point3d in existingLine.GetPoints(FeatureLinePointType.AllPoints))
            {
                elevationPoints.Add(point3d);
            }

            foreach (Point3d point3d in proposedLine.GetPoints(FeatureLinePointType.AllPoints))
            {
                elevationPoints.Add(point3d);
            }

            elevationPoints.Add(EndPoint);

            return elevationPoints;
        }

        public static FoundationCentreLine CreateFromLine(LineDrawingObject lineDrawingObject, SoilProperties soilProperties)
        {
            LayerManager layerManager = DataService.Current.GetStore<DocumentStore>(lineDrawingObject.Document.Name).LayerManager;
            string foundationLayerName = layerManager.GetLayerName(FoundationCentreLine.FOUNDATION_LAYER);

            FoundationCentreLine foundationCentreLine = new FoundationCentreLine(lineDrawingObject.Document, soilProperties, foundationLayerName);
            foundationCentreLine.BaseObject = lineDrawingObject.BaseObject;

            return foundationCentreLine;
        }
    }

    public struct DepthPoint
    {
        public double DistanceParameter { get; set; }
        public double RequiredDepth { get; set; }
    }
}
