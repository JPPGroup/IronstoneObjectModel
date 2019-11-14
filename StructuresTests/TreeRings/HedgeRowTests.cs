using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Structures.ObjectModel.TreeRings;
using NUnit.Framework;
using System;
using System.Reflection;

namespace Jpp.Ironstone.Structures.ObjectModel.Test.TreeRings
{
    [TestFixture]
    public class HedgeRowTests : IronstoneTestFixture
    {
        public HedgeRowTests() : base(Assembly.GetExecutingAssembly(), typeof(HedgeRowTests)) { }

        [Test]
        public void VerifyGenerateValidationRemoved()
        {
            var result = RunTest<bool>(nameof(VerifyGenerateValidationRemovedResident));
            Assert.IsTrue(result);
        }

        public bool VerifyGenerateValidationRemovedResident()
        {
            var acDoc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;

            using (var acTrans = acDoc.TransactionManager.StartTransaction())
            {
                var acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                if (acBlkTbl == null) return false;

                var acBlkTblRec =
                    acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                if (acBlkTblRec == null) return false;

                var acPoly = new Polyline();
                acPoly.AddVertexAt(0, new Point2d(0, 0), 0, 0, 0);
                acPoly.AddVertexAt(1, new Point2d(0, 50), 0, 0, 0);
                acPoly.AddVertexAt(2, new Point2d(0, 100), 0, 0, 0);
                acPoly.AddVertexAt(3, new Point2d(100, 100), 0, 0, 0);

                var polyId = acBlkTblRec.AppendEntity(acPoly);
                acTrans.AddNewlyCreatedDBObject(acPoly, true);

                var hedge = new HedgeRow
                {
                    Phase = Phase.Existing,
                    Species = "WHITeWillow",
                    TreeType = TreeType.Deciduous,
                    WaterDemand = WaterDemand.High,
                    Height = Tree.DeciduousHigh["WHITeWillow"],
                    ID = "test-hedge",
                    BaseObject = polyId
                };

                hedge.Generate();
                return acPoly.NumberOfVertices == 3;
            }
        }

        [Test]
        public void VerifyGenerateValidationNotRemoved()
        {
            var result = RunTest<bool>(nameof(VerifyGenerateValidationNotRemovedResident));
            Assert.IsTrue(result);
        }

        public bool VerifyGenerateValidationNotRemovedResident()
        {
            var acDoc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;

            using (var acTrans = acDoc.TransactionManager.StartTransaction())
            {
                var acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                if (acBlkTbl == null) return false;

                var acBlkTblRec =
                    acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                if (acBlkTblRec == null) return false;

                var acPoly = new Polyline();
                acPoly.AddVertexAt(0, new Point2d(0, 0), 0, 0, 0);
                acPoly.AddVertexAt(1, new Point2d(0, 50), 0, 0, 0);
                acPoly.AddVertexAt(2, new Point2d(50, 100), 0, 0, 0);
                acPoly.AddVertexAt(3, new Point2d(100, 100), 0, 0, 0);

                var polyId = acBlkTblRec.AppendEntity(acPoly);
                acTrans.AddNewlyCreatedDBObject(acPoly, true);

                var hedge = new HedgeRow
                {
                    Phase = Phase.Existing,
                    Species = "WHITeWillow",
                    TreeType = TreeType.Deciduous,
                    WaterDemand = WaterDemand.High,
                    Height = Tree.DeciduousHigh["WHITeWillow"],
                    ID = "test-hedge",
                    BaseObject = polyId
                };

                hedge.Generate();
                return acPoly.NumberOfVertices == 4;
            }
        }

        [TestCase(2)]
        [TestCase(0)]
        [TestCase(-1)]
        public void VerifyDrawRingElevation(double elevation)
        {
            const double expected = 0;
            var result = RunTest<double>(nameof(VerifyDrawRingElevationResident), elevation);

            Assert.AreEqual(expected, result);
        }

        public double VerifyDrawRingElevationResident(double elevation)
        {
            var acDoc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;

            using (var acTrans = acDoc.TransactionManager.StartTransaction())
            {
                var acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                if (acBlkTbl == null) return -1;

                var acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                if (acBlkTblRec == null) return -1;

                var acPoly = new Polyline();
                acPoly.AddVertexAt(0, new Point2d(0, 0), 0, 0, 0);
                acPoly.AddVertexAt(1, new Point2d(0, 50), 0, 0, 0);
                acPoly.AddVertexAt(2, new Point2d(50, 50), 0, 0, 0);
                acPoly.Elevation = elevation;

                var polyId = acBlkTblRec.AppendEntity(acPoly);
                acTrans.AddNewlyCreatedDBObject(acPoly, true);

                var hedge = new HedgeRow
                {
                    Phase = Phase.Proposed,
                    Species = "EnglishElm",
                    TreeType = TreeType.Deciduous,
                    WaterDemand = WaterDemand.High,
                    Height = Tree.DeciduousHigh["EnglishElm"],
                    ID = "test-hedge",
                    BaseObject = polyId
                };

                var shape = hedge.DrawShape(0.9, Shrinkage.High) as Polyline;
                return shape == null ? -1 : shape.Elevation;
            }
        }

        [TestCase(1, true)]
        [TestCase(2, true)]
        [TestCase(100, false)]
        [TestCase(-100, false)]
        public void VerifyDrawRingPolylineLength(double length, bool expected)
        {
            var result = RunTest<bool>(nameof(VerifyDrawRingPolylineLengthResident), length);

            Assert.AreEqual(expected, result);
        }

        public bool VerifyDrawRingPolylineLengthResident(double length)
        {
            try
            {
                var acDoc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
                var acCurDb = acDoc.Database;

                using (var acTrans = acDoc.TransactionManager.StartTransaction())
                {
                    var acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                    if (acBlkTbl == null) throw new Exception("Null BlockTable");

                    var acBlkTblRec =
                        acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                    if (acBlkTblRec == null) throw new Exception("Null BlockTable");

                    var acPoly = new Polyline();
                    acPoly.AddVertexAt(0, new Point2d(0, 0), 0, 0, 0);
                    acPoly.AddVertexAt(1, new Point2d(0, length), 0, 0, 0);
                    acPoly.AddVertexAt(2, new Point2d(length, length), 0, 0, 0);

                    var polyId = acBlkTblRec.AppendEntity(acPoly);
                    acTrans.AddNewlyCreatedDBObject(acPoly, true);

                    var hedge = new HedgeRow
                    {
                        Phase = Phase.Proposed,
                        Species = "EnglishElm",
                        TreeType = TreeType.Deciduous,
                        WaterDemand = WaterDemand.High,
                        Height = Tree.DeciduousHigh["EnglishElm"],
                        ID = "test-hedge",
                        BaseObject = polyId
                    };

                    var _ = hedge.DrawShape(0.9, Shrinkage.High) as Polyline;
                    return false;
                }
            }
            catch (ArgumentException argEx)
            {
                return argEx.Message == "No offset curve created.";
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
