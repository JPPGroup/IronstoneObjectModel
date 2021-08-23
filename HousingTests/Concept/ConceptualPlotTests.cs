using System.IO;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.Autocad.DrawingObjects.Primitives;
using Jpp.Ironstone.Housing.ObjectModel.Concept;
using NUnit.Framework;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Jpp.Ironstone.Housing.ObjectModel.Tests.Concept
{
    [TestFixture]
    class ConceptualPlotTests : IronstoneTestFixture
    {
        public ConceptualPlotTests() : base(Assembly.GetExecutingAssembly(), typeof(ConceptualPlotTests),
            Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "Concept\\ConceptPreppedDrawing.dwg"))
        {
        }

        //TODO: Renable later
        /*[Test]
        public void VerifyGenerate()
        {
            StringAssert.AreEqualIgnoringCase("jpp_plot_boundary", RunTest<string>(nameof(VerifyGenerateResident)));
        }*/

        public string VerifyGenerateResident()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;

            ConceptualPlot plot;
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl =
                    trans.GetObject(doc.Database.BlockTableId, OpenMode.ForRead) as BlockTable;

                BlockTableRecord acBlkTblRecRec =
                    trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as
                        BlockTableRecord;


                Polyline acPoly = new Polyline();
                acPoly.AddVertexAt(0, new Point2d(2, 4), 0, 0, 0);
                acPoly.AddVertexAt(1, new Point2d(4, 2), 0, 0, 0);
                acPoly.AddVertexAt(2, new Point2d(6, 4), 0, 0, 0);
                acPoly.Closed = true;

                acBlkTblRecRec.AppendEntity(acPoly);
                trans.AddNewlyCreatedDBObject(acPoly, true);

                PolylineDrawingObject polyObject = new PolylineDrawingObject(acPoly);
                plot = new ConceptualPlot(polyObject);
                plot.Generate();
                trans.Commit();
            }

            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                Entity baseEntity = trans.GetObject(plot.BaseObject, OpenMode.ForRead) as Entity;
                return baseEntity.Layer;
            }
        }

        [Test]
        public void ConvertPlotToPolyline()
        {
            RunTest<bool>(nameof(ConvertPolylineToPlotResident));
        }

        public bool ConvertPolylineToPlotResident()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;

            ConceptualPlot plot;
            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                ObjectId obj = doc.Database.GetObjectId(false, new Handle(146034), 0);

                DBObject ent = trans.GetObject(obj, OpenMode.ForWrite);

                PolylineDrawingObject polyObject = new PolylineDrawingObject(ent as Polyline);
                plot = new ConceptualPlot(polyObject);
            }

            return true;
        }
    }
}
