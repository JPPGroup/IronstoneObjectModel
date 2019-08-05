using System;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Jpp.Ironstone.Core.ServiceInterfaces;
using NUnit.Framework;

namespace Jpp.Ironstone.Structures.ObjectModel.Test
{
    [TestFixture]
    public class StructureDocumentStoreTests : IronstoneTestFixture
    {
        public StructureDocumentStoreTests() : base(Assembly.GetExecutingAssembly(), typeof(StructureDocumentStoreTests)) { }

        [Test]
        public void VerifyStoreLoaded()
        {
            var resultLoaded = RunTest<bool>(nameof(VerifyStoreLoadedResident));

            Assert.Multiple(() =>
            {
                Assert.IsTrue(resultLoaded, "Data store should be loaded.");
            });
        }

        public bool VerifyStoreLoadedResident()
        {
            try
            {
                var acDoc = Application.DocumentManager.MdiActiveDocument;
                var ds = DataService.Current;
                ds.InvalidateStoreTypes();
                var store = ds.GetStore<StructureDocumentStore>(acDoc.Name);

                return store != null;
            }
            catch (Exception)
            {
                return false;
            }            
        }
    }
}
