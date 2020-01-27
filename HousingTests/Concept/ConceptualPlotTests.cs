using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.Autocad.DrawingObjects.Primitives;
using Jpp.Ironstone.Housing.ObjectModel.Concept;
using NUnit.Framework;
using TransactionManager = Autodesk.AutoCAD.DatabaseServices.TransactionManager;

namespace Jpp.Ironstone.Housing.ObjectModel.Tests.Concept
{
    [TestFixture]
    class ConceptualPlotTests : IronstoneTestFixture
    {
        public ConceptualPlotTests() : base(Assembly.GetExecutingAssembly(), typeof(ConceptualPlotTests)) { }

        [Test]
        public void VerifyGenerate()
        {
            StringAssert.AreEqualIgnoringCase("jpp_plot_boundary", RunTest<string>(nameof(VerifyGenerateResident)));
        }

        public string VerifyGenerateResident()
        {
            Debugger.Launch();
            Document doc = Application.DocumentManager.MdiActiveDocument;

            ConceptualPlot plot;

            using (Transaction trans = doc.TransactionManager.StartTransaction())
            {
                BlockTable acBlkTbl = trans.GetObject(doc.Database.BlockTableId, OpenMode.ForRead) as BlockTable;

                BlockTableRecord acBlkTblRecRec =
                    trans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;


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
    }
}
