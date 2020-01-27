using System;
using System.Xml.Serialization;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;
using Jpp.Ironstone.Core.Autocad.DrawingObjects.Primitives;
using DBObject = Autodesk.AutoCAD.DatabaseServices.DBObject;

namespace Jpp.Ironstone.Housing.ObjectModel.Concept
{
    /// <summary>
    /// Drawing object representing a conceptual plot formed by a perimeter polyline
    /// </summary>
    public class ConceptualPlot : ClosedPolylineDrawingObject
    {
        [XmlIgnore]
        public ConceptualPlotManager Manager { get; set; }

        public string PlotId { get; set; }

        public ConceptualPlot(PolylineDrawingObject drawingObject) : base(drawingObject)
        {
        }

        private ConceptualPlot() : base()
        {

        }

        public override Point3d Location { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override double Rotation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Erase()
        {
            throw new NotImplementedException();
        }

        public override void Generate()
        {
            SetLayer(Constants.PLOT_BOUNDARY_LAYER);
        }

        // TODO: Port of existing code, requires refactoring immininently
        public void EstimateFFLFromSurface()
        {
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
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
                perim.AssignElevationsFromSurface(Manager.ProposedLevels.Id, false);
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
    }
}
