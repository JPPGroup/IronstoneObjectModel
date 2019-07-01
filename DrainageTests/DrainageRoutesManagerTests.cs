using System;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Drainage.ObjectModel.Managers;
using NUnit.Framework;

namespace Jpp.Ironstone.Drainage.ObjectModel.Tests
{
    [TestFixture]
    public class DrainageRoutesManagerTests : IronstoneTestFixture
    {
        public DrainageRoutesManagerTests() : base(Assembly.GetExecutingAssembly(), typeof(DrainageRoutesManagerTests)) { }

        [Test]
        public void VerifyManagerConstructor()
        {
            var result = RunTest<bool>(nameof(VerifyManagerConstructorResident));
            Assert.IsTrue(result, "Unable to create instance.");
        }

        public bool VerifyManagerConstructorResident()
        {
            return Activator.CreateInstance(typeof(DrainageRoutesManager), true) is DrainageRoutesManager;
        }

        [Test]
        public void VerifyManagerLoaded()
        {
            var result = RunTest<bool>(nameof(VerifyManagerLoadedResident));
            Assert.IsTrue(result, "Manager not loaded.");
        }

        public bool VerifyManagerLoadedResident()
        {
            try
            {
                var acDoc = Application.DocumentManager.MdiActiveDocument;
                var ds = GetDataService();
                var manager = ds.GetStore<DrainageDocumentStore>(acDoc.Name).GetManager<DrainageRoutesManager>();

                return manager != null;
            }
            catch (Exception)
            {
                return false;
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