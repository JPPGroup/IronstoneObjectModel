using System;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices.Core;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Core.Autocad;
using Jpp.Ironstone.Highways.ObjectModel.Abstract;
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
        public void VerifyOffsetValidArc()
        {
            var result = RunTest<bool>(nameof(VerifyOffsetArcResident), new double[] { 3, 2.5 });
            Assert.IsTrue(result, "Should be valid offset.");
        }

        [Test]
        public void VerifyOffsetValidLine()
        {
            var result = RunTest<bool>(nameof(VerifyOffsetLineResident));
            Assert.IsTrue(result, "Should be valid offset.");
        }

        [Test]
        public void VerifyOffsetInvalidArc()
        {
            var result = RunTest<bool>(nameof(VerifyOffsetArcResident), new double[] { 3, 3 });
            Assert.IsFalse(result, "Should not be valid offset.");
        }

        public bool VerifyOffsetArcResident(double[] values)
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var blockTable = (BlockTable)acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead);
                var blockTableRecord = (BlockTableRecord)acTrans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                var arc = new Arc(new Point3d(0, 0, 0), values[0], 0, Math.PI / 2);
                var centre = new RoadCentreLine { BaseObject = blockTableRecord.AppendEntity(arc) };
                var road = new Road();
                road.CentreLines.Add(centre);

                var carriage = new CarriageWayLeft();
                return carriage.IsValid(centre, values[1]);
            }
        }

        public bool VerifyOffsetLineResident()
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var blockTable = (BlockTable)acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead);
                var blockTableRecord = (BlockTableRecord)acTrans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                acCurDb.RegisterLayer(Constants.LAYER_DEF_POINTS);

                var line = new Line(new Point3d(0, 0, 0), new Point3d(0, 10, 0));
                var centre = new RoadCentreLine { BaseObject = blockTableRecord.AppendEntity(line) };
                var road = new Road();
                road.CentreLines.Add(centre);

                var carriage = new CarriageWayLeft();
                return carriage.IsValid(centre, 5);
            }
        }

        [Test]
        public void VerifyPavementLeft()
        {
            var result = RunTest<PavementProperties>(nameof(VerifyPavementLeftResident));

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Left, result.Side, "Unexpected value for pavement side of centre.");
                Assert.AreEqual(OffsetTypes.Pavement, result.Type, "Unexpected value for pavement offset type.");
                Assert.AreEqual(Constants.DEFAULT_PAVEMENT, result.Distance, "Unexpected value for pavement distance.");
                Assert.AreEqual(0, result.Curves, "Unexpected value for pavement curves count.");
            });
        }

        [Test]
        public void VerifyPavementLeftClear()
        {
            var result = RunTest<PavementProperties>(nameof(VerifyPavementLeftClearResident));

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Left, result.Side, "Unexpected value for pavement side of centre.");
                Assert.AreEqual(OffsetTypes.Pavement, result.Type, "Unexpected value for pavement offset type.");
                Assert.AreEqual(Constants.DEFAULT_PAVEMENT, result.Distance, "Unexpected value for pavement distance.");
                Assert.AreEqual(0, result.Curves, "Unexpected value for pavement curves count.");
            });
        }       

        [Test]
        public void VerifyPavementRight()
        {
            var result = RunTest<PavementProperties>(nameof(VerifyPavementRightResident));

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Right, result.Side, "Unexpected value for pavement side of centre.");
                Assert.AreEqual(OffsetTypes.Pavement, result.Type, "Unexpected value for pavement offset type.");
                Assert.AreEqual(Constants.DEFAULT_PAVEMENT, result.Distance, "Unexpected value for pavement distance.");
                Assert.AreEqual(0, result.Curves, "Unexpected value for pavement curves count.");
            });
        }

        [Test]
        public void VerifyPavementRightClear()
        {
            var result = RunTest<PavementProperties>(nameof(VerifyPavementRightClearResident));

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Right, result.Side, "Unexpected value for pavement side of centre.");
                Assert.AreEqual(OffsetTypes.Pavement, result.Type, "Unexpected value for pavement offset type.");
                Assert.AreEqual(Constants.DEFAULT_PAVEMENT, result.Distance, "Unexpected value for pavement distance.");
                Assert.AreEqual(0, result.Curves, "Unexpected value for pavement curves count.");
            });
        }

        public PavementProperties VerifyPavementLeftResident()
        {
            return SetPavementProps(new PavementLeft());
        }

        public PavementProperties VerifyPavementLeftClearResident()
        {

            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;
            var pavement = new PavementLeft();

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                pavement.Clear();
                return SetPavementProps(pavement);
            }
        }

        public PavementProperties VerifyPavementRightResident()
        {
            return SetPavementProps(new PavementRight());
        }

        public PavementProperties VerifyPavementRightClearResident()
        {

            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;
            var pavement = new PavementRight();

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                pavement.Clear();
                return SetPavementProps(pavement);
            }
        }

        private static PavementProperties SetPavementProps(Pavement pavement)
        {
            return new PavementProperties
            {
                Side = pavement.Side,
                Type = pavement.OffsetType,
                Distance = pavement.DistanceFromCentre,
                Curves = pavement.Curves.Count,
            };
        }

        [Test]
        public void VerifyCarriageWayLeft()
        {
            var result = RunTest<CarriageWayProperties>(nameof(VerifyCarriageWayLeftResident));

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Left, result.Side, "Unexpected value for carriage way side of centre.");
                Assert.AreEqual(OffsetTypes.CarriageWay, result.Type, "Unexpected value for carriage way offset type.");
                Assert.AreEqual(Constants.DEFAULT_CARRIAGE_WAY, result.Distance, "Unexpected value for carriage way distance.");
                Assert.AreEqual(0, result.Intersections, "Unexpected value for carriage way intersections count.");
                Assert.AreEqual(0, result.Curves, "Unexpected value for carriage way curves count.");
                Assert.IsFalse(result.Ignored, "Unexpected value for carriage way ignored.");
                Assert.AreEqual(SidesOfCentre.Left, result.Pavement.Side, "Unexpected value for pavement side of centre.");
                Assert.AreEqual(OffsetTypes.Pavement, result.Pavement.Type, "Unexpected value for pavement offset type.");
                Assert.AreEqual(Constants.DEFAULT_PAVEMENT, result.Pavement.Distance, "Unexpected value for pavement distance.");
                Assert.AreEqual(0, result.Pavement.Curves, "Unexpected value for pavement curves count.");
            });
        }

        [Test]
        public void VerifyCarriageWayLeftClear()
        {
            var result = RunTest<CarriageWayProperties>(nameof(VerifyCarriageWayLeftClearResident));

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Left, result.Side, "Unexpected value for carriage way side of centre.");
                Assert.AreEqual(OffsetTypes.CarriageWay, result.Type, "Unexpected value for carriage way offset type.");
                Assert.AreEqual(Constants.DEFAULT_CARRIAGE_WAY, result.Distance, "Unexpected value for carriage way distance.");
                Assert.AreEqual(0, result.Intersections, "Unexpected value for carriage way intersections count.");
                Assert.AreEqual(0, result.Curves, "Unexpected value for carriage way curves count.");
                Assert.IsFalse(result.Ignored, "Unexpected value for carriage way ignored.");
                Assert.AreEqual(SidesOfCentre.Left, result.Pavement.Side, "Unexpected value for pavement side of centre.");
                Assert.AreEqual(OffsetTypes.Pavement, result.Pavement.Type, "Unexpected value for pavement offset type.");
                Assert.AreEqual(Constants.DEFAULT_PAVEMENT, result.Pavement.Distance, "Unexpected value for pavement distance.");
                Assert.AreEqual(0, result.Pavement.Curves, "Unexpected value for pavement curves count.");
            });
        }

        [Test]
        public void VerifyCarriageWayLeftCreate()
        {
            var result = RunTest<CarriageWayProperties>(nameof(VerifyCarriageWayLeftCreateResident), false);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Left, result.Side, "Unexpected value for carriage way side of centre.");
                Assert.AreEqual(OffsetTypes.CarriageWay, result.Type, "Unexpected value for carriage way offset type.");
                Assert.AreEqual(Constants.DEFAULT_CARRIAGE_WAY, result.Distance, "Unexpected value for carriage way distance.");
                Assert.AreEqual(0, result.Intersections, "Unexpected value for carriage way intersections count.");
                Assert.AreEqual(1, result.Curves, "Unexpected value for carriage way curves count.");
                Assert.IsFalse(result.Ignored, "Unexpected value for carriage way ignored.");
                Assert.AreEqual(SidesOfCentre.Left, result.Pavement.Side, "Unexpected value for pavement side of centre.");
                Assert.AreEqual(OffsetTypes.Pavement, result.Pavement.Type, "Unexpected value for pavement offset type.");
                Assert.AreEqual(Constants.DEFAULT_PAVEMENT, result.Pavement.Distance, "Unexpected value for pavement distance.");
                Assert.AreEqual(1, result.Pavement.Curves, "Unexpected value for pavement curves count.");
            });
        }

        [Test]
        public void VerifyCarriageWayLeftCreateWithIntersectionBefore()
        {
            var values = new object[] { new object[] { new double[] { -Constants.DEFAULT_CARRIAGE_WAY, 5, 0 }, true } };
            var result = RunTest<CarriageWayProperties>(nameof(VerifyCarriageWayLeftCreateWithIntersectResident), values);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Left, result.Side, "Unexpected value for carriage way side of centre.");
                Assert.AreEqual(OffsetTypes.CarriageWay, result.Type, "Unexpected value for carriage way offset type.");
                Assert.AreEqual(Constants.DEFAULT_CARRIAGE_WAY, result.Distance, "Unexpected value for carriage way distance.");
                Assert.AreEqual(1, result.Intersections, "Unexpected value for carriage way intersections count.");
                Assert.AreEqual(1, result.Curves, "Unexpected value for carriage way curves count.");
                Assert.IsFalse(result.Ignored, "Unexpected value for carriage way ignored.");
                Assert.AreEqual(SidesOfCentre.Left, result.Pavement.Side, "Unexpected value for pavement side of centre.");
                Assert.AreEqual(OffsetTypes.Pavement, result.Pavement.Type, "Unexpected value for pavement offset type.");
                Assert.AreEqual(Constants.DEFAULT_PAVEMENT, result.Pavement.Distance, "Unexpected value for pavement distance.");
                Assert.AreEqual(1, result.Pavement.Curves, "Unexpected value for pavement curves count.");
            });
        }

        [Test]
        public void VerifyCarriageWayLeftCreateWithoutIntersectionBefore()
        {
            var values = new object[] { new object[] { new double[] { 0, 5, 0 }, true } };
            var result = RunTest<CarriageWayProperties>(nameof(VerifyCarriageWayLeftCreateWithIntersectResident), values);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Left, result.Side, "Unexpected value for carriage way side of centre.");
                Assert.AreEqual(OffsetTypes.CarriageWay, result.Type, "Unexpected value for carriage way offset type.");
                Assert.AreEqual(Constants.DEFAULT_CARRIAGE_WAY, result.Distance, "Unexpected value for carriage way distance.");
                Assert.AreEqual(1, result.Intersections, "Unexpected value for carriage way intersections count.");
                Assert.AreEqual(1, result.Curves, "Unexpected value for carriage way curves count.");
                Assert.IsFalse(result.Ignored, "Unexpected value for carriage way ignored.");
                Assert.AreEqual(SidesOfCentre.Left, result.Pavement.Side, "Unexpected value for pavement side of centre.");
                Assert.AreEqual(OffsetTypes.Pavement, result.Pavement.Type, "Unexpected value for pavement offset type.");
                Assert.AreEqual(Constants.DEFAULT_PAVEMENT, result.Pavement.Distance, "Unexpected value for pavement distance.");
                Assert.AreEqual(1, result.Pavement.Curves, "Unexpected value for pavement curves count.");
            });
        }

        [Test]
        public void VerifyCarriageWayLeftCreateWithIntersectionAfter()
        {
            var values = new object[] {  new object[] {new double[] { -Constants.DEFAULT_CARRIAGE_WAY, 5, 0 }, false }} ;
            var result = RunTest<CarriageWayProperties>(nameof(VerifyCarriageWayLeftCreateWithIntersectResident), values);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Left, result.Side, "Unexpected value for carriage way side of centre.");
                Assert.AreEqual(OffsetTypes.CarriageWay, result.Type, "Unexpected value for carriage way offset type.");
                Assert.AreEqual(Constants.DEFAULT_CARRIAGE_WAY, result.Distance, "Unexpected value for carriage way distance.");
                Assert.AreEqual(1, result.Intersections, "Unexpected value for carriage way intersections count.");
                Assert.AreEqual(1, result.Curves, "Unexpected value for carriage way curves count.");
                Assert.IsFalse(result.Ignored, "Unexpected value for carriage way ignored.");
                Assert.AreEqual(SidesOfCentre.Left, result.Pavement.Side, "Unexpected value for pavement side of centre.");
                Assert.AreEqual(OffsetTypes.Pavement, result.Pavement.Type, "Unexpected value for pavement offset type.");
                Assert.AreEqual(Constants.DEFAULT_PAVEMENT, result.Pavement.Distance, "Unexpected value for pavement distance.");
                Assert.AreEqual(1, result.Pavement.Curves, "Unexpected value for pavement curves count.");
            });
        }

        [Test]
        public void VerifyCarriageWayLeftCreateWithoutIntersectionAfter()
        {
            var values = new object[] { new object[] { new double[] { 0, 5, 0 }, false } };
            var result = RunTest<CarriageWayProperties>(nameof(VerifyCarriageWayLeftCreateWithIntersectResident), values);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Left, result.Side, "Unexpected value for carriage way side of centre.");
                Assert.AreEqual(OffsetTypes.CarriageWay, result.Type, "Unexpected value for carriage way offset type.");
                Assert.AreEqual(Constants.DEFAULT_CARRIAGE_WAY, result.Distance, "Unexpected value for carriage way distance.");
                Assert.AreEqual(1, result.Intersections, "Unexpected value for carriage way intersections count.");
                Assert.AreEqual(1, result.Curves, "Unexpected value for carriage way curves count.");
                Assert.IsFalse(result.Ignored, "Unexpected value for carriage way ignored.");
                Assert.AreEqual(SidesOfCentre.Left, result.Pavement.Side, "Unexpected value for pavement side of centre.");
                Assert.AreEqual(OffsetTypes.Pavement, result.Pavement.Type, "Unexpected value for pavement offset type.");
                Assert.AreEqual(Constants.DEFAULT_PAVEMENT, result.Pavement.Distance, "Unexpected value for pavement distance.");
                Assert.AreEqual(1, result.Pavement.Curves, "Unexpected value for pavement curves count.");
            });
        }

        [Test]
        public void VerifyCarriageWayLeftCreateWithTwoIntersectionOnePart1()
        {
            var values = new object[]
            {
                new object[] { new double[] { -Constants.DEFAULT_CARRIAGE_WAY, 2, 0 }, false },
                new object[] { new double[] { -Constants.DEFAULT_CARRIAGE_WAY, 6, 0 }, true }
            };
            var result = RunTest<CarriageWayProperties>(nameof(VerifyCarriageWayLeftCreateWithIntersectResident), values);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Left, result.Side, "Unexpected value for carriage way side of centre.");
                Assert.AreEqual(OffsetTypes.CarriageWay, result.Type, "Unexpected value for carriage way offset type.");
                Assert.AreEqual(Constants.DEFAULT_CARRIAGE_WAY, result.Distance, "Unexpected value for carriage way distance.");
                Assert.AreEqual(2, result.Intersections, "Unexpected value for carriage way intersections count.");
                Assert.AreEqual(1, result.Curves, "Unexpected value for carriage way curves count.");
                Assert.IsFalse(result.Ignored, "Unexpected value for carriage way ignored.");
                Assert.AreEqual(SidesOfCentre.Left, result.Pavement.Side, "Unexpected value for pavement side of centre.");
                Assert.AreEqual(OffsetTypes.Pavement, result.Pavement.Type, "Unexpected value for pavement offset type.");
                Assert.AreEqual(Constants.DEFAULT_PAVEMENT, result.Pavement.Distance, "Unexpected value for pavement distance.");
                Assert.AreEqual(1, result.Pavement.Curves, "Unexpected value for pavement curves count.");
            });
        }

        [Test]
        public void VerifyCarriageWayLeftCreateWithTwoIntersectionOnePart2()
        {
            var values = new object[]
            {
                new object[] { new double[] { -Constants.DEFAULT_CARRIAGE_WAY, 2, 0 }, false },
                new object[] { new double[] { 0, 6, 0 }, true }
            };
            var result = RunTest<CarriageWayProperties>(nameof(VerifyCarriageWayLeftCreateWithIntersectResident), values);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Left, result.Side, "Unexpected value for carriage way side of centre.");
                Assert.AreEqual(OffsetTypes.CarriageWay, result.Type, "Unexpected value for carriage way offset type.");
                Assert.AreEqual(Constants.DEFAULT_CARRIAGE_WAY, result.Distance, "Unexpected value for carriage way distance.");
                Assert.AreEqual(2, result.Intersections, "Unexpected value for carriage way intersections count.");
                Assert.AreEqual(1, result.Curves, "Unexpected value for carriage way curves count.");
                Assert.IsFalse(result.Ignored, "Unexpected value for carriage way ignored.");
                Assert.AreEqual(SidesOfCentre.Left, result.Pavement.Side, "Unexpected value for pavement side of centre.");
                Assert.AreEqual(OffsetTypes.Pavement, result.Pavement.Type, "Unexpected value for pavement offset type.");
                Assert.AreEqual(Constants.DEFAULT_PAVEMENT, result.Pavement.Distance, "Unexpected value for pavement distance.");
                Assert.AreEqual(1, result.Pavement.Curves, "Unexpected value for pavement curves count.");
            });
        }

        [Test]
        public void VerifyCarriageWayLeftCreateWithTwoIntersectionTwoPart1()
        {
            var values = new object[]
            {
                new object[] { new double[] { -Constants.DEFAULT_CARRIAGE_WAY, 2, 0 }, true },
                new object[] { new double[] { -Constants.DEFAULT_CARRIAGE_WAY, 6, 0 }, false }
            };
            var result = RunTest<CarriageWayProperties>(nameof(VerifyCarriageWayLeftCreateWithIntersectResident), values);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Left, result.Side, "Unexpected value for carriage way side of centre.");
                Assert.AreEqual(OffsetTypes.CarriageWay, result.Type, "Unexpected value for carriage way offset type.");
                Assert.AreEqual(Constants.DEFAULT_CARRIAGE_WAY, result.Distance, "Unexpected value for carriage way distance.");
                Assert.AreEqual(2, result.Intersections, "Unexpected value for carriage way intersections count.");
                Assert.AreEqual(2, result.Curves, "Unexpected value for carriage way curves count.");
                Assert.IsFalse(result.Ignored, "Unexpected value for carriage way ignored.");
                Assert.AreEqual(SidesOfCentre.Left, result.Pavement.Side, "Unexpected value for pavement side of centre.");
                Assert.AreEqual(OffsetTypes.Pavement, result.Pavement.Type, "Unexpected value for pavement offset type.");
                Assert.AreEqual(Constants.DEFAULT_PAVEMENT, result.Pavement.Distance, "Unexpected value for pavement distance.");
                Assert.AreEqual(2, result.Pavement.Curves, "Unexpected value for pavement curves count.");
            });
        }

        [Test]
        public void VerifyCarriageWayLeftCreateWithTwoIntersectionTwoPart2()
        {
            var values = new object[]
            {
                new object[] { new double[] { -Constants.DEFAULT_CARRIAGE_WAY, 6, 0 }, false },
                new object[] { new double[] { -Constants.DEFAULT_CARRIAGE_WAY, 2, 0 }, true }
            };
            var result = RunTest<CarriageWayProperties>(nameof(VerifyCarriageWayLeftCreateWithIntersectResident), values);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Left, result.Side, "Unexpected value for carriage way side of centre.");
                Assert.AreEqual(OffsetTypes.CarriageWay, result.Type, "Unexpected value for carriage way offset type.");
                Assert.AreEqual(Constants.DEFAULT_CARRIAGE_WAY, result.Distance, "Unexpected value for carriage way distance.");
                Assert.AreEqual(2, result.Intersections, "Unexpected value for carriage way intersections count.");
                Assert.AreEqual(2, result.Curves, "Unexpected value for carriage way curves count.");
                Assert.IsFalse(result.Ignored, "Unexpected value for carriage way ignored.");
                Assert.AreEqual(SidesOfCentre.Left, result.Pavement.Side, "Unexpected value for pavement side of centre.");
                Assert.AreEqual(OffsetTypes.Pavement, result.Pavement.Type, "Unexpected value for pavement offset type.");
                Assert.AreEqual(Constants.DEFAULT_PAVEMENT, result.Pavement.Distance, "Unexpected value for pavement distance.");
                Assert.AreEqual(2, result.Pavement.Curves, "Unexpected value for pavement curves count.");
            });
        }

        [Test]
        public void VerifyCarriageWayLeftCreateIgnored()
        {
            var result = RunTest<CarriageWayProperties>(nameof(VerifyCarriageWayLeftCreateResident), true);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Left, result.Side, "Unexpected value for carriage way side of centre.");
                Assert.AreEqual(OffsetTypes.CarriageWay, result.Type, "Unexpected value for carriage way offset type.");
                Assert.AreEqual(Constants.DEFAULT_CARRIAGE_WAY, result.Distance, "Unexpected value for carriage way distance.");
                Assert.AreEqual(0, result.Intersections, "Unexpected value for carriage way intersections count.");
                Assert.AreEqual(0, result.Curves, "Unexpected value for carriage way curves count.");
                Assert.IsTrue(result.Ignored, "Unexpected value for carriage way ignored.");
                Assert.AreEqual(SidesOfCentre.Left, result.Pavement.Side, "Unexpected value for pavement side of centre.");
                Assert.AreEqual(OffsetTypes.Pavement, result.Pavement.Type, "Unexpected value for pavement offset type.");
                Assert.AreEqual(Constants.DEFAULT_PAVEMENT, result.Pavement.Distance, "Unexpected value for pavement distance.");
                Assert.AreEqual(0, result.Pavement.Curves, "Unexpected value for pavement curves count.");
            });
        }

        [Test]
        public void VerifyCarriageWayLeftCreateAndClear()
        {
            var result = RunTest<CarriageWayProperties>(nameof(VerifyCarriageWayLeftCreateAndClearResident));

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Left, result.Side, "Unexpected value for carriage way side of centre.");
                Assert.AreEqual(OffsetTypes.CarriageWay, result.Type, "Unexpected value for carriage way offset type.");
                Assert.AreEqual(Constants.DEFAULT_CARRIAGE_WAY, result.Distance, "Unexpected value for carriage way distance.");
                Assert.AreEqual(0, result.Intersections, "Unexpected value for carriage way intersections count.");
                Assert.AreEqual(0, result.Curves, "Unexpected value for carriage way curves count.");
                Assert.IsFalse(result.Ignored, "Unexpected value for carriage way ignored.");
                Assert.AreEqual(SidesOfCentre.Left, result.Pavement.Side, "Unexpected value for pavement side of centre.");
                Assert.AreEqual(OffsetTypes.Pavement, result.Pavement.Type, "Unexpected value for pavement offset type.");
                Assert.AreEqual(Constants.DEFAULT_PAVEMENT, result.Pavement.Distance, "Unexpected value for pavement distance.");
                Assert.AreEqual(0, result.Pavement.Curves, "Unexpected value for pavement curves count.");
            });
        }

        [Test]
        public void VerifyCarriageWayRight()
        {
            var result = RunTest<CarriageWayProperties>(nameof(VerifyCarriageWayRightResident));

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Right, result.Side, "Unexpected value for carriage way side of centre.");
                Assert.AreEqual(OffsetTypes.CarriageWay, result.Type, "Unexpected value for carriage way offset type.");
                Assert.AreEqual(Constants.DEFAULT_CARRIAGE_WAY, result.Distance, "Unexpected value for carriage way distance.");
                Assert.AreEqual(0, result.Intersections, "Unexpected value for carriage way intersections count.");
                Assert.AreEqual(0, result.Curves, "Unexpected value for carriage way curves count.");
                Assert.IsFalse(result.Ignored, "Unexpected value for carriage way ignored.");
                Assert.AreEqual(SidesOfCentre.Right, result.Pavement.Side, "Unexpected value for pavement side of centre.");
                Assert.AreEqual(OffsetTypes.Pavement, result.Pavement.Type, "Unexpected value for pavement offset type.");
                Assert.AreEqual(Constants.DEFAULT_PAVEMENT, result.Pavement.Distance, "Unexpected value for pavement distance.");
                Assert.AreEqual(0, result.Pavement.Curves, "Unexpected value for pavement curves count.");
            });
        }

        [Test]
        public void VerifyCarriageWayRightClear()
        {
            var result = RunTest<CarriageWayProperties>(nameof(VerifyCarriageWayRightClearResident));

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Right, result.Side, "Unexpected value for carriage way side of centre.");
                Assert.AreEqual(OffsetTypes.CarriageWay, result.Type, "Unexpected value for carriage way offset type.");
                Assert.AreEqual(Constants.DEFAULT_CARRIAGE_WAY, result.Distance, "Unexpected value for carriage way distance.");
                Assert.AreEqual(0, result.Intersections, "Unexpected value for carriage way intersections count.");
                Assert.AreEqual(0, result.Curves, "Unexpected value for carriage way curves count.");
                Assert.IsFalse(result.Ignored, "Unexpected value for carriage way ignored.");
                Assert.AreEqual(SidesOfCentre.Right, result.Pavement.Side, "Unexpected value for pavement side of centre.");
                Assert.AreEqual(OffsetTypes.Pavement, result.Pavement.Type, "Unexpected value for pavement offset type.");
                Assert.AreEqual(Constants.DEFAULT_PAVEMENT, result.Pavement.Distance, "Unexpected value for pavement distance.");
                Assert.AreEqual(0, result.Pavement.Curves, "Unexpected value for pavement curves count.");
            });
        }

        [Test]
        public void VerifyCarriageWayRightCreate()
        {
            var result = RunTest<CarriageWayProperties>(nameof(VerifyCarriageWayRightCreateResident), false);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Right, result.Side, "Unexpected value for carriage way side of centre.");
                Assert.AreEqual(OffsetTypes.CarriageWay, result.Type, "Unexpected value for carriage way offset type.");
                Assert.AreEqual(Constants.DEFAULT_CARRIAGE_WAY, result.Distance, "Unexpected value for carriage way distance.");
                Assert.AreEqual(0, result.Intersections, "Unexpected value for carriage way intersections count.");
                Assert.AreEqual(1, result.Curves, "Unexpected value for carriage way curves count.");
                Assert.IsFalse(result.Ignored, "Unexpected value for carriage way ignored.");
                Assert.AreEqual(SidesOfCentre.Right, result.Pavement.Side, "Unexpected value for pavement side of centre.");
                Assert.AreEqual(OffsetTypes.Pavement, result.Pavement.Type, "Unexpected value for pavement offset type.");
                Assert.AreEqual(Constants.DEFAULT_PAVEMENT, result.Pavement.Distance, "Unexpected value for pavement distance.");
                Assert.AreEqual(1, result.Pavement.Curves, "Unexpected value for pavement curves count.");
            });
        }

        [Test]
        public void VerifyCarriageWayRightCreateIgnored()
        {
            var result = RunTest<CarriageWayProperties>(nameof(VerifyCarriageWayRightCreateResident), true);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Right, result.Side, "Unexpected value for carriage way side of centre.");
                Assert.AreEqual(OffsetTypes.CarriageWay, result.Type, "Unexpected value for carriage way offset type.");
                Assert.AreEqual(Constants.DEFAULT_CARRIAGE_WAY, result.Distance, "Unexpected value for carriage way distance.");
                Assert.AreEqual(0, result.Intersections, "Unexpected value for carriage way intersections count.");
                Assert.AreEqual(0, result.Curves, "Unexpected value for carriage way curves count.");
                Assert.IsTrue(result.Ignored, "Unexpected value for carriage way ignored.");
                Assert.AreEqual(SidesOfCentre.Right, result.Pavement.Side, "Unexpected value for pavement side of centre.");
                Assert.AreEqual(OffsetTypes.Pavement, result.Pavement.Type, "Unexpected value for pavement offset type.");
                Assert.AreEqual(Constants.DEFAULT_PAVEMENT, result.Pavement.Distance, "Unexpected value for pavement distance.");
                Assert.AreEqual(0, result.Pavement.Curves, "Unexpected value for pavement curves count.");
            });
        }

        [Test]
        public void VerifyCarriageWayRightCreateWithIntersectionBefore()
        {
            var values = new object[] { new object[] { new double[] { Constants.DEFAULT_CARRIAGE_WAY, 5, 0 }, true } };
            var result = RunTest<CarriageWayProperties>(nameof(VerifyCarriageWayRightCreateWithIntersectResident), values);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Right, result.Side, "Unexpected value for carriage way side of centre.");
                Assert.AreEqual(OffsetTypes.CarriageWay, result.Type, "Unexpected value for carriage way offset type.");
                Assert.AreEqual(Constants.DEFAULT_CARRIAGE_WAY, result.Distance, "Unexpected value for carriage way distance.");
                Assert.AreEqual(1, result.Intersections, "Unexpected value for carriage way intersections count.");
                Assert.AreEqual(1, result.Curves, "Unexpected value for carriage way curves count.");
                Assert.IsFalse(result.Ignored, "Unexpected value for carriage way ignored.");
                Assert.AreEqual(SidesOfCentre.Right, result.Pavement.Side, "Unexpected value for pavement side of centre.");
                Assert.AreEqual(OffsetTypes.Pavement, result.Pavement.Type, "Unexpected value for pavement offset type.");
                Assert.AreEqual(Constants.DEFAULT_PAVEMENT, result.Pavement.Distance, "Unexpected value for pavement distance.");
                Assert.AreEqual(1, result.Pavement.Curves, "Unexpected value for pavement curves count.");
            });
        }

        [Test]
        public void VerifyCarriageWayRightCreateWithoutIntersectionBefore()
        {
            var values = new object[] { new object[] { new double[] { 0, 5, 0 }, true } };
            var result = RunTest<CarriageWayProperties>(nameof(VerifyCarriageWayRightCreateWithIntersectResident), values);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Right, result.Side, "Unexpected value for carriage way side of centre.");
                Assert.AreEqual(OffsetTypes.CarriageWay, result.Type, "Unexpected value for carriage way offset type.");
                Assert.AreEqual(Constants.DEFAULT_CARRIAGE_WAY, result.Distance, "Unexpected value for carriage way distance.");
                Assert.AreEqual(1, result.Intersections, "Unexpected value for carriage way intersections count.");
                Assert.AreEqual(1, result.Curves, "Unexpected value for carriage way curves count.");
                Assert.IsFalse(result.Ignored, "Unexpected value for carriage way ignored.");
                Assert.AreEqual(SidesOfCentre.Right, result.Pavement.Side, "Unexpected value for pavement side of centre.");
                Assert.AreEqual(OffsetTypes.Pavement, result.Pavement.Type, "Unexpected value for pavement offset type.");
                Assert.AreEqual(Constants.DEFAULT_PAVEMENT, result.Pavement.Distance, "Unexpected value for pavement distance.");
                Assert.AreEqual(1, result.Pavement.Curves, "Unexpected value for pavement curves count.");
            });
        }

        [Test]
        public void VerifyCarriageWayRightCreateWithIntersectionAfter()
        {
            var values = new object[] { new object[] { new double[] { Constants.DEFAULT_CARRIAGE_WAY, 5, 0 }, false } };
            var result = RunTest<CarriageWayProperties>(nameof(VerifyCarriageWayRightCreateWithIntersectResident), values);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Right, result.Side, "Unexpected value for carriage way side of centre.");
                Assert.AreEqual(OffsetTypes.CarriageWay, result.Type, "Unexpected value for carriage way offset type.");
                Assert.AreEqual(Constants.DEFAULT_CARRIAGE_WAY, result.Distance, "Unexpected value for carriage way distance.");
                Assert.AreEqual(1, result.Intersections, "Unexpected value for carriage way intersections count.");
                Assert.AreEqual(1, result.Curves, "Unexpected value for carriage way curves count.");
                Assert.IsFalse(result.Ignored, "Unexpected value for carriage way ignored.");
                Assert.AreEqual(SidesOfCentre.Right, result.Pavement.Side, "Unexpected value for pavement side of centre.");
                Assert.AreEqual(OffsetTypes.Pavement, result.Pavement.Type, "Unexpected value for pavement offset type.");
                Assert.AreEqual(Constants.DEFAULT_PAVEMENT, result.Pavement.Distance, "Unexpected value for pavement distance.");
                Assert.AreEqual(1, result.Pavement.Curves, "Unexpected value for pavement curves count.");
            });
        }

        [Test]
        public void VerifyCarriageWayRightCreateWithoutIntersectionAfter()
        {
            var values = new object[] { new object[] { new double[] { 0, 5, 0 }, false } };
            var result = RunTest<CarriageWayProperties>(nameof(VerifyCarriageWayRightCreateWithIntersectResident), values);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Right, result.Side, "Unexpected value for carriage way side of centre.");
                Assert.AreEqual(OffsetTypes.CarriageWay, result.Type, "Unexpected value for carriage way offset type.");
                Assert.AreEqual(Constants.DEFAULT_CARRIAGE_WAY, result.Distance, "Unexpected value for carriage way distance.");
                Assert.AreEqual(1, result.Intersections, "Unexpected value for carriage way intersections count.");
                Assert.AreEqual(1, result.Curves, "Unexpected value for carriage way curves count.");
                Assert.IsFalse(result.Ignored, "Unexpected value for carriage way ignored.");
                Assert.AreEqual(SidesOfCentre.Right, result.Pavement.Side, "Unexpected value for pavement side of centre.");
                Assert.AreEqual(OffsetTypes.Pavement, result.Pavement.Type, "Unexpected value for pavement offset type.");
                Assert.AreEqual(Constants.DEFAULT_PAVEMENT, result.Pavement.Distance, "Unexpected value for pavement distance.");
                Assert.AreEqual(1, result.Pavement.Curves, "Unexpected value for pavement curves count.");
            });
        }

        [Test]
        public void VerifyCarriageWayRightCreateWithTwoIntersectionOnePart1()
        {
            var values = new object[]
            {
                new object[] { new double[] { Constants.DEFAULT_CARRIAGE_WAY, 2, 0 }, false },
                new object[] { new double[] { Constants.DEFAULT_CARRIAGE_WAY, 6, 0 }, true }
            };
            var result = RunTest<CarriageWayProperties>(nameof(VerifyCarriageWayRightCreateWithIntersectResident), values);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Right, result.Side, "Unexpected value for carriage way side of centre.");
                Assert.AreEqual(OffsetTypes.CarriageWay, result.Type, "Unexpected value for carriage way offset type.");
                Assert.AreEqual(Constants.DEFAULT_CARRIAGE_WAY, result.Distance, "Unexpected value for carriage way distance.");
                Assert.AreEqual(2, result.Intersections, "Unexpected value for carriage way intersections count.");
                Assert.AreEqual(1, result.Curves, "Unexpected value for carriage way curves count.");
                Assert.IsFalse(result.Ignored, "Unexpected value for carriage way ignored.");
                Assert.AreEqual(SidesOfCentre.Right, result.Pavement.Side, "Unexpected value for pavement side of centre.");
                Assert.AreEqual(OffsetTypes.Pavement, result.Pavement.Type, "Unexpected value for pavement offset type.");
                Assert.AreEqual(Constants.DEFAULT_PAVEMENT, result.Pavement.Distance, "Unexpected value for pavement distance.");
                Assert.AreEqual(1, result.Pavement.Curves, "Unexpected value for pavement curves count.");
            });
        }

        [Test]
        public void VerifyCarriageWayRightCreateWithTwoIntersectionOnePart2()
        {
            var values = new object[]
            {
                new object[] { new double[] { Constants.DEFAULT_CARRIAGE_WAY, 2, 0 }, false },
                new object[] { new double[] { 0, 6, 0 }, true }
            };
            var result = RunTest<CarriageWayProperties>(nameof(VerifyCarriageWayRightCreateWithIntersectResident), values);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Right, result.Side, "Unexpected value for carriage way side of centre.");
                Assert.AreEqual(OffsetTypes.CarriageWay, result.Type, "Unexpected value for carriage way offset type.");
                Assert.AreEqual(Constants.DEFAULT_CARRIAGE_WAY, result.Distance, "Unexpected value for carriage way distance.");
                Assert.AreEqual(2, result.Intersections, "Unexpected value for carriage way intersections count.");
                Assert.AreEqual(1, result.Curves, "Unexpected value for carriage way curves count.");
                Assert.IsFalse(result.Ignored, "Unexpected value for carriage way ignored.");
                Assert.AreEqual(SidesOfCentre.Right, result.Pavement.Side, "Unexpected value for pavement side of centre.");
                Assert.AreEqual(OffsetTypes.Pavement, result.Pavement.Type, "Unexpected value for pavement offset type.");
                Assert.AreEqual(Constants.DEFAULT_PAVEMENT, result.Pavement.Distance, "Unexpected value for pavement distance.");
                Assert.AreEqual(1, result.Pavement.Curves, "Unexpected value for pavement curves count.");
            });
        }

        [Test]
        public void VerifyCarriageWayRightCreateWithTwoIntersectionTwoPart1()
        {
            var values = new object[]
            {
                new object[] { new double[] { Constants.DEFAULT_CARRIAGE_WAY, 2, 0 }, true },
                new object[] { new double[] { Constants.DEFAULT_CARRIAGE_WAY, 6, 0 }, false }
            };
            var result = RunTest<CarriageWayProperties>(nameof(VerifyCarriageWayRightCreateWithIntersectResident), values);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Right, result.Side, "Unexpected value for carriage way side of centre.");
                Assert.AreEqual(OffsetTypes.CarriageWay, result.Type, "Unexpected value for carriage way offset type.");
                Assert.AreEqual(Constants.DEFAULT_CARRIAGE_WAY, result.Distance, "Unexpected value for carriage way distance.");
                Assert.AreEqual(2, result.Intersections, "Unexpected value for carriage way intersections count.");
                Assert.AreEqual(2, result.Curves, "Unexpected value for carriage way curves count.");
                Assert.IsFalse(result.Ignored, "Unexpected value for carriage way ignored.");
                Assert.AreEqual(SidesOfCentre.Right, result.Pavement.Side, "Unexpected value for pavement side of centre.");
                Assert.AreEqual(OffsetTypes.Pavement, result.Pavement.Type, "Unexpected value for pavement offset type.");
                Assert.AreEqual(Constants.DEFAULT_PAVEMENT, result.Pavement.Distance, "Unexpected value for pavement distance.");
                Assert.AreEqual(2, result.Pavement.Curves, "Unexpected value for pavement curves count.");
            });
        }

        [Test]
        public void VerifyCarriageWayRightCreateWithTwoIntersectionTwoPart2()
        {
            var values = new object[]
            {
                new object[] { new double[] { Constants.DEFAULT_CARRIAGE_WAY, 6, 0 }, false },
                new object[] { new double[] { Constants.DEFAULT_CARRIAGE_WAY, 2, 0 }, true }
            };
            var result = RunTest<CarriageWayProperties>(nameof(VerifyCarriageWayRightCreateWithIntersectResident), values);

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Right, result.Side, "Unexpected value for carriage way side of centre.");
                Assert.AreEqual(OffsetTypes.CarriageWay, result.Type, "Unexpected value for carriage way offset type.");
                Assert.AreEqual(Constants.DEFAULT_CARRIAGE_WAY, result.Distance, "Unexpected value for carriage way distance.");
                Assert.AreEqual(2, result.Intersections, "Unexpected value for carriage way intersections count.");
                Assert.AreEqual(2, result.Curves, "Unexpected value for carriage way curves count.");
                Assert.IsFalse(result.Ignored, "Unexpected value for carriage way ignored.");
                Assert.AreEqual(SidesOfCentre.Right, result.Pavement.Side, "Unexpected value for pavement side of centre.");
                Assert.AreEqual(OffsetTypes.Pavement, result.Pavement.Type, "Unexpected value for pavement offset type.");
                Assert.AreEqual(Constants.DEFAULT_PAVEMENT, result.Pavement.Distance, "Unexpected value for pavement distance.");
                Assert.AreEqual(2, result.Pavement.Curves, "Unexpected value for pavement curves count.");
            });
        }

        [Test]
        public void VerifyCarriageWayRightCreateAndClear()
        {
            var result = RunTest<CarriageWayProperties>(nameof(VerifyCarriageWayRightCreateAndClearResident));

            Assert.Multiple(() =>
            {
                Assert.AreEqual(SidesOfCentre.Right, result.Side, "Unexpected value for carriage way side of centre.");
                Assert.AreEqual(OffsetTypes.CarriageWay, result.Type, "Unexpected value for carriage way offset type.");
                Assert.AreEqual(Constants.DEFAULT_CARRIAGE_WAY, result.Distance, "Unexpected value for carriage way distance.");
                Assert.AreEqual(0, result.Intersections, "Unexpected value for carriage way intersections count.");
                Assert.AreEqual(0, result.Curves, "Unexpected value for carriage way curves count.");
                Assert.IsFalse(result.Ignored, "Unexpected value for carriage way ignored.");
                Assert.AreEqual(SidesOfCentre.Right, result.Pavement.Side, "Unexpected value for pavement side of centre.");
                Assert.AreEqual(OffsetTypes.Pavement, result.Pavement.Type, "Unexpected value for pavement offset type.");
                Assert.AreEqual(Constants.DEFAULT_PAVEMENT, result.Pavement.Distance, "Unexpected value for pavement distance.");
                Assert.AreEqual(0, result.Pavement.Curves, "Unexpected value for pavement curves count.");
            });
        }

        public CarriageWayProperties VerifyCarriageWayLeftResident()
        {
            var carriage = new CarriageWayLeft();
            var carriageProps = SetCarriageWayProps(carriage);
            carriageProps.Pavement = SetPavementProps(carriage.Pavement);

            return carriageProps;
        }

        public CarriageWayProperties VerifyCarriageWayLeftClearResident()
        {

            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;
            var carriage = new CarriageWayLeft();

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                carriage.Clear();
                var carriageProps = SetCarriageWayProps(carriage);
                carriageProps.Pavement = SetPavementProps(carriage.Pavement);

                return carriageProps;
            }
        }

        public CarriageWayProperties VerifyCarriageWayLeftCreateResident(bool ignored)
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var blockTable = (BlockTable)acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead);
                var blockTableRecord = (BlockTableRecord)acTrans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                acCurDb.RegisterLayer(Constants.LAYER_DEF_POINTS);

                var line = new Line(new Point3d(0,0,0), new Point3d(0, 10, 0));
                var centre = new RoadCentreLine {BaseObject = blockTableRecord.AppendEntity(line)};
                var road = new Road();
                road.CentreLines.Add(centre);

                var carriage = new CarriageWayLeft { Ignore = ignored };
                carriage.Create(centre);

                var carriageProps = SetCarriageWayProps(carriage);
                carriageProps.Pavement = SetPavementProps(carriage.Pavement);

                return carriageProps;
            }
        }

        public CarriageWayProperties VerifyCarriageWayLeftCreateWithIntersectResident(object[] values)
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var blockTable = (BlockTable)acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead);
                var blockTableRecord = (BlockTableRecord)acTrans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                acCurDb.RegisterLayer(Constants.LAYER_DEF_POINTS);

                var line = new Line(new Point3d(0, 0, 0), new Point3d(0, 10, 0));
                var centre = new RoadCentreLine { BaseObject = blockTableRecord.AppendEntity(line) };
                var road = new Road();
                road.CentreLines.Add(centre);

                var carriage = new CarriageWayLeft();
                foreach (object[] value in values)
                {
                    carriage.Intersections.Add(BuildIntersect(value));
                }
                
                carriage.Create(centre);

                var carriageProps = SetCarriageWayProps(carriage);
                carriageProps.Pavement = SetPavementProps(carriage.Pavement);

                return carriageProps;
            }
        }

        public CarriageWayProperties VerifyCarriageWayLeftCreateAndClearResident()
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var blockTable = (BlockTable)acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead);
                var blockTableRecord = (BlockTableRecord)acTrans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                acCurDb.RegisterLayer(Constants.LAYER_DEF_POINTS);

                var line = new Line(new Point3d(0, 0, 0), new Point3d(0, 10, 0));
                var centre = new RoadCentreLine { BaseObject = blockTableRecord.AppendEntity(line) };
                var road = new Road();
                road.CentreLines.Add(centre);

                var carriage = new CarriageWayLeft();
                carriage.Create(centre);
                carriage.Clear();

                var carriageProps = SetCarriageWayProps(carriage);
                carriageProps.Pavement = SetPavementProps(carriage.Pavement);

                return carriageProps;
            }
        }

        public CarriageWayProperties VerifyCarriageWayRightResident()
        {
            var carriage = new CarriageWayRight();
            var carriageProps = SetCarriageWayProps(carriage);
            carriageProps.Pavement = SetPavementProps(carriage.Pavement);

            return carriageProps;
        }

        public CarriageWayProperties VerifyCarriageWayRightClearResident()
        {

            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;
            var carriage = new CarriageWayRight();

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                carriage.Clear();
                var carriageProps = SetCarriageWayProps(carriage);
                carriageProps.Pavement = SetPavementProps(carriage.Pavement);

                return carriageProps;
            }
        }

        public CarriageWayProperties VerifyCarriageWayRightCreateResident(bool ignored)
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var blockTable = (BlockTable)acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead);
                var blockTableRecord = (BlockTableRecord)acTrans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                acCurDb.RegisterLayer(Constants.LAYER_DEF_POINTS);

                var line = new Line(new Point3d(0, 0, 0), new Point3d(0, 10, 0));
                var centre = new RoadCentreLine { BaseObject = blockTableRecord.AppendEntity(line) };
                var road = new Road();
                road.CentreLines.Add(centre);

                var carriage = new CarriageWayRight { Ignore = ignored };
                carriage.Create(centre);

                var carriageProps = SetCarriageWayProps(carriage);
                carriageProps.Pavement = SetPavementProps(carriage.Pavement);

                return carriageProps;
            }
        }

        public CarriageWayProperties VerifyCarriageWayRightCreateWithIntersectResident(object[] values)
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var blockTable = (BlockTable)acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead);
                var blockTableRecord = (BlockTableRecord)acTrans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                acCurDb.RegisterLayer(Constants.LAYER_DEF_POINTS);

                var line = new Line(new Point3d(0, 0, 0), new Point3d(0, 10, 0));
                var centre = new RoadCentreLine { BaseObject = blockTableRecord.AppendEntity(line) };
                var road = new Road();
                road.CentreLines.Add(centre);

                var carriage = new CarriageWayRight();
                foreach (object[] value in values)
                {
                    carriage.Intersections.Add(BuildIntersect(value));
                }
                carriage.Create(centre);

                var carriageProps = SetCarriageWayProps(carriage);
                carriageProps.Pavement = SetPavementProps(carriage.Pavement);

                return carriageProps;
            }
        }

        public CarriageWayProperties VerifyCarriageWayRightCreateAndClearResident()
        {
            var acDoc = Application.DocumentManager.MdiActiveDocument;
            var acCurDb = acDoc.Database;

            using (var acTrans = acCurDb.TransactionManager.StartTransaction())
            {
                var blockTable = (BlockTable)acTrans.GetObject(acCurDb.BlockTableId, OpenMode.ForRead);
                var blockTableRecord = (BlockTableRecord)acTrans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                acCurDb.RegisterLayer(Constants.LAYER_DEF_POINTS);

                var line = new Line(new Point3d(0, 0, 0), new Point3d(0, 10, 0));
                var centre = new RoadCentreLine { BaseObject = blockTableRecord.AppendEntity(line) };
                var road = new Road();
                road.CentreLines.Add(centre);

                var carriage = new CarriageWayRight();
                carriage.Create(centre);
                carriage.Clear();

                var carriageProps = SetCarriageWayProps(carriage);
                carriageProps.Pavement = SetPavementProps(carriage.Pavement);

                return carriageProps;
            }
        }

        private static CarriageWayProperties SetCarriageWayProps(CarriageWay carriageWay)
        {
            return new CarriageWayProperties
            {
                Side = carriageWay.Side,
                Type = carriageWay.OffsetType,
                Distance = carriageWay.DistanceFromCentre,
                Intersections = carriageWay.Intersections.Count,
                Curves = carriageWay.Curves.Count,
                Ignored = carriageWay.Ignore
            };
        }               

        private OffsetIntersect BuildIntersect(object[] values)
        {
            var points = (double[])values[0];
            var before = (bool)values[1];
            return new OffsetIntersect(new Point3d(points[0], points[1], points[2]), before);
        }

        [Test]
        public void VerifyOffsetIntersectConstructor()
        {
            var result = RunTest<bool>(nameof(VerifyOffsetIntersectConstructorResident));
            Assert.IsTrue(result, "Unable to create instance.");
        }

        public bool VerifyOffsetIntersectConstructorResident()
        {
            return Activator.CreateInstance(typeof(OffsetIntersect), true) is OffsetIntersect;
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
