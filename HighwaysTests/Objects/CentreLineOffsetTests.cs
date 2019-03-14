using System.Diagnostics;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Highways.ObjectModel.Objects;
using Jpp.Ironstone.Highways.ObjectModel.Objects.Offsets;
using Jpp.Ironstone.Highways.ObjectModel.Tests.Response;
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
            const double carriageDistance = 2.5;
            const double pavementDistance = 0.5;
            var result = RunTest<CarriageWayProperties>(nameof(VerifyCarriageWayResident), new object[] {carriageDistance, pavementDistance, SidesOfCentre.Left} );

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Left, result.Side, "Unexpected value for carriage way side of centre.");
                Assert.AreEqual(OffsetTypes.CarriageWay, result.Type, "Unexpected value for carriage way offset type.");
                Assert.AreEqual(carriageDistance, result.CarriageDistance, "Unexpected value for carriage way distance.");
                Assert.AreEqual(0, result.Intersections, "Unexpected value for carriage way intersections count.");
                Assert.AreEqual(SidesOfCentre.Left, result.PavementSide, "Unexpected value for pavement side of centre.");
                Assert.AreEqual(OffsetTypes.Pavement, result.PavementType, "Unexpected value for pavement offset type.");
                Assert.AreEqual(carriageDistance + pavementDistance, result.PavementDistance, "Unexpected value for pavement distance.");
            });
        }

        [Test]
        public void VerifyCarriageWayRight()
        {
            const double carriageDistance = 2.5;
            const double pavementDistance = 0.5;
            var result = RunTest<CarriageWayProperties>(nameof(VerifyCarriageWayResident), new object[] { carriageDistance, pavementDistance, SidesOfCentre.Right });

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Right, result.Side, "Unexpected value for carriage way side of centre.");
                Assert.AreEqual(OffsetTypes.CarriageWay, result.Type, "Unexpected value for carriage way offset type.");
                Assert.AreEqual(carriageDistance, result.CarriageDistance, "Unexpected value for carriage way distance.");
                Assert.AreEqual(0, result.Intersections, "Unexpected value for carriage way intersections count.");
                Assert.AreEqual(SidesOfCentre.Right, result.PavementSide, "Unexpected value for pavement side of centre.");
                Assert.AreEqual(OffsetTypes.Pavement, result.PavementType, "Unexpected value for pavement offset type.");
                Assert.AreEqual(carriageDistance + pavementDistance, result.PavementDistance, "Unexpected value for pavement distance.");
            });
        }

        public CarriageWayProperties VerifyCarriageWayResident(object[] values)
        {
            var db = Application.DocumentManager.MdiActiveDocument.Database;
            CarriageWay carriage;
            using (var trans = db.TransactionManager.StartTransaction())
            {
                var blockTable = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                var blockTableRecord = (BlockTableRecord)trans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                var point1 = new Point3d(0, 0, 0);
                var point2 = new Point3d(10, 0, 0);
                var line = new Line(point1, point2);

                var objectId = blockTableRecord.AppendEntity(line);
                trans.AddNewlyCreatedDBObject(line, true);

                var centre = new CentreLine { BaseObject = objectId };
                carriage = new CarriageWay((double)values[0], (double)values[1], (SidesOfCentre)values[2], centre);
            }
     
            return new CarriageWayProperties
            {
                Side = carriage.Side,
                Type = carriage.OffsetType,
                CarriageDistance = carriage.DistanceFromCentre,
                Intersections = carriage.Intersections.Count,
                Ignored = carriage.Ignore,
                PavementSide = carriage.Pavement.Side,
                PavementType = carriage.Pavement.OffsetType,
                PavementDistance = carriage.Pavement.DistanceFromCentre
            };
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
