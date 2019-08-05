using System;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Jpp.Ironstone.Core.ServiceInterfaces;
using NUnit.Framework;

namespace Jpp.Ironstone.Highways.ObjectModel.Tests
{
    [TestFixture]
    public class HighwaysDocumentStoreTests : IronstoneTestFixture
    {
        public HighwaysDocumentStoreTests() : base(Assembly.GetExecutingAssembly(), typeof(HighwaysDocumentStoreTests)) { }

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
                var store = ds.GetStore<HighwaysDocumentStore>(acDoc.Name);

                return store != null;
            }
            catch (Exception)
            {
                return false;
            }            
        }
    }
}
