using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Structures.ObjectModel.TreeRings;
using NUnit.Framework;

namespace Jpp.Ironstone.Structures.ObjectModel.Test.TreeRings
{
    [TestFixture]
    class NHBCTreeTests : IronstoneTestFixture
    {
        public NHBCTreeTests() : base(Assembly.GetExecutingAssembly(), typeof(NHBCTreeTests)) { }

        [TestCase("EnglishElm", 22.8, 2)]
        //TODO: Add more test case for other soil types
        public void ConfirmRingRadius(string Tree, double expected, int ring)
        {
            RingTestData rtd = new RingTestData()
            {
                Tree = Tree,
                ExpectedRadius = expected,
                ExpectedIndex = ring
            };

            double calculated = RunTest<double>(nameof(ConfirmRingRadiusResident), rtd);

            Assert.AreEqual(rtd.ExpectedRadius, calculated, 0);
        }

        public double ConfirmRingRadiusResident(RingTestData rtd)
        {
            Document acDoc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            using (Transaction acTrans = acDoc.TransactionManager.StartTransaction())
            {
                NHBCTree newTree = new NHBCTree();
                newTree.Phase = Phase.Existing;
                newTree.Species = rtd.Tree;
                newTree.Location = new Autodesk.AutoCAD.Geometry.Point3d(0, 0, 0);

                bool found = false;
                double startDepth = 0;

                if (NHBCTree.DeciduousHigh.ContainsKey(rtd.Tree))
                {
                    newTree.Height = NHBCTree.DeciduousHigh[rtd.Tree];
                    newTree.TreeType = TreeType.Deciduous;
                    newTree.WaterDemand = WaterDemand.High;
                    found = true;
                    startDepth = 1;
                }

                if (NHBCTree.DeciduousMedium.ContainsKey(rtd.Tree))
                {
                    newTree.Height = NHBCTree.DeciduousHigh[rtd.Tree];
                    newTree.TreeType = TreeType.Deciduous;
                    newTree.WaterDemand = WaterDemand.Medium;
                    found = true;
                    startDepth = 0.9;
                }

                if (NHBCTree.DeciduousLow.ContainsKey(rtd.Tree))
                {
                    newTree.Height = NHBCTree.DeciduousHigh[rtd.Tree];
                    newTree.TreeType = TreeType.Deciduous;
                    newTree.WaterDemand = WaterDemand.Low;
                    found = true;
                    startDepth = 0.75;
                }

                if (NHBCTree.ConiferousHigh.ContainsKey(rtd.Tree))
                {
                    newTree.Height = NHBCTree.DeciduousHigh[rtd.Tree];
                    newTree.TreeType = TreeType.Coniferous;
                    newTree.WaterDemand = WaterDemand.High;
                    found = true;
                    startDepth = 1;
                }

                if (NHBCTree.ConiferousMedium.ContainsKey(rtd.Tree))
                {
                    newTree.Height = NHBCTree.DeciduousHigh[rtd.Tree];
                    newTree.TreeType = TreeType.Coniferous;
                    newTree.WaterDemand = WaterDemand.Medium;
                    found = true;
                    startDepth = 0.9;
                }

                if (!found)
                    return -1;

                var rings = newTree.DrawRings(Shrinkage.High, startDepth, 0.3);
                Circle c = rings[rtd.ExpectedIndex] as Circle;
                
                return c.Radius;
            }
        }
    }

    [Serializable]
    struct RingTestData
    {
        public string Tree { get; set; }
        public double ExpectedRadius { get; set; }
        public int ExpectedIndex { get; set; }
    }
}
