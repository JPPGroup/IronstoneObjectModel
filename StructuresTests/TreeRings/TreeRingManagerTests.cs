using System;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Structures.ObjectModel.TreeRings;
using NUnit.Framework;

namespace Jpp.Ironstone.Structures.ObjectModel.Test.TreeRings
{
    [TestFixture]
    public class TreeRingManagerTests : IronstoneTestFixture
    {
        public TreeRingManagerTests() : base(Assembly.GetExecutingAssembly(), typeof(TreeRingManagerTests)) { }

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
    }
}

