using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Jpp.Ironstone.Structures.ObjectModel.Test.Ground
{
    [TestFixture]
    class SoilPropertiesTests : IronstoneTestFixture
    {
        public SoilPropertiesTests() : base(Assembly.GetExecutingAssembly(), typeof(SoilPropertiesTests))
        {
        }

        [Test]
        public void VerifyDefaultDepthBandsLoaded()
        {
            List<DepthBand> expected = new List<DepthBand>()
            {
                new DepthBand()
                {
                    HexColor = "#008000",
                    StartDepth = 0,
                    EndDepth = 1
                },
                new DepthBand()
                {
                    HexColor = "#7CFC00",
                    StartDepth = 1,
                    EndDepth = 1.5
                },
                new DepthBand()
                {
                    HexColor = "#FFFF00",
                    StartDepth = 1.5,
                    EndDepth = 2
                },
                new DepthBand()
                {
                    HexColor = "#FFA500",
                    StartDepth = 2,
                    EndDepth = 2.5
                },
                new DepthBand()
                {
                    HexColor = "#FF0000",
                    StartDepth = 2.5,
                    EndDepth = 5
                }
            };

            var result = RunTest<List<DepthBand>>(nameof(VerifyDefaultDepthBandsLoadedResident));

            Assert.AreEqual(expected.Count, result.Count);

            for (int i = 0; i < result.Count; i++)
            {
                StringAssert.AreEqualIgnoringCase(expected[i].HexColor, result[i].HexColor);
                Assert.AreEqual(expected[i].StartDepth, result[i].StartDepth);
                Assert.AreEqual(expected[i].EndDepth, result[i].EndDepth);
            }
        }

        public List<DepthBand> VerifyDefaultDepthBandsLoadedResident()
        {
            SoilProperties soilProperties = new SoilProperties();
            return soilProperties.DepthBands.ToList();
        }
    }
}
