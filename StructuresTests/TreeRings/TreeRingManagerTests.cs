using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Structures.Objectmodel.TreeRings;
using NUnit.Framework;

namespace Jpp.Ironstone.Structures.Objectmodel.Test.TreeRings
{
    [TestFixture]
    class TreeRingManagerTests
    {
        [OneTimeSetUp]
        public void Setup()
        {

        }

        [Test]
        public void AddNewTree()
        {
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;

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
            }
        }
    }
}

