using Autodesk.AutoCAD.ApplicationServices.Core;
using Jpp.Ironstone.Core.ServiceInterfaces;
using Jpp.Ironstone.Highways.ObjectModel.Roads;
using NUnit.Framework;
using System;
using System.Reflection;

namespace Jpp.Ironstone.Highways.ObjectModel.Tests.Roads
{
    public class RoadManagerTests : IronstoneTestFixture
    {
        public RoadManagerTests() : base(Assembly.GetExecutingAssembly(), typeof(RoadManagerTests)) { }

        [Test]
        public void VerifyManagerConstructor()
        {
            var result = RunTest<bool>(nameof(VerifyManagerConstructorResident));
            Assert.IsTrue(result, "Unable to create instance.");
        }

        public bool VerifyManagerConstructorResident()
        {
            return Activator.CreateInstance(typeof(RoadManager), true) is RoadManager;
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
                var manager = ds.GetStore<HighwaysDocumentStore>(acDoc.Name).GetManager<RoadManager>();

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