using System.Reflection;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Structures.ObjectModel.TreeRings;
using NUnit.Framework;

namespace Jpp.Ironstone.Structures.ObjectModel.Test.TreeRings
{
    [TestFixture]
    public class HedgeRowTests : IronstoneTestFixture
    {
        public HedgeRowTests() : base(Assembly.GetExecutingAssembly(), typeof(HedgeRowTests)) { }

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
    }
}
