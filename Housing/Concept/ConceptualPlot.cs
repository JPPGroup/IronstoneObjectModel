using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil;
using Autodesk.Civil.DatabaseServices;
using Jpp.DesignCalculations.Calculations.Design.Foundations;
using Jpp.Ironstone.Core.Autocad.DrawingObjects.Primitives;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Structures.ObjectModel;
using DBObject = Autodesk.AutoCAD.DatabaseServices.DBObject;
using CivSurface = Autodesk.Civil.DatabaseServices.Surface;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Region = Autodesk.AutoCAD.DatabaseServices.Region;
using Entity = Autodesk.AutoCAD.DatabaseServices.Entity;

namespace Jpp.Ironstone.Housing.ObjectModel.Concept
{
    /// <summary>
    /// Drawing object representing a conceptual plot formed by a perimeter polyline
    /// </summary>
    public class ConceptualPlot : ClosedPolylineDrawingObject
    {
        public string PlotId { get; set; }

        private NHBC2020FoundationDepth _depth;
        
        public double FoundationDepth
        {
            get
            {
                if (!_depth.Calculated)
                    throw new InvalidOperationException();

                return _depth.FoundationDepth.Value;
            }
        }

        public HatchDrawingObject DepthHatch
        {
            get
            {
                if (SubObjects.ContainsKey(DEPTH_HATCH_KEY))
                    return SubObjects[DEPTH_HATCH_KEY] as HatchDrawingObject;

                return null;
            }
            set
            {
                if (SubObjects.ContainsKey(DEPTH_HATCH_KEY))
                {
                    SubObjects[DEPTH_HATCH_KEY] = value;
                }
                else
                {
                    SubObjects.Add(DEPTH_HATCH_KEY, value);
                }

            }
        }

        private const string DEPTH_HATCH_KEY = "DEPTH_HATCH";

        public ConceptualPlot(PolylineDrawingObject drawingObject) : base(drawingObject)
        {
            _depth = new NHBC2020FoundationDepth();
        }

        private ConceptualPlot() : base()
        {
            _depth = new NHBC2020FoundationDepth();
        }

        public bool FoundationsEnabled { get; set; }

        // TODO: Properly implement this
        public override Point3d Location
        {
            get
            {
                return Point3d.Origin;
                
            }
            set { ; }
        }

        // TODO: Properly implement this
        public override double Rotation
        {
            get { return 0; }
            set { ; }
        }

        public override void Erase()
        {
            // TODO: Implement
            //throw new NotImplementedException();
        }

        public override void Generate()
        {
            SetLayer(Constants.PLOT_BOUNDARY_LAYER);
        }

        // TODO: Port of existing code, requires refactoring immininently
        public void EstimateFFLFromSurface(CivSurface proposed)
        {
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            using (Transaction acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                DBObject obj = acTrans.GetObject(this.BaseObject, OpenMode.ForWrite);

                //Need to add the temp line to create feature line from it
                BlockTable acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord acBlkTblRec =
                    acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                ObjectId perimId = FeatureLine.Create("plot" + PlotId, obj.ObjectId);
                FeatureLine perim = acTrans.GetObject(perimId, OpenMode.ForWrite) as FeatureLine;
                perim.AssignElevationsFromSurface(proposed.Id, false);
                var points = perim.GetPoints(Autodesk.Civil.FeatureLinePointType.PIPoint);

                // TODO: Move to settings
                double FinishedFloorLevel = Math.Ceiling(perim.MaxElevation * 20) / 20 + 0.15;

                // TODO: Move to generation code
                //Ad the FFL Label
                // Create a multiline text object
                using (MText acMText = new MText())
                {
                    Solid3d Solid = new Solid3d();
                    DBObjectCollection coll = new DBObjectCollection();
                    coll.Add(obj);
                    Solid.Extrude(((Region) Region.CreateFromCurves(coll)[0]), 1, 0);
                    Point3d centroid = new Point3d(Solid.MassProperties.Centroid.X, Solid.MassProperties.Centroid.Y, 0);
                    Solid.Dispose();

                    acMText.Location = centroid;
                    acMText.Contents = "FFL = " + FinishedFloorLevel.ToString("F3");

                    //acMText.Rotation = Rotation;
                    acMText.Height = 8;
                    acMText.Attachment = AttachmentPoint.MiddleCenter;

                    acBlkTblRec.AppendEntity(acMText);
                    acTrans.AddNewlyCreatedDBObject(acMText, true);
                }

                // TODO: Move to generation code
                foreach (Point3d p in points)
                {
                    using (MText acMText = new MText())
                    {
                        Point3d insert = new Point3d(p.X, p.Y, 0);
                        acMText.Location = insert;

                        //Number of course
                        int courses = (int) Math.Ceiling((double) (((FinishedFloorLevel - 0.15f - p.Z) / 0.075f)));

                        if (courses > 0)
                        {
                            acMText.Contents = courses + " Courses";
                            acMText.Height = 4;
                            acMText.Attachment = AttachmentPoint.TopRight;

                            acBlkTblRec.AppendEntity(acMText);
                            acTrans.AddNewlyCreatedDBObject(acMText, true);
                        }
                    }
                }

                //perim.Erase();
                //obj.Erase();
                acTrans.Commit();
            }
        }

