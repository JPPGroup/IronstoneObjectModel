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
    public class ExistingTreeRingManagerTests : IronstoneTestFixture
    {
        public ExistingTreeRingManagerTests() : base(Assembly.GetExecutingAssembly(), typeof(ExistingTreeRingManagerTests), Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\Test Drawings\\ExampleManager.dwg")
        {
        }

        [Test]
        public void VerifyManagerLoaded()
        {
            var result = RunTest<int>(nameof(VerifyManagerLoadedResident));
            Assert.AreEqual(2, result, "Manager not loaded correctly.");
        }

        public int VerifyManagerLoadedResident()
        {
            try
            {
                var acDoc = Application.DocumentManager.MdiActiveDocument;
                var ds = GetDataService();
                var manager = ds.GetStore<StructureDocumentStore>(acDoc.Name).GetManager<TreeRingManager>();

                return manager.ActiveObjects.Count;
            }
            catch (Exception)
            {
                return -1;
            }
        }

        private static DataService GetDataService()
        {
            var ds = DataService.Current;
            ds.InvalidateStoreTypes();
            return ds;
        }
    }
}

