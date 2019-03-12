using System;
using System.Reflection;
using Jpp.AcTestFramework;
using Jpp.Ironstone.Core;
using Jpp.Ironstone.Core.Mocking;

namespace Jpp.Ironstone.Highways.ObjectModel.Tests
{
    public abstract class IronstoneTestFixture : BaseNUnitTestFixture
    {
        public override int ClientTimeout { get; } = 10000;

        protected IronstoneTestFixture(Assembly fixtureAssembly, Type fixtureType) : base(fixtureAssembly, fixtureType) {}

        protected IronstoneTestFixture(Assembly fixtureAssembly, Type fixtureType, string drawingFile) : base(fixtureAssembly, fixtureType, drawingFile) { }

        public override void Setup()
        {
            var config = new Configuration();
            config.TestSettings();
            ConfigurationHelper.CreateConfiguration(config);
        }
    }
}
