using Autodesk.AutoCAD.ApplicationServices.Core;
using Jpp.Ironstone.Core.ServiceInterfaces;
using NUnit.Framework;
using System.Reflection;

namespace Jpp.Ironstone.Housing.ObjectModel.Tests
{
    [TestFixture]
    public class HousingDocumentStoreTests : IronstoneTestFixture
    {
        public HousingDocumentStoreTests() : base(Assembly.GetExecutingAssembly(), typeof(HousingDocumentStoreTests)) { }

        [Test]
        public void VerifyStoreLoaded()
        {
            var resultLoaded = RunTest<bool>(nameof(VerifyStoreLoadedResident));

            Assert.Multiple(() =>
            {
                Assert.IsTrue(resultLoaded, "Data store should be loaded.");
            });
        }

        public static bool VerifyStoreLoadedResident()
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            
            var ds = DataService.Current;
            ds.InvalidateStoreTypes();

            var store = ds.GetStore<HousingDocumentStore>(acDoc.Name);

            return store != null;
        }
    }
}
