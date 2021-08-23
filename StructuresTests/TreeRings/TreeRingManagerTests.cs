using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Structures.ObjectModel.TreeRings;
using NUnit.Framework;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Jpp.Ironstone.Structures.ObjectModel.Test.TreeRings
{
    [TestFixture]
    public class TreeRingManagerTests : IronstoneTestFixture
    {
        public TreeRingManagerTests() : base(Assembly.GetExecutingAssembly(), typeof(TreeRingManagerTests), Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Test Drawings\\blank.dwg")
        {
        }

        [Test]
        public void VerifyManagerLoaded()
        {
            var result = RunTest<bool>(nameof(VerifyManagerLoadedResident));
            Assert.IsTrue(result, "Manager not loaded.");
        }

        public bool VerifyManagerLoadedResident()
        {
            try
            {
                var acDoc = Application.DocumentManager.MdiActiveDocument;
                var ds = GetDataService();
                var manager = ds.GetStore<StructureDocumentStore>(acDoc.Name).GetManager<TreeRingManager>();

                return manager != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static DataService GetDataService()
        {
            var ds = DataService.Current;
            ds.InvalidateStoreTypes();
            return ds;
        }

        public void AddNewTree()
        {
            /*Document acDoc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;

            using (acDoc.LockDocument())
            {
                using (Transaction acTrans = acDoc.TransactionManager.StartTransaction())
                {
                    NHBCTree newTree = new NHBCTree();
                    newTree.Generate();
                    newTree.TreeType = TreeType.Deciduous;
                    newTree.WaterDemand = WaterDemand.High;
                    newTree.Species = "EnglishElm";
                    newTree.Height = 24;
                    newTree.ID = "0";
                    newTree.Phase = Phase.Existing;
                    
                    newTree.Location = new Autodesk.AutoCAD.Geometry.Point3d(0, 0, 0);
                    newTree.AddLabel();

                    TreeRingManager treeRingManager = DataService.Current.GetStore<StructureDocumentStore>(acDoc.Name)
                        .GetManager<TreeRingManager>();
                    treeRingManager.AddTree(newTree);

                    acTrans.Commit();
                    Assert.Pass("Test passed");
                }
            }*/
        }

        //TODO: Think this test may be interfering with others in this gorup
        /*[Test]
        public void VerifyAddValidHedgeRow()
        {
            var result = RunTest<bool>(nameof(VerifyAddValidHedgeRowResident));

            Assert.IsTrue(result);
        }*/

        [Test]
        public void VerifyAddValidAndThenInvalidHedgeRow()
        {
            var resultValid = RunTest<bool>(nameof(VerifyAddValidHedgeRowResident));
            var resultInvalid = RunTest<bool>(nameof(VerifyAddInvalidHedgeRowResident));

            Assert.Multiple(() =>
            {
                Assert.IsTrue(resultValid);
                Assert.IsTrue(resultInvalid);
            });

        }

        public bool VerifyAddValidHedgeRowResident()
        {
            try
            {
                var acDoc = Application.DocumentManager.MdiActiveDocument;
                var acCurDb = acDoc.Database;

                using (var acTrans = acDoc.TransactionManager.StartTransaction())
                {
                    var acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                    if (acBlkTbl == null) throw new Exception("Null BlockTable");

                    var acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                    if (acBlkTblRec == null) throw new Exception("Null BlockTable");

                    var acPoly = new Polyline();
                    acPoly.AddVertexAt(0, new Point2d(0, 0), 0, 0, 0);
                    acPoly.AddVertexAt(1, new Point2d(0, 50), 0, 0, 0);
                    acPoly.AddVertexAt(2, new Point2d(50, 50), 0, 0, 0);

                    var polyId = acBlkTblRec.AppendEntity(acPoly);
                    acTrans.AddNewlyCreatedDBObject(acPoly, true);
                    
                    var hedge = new HedgeRow
                    {
                        Phase = Phase.Proposed,
                        Species = "EnglishElm",
                        TreeType = TreeType.Deciduous,
                        WaterDemand = WaterDemand.High,
                        ActualHeight = Tree.DeciduousHigh["EnglishElm"],
                        ID = "valid-hedge",
                        BaseObject = polyId
                    };

                    var ds = DataService.Current;
                    ds.InvalidateStoreTypes();
                    var treeRingManager = ds.GetStore<StructureDocumentStore>(acDoc.Name).GetManager<TreeRingManager>();
                    treeRingManager.Clear();
                    treeRingManager.UpdateAll();

                    var count = treeRingManager.ActiveObjects.Count;

                    treeRingManager.AddTree(hedge);
                    treeRingManager.UpdateAll();
                    acTrans.Commit();

                    bool hedgeAdded = treeRingManager.ActiveObjects.Count == count + 1;
                    bool ringsPresent = treeRingManager.RingsCollection.Count > 0;
                    return hedgeAdded && ringsPresent;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        public bool VerifyAddInvalidHedgeRowResident()
        {
            try
            {
                var acDoc = Application.DocumentManager.MdiActiveDocument;
                var acCurDb = acDoc.Database;

                using (var acTrans = acDoc.TransactionManager.StartTransaction())
                {
                    var acBlkTbl = acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;
                    if (acBlkTbl == null) throw new Exception("Null BlockTable");

                    var acBlkTblRec = acTrans.GetObject(acBlkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                    if (acBlkTblRec == null) throw new Exception("Null BlockTable");

                    var acPoly = new Polyline();
                    acPoly.AddVertexAt(0, new Point2d(0, 0), 0, 0, 0);
                    acPoly.AddVertexAt(1, new Point2d(0, 1), 0, 0, 0);
                    acPoly.AddVertexAt(2, new Point2d(1, 1), 0, 0, 0);

                    var polyId = acBlkTblRec.AppendEntity(acPoly);
                    acTrans.AddNewlyCreatedDBObject(acPoly, true);
                    
                    var hedge = new HedgeRow
                    {
                        Phase = Phase.Proposed,
                        Species = "EnglishElm",
                        TreeType = TreeType.Deciduous,
                        WaterDemand = WaterDemand.High,
                        ActualHeight = Tree.DeciduousHigh["EnglishElm"],
                        ID = "invalid-hedge",
                        BaseObject = polyId
                    };

                    var ds = DataService.Current;
                    ds.InvalidateStoreTypes();
                    var treeRingManager = ds.GetStore<StructureDocumentStore>(acDoc.Name).GetManager<TreeRingManager>();
                    var count = treeRingManager.ActiveObjects.Count;

                    treeRingManager.AddTree(hedge);
                    treeRingManager.UpdateAll();
                    acTrans.Commit();

                    bool hedgeAdded = treeRingManager.ActiveObjects.Count == count + 1;
                    bool ringsNotPresent = treeRingManager.RingsCollection.Count == 0;
                    return hedgeAdded && ringsNotPresent;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        [Test]
        public void CanLoadRingColorsFromSettings()
        {
            int[] expected = new int[] {94, 130, 160, 200, 32, 240, 160};
            int[] result = RunTest<int[]>(nameof(CanLoadRingColorsFromSettingsResident));

            Assert.AreEqual(expected, result);
        }

        public int[] CanLoadRingColorsFromSettingsResident()
        {
            Document acDoc = Application.DocumentManager.MdiActiveDocument;

            using (var acTrans = acDoc.TransactionManager.StartTransaction())
            {
                var ds = DataService.Current;
                ds.InvalidateStoreTypes();
                var treeRingManager = ds.GetStore<StructureDocumentStore>(acDoc.Name).GetManager<TreeRingManager>();

                //This line is needed to ensure layers created by manager persist through to other tests
                acTrans.Commit();

                return treeRingManager.RingColors.ToArray();
            }
        }
    }
}

