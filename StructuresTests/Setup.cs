using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Runtime;
using NUnit.Framework;

namespace Jpp.Ironstone.Structures.Objectmodel.Test
{
    [SetUpFixture]
    public class Setup
    {
        [OneTimeSetUp]
        public void SetupFixtures()
        {
            ExtensionLoader.Load("IronstoneCore.dll");
        }
    }
}
