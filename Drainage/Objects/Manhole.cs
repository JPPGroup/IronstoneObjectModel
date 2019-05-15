using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Drainage.ObjectModel.Extensions;
using Jpp.Ironstone.Drainage.ObjectModel.Standards;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Jpp.Ironstone.Drainage.ObjectModel.Objects
{
    public class Manhole
    {
        private const float DEPTH_TO_SOFFIT_LEVEL_DIVIDER = 1000;
        private static readonly Dictionary<double, double> WallThicknesses = new Dictionary<double, double>{
            {900, 70},
            {1050, 80},
            {1200, 90},
            {1350, 95},
            {1500, 105},
            {1800, 115},
            {2100, 125},
            {2400, 140},
            {2700, 150},
            {3000, 165}
        };

        public const double SCALE = 0.05;

        public IDrainageStandard Standard { get; }
        public double Diameter { get; set; }
        public float InvertLevel { get; set; }
        public float CoverLevel { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public bool SafetyChain { get; set; }
        public bool SafetyRail { get; set; }
        public int MinimumMinorBenching { get; set; }
        public int MinimumMajorBenching { get; set; }
        public Point3d IntersectionPoint { get; set; }
        public List<PipeConnection> IncomingPipes { get; set; }
        public PipeConnection OutgoingConnection { get; set; }
        public double LargestInternalPipeDiameter
        {
            get
            {
                var largestIncoming = IncomingPipes.OrderByDescending(p => p.Diameter).First().Diameter;
                return OutgoingConnection.Diameter > largestIncoming ? OutgoingConnection.Diameter : largestIncoming;
            }
        }
        public double DepthToSoffitLevel => CoverLevel - (InvertLevel + LargestInternalPipeDiameter / DEPTH_TO_SOFFIT_LEVEL_DIVIDER);   
        public double WallThickness => WallThicknesses[Diameter];
        public double LayoutHeight { get; set; }
        public double LayoutWidth { get; set; }
        public int SpaceColumns => 3;
        public int SpaceRows => 3;
        public Point3d Centre { get; set; }


        public Manhole(IDrainageStandard standard)
        {
            IncomingPipes = new List<PipeConnection>();
            Standard = standard;
        }

        public void GeneratePlan(Point3d location)
        {
            Standard.Apply(this);

            var pLines = new List<Polyline>();
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;

            //Sort pipe connections for ease of drawing
            var sortedPipes = IncomingPipes.OrderBy(p => p.Angle).ToList();
            
            using (var tr = acCurDb.TransactionManager.StartTransaction())
            {
                var offset = location.GetAsVector().Subtract(IntersectionPoint.GetAsVector());

                //Calculate alternate connection point
                Circle slopeCircle = new Circle(location, Vector3d.ZAxis, 450);
                Vector3d slopeIntersect = location.GetVectorTo(OutgoingConnection.Location.Add(offset));
                slopeIntersect = slopeIntersect * 450 / slopeIntersect.Length;
                Point2d slopePoint2D = new Point2d(location.Add(slopeIntersect).X, location.Add(slopeIntersect).Y);

                // Open the Block table for read
                BlockTable acBlkTbl;
                acBlkTbl = tr.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Open the Block table record Model space for write
                BlockTableRecord acBlkTblRec;
                acBlkTblRec = tr.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                acCurDb.RegisterLayer(Constants.LAYER_PIPE_WALLS_NAME, Constants.LAYER_PIPE_WALLS_COLOR);
                acCurDb.RegisterLayer(Constants.LAYER_PIPE_CENTRE_LINE_NAME, Constants.LAYER_PIPE_CENTRE_LINE_COLOR, Constants.LAYER_PIPE_CENTRE_LINE_TYPE);
                acCurDb.RegisterLayer(Constants.LAYER_MANHOLE_WALL_NAME, Constants.LAYER_MANHOLE_WALL_COLOR);
                acCurDb.RegisterLayer(Constants.LAYER_MANHOLE_FURNITURE_NAME, Constants.LAYER_MANHOLE_FURNITURE_COLOR);

                Circle pipeIntrusion = new Circle(location, Vector3d.ZAxis, (double)(Diameter / 2) - 150f);

                //Outgoing line
                Polyline outgoingLine = new Polyline();
                outgoingLine.AddVertexAt(0, new Point2d(location.X, location.Y), 0, 0, 0);
                outgoingLine.AddVertexAt(1, new Point2d(OutgoingConnection.Location.Add(offset).X, OutgoingConnection.Location.Add(offset).Y), 0, 0, 0);
                outgoingLine.Layer = Constants.LAYER_PIPE_CENTRE_LINE_NAME;

                Polyline outgoingoffsetPlus = outgoingLine.GetOffsetCurves(OutgoingConnection.Diameter / 2)[0] as Polyline;
                outgoingoffsetPlus.Layer = Constants.LAYER_PIPE_WALLS_NAME;
                Polyline outgoingoffsetMinus = outgoingLine.GetOffsetCurves(-OutgoingConnection.Diameter / 2)[0] as Polyline;
                outgoingoffsetMinus.Layer = Constants.LAYER_PIPE_WALLS_NAME;

                //Add descriptive text
                var outLabel = new MText
                {
                    Location = new Point3d(OutgoingConnection.Location.Add(offset).X, OutgoingConnection.Location.Add(offset).Y, 0),
                    Contents = OutgoingConnection.Code + "\\P" + OutgoingConnection.Diameter + "%%C",
                    TextHeight = 40
                };

                outLabel.AlignTo(outgoingLine);
                acBlkTblRec.AppendEntity(outLabel);
                tr.AddNewlyCreatedDBObject(outLabel, true);

                //Pipe walls
                Polyline outouteroffsetPlus = outgoingLine.GetOffsetCurves(OutgoingConnection.Diameter / 2 + 20f)[0] as Polyline;
                outouteroffsetPlus.Layer = Constants.LAYER_PIPE_WALLS_NAME;
                Point3dCollection intersection = new Point3dCollection();
                outouteroffsetPlus.IntersectWith(pipeIntrusion, Intersect.ExtendArgument, intersection, IntPtr.Zero, IntPtr.Zero);
                outouteroffsetPlus.AddVertexAt(0, new Point2d(intersection[0].X, intersection[0].Y), 0, 0, 0);
                outouteroffsetPlus.RemoveVertexAt(1);

                Polyline outouteroffsetMinus = outgoingLine.GetOffsetCurves(-OutgoingConnection.Diameter / 2 - 20f)[0] as Polyline;
                outouteroffsetMinus.Layer = Constants.LAYER_PIPE_WALLS_NAME;
                intersection = new Point3dCollection();
                outouteroffsetMinus.IntersectWith(pipeIntrusion, Intersect.ExtendArgument, intersection, IntPtr.Zero, IntPtr.Zero);
                outouteroffsetMinus.AddVertexAt(0, new Point2d(intersection[0].X, intersection[0].Y), 0, 0, 0);
                outouteroffsetMinus.RemoveVertexAt(1);

                Polyline outcloseWall = new Polyline();
                outcloseWall.AddVertexAt(0, new Point2d(outouteroffsetMinus.StartPoint.X, outouteroffsetMinus.StartPoint.Y), 0, 0, 0);
                outcloseWall.AddVertexAt(0, new Point2d(outouteroffsetPlus.StartPoint.X, outouteroffsetPlus.StartPoint.Y), 0, 0, 0);
                outcloseWall.Layer = Constants.LAYER_PIPE_WALLS_NAME;

                acBlkTblRec.AppendEntity(outgoingLine);
                tr.AddNewlyCreatedDBObject(outgoingLine, true);

                acBlkTblRec.AppendEntity(outouteroffsetMinus);
                tr.AddNewlyCreatedDBObject(outouteroffsetMinus, true);
                acBlkTblRec.AppendEntity(outouteroffsetPlus);
                tr.AddNewlyCreatedDBObject(outouteroffsetPlus, true);
                acBlkTblRec.AppendEntity(outcloseWall);
                tr.AddNewlyCreatedDBObject(outcloseWall, true);

                Polyline lastLine = outgoingoffsetPlus;

                for (int i = 0; i < sortedPipes.Count(); i++)
                {
                    PipeConnection pipeConnection = sortedPipes.ToArray()[i];

                    //Create centreline
                    Polyline newLine = new Polyline();//location, pipeConnection.Location.Add(offset)
                    newLine.AddVertexAt(0, new Point2d(location.X, location.Y), 0, 0, 0);
                    newLine.AddVertexAt(1, new Point2d(pipeConnection.Location.Add(offset).X, pipeConnection.Location.Add(offset).Y), 0, 0, 0);
                    newLine.Layer = Constants.LAYER_PIPE_CENTRE_LINE_NAME;

                    //Add descriptive text
                    var label = new MText
                    {
                        Location = new Point3d(pipeConnection.Location.Add(offset).X, pipeConnection.Location.Add(offset).Y, 0),
                        Contents = pipeConnection.Code + "\\P" + pipeConnection.Diameter + "%%C"
                    };


                    acBlkTblRec.AppendEntity(label);
                    tr.AddNewlyCreatedDBObject(label, true);

                    label.TextHeight = 40;
                    label.AlignTo(newLine);

                    //Pipe walls
                    Polyline outeroffsetPlus = newLine.GetOffsetCurves(pipeConnection.Diameter / 2 + 20f)[0] as Polyline;
                    outeroffsetPlus.Layer = Constants.LAYER_PIPE_WALLS_NAME;
                    intersection = new Point3dCollection();
                    outeroffsetPlus.IntersectWith(pipeIntrusion, Intersect.ExtendArgument, intersection, IntPtr.Zero, IntPtr.Zero);
                    outeroffsetPlus.AddVertexAt(0, new Point2d(intersection[0].X, intersection[0].Y), 0, 0, 0);
                    outeroffsetPlus.RemoveVertexAt(1);

                    Polyline outeroffsetMinus = newLine.GetOffsetCurves(-pipeConnection.Diameter / 2 - 20f)[0] as Polyline;
                    outeroffsetMinus.Layer = Constants.LAYER_PIPE_WALLS_NAME;
                    intersection = new Point3dCollection();
                    outeroffsetMinus.IntersectWith(pipeIntrusion, Intersect.ExtendArgument, intersection, IntPtr.Zero, IntPtr.Zero);
                    outeroffsetMinus.AddVertexAt(0, new Point2d(intersection[0].X, intersection[0].Y), 0, 0, 0);
                    outeroffsetMinus.RemoveVertexAt(1);

                    Polyline closeWall = new Polyline();
                    closeWall.AddVertexAt(0, new Point2d(outeroffsetMinus.StartPoint.X, outeroffsetMinus.StartPoint.Y), 0, 0, 0);
                    closeWall.AddVertexAt(0, new Point2d(outeroffsetPlus.StartPoint.X, outeroffsetPlus.StartPoint.Y), 0, 0, 0);
                    closeWall.Layer = Constants.LAYER_PIPE_WALLS_NAME;


                    //Check that angle is ok
                    //Skip the check if this is the only incoming pipe
                    if (sortedPipes.Count() > 1)
                    {
                        if (pipeConnection.Angle < 135 || pipeConnection.Angle > 225)
                        {
                            //Angle exceeds 45° so change
                            Point3dCollection slopeIntersectCollection = new Point3dCollection();
                            newLine.IntersectWith(slopeCircle, Intersect.ExtendArgument, slopeIntersectCollection, IntPtr.Zero, IntPtr.Zero);
                            Point3d circleIntersectPoint = slopeIntersectCollection[0];
                            newLine.AddVertexAt(1, new Point2d(circleIntersectPoint.X, circleIntersectPoint.Y), 0, 0, 0);

                            newLine.SetPointAt(0, slopePoint2D);
                        }
                    }

                    Polyline offsetPlus = newLine.GetOffsetCurves(pipeConnection.Diameter / 2)[0] as Polyline;
                    offsetPlus.Layer = Constants.LAYER_PIPE_WALLS_NAME;
                    Polyline offsetMinus = newLine.GetOffsetCurves(-pipeConnection.Diameter / 2)[0] as Polyline;
                    offsetMinus.Layer = Constants.LAYER_PIPE_WALLS_NAME;

                    //Fillet
                    Point3dCollection collection = new Point3dCollection();
                    offsetMinus.IntersectWith(lastLine, Intersect.ExtendBoth, collection, IntPtr.Zero, IntPtr.Zero);

                    //Check that the lines do intersect, may not if small pipe to large when parallel
                    if (collection.Count > 0)
                    {
                        var inter = collection[0];
                        lastLine.SetPointAt(0, new Point2d(inter.X, inter.Y));
                        offsetMinus.SetPointAt(0, new Point2d(inter.X, inter.Y));
                    }

                    acBlkTblRec.AppendEntity(newLine);
                    tr.AddNewlyCreatedDBObject(newLine, true);

                    lastLine.JoinEntity(offsetMinus);
                    pLines.Add(lastLine);

                    acBlkTblRec.AppendEntity(outeroffsetMinus);
                    tr.AddNewlyCreatedDBObject(outeroffsetMinus, true);
                    acBlkTblRec.AppendEntity(outeroffsetPlus);
                    tr.AddNewlyCreatedDBObject(outeroffsetPlus, true);

                    acBlkTblRec.AppendEntity(closeWall);
                    tr.AddNewlyCreatedDBObject(closeWall, true);

                    lastLine = offsetPlus;
                }

                Point3dCollection lastCollection = new Point3dCollection();
                outgoingoffsetMinus.IntersectWith(lastLine, Intersect.ExtendBoth, lastCollection, IntPtr.Zero, IntPtr.Zero);

                if (lastCollection.Count > 0)
                {
                    Point3d lastIntersection = lastCollection[0]; //No intersection throwing error???
                    lastLine.SetPointAt(0, new Point2d(lastIntersection.X, lastIntersection.Y));
                    outgoingoffsetMinus.SetPointAt(0, new Point2d(lastIntersection.X, lastIntersection.Y));
                }

                lastLine.JoinEntity(outgoingoffsetMinus);
                pLines.Add(lastLine);

                foreach (var line in pLines)
                {
                    line.FilletAll();
                    acBlkTblRec.AppendEntity(line);
                    tr.AddNewlyCreatedDBObject(line, true);
                }

                //Create rings
                var innerManhole = new Circle(location, Vector3d.ZAxis, Diameter / 2) { Layer = Constants.LAYER_MANHOLE_WALL_NAME };
                acBlkTblRec.AppendEntity(innerManhole);
                tr.AddNewlyCreatedDBObject(innerManhole, true);

                //Calculate best location for steps and check minimum benching
                double maxAngle = 0;
                int maxSegment = 0;

                double minAngle = double.MaxValue;
                int minSegment = 0;

                double previousAngle = 0;               
                for (int i = 0; i < sortedPipes.Count(); i++)
                {
                    PipeConnection pc = sortedPipes.ToArray()[i];

                    double anglebetween = pc.Angle - previousAngle;
                    previousAngle += pc.Angle;

                    if (anglebetween > maxAngle)
                    {
                        maxAngle = anglebetween;
                        maxSegment = i;
                    }

                    if (anglebetween < maxAngle)
                    {
                        maxAngle = anglebetween;
                        maxSegment = i;
                    }
                }

                PipeConnection edgeSegment = sortedPipes.ToArray()[maxSegment];
                double stepCenter = edgeSegment.Angle - maxAngle / 2;

                //check the last segment is not better
                PipeConnection endSegment = sortedPipes.ToArray().Last();
                double lastAngle = 360 - endSegment.Angle;
                if (lastAngle > maxAngle)
                {
                    stepCenter = 360 - lastAngle / 2;
                }

                Polyline stepCenterLine = new Polyline();
                stepCenterLine.AddVertexAt(0, new Point2d(outgoingLine.StartPoint.X, outgoingLine.StartPoint.Y), 0, 0, 0);
                stepCenterLine.AddVertexAt(1, new Point2d(outgoingLine.EndPoint.X, outgoingLine.EndPoint.Y), 0, 0, 0);

                Matrix3d curUCSMatrix = acDoc.Editor.CurrentUserCoordinateSystem;
                CoordinateSystem3d curUCS = curUCSMatrix.CoordinateSystem3d;

                // Rotate the polyline 45 degrees, around the Z-axis of the current UCS
                // using a base point of (4,4.25,0)
                stepCenterLine.TransformBy(Matrix3d.Rotation(stepCenter * Math.PI / 180, -curUCS.Zaxis, stepCenterLine.StartPoint));
                /*acBlkTblRec.AppendEntity(stepCenterLine);
                tr.AddNewlyCreatedDBObject(stepCenterLine, true);*/


                //TODO: Needs reviewing - as it need to be kept?
                //TODO: Make sure this works and allow for pipe widths not centre lines
                /*//Calculate both intersection points
                (Vector3d majorBenching, Vector3d minorBenching) = CalculateBenching(stepCenterLine, innerManhole);

                
                if (minorBenching.Length < MinimumMinorBenching)
                {
                    Vector3d adjustment = minorBenching * (MinimumMinorBenching / minorBenching.Length);
                    innerManhole.Center = innerManhole.Center.Add(adjustment);
                    (majorBenching, minorBenching) = CalculateBenching(stepCenterLine, innerManhole);
                }

                if (majorBenching.Length < MinimumMajorBenching)
                {
                    if (minorBenching.Length > MinimumMinorBenching)
                    {
                        Vector3d adjustment = minorBenching * (MinimumMinorBenching / minorBenching.Length);
                        innerManhole.Center = innerManhole.Center.Add(adjustment);
                        (majorBenching, minorBenching) = CalculateBenching(stepCenterLine, innerManhole);

                        if (majorBenching.Length < MinimumMajorBenching)
                        {
                            throw new ArgumentException("Major benching does not meet minimum distances");
                        }
                    }

                    throw new ArgumentException("Major benching does not meet minimum distances");
                }*/

                //Create rings now offset is fixed
                var outerManhole = new Circle(location, Vector3d.ZAxis, Diameter / 2 + WallThickness) { Layer = Constants.LAYER_MANHOLE_WALL_NAME };
                acBlkTblRec.AppendEntity(outerManhole);
                tr.AddNewlyCreatedDBObject(outerManhole, true);

                var outerSurround = new Circle(location, Vector3d.ZAxis, Diameter / 2 + WallThickness + 100f) { Layer = Constants.LAYER_MANHOLE_WALL_NAME };
                acBlkTblRec.AppendEntity(outerSurround);
                tr.AddNewlyCreatedDBObject(outerSurround, true);

               
                //Generate manhole cover
                var cover = GenerateManholeCover(stepCenterLine, innerManhole);
                cover.Layer = Constants.LAYER_MANHOLE_FURNITURE_NAME;
                acBlkTblRec.AppendEntity(cover);
                tr.AddNewlyCreatedDBObject(cover, true);

                //Generate manhole step
                var step = GenerateManholeStep(stepCenterLine, innerManhole);
                foreach (var entity in step)
                {
                    entity.Layer = Constants.LAYER_MANHOLE_FURNITURE_NAME;
                    acBlkTblRec.AppendEntity(entity);
                    tr.AddNewlyCreatedDBObject(entity, true);
                }

                //Generate benching
                var bench = GenerateBenching(stepCenterLine, innerManhole, pLines);
                foreach (var entity in bench)
                {
                    entity.Layer = Constants.LAYER_MANHOLE_FURNITURE_NAME;
                    acBlkTblRec.AppendEntity(entity);
                    tr.AddNewlyCreatedDBObject(entity, true);
                }

                //Add safety features
                if (SafetyChain) throw new NotImplementedException();
                if (SafetyRail) throw new NotImplementedException();

                //Generate Table
                var tbLocation = new Point3d(location.X + (1800 *1.1), location.Y + 450, 0);
                var tb = GenerateTable(this, tbLocation);
                acBlkTblRec.AppendEntity(tb);
                tr.AddNewlyCreatedDBObject(tb, true);

                var height = outgoingLine.Length * 2;
                var width = outgoingLine.Length + tb.Width + (tbLocation.X - location.X);
                LayoutHeight = height * SCALE * 1.1;
                LayoutWidth = width * SCALE * 1.1;
                var shift = (width / 2) - outgoingLine.Length;
                Centre = new Point3d(location.X + shift, location.Y, 0);
                //Finalize
                tr.Commit();
            }
        }

        private static Table GenerateTable(Manhole manhole, Point3d location)
        {
            var tb = new Table();
            tb.SetSize(15, 2);
            tb.SetRowHeight(60);
            tb.Columns[0].Width = 1200;
            tb.Columns[1].Width = 400;
            tb.Position = location;

            tb.SetTableRow("MANHOLE", manhole.Name.ToUpper(), 0);
            tb.SetTableRow("MANHOLE DIAMETER", manhole.Diameter.ToString(CultureInfo.InvariantCulture), 1);
            tb.SetTableRow("COVER LEVEL", manhole.CoverLevel.ToString(CultureInfo.InvariantCulture), 2);
            tb.SetTableRow("INVERT LEVEL", manhole.InvertLevel.ToString(CultureInfo.InvariantCulture), 3);
            tb.SetTableRow("MANHOLE TYPE", "", 4);
            tb.SetTableRow("DEPTH TO SOFFIT", manhole.DepthToSoffitLevel.ToString(CultureInfo.InvariantCulture), 5);
            tb.SetTableRow("DEPTH TO CUT OUT RECESS", "N/A", 6);
            tb.SetTableRow("COVER SIZE", "", 7);
            tb.SetTableRow("COVER SPEC", "TBC", 8);
            tb.SetTableRow("COVER DEPTH", "", 9);
            tb.SetTableRow("LADDER OR DOUBLE STEPS", "", 10);
            tb.SetTableRow("SAFETY CHAIN", manhole.SafetyChain.ToString().ToUpper(), 11);
            tb.SetTableRow("SAFETY RAIL", manhole.SafetyRail.ToString().ToUpper(), 12);
            tb.SetTableRow("AMPS PLATFORM", "N/A", 13); //TODO: Add proper amps check
            tb.SetTableRow("HOLE SIZE IN COVER SLAB", "", 14);

            tb.GenerateLayout();
            return tb;
        }

        private static (Vector3d major, Vector3d minor) CalculateBenching(Entity centerLine, Circle manhole)
        {
            var intersection = new Point3dCollection();
            centerLine.IntersectWith(manhole, Intersect.ExtendThis, intersection, IntPtr.Zero, IntPtr.Zero);
            var benchingLengths = new List<Vector3d>();
            foreach (Point3d p in intersection)
            {
                benchingLengths.Add(p.GetVectorTo(manhole.Center));
            }

            var minorBenching = benchingLengths.OrderBy(o => o.Length).First();
            var majorBenching = benchingLengths.OrderBy(o => o.Length).Last();

            return (majorBenching, minorBenching);
        }

        private static Polyline GenerateManholeCover(Curve stepLine, Entity innerRing)
        {
            const double manholeSize = 600;
            var innerPt = new Point3dCollection();
            stepLine.IntersectWith(innerRing, Intersect.OnBothOperands, innerPt, IntPtr.Zero, IntPtr.Zero);

            var vec = innerPt[0].GetVectorTo(stepLine.StartPoint);
            var distFromOuter = vec.Length - Math.Sqrt(Math.Pow(vec.Length, 2) - Math.Pow(manholeSize/2, 2));
            var start = innerPt[0] + vec * (distFromOuter / vec.Length);
            var end = innerPt[0] + vec * ((distFromOuter + manholeSize) / vec.Length);
            var line = new Line(start, end);
            var offsetPlus = (Line)line.GetOffsetCurves(manholeSize / 2)[0];
            var offsetMinus = (Line)line.GetOffsetCurves(-manholeSize / 2)[0];

            var pLine = new Polyline();
            pLine.AddVertexAt(0, new Point2d(offsetPlus.StartPoint.X, offsetPlus.StartPoint.Y), 0, 0, 0);
            pLine.AddVertexAt(1, new Point2d(offsetPlus.EndPoint.X, offsetPlus.EndPoint.Y), 0, 0, 0);
            pLine.AddVertexAt(2, new Point2d(offsetMinus.EndPoint.X, offsetMinus.EndPoint.Y), 0, 0, 0);
            pLine.AddVertexAt(3, new Point2d(offsetMinus.StartPoint.X, offsetMinus.StartPoint.Y), 0, 0, 0);
            pLine.Closed = true;
            return pLine;
        }

        private static IEnumerable<Polyline> GenerateManholeStep(Curve stepLine, Entity innerRing)
        {
            const double outerLength = 350;
            const double outerDepth = 129;
            const double innerLength = outerLength - 60;
            const double innerDepth = outerDepth - 30;

            var innerPt = new Point3dCollection();
            stepLine.IntersectWith(innerRing, Intersect.OnBothOperands, innerPt, IntPtr.Zero, IntPtr.Zero);
            var vec = innerPt[0].GetVectorTo(stepLine.StartPoint);

            var outerDist = vec.Length - Math.Sqrt(Math.Pow(vec.Length, 2) - Math.Pow(outerLength / 2, 2));
            var outerStart = innerPt[0] + vec * (outerDist / vec.Length);
            var outerEnd = innerPt[0] + vec * ((outerDist + outerDepth) / vec.Length);
            var outerLine = new Line(outerStart, outerEnd);
            var outerOffsetPlus = (Line)outerLine.GetOffsetCurves(outerLength / 2)[0];
            var outerOffsetMinus = (Line)outerLine.GetOffsetCurves(-outerLength / 2)[0];

            var outer = new Polyline();
            outer.AddVertexAt(0, new Point2d(outerOffsetPlus.StartPoint.X, outerOffsetPlus.StartPoint.Y), 0, 0, 0);
            outer.AddVertexAt(1, new Point2d(outerOffsetPlus.EndPoint.X, outerOffsetPlus.EndPoint.Y), 0, 0, 0);
            outer.AddVertexAt(2, new Point2d(outerOffsetMinus.EndPoint.X, outerOffsetMinus.EndPoint.Y), 0, 0, 0);
            outer.AddVertexAt(3, new Point2d(outerOffsetMinus.StartPoint.X, outerOffsetMinus.StartPoint.Y), 0, 0, 0);

            var innerDist = vec.Length - Math.Sqrt(Math.Pow(vec.Length, 2) - Math.Pow(innerLength / 2, 2));
            var innerStart = innerPt[0] + vec * (innerDist / vec.Length);
            var innerEnd = innerPt[0] + vec * ((outerDist + innerDepth) / vec.Length);
            var innerLine = new Line(innerStart, innerEnd);
            var innerOffsetPlus = (Line)innerLine.GetOffsetCurves(innerLength / 2)[0];
            var innerOffsetMinus = (Line)innerLine.GetOffsetCurves(-innerLength / 2)[0];

            var inner = new Polyline();
            inner.AddVertexAt(0, new Point2d(innerOffsetPlus.StartPoint.X, innerOffsetPlus.StartPoint.Y), 0, 0, 0);
            inner.AddVertexAt(1, new Point2d(innerOffsetPlus.EndPoint.X, innerOffsetPlus.EndPoint.Y), 0, 0, 0);
            inner.AddVertexAt(2, new Point2d(innerOffsetMinus.EndPoint.X, innerOffsetMinus.EndPoint.Y), 0, 0, 0);
            inner.AddVertexAt(3, new Point2d(innerOffsetMinus.StartPoint.X, innerOffsetMinus.StartPoint.Y), 0, 0, 0);

            return new[] { outer, inner };
        }

        private static IEnumerable<Entity> GenerateBenching(Polyline stepLine, Circle innerRing, List<Polyline> pipeOuter)
        {
            var outerPts = new Point3dCollection();
            stepLine.IntersectWith(innerRing, Intersect.OnBothOperands, outerPts, IntPtr.Zero, IntPtr.Zero);

            var innerPts = new Point3dCollection();
            foreach (var line in pipeOuter)
            {
                var linePts = new Point3dCollection();
                stepLine.IntersectWith(line, Intersect.ExtendThis, linePts, IntPtr.Zero, IntPtr.Zero);
                if (linePts.Count <= 0) continue;
                foreach (Point3d pt in linePts) innerPts.Add(pt);
            }

            var minLength = double.MaxValue; //Highest possible value
            var endPt = new Point3d();
            foreach (Point3d pts in innerPts)
            {
                var vec = outerPts[0].GetVectorTo(pts);
                if (!(vec.Length < minLength)) continue;

                minLength = vec.Length;
                endPt = pts;
            }

            var benchLine = new Line(outerPts[0], endPt);

            Vector3d lineVector = benchLine.EndPoint.GetAsVector() - benchLine.StartPoint.GetAsVector();
            var midPt = benchLine.StartPoint + lineVector * 0.5;
            var label = new MText
            {
                Location = midPt,
                Contents = $"{Math.Floor(minLength)}mm",
                TextHeight = 40
            };

            label.AlignTo(benchLine);


            return new Entity[] { benchLine, label };
        }
    }
}
