using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Structures.ObjectModel.TreeRings;
using NUnit.Framework;

namespace Jpp.Ironstone.Structures.ObjectModel.Test.TreeRings
{
    [TestFixture]
    class TreeTests : IronstoneTestFixture
    {
        private static readonly Random Random = new Random();

        public TreeTests() : base(Assembly.GetExecutingAssembly(), typeof(TreeTests), Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Test Drawings\\blank.dwg") { }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(100)]
        [TestCase(200)]
        public void VerifyTreeCount(int count)
        {
            var result = RunTest<bool>(nameof(VerifyTreeCountResident), count);
            Assert.IsTrue(result, "Unexpected result from manager.");
        }

        public bool VerifyTreeCountResident(int count)
        {
            try
            {
                var acDoc = Autodesk.AutoCAD.ApplicationServices.Core.Application.DocumentManager.MdiActiveDocument;

                using (var acTrans = acDoc.TransactionManager.StartTransaction())
                {
                    var ds = DataService.Current;
                    ds.InvalidateStoreTypes();
                    var treeRingManager = ds.GetStore<StructureDocumentStore>(acDoc.Name).GetManager<TreeRingManager>();

                    treeRingManager.ManagedObjects.Clear();
                    treeRingManager.UpdateAll();

                    for (var i = 0; i < count; i++)
                    {
                        var t = RandomTree(i);
                        treeRingManager.AddTree(t);
                    }

                    treeRingManager.UpdateAll();

                    acTrans.Commit();

                    return treeRingManager.ManagedObjects.Count == count && treeRingManager.RingsCollection.Count > 0;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static Tree RandomTree(int id)
        {
            return new Tree
            {
                Phase = Phase.Proposed,
                Species = "EnglishElm",
                TreeType = TreeType.Deciduous,
                WaterDemand = WaterDemand.High,
                ActualHeight = Tree.DeciduousHigh["EnglishElm"],
                ID = id.ToString(),
                Location = new Point3d(Random.NextDouble(), Random.NextDouble(), 0)
            };
        }

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
                Tree newTree = new Tree();
                newTree.Phase = Phase.Existing;
                newTree.Species = rtd.Tree;
                newTree.Location = new Autodesk.AutoCAD.Geometry.Point3d(0, 0, 0);

                bool found = false;
                double startDepth = 0;

                if (Tree.DeciduousHigh.ContainsKey(rtd.Tree))
                {
                    newTree.ActualHeight = Tree.DeciduousHigh[rtd.Tree];
                    newTree.TreeType = TreeType.Deciduous;
                    newTree.WaterDemand = WaterDemand.High;
                    found = true;
                    startDepth = 1;
                }

                if (Tree.DeciduousMedium.ContainsKey(rtd.Tree))
                {
                    newTree.ActualHeight = Tree.DeciduousHigh[rtd.Tree];
                    newTree.TreeType = TreeType.Deciduous;
                    newTree.WaterDemand = WaterDemand.Medium;
                    found = true;
                    startDepth = 0.9;
                }

                if (Tree.DeciduousLow.ContainsKey(rtd.Tree))
                {
                    newTree.ActualHeight = Tree.DeciduousHigh[rtd.Tree];
                    newTree.TreeType = TreeType.Deciduous;
                    newTree.WaterDemand = WaterDemand.Low;
                    found = true;
                    startDepth = 0.75;
                }

                if (Tree.ConiferousHigh.ContainsKey(rtd.Tree))
                {
                    newTree.ActualHeight = Tree.DeciduousHigh[rtd.Tree];
                    newTree.TreeType = TreeType.Coniferous;
                    newTree.WaterDemand = WaterDemand.High;
                    found = true;
                    startDepth = 1;
                }

                if (Tree.ConiferousMedium.ContainsKey(rtd.Tree))
                {
                    newTree.ActualHeight = Tree.DeciduousHigh[rtd.Tree];
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

        [TestCaseSource(typeof(KeywordsArrayTestDataSource))]
        public void VerifyTreeKeywords(string[] keywords)
        {
            var result = RunTest<bool>(nameof(VerifyKeywordsResident), keywords);
            Assert.IsTrue(result);
        }

        public bool VerifyKeywordsResident(string[] words)
        {
            var listUppers = new List<string>();
            foreach (var word in words)
            {
                var ch = word.First(char.IsUpper);
                if (ch == 0) continue;

                var uppers = string.Concat(ch);
                var idx = word.IndexOf(ch);
                for (var i = idx + 1; i < word.Length; i++)
                {
                    if (!char.IsUpper(word[i])) break;
                    uppers = string.Concat(uppers, word[i]);
                }

                if (listUppers.Contains(uppers)) return false;

                listUppers.Add(uppers);

            }

            return listUppers.Count == words.Length;
        }
    }

    public class KeywordsArrayTestDataSource : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            yield return Tree.ConiferousMedium.Keys.ToArray();
            yield return Tree.ConiferousHigh.Keys.ToArray();
            yield return Tree.DeciduousLow.Keys.ToArray();
            yield return Tree.DeciduousMedium.Keys.ToArray();
            yield return Tree.DeciduousHigh.Keys.ToArray();
            yield return Enum.GetNames(typeof(WaterDemand)).ToArray();
            yield return Enum.GetNames(typeof(TreeType)).ToArray();
            yield return Enum.GetNames(typeof(Phase)).ToArray();
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