        public bool EstimateFoundationLevel(CivSurface existing, CivSurface proposed, SoilProperties properties)
        {
            if (!SetFoundationInputs(existing, proposed, properties))
                return false;

            _depth.Run();
            if (_depth.Calculated)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // TODO: Revoew this method
        /*private SurfaceProperties? ExtractSurfaceInformation(CivSurface targetSurface)
        {
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Transaction acTrans = acCurDb.TransactionManager.TopTransaction;
            Polyline boundary = (Polyline) acTrans.GetObject(this.BaseObject, OpenMode.ForRead);

            Point2dCollection points = new Point2dCollection();
            for (int i = 0, size = boundary.NumberOfVertices; i < size; i++)
                points.Add(boundary.GetPoint2dAt(i));

            SurfaceProperties result = new SurfaceProperties();

            using (Database destDb = new Database(true, true))
            {
                using (Transaction transDest = destDb.TransactionManager.StartTransaction())
                {
            
                    Database db = Application.DocumentManager.MdiActiveDocument.Database;
                    HostApplicationServices.WorkingDatabase = destDb;

                    // TODO: Review if exception handling is the answer
                    ObjectId newSurfaceId;
                    try
                    {
                        // This errors out when surface has ben copied
                        newSurfaceId = TinSurface.CreateByCropping(destDb, "Surface<[Next Counter(CP)]>",
                            targetSurface.ObjectId, points);

                        TinSurface newSurface = transDest.GetObject(newSurfaceId, OpenMode.ForRead) as TinSurface;
                        GeneralSurfaceProperties genProps = newSurface.GetGeneralProperties();
                        result.MaxElevation = genProps.MaximumElevation;
                        result.MinElevation = genProps.MinimumElevation;
                    }
                    catch (SurfaceException e)
                    {
                        return null;
                    }
                    finally
                    {
                        HostApplicationServices.WorkingDatabase = db;
                    }
                }
            }

            return result;
        }*/

        public FeatureLine GetPerim(CivSurface targetSurface)
        {
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;
            Transaction acTrans = acCurDb.TransactionManager.TopTransaction;
            //Polyline boundary = (Polyline) acTrans.GetObject(this.BaseObject, OpenMode.ForRead);
            
            ObjectId perimId = FeatureLine.Create(Guid.NewGuid().ToString(), this.BaseObject);
            FeatureLine perim = acTrans.GetObject(perimId, OpenMode.ForWrite) as FeatureLine;
            perim.AssignElevationsFromSurface(targetSurface.Id, true);

            return perim;
        }

        private bool SetFoundationInputs(CivSurface existing, CivSurface proposed, SoilProperties properties)
        {
            /*SurfaceProperties? existingProps = ExtractSurfaceInformation(existing);
            if (existingProps.HasValue)
            {
                _depth.ExistingGroundLevel = existingProps.Value.MinElevation;
            }
            else
            {
                return false;
            }*/
            FeatureLine existingLine = GetPerim(existing);
            _depth.ExistingGroundLevel = existingLine.MinElevation;

            if (proposed != null)
            {
                /*SurfaceProperties? proposedProps = ExtractSurfaceInformation(proposed);
                if (proposedProps.HasValue)
                {
                    _depth.ProposedGroundLevel = proposedProps.Value.MinElevation;
                }*/
                FeatureLine proposedLine = GetPerim(proposed);
                _depth.ProposedGroundLevel = proposedLine.MinElevation;
            }

            switch (properties.SoilShrinkability)
            {
                case Shrinkage.High:
                    _depth.SoilPlasticity = VolumeChangePotential.High;
                    break;

                case Shrinkage.Medium:
                    _depth.SoilPlasticity = VolumeChangePotential.Medium;
                    break;

                case Shrinkage.Low:
                    _depth.SoilPlasticity = VolumeChangePotential.Low;
                    break;
            }

            return true;
        }

        public void RenderFoundations(IEnumerable<DepthBand> depthBands, ILogger logger)
        {
            if(!_depth.Calculated)
                throw new InvalidOperationException("Please run calculation before attempting to render the depths");

            double relativeDepth = _depth.ExistingGroundLevel.Value - _depth.FoundationDepth.Value;

            var band = depthBands.Where(db => db.StartDepth <= relativeDepth && db.EndDepth > relativeDepth);
            if(band.Count() > 1)
                logger.Entry("Multiple overlapping depth bands found, using first band encountered", Severity.Warning);

            if (band.Count() == 0)
                throw new InvalidOperationException("No matching band found");

            // TODO: CHange to pull pattern from settings file
            DepthHatch = CreateHatch("SOLID");
            DepthHatch.Color = band.First().Color;
        }

        private struct SurfaceProperties
        {
            public double MaxElevation { get; set; }
            public double MinElevation { get; set; }
        }
    }
}
