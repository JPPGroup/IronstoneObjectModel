using Autodesk.AutoCAD.ApplicationServices.Core;
using Jpp.Ironstone.Core.Autocad;
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

        [Test]
        public void VerifyStoreLayerCreation()
        {
            Assert.Multiple(() =>
            {
                Assert.IsTrue(RunTest<bool>(nameof(VerifyLayerCreationForReviewGradientResident)));
                Assert.IsTrue(RunTest<bool>(nameof(VerifyLayerCreationForReviewLevelResident)));
            });
        }

        public static bool VerifyLayerCreationForReviewGradientResident()
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acDb = acDoc.Database;
            string layername;

            using (var trans = acDb.TransactionManager.StartTransaction())
            {
                var ds = DataService.Current;
                ds.InvalidateStoreTypes();
                var unused = ds.GetStore<HousingDocumentStore>(acDoc.Name);
                layername = unused.LayerManager.GetLayerName(Constants.FOR_REVIEW_GRADIENT_LAYER);

                trans.Commit();
            }

            using (var unused = acDb.TransactionManager.StartTransaction())
            {
                return acDb.GetLayer(layername) != null;
            }
        }
        
        public static bool VerifyLayerCreationForReviewLevelResident()
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acDb = acDoc.Database;
            string layername;

            using (var trans = acDb.TransactionManager.StartTransaction())
            {
                var ds = DataService.Current;
                ds.InvalidateStoreTypes();
                var unused = ds.GetStore<HousingDocumentStore>(acDoc.Name);
                layername = unused.LayerManager.GetLayerName(Constants.FOR_REVIEW_LEVEL_LAYER);

                trans.Commit();
            }

            using (var unused = acDb.TransactionManager.StartTransaction())
            {
                return acDb.GetLayer(layername) != null;
            }
        }
    }
}
