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
            var result = RunTest<bool>(nameof(VerifyStoreLoadedResident));
            Assert.IsTrue(result, "Data store not loaded.");
        }

        public bool VerifyStoreLoadedResident()
        {
            try
            {
                var acDoc = Application.DocumentManager.MdiActiveDocument;
                var ds = DataService.Current;
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
