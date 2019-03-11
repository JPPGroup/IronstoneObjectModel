using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Highways.ObjectModel.Objects;
using Jpp.Ironstone.Highways.ObjectModel.Abstract;
using NUnit.Framework;

namespace Jpp.Ironstone.Highways.ObjectModel.Tests
{
    [TestFixture]
    public class CentreLineOffsetTests 
    {
        [Test]
        public void VerifyCarriageWayLeft()
        {
            const double dist = 2.5;
            ICentreLineOffset offset = new CarriageWayLeft(dist);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Left, offset.Side, "Unexpected value for Side Of Centre.");
                Assert.AreEqual(OffsetTypes.CarriageWay, offset.OffsetType, "Unexpected value for Offset Type.");
                Assert.AreEqual(dist, offset.Distance, "Unexpected value for Distance.");
                Assert.AreEqual(0, offset.Intersection.Count, "Unexpected value for Intersection count.");
            });
        }

        [Test]
        public void VerifyCarriageWayRight()
        {
            const double dist = 2.5;
            ICentreLineOffset offset = new CarriageWayRight(dist);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Right, offset.Side, "Unexpected value for Side Of Centre.");
                Assert.AreEqual(OffsetTypes.CarriageWay, offset.OffsetType, "Unexpected value for Offset Type.");
                Assert.AreEqual(dist, offset.Distance, "Unexpected value for Distance.");
                Assert.AreEqual(0, offset.Intersection.Count, "Unexpected value for Intersection count.");
            });
        }

        [Test]
        public void VerifyOffsetIntersectBefore()
        {
            const bool before = true;
            var intersect = new OffsetIntersect(new Point3d(0, 0, 0), before);

            Assert.True(intersect.Before, "Unexpected value for Before.");                
        }

        [Test]
        public void VerifyOffsetIntersectAfter()
        {
            const bool before = false;
            var intersect = new OffsetIntersect(new Point3d(0, 0, 0), before);

            Assert.False(intersect.Before, "Unexpected value for Before.");
        }
    }
}
