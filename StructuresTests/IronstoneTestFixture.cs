﻿using System;
using System.Reflection;
using Jpp.AcTestFramework;
using Jpp.Ironstone.Core;
using Jpp.Ironstone.Core.Mocking;

namespace Jpp.Ironstone.Structures.ObjectModel.Test
{
    public abstract class IronstoneTestFixture : BaseNUnitTestFixture
    {
        private const string CORE_LIBRARY = "IronstoneCore.dll";
        public override int ClientTimeout { get; } = 10000;

        protected IronstoneTestFixture(Assembly fixtureAssembly, Type fixtureType) : base(fixtureAssembly, fixtureType, CORE_LIBRARY) { }
        protected IronstoneTestFixture(Assembly fixtureAssembly, Type fixtureType, string drawingFile) : base(fixtureAssembly, fixtureType, drawingFile, CORE_LIBRARY) { }

        public override void Setup()
        {
            var config = new Configuration();
            config.TestSettings();
            ConfigurationHelper.CreateConfiguration(config);
        }
    }
}
