using System;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Jpp.Ironstone.Core.ServiceInterfaces;
using NUnit.Framework;

namespace Jpp.Ironstone.Drainage.ObjectModel.Tests
{
    [TestFixture]
    public class DrainageDocumentStoreTests : IronstoneTestFixture
    {
        public DrainageDocumentStoreTests() : base(Assembly.GetExecutingAssembly(), typeof(DrainageDocumentStoreTests)) { }

        [Test]
        public void VerifyStoreLoaded()
        {
            var resultNotLoaded = RunTest<bool>(nameof(VerifyStoreLoadedResident), false);
            var resultLoaded = RunTest<bool>(nameof(VerifyStoreLoadedResident), true);

            Assert.Multiple(() =>
            {
                Assert.IsFalse(resultNotLoaded, "Data store should not be loaded.");
                Assert.IsTrue(resultLoaded, "Data store should be loaded.");
            });
        }

        public bool VerifyStoreLoadedResident(bool invalidate)
        {
            try
            {
                var acDoc = Application.DocumentManager.MdiActiveDocument;
                var ds = DataService.Current;
                if (invalidate) ds.InvalidateStoreTypes();
                var store = ds.GetStore<DrainageDocumentStore>(acDoc.Name);

                return store != null;
            }
            catch (Exception)
            {
                return false;
            }            
        }
    }
}
