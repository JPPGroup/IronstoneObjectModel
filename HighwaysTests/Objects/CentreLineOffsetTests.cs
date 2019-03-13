using System.Reflection;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Highways.ObjectModel.Abstract;
using Jpp.Ironstone.Highways.ObjectModel.Objects;
using NUnit.Framework;

namespace Jpp.Ironstone.Highways.ObjectModel.Tests.Objects
{
    [TestFixture]
    public class CentreLineOffsetTests : IronstoneTestFixture
    {
        public CentreLineOffsetTests() : base(Assembly.GetExecutingAssembly(), typeof(CentreLineOffsetTests)) { }

        [Test]
        public void VerifyCarriageWayLeft()
        {
            const double dist = 2.5;
            ICentreLineOffset offset = new CarriageWayLeft(dist);
            
            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Left, offset.Side, "Unexpected value for Side Of Centre.");
                Assert.AreEqual(OffsetTypes.CarriageWay, offset.OffsetType, "Unexpected value for Offset Type.");
                Assert.IsFalse(offset.Ignore, "Unexpected value for Ignore");
                Assert.AreEqual(dist, offset.Distance, "Unexpected value for Distance.");
                Assert.AreEqual(0, offset.Intersection.Count, "Unexpected value for Intersection count.");
            });
        }

        [Test]
        public void VerifyCarriageWayLeftIgnored()
        {
            const double dist = 3.5;
            ICentreLineOffset offset = new CarriageWayLeft(dist) { Ignore = true };

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Left, offset.Side, "Unexpected value for Side Of Centre.");
                Assert.AreEqual(OffsetTypes.CarriageWay, offset.OffsetType, "Unexpected value for Offset Type.");
                Assert.IsTrue(offset.Ignore, "Unexpected value for Ignore");
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
                Assert.IsFalse(offset.Ignore, "Unexpected value for Ignore");
                Assert.AreEqual(dist, offset.Distance, "Unexpected value for Distance.");
                Assert.AreEqual(0, offset.Intersection.Count, "Unexpected value for Intersection count.");
            });
        }

        [Test]
        public void VerifyCarriageWayRightIgnored()
        {
            const double dist = 3.5;
            ICentreLineOffset offset = new CarriageWayRight(dist) { Ignore = true };

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Right, offset.Side, "Unexpected value for Side Of Centre.");
                Assert.AreEqual(OffsetTypes.CarriageWay, offset.OffsetType, "Unexpected value for Offset Type.");
                Assert.IsTrue(offset.Ignore, "Unexpected value for Ignore");
                Assert.AreEqual(dist, offset.Distance, "Unexpected value for Distance.");
                Assert.AreEqual(0, offset.Intersection.Count, "Unexpected value for Intersection count.");
            });
        }

        [Test]
        public void VerifyOffsetIntersectBefore()
        {
            const bool before = true;
            const double x = 1;
            const double y = 2;
            const double z = 3;

            var intersect = RunTest<object[]>(nameof(VerifyOffsetIntersectBeforeResident), new object[] { before, x, y ,z });

            Assert.AreEqual(before, (bool)intersect[0], "Unexpected value for Before.");
            Assert.AreEqual(x, (double)intersect[1], "Unexpected value for Point X.");
            Assert.AreEqual(y, (double)intersect[2], "Unexpected value for Point Y.");
            Assert.AreEqual(z, (double)intersect[3], "Unexpected value for Point Z.");
        }

        public object[] VerifyOffsetIntersectBeforeResident(object[] values)
        {
            if (values.Length != 4) return new object[4];

            var before = (bool) values[0];
            var x = (double) values[1];
            var y = (double) values[2];
            var z = (double) values[3];

            var intersect = new OffsetIntersect(new Point3d(x, y, z), before);

            return new object[] {intersect.Before, intersect.Point.X, intersect.Point.Y, intersect.Point.Z};
        }

        [Test]
        public void VerifyOffsetIntersectAfter()
        {
            const bool before = false;
            const double x = 3;
            const double y = 2;
            const double z = 1;

            var intersect = RunTest<object[]>(nameof(VerifyOffsetIntersectAfterResident), new object[] { before, x, y, z });

            Assert.AreEqual(before, (bool)intersect[0], "Unexpected value for Before.");
            Assert.AreEqual(x, (double)intersect[1], "Unexpected value for Point X.");
            Assert.AreEqual(y, (double)intersect[2], "Unexpected value for Point Y.");
            Assert.AreEqual(z, (double)intersect[3], "Unexpected value for Point Z.");
        }

        public object[] VerifyOffsetIntersectAfterResident(object[] values)
        {
            if (values.Length != 4) return new object[4];

            var before = (bool)values[0];
            var x = (double)values[1];
            var y = (double)values[2];
            var z = (double)values[3];

            var intersect = new OffsetIntersect(new Point3d(x, y, z), before);

            return new object[] { intersect.Before, intersect.Point.X, intersect.Point.Y, intersect.Point.Z };
        }


    }
}
