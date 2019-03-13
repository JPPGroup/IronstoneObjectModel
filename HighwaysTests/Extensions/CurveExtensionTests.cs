using System;
using System.Reflection;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.Ironstone.Highways.ObjectModel.Extensions;
using NUnit.Framework;

namespace Jpp.Ironstone.Highways.ObjectModel.Tests.Extensions
{
    [TestFixture]
    public class CurveExtensionTests : IronstoneTestFixture
    {
        public CurveExtensionTests() : base(Assembly.GetExecutingAssembly(), typeof(CurveExtensionTests)) { }

        [Test]
        public void VerifyCurveExtensionInvalidOffsetSide()
        {
            var result = RunTest<bool>(nameof(VerifyCurveExtensionInvalidOffsetSideResident));

            Assert.IsTrue(result,"Offset should be invalid.");
        }

        public bool VerifyCurveExtensionInvalidOffsetSideResident()
        {
            try
            {
                var point1 = new Point3d(0, 0, 0);
                var point2 = new Point3d(10, 0, 0);
                var line = new Line(point1, point2);
                line.CreateOffset((SidesOfCentre)999, 2.5);

                return false;
            }
            catch (ArgumentOutOfRangeException)
            {
                return true;
            }
        }

        [Test]
        public void VerifyCurveExtensionLine1OffsetLeft()
        {
            const double distance = 2.5;
            var startPoint = new double[] { 0, 0, 0 };
            var endPoint = new double[] { 10, 0, 0 };

            var result = RunTest<object[]>(nameof(VerifyCurveExtensionLineOffsetLeftResident), new object[] {distance, startPoint, endPoint});
            var returnStart = (double[]) result[0];
            var returnEnd = (double[])result[1];

            Assert.Multiple(() =>
            {
                Assert.AreEqual(startPoint[0], returnStart[0], "Incorrect start X.");
                Assert.AreEqual(startPoint[1] + distance, returnStart[1], "Incorrect start Y.");
                Assert.AreEqual(startPoint[2], returnStart[2], "Incorrect start Z.");
                Assert.AreEqual(endPoint[0], returnEnd[0], "Incorrect end X.");
                Assert.AreEqual(endPoint[1] + distance, returnEnd[1], "Incorrect end Y.");
                Assert.AreEqual(endPoint[2], returnEnd[2], "Incorrect end Z.");
            });
        }

        [Test]
        public void VerifyCurveExtensionLine1OffsetRight()
        {
            const double distance = 2.5;
            var startPoint = new double[] { 0, 0, 0 };
            var endPoint = new double[] { 10, 0, 0 };

            var result = RunTest<object[]>(nameof(VerifyCurveExtensionLineOffsetRightResident), new object[] { distance, startPoint, endPoint });
            var returnStart = (double[])result[0];
            var returnEnd = (double[])result[1];

            Assert.Multiple(() =>
            {
                Assert.AreEqual(startPoint[0], returnStart[0], "Incorrect start X.");
                Assert.AreEqual(startPoint[1] - distance, returnStart[1], "Incorrect start Y.");
                Assert.AreEqual(startPoint[2], returnStart[2], "Incorrect start Z.");
                Assert.AreEqual(endPoint[0], returnEnd[0], "Incorrect end X.");
                Assert.AreEqual(endPoint[1] - distance, returnEnd[1], "Incorrect end Y.");
                Assert.AreEqual(endPoint[2], returnEnd[2], "Incorrect end Z.");
            });
        }

        [Test]
        public void VerifyCurveExtensionLine2OffsetLeft()
        {
            const double distance = 3;
            var startPoint = new double[] { 10, 0, 0 };
            var endPoint = new double[] { 0, 0, 0 };

            var result = RunTest<object[]>(nameof(VerifyCurveExtensionLineOffsetLeftResident), new object[] { distance, startPoint, endPoint });
            var returnStart = (double[])result[0];
            var returnEnd = (double[])result[1];

            Assert.Multiple(() =>
            {
                Assert.AreEqual(startPoint[0], returnStart[0], "Incorrect start X.");
                Assert.AreEqual(startPoint[1] - distance, returnStart[1], "Incorrect start Y.");
                Assert.AreEqual(startPoint[2], returnStart[2], "Incorrect start Z.");
                Assert.AreEqual(endPoint[0], returnEnd[0], "Incorrect end X.");
                Assert.AreEqual(endPoint[1] - distance, returnEnd[1], "Incorrect end Y.");
                Assert.AreEqual(endPoint[2], returnEnd[2], "Incorrect end Z.");
            });
        }

        [Test]
        public void VerifyCurveExtensionLine2OffsetRight()
        {
            const double distance = 3;
            var startPoint = new double[] { 10, 0, 0 };
            var endPoint = new double[] { 0, 0, 0 };

            var result = RunTest<object[]>(nameof(VerifyCurveExtensionLineOffsetRightResident), new object[] { distance, startPoint, endPoint });
            var returnStart = (double[])result[0];
            var returnEnd = (double[])result[1];

            Assert.Multiple(() =>
            {
                Assert.AreEqual(startPoint[0], returnStart[0], "Incorrect start X.");
                Assert.AreEqual(startPoint[1] + distance, returnStart[1], "Incorrect start Y.");
                Assert.AreEqual(startPoint[2], returnStart[2], "Incorrect start Z.");
                Assert.AreEqual(endPoint[0], returnEnd[0], "Incorrect end X.");
                Assert.AreEqual(endPoint[1] + distance, returnEnd[1], "Incorrect end Y.");
                Assert.AreEqual(endPoint[2], returnEnd[2], "Incorrect end Z.");
            });
        }

        [Test]
        public void VerifyCurveExtensionLine3OffsetLeft()
        {
            const double distance = 4;
            var startPoint = new double[] { 0, 0, 0 };
            var endPoint = new double[] { 0, 10, 0 };

            var result = RunTest<object[]>(nameof(VerifyCurveExtensionLineOffsetLeftResident), new object[] { distance, startPoint, endPoint });
            var returnStart = (double[])result[0];
            var returnEnd = (double[])result[1];

            Assert.Multiple(() =>
            {
                Assert.AreEqual(startPoint[0] - distance, returnStart[0], "Incorrect start X.");
                Assert.AreEqual(startPoint[1] , returnStart[1], "Incorrect start Y.");
                Assert.AreEqual(startPoint[2], returnStart[2], "Incorrect start Z.");
                Assert.AreEqual(endPoint[0] - distance, returnEnd[0], "Incorrect end X.");
                Assert.AreEqual(endPoint[1] , returnEnd[1], "Incorrect end Y.");
                Assert.AreEqual(endPoint[2], returnEnd[2], "Incorrect end Z.");
            });
        }

        [Test]
        public void VerifyCurveExtensionLine3OffsetRight()
        {
            const double distance = 4;
            var startPoint = new double[] { 0, 0, 0 };
            var endPoint = new double[] { 0, 10, 0 };

            var result = RunTest<object[]>(nameof(VerifyCurveExtensionLineOffsetRightResident), new object[] { distance, startPoint, endPoint });
            var returnStart = (double[])result[0];
            var returnEnd = (double[])result[1];

            Assert.Multiple(() =>
            {
                Assert.AreEqual(startPoint[0] + distance, returnStart[0], "Incorrect start X.");
                Assert.AreEqual(startPoint[1], returnStart[1], "Incorrect start Y.");
                Assert.AreEqual(startPoint[2], returnStart[2], "Incorrect start Z.");
                Assert.AreEqual(endPoint[0] + distance, returnEnd[0], "Incorrect end X.");
                Assert.AreEqual(endPoint[1], returnEnd[1], "Incorrect end Y.");
                Assert.AreEqual(endPoint[2], returnEnd[2], "Incorrect end Z.");
            });
        }

        [Test]
        public void VerifyCurveExtensionLine4OffsetLeft()
        {
            const double distance = 4;
            var startPoint = new double[] { 0, 10, 0 };
            var endPoint = new double[] { 0, 0, 0 };

            var result = RunTest<object[]>(nameof(VerifyCurveExtensionLineOffsetLeftResident), new object[] { distance, startPoint, endPoint });
            var returnStart = (double[])result[0];
            var returnEnd = (double[])result[1];

            Assert.Multiple(() =>
            {
                Assert.AreEqual(startPoint[0] + distance, returnStart[0], "Incorrect start X.");
                Assert.AreEqual(startPoint[1], returnStart[1], "Incorrect start Y.");
                Assert.AreEqual(startPoint[2], returnStart[2], "Incorrect start Z.");
                Assert.AreEqual(endPoint[0] + distance, returnEnd[0], "Incorrect end X.");
                Assert.AreEqual(endPoint[1], returnEnd[1], "Incorrect end Y.");
                Assert.AreEqual(endPoint[2], returnEnd[2], "Incorrect end Z.");
            });
        }

        [Test]
        public void VerifyCurveExtensionLine4OffsetRight()
        {
            const double distance = 4;
            var startPoint = new double[] { 0, 10, 0 };
            var endPoint = new double[] { 0, 0, 0 };

            var result = RunTest<object[]>(nameof(VerifyCurveExtensionLineOffsetRightResident), new object[] { distance, startPoint, endPoint });
            var returnStart = (double[])result[0];
            var returnEnd = (double[])result[1];

            Assert.Multiple(() =>
            {
                Assert.AreEqual(startPoint[0] - distance, returnStart[0], "Incorrect start X.");
                Assert.AreEqual(startPoint[1], returnStart[1], "Incorrect start Y.");
                Assert.AreEqual(startPoint[2], returnStart[2], "Incorrect start Z.");
                Assert.AreEqual(endPoint[0] - distance, returnEnd[0], "Incorrect end X.");
                Assert.AreEqual(endPoint[1], returnEnd[1], "Incorrect end Y.");
                Assert.AreEqual(endPoint[2], returnEnd[2], "Incorrect end Z.");
            });
        }

        public object[] VerifyCurveExtensionLineOffsetLeftResident(object[] values)
        {
            var distance = (double) values[0];
            var start = (double[]) values[1];
            var end = (double[]) values[2];

            var point1 = new Point3d(start[0], start[1], start[2]);
            var point2 = new Point3d(end[0], end[1], end[2]);
            var line = new Line(point1, point2);

            var offset = line.CreateOffset(SidesOfCentre.Left, distance);

            if (offset == null) return null;

            var offsetStartPoint = new [] { offset.StartPoint.X, offset.StartPoint.Y, offset.StartPoint.Z };
            var offsetEndPoint = new[] { offset.EndPoint.X, offset.EndPoint.Y, offset.EndPoint.Z };

            return new object[] { offsetStartPoint, offsetEndPoint };
        }

        public object[] VerifyCurveExtensionLineOffsetRightResident(object[] values)
        {
            var distance = (double)values[0];
            var start = (double[])values[1];
            var end = (double[])values[2];

            var point1 = new Point3d(start[0], start[1], start[2]);
            var point2 = new Point3d(end[0], end[1], end[2]);
            var line = new Line(point1, point2);

            var offset = line.CreateOffset(SidesOfCentre.Right, distance);

            if (offset == null) return null;

            var offsetStartPoint = new[] { offset.StartPoint.X, offset.StartPoint.Y, offset.StartPoint.Z };
            var offsetEndPoint = new[] { offset.EndPoint.X, offset.EndPoint.Y, offset.EndPoint.Z };

            return new object[] { offsetStartPoint, offsetEndPoint };
        }

        [Test]
        public void VerifyCurveExtensionArc1OffsetLeft()
        {
            const double distance = 4;
            const double radius = 10;
            const double start = 0;
            const double end = Math.PI / 2;

            var result = RunTest<double[]>(nameof(VerifyCurveExtensionArcOffsetLeftResident), new[] { distance, radius, start, end });

            Assert.Multiple(() =>
            {
                Assert.AreEqual(radius - distance, result[0], "Incorrect radius.");
                Assert.AreEqual(start, result[1], "Incorrect start angle.");
                Assert.AreEqual(end, result[2], "Incorrect start end.");
            });
        }

        [Test]
        public void VerifyCurveExtensionArc1OffsetRight()
        {
            const double distance = 4;
            const double radius = 10;
            const double start = 0;
            const double end = Math.PI / 2;

            var result = RunTest<double[]>(nameof(VerifyCurveExtensionArcOffsetRightResident), new[] { distance, radius, start, end });

            Assert.Multiple(() =>
            {
                Assert.AreEqual(radius + distance, result[0], "Incorrect radius.");
                Assert.AreEqual(start, result[1], "Incorrect start angle.");
                Assert.AreEqual(end, result[2], "Incorrect start end.");
            });
        }

        [Test]
        public void VerifyCurveExtensionArc2OffsetLeft()
        {
            const double distance = 2;
            const double radius = 6;
            const double start = Math.PI / 2;
            const double end = 0;

            var result = RunTest<double[]>(nameof(VerifyCurveExtensionArcOffsetLeftResident), new[] { distance, radius, start, end });

            Assert.Multiple(() =>
            {
                Assert.AreEqual(radius + distance, result[0], "Incorrect radius.");
                Assert.AreEqual(start, result[1], "Incorrect start angle.");
                Assert.AreEqual(end, result[2], "Incorrect start end.");
            });
        }

        [Test]
        public void VerifyCurveExtensionArc2OffsetRight()
        {
            const double distance = 2;
            const double radius = 6;
            const double start = Math.PI / 2;
            const double end = 0;

            var result = RunTest<double[]>(nameof(VerifyCurveExtensionArcOffsetRightResident), new[] { distance, radius, start, end });

            Assert.Multiple(() =>
            {
                Assert.AreEqual(radius - distance, result[0], "Incorrect radius.");
                Assert.AreEqual(start, result[1], "Incorrect start angle.");
                Assert.AreEqual(end, result[2], "Incorrect start end.");
            });
        }

        [Test]
        public void VerifyCurveExtensionArc3OffsetLeft()
        {
            const double distance = 2;
            const double radius = 1.5;
            const double start = Math.PI / 2;
            const double end = 0;

            var result = RunTest<bool>(nameof(VerifyCurveExtensionArcOffsetLeftValidResident), new[] { distance, radius, start, end });

            Assert.IsTrue(result, "Offset should be valid");
        }

        [Test]
        public void VerifyCurveExtensionArc3OffsetRight()
        {
            const double distance = 2;
            const double radius = 1.5;
            const double start = Math.PI / 2;
            const double end = 0;

            var result = RunTest<bool>(nameof(VerifyCurveExtensionArcOffsetRightValidResident), new[] { distance, radius, start, end });

            Assert.IsFalse(result, "Offset should not be valid");
        }

        [Test]
        public void VerifyCurveExtensionArc4OffsetLeft()
        {
            const double distance = 2;
            const double radius = 1.5;
            const double start = 0;
            const double end = Math.PI / 2;

            var result = RunTest<bool>(nameof(VerifyCurveExtensionArcOffsetLeftValidResident), new[] { distance, radius, start, end });

            Assert.IsFalse(result, "Offset should not be valid");
        }

        [Test]
        public void VerifyCurveExtensionArc4OffsetRight()
        {
            const double distance = 2;
            const double radius = 1.5;
            const double start = 0;
            const double end = Math.PI / 2;

            var result = RunTest<bool>(nameof(VerifyCurveExtensionArcOffsetRightValidResident), new[] { distance, radius, start, end });

            Assert.IsTrue(result, "Offset should be valid");
        }

        public double[] VerifyCurveExtensionArcOffsetLeftResident(double[] values)
        {
            var distance = values[0];
            var radius = values[1];
            var startAngle = values[2];
            var endAngle = values[3];
            var centre = new Point3d(0, 0, 0);

            var arc = new Arc(centre, radius, startAngle, endAngle);
            var offset = arc.CreateOffset(SidesOfCentre.Left, distance) as Arc;

            return offset == null ? null : new[] { offset.Radius, offset.StartAngle, offset.EndAngle };
        }

        public double[] VerifyCurveExtensionArcOffsetRightResident(double[] values)
        {
            var distance = values[0];
            var radius = values[1];
            var startAngle = values[2];
            var endAngle = values[3];
            var centre = new Point3d(0, 0, 0);

            var arc = new Arc(centre, radius, startAngle, endAngle);
            var offset = arc.CreateOffset(SidesOfCentre.Right, distance) as Arc;

            return offset == null ? null : new[] { offset.Radius, offset.StartAngle, offset.EndAngle };
        }

        public bool VerifyCurveExtensionArcOffsetLeftValidResident(double[] values)
        {
            var distance = values[0];
            var radius = values[1];
            var startAngle = values[2];
            var endAngle = values[3];
            var centre = new Point3d(0, 0, 0);

            var arc = new Arc(centre, radius, startAngle, endAngle);
            var offset = arc.CreateOffset(SidesOfCentre.Left, distance) as Arc;

            return offset != null;
        }

        public bool VerifyCurveExtensionArcOffsetRightValidResident(double[] values)
        {
            var distance = values[0];
            var radius = values[1];
            var startAngle = values[2];
            var endAngle = values[3];
            var centre = new Point3d(0, 0, 0);

            var arc = new Arc(centre, radius, startAngle, endAngle);
            var offset = arc.CreateOffset(SidesOfCentre.Right, distance) as Arc;

            return offset != null;
        }

        [Test]
        public void VerifyCurveExtensionPolyLineOffsetLeftInvalid()
        {
            var result = RunTest<bool>(nameof(VerifyCurveExtensionPolyLineOffsetLeftInvalidResident));

            Assert.IsTrue(result, "Offset should be invalid");
        }

        [Test]
        public void VerifyCurveExtensionPolyLineOffsetRightInvalid()
        {
            var result = RunTest<bool>(nameof(VerifyCurveExtensionPolyLineOffsetRightInvalidResident));

            Assert.IsTrue(result, "Offset should be invalid");
        }

        public bool VerifyCurveExtensionPolyLineOffsetLeftInvalidResident()
        {
            try
            {
                var acPoly = new Polyline();

                acPoly.AddVertexAt(0, new Point2d(2, 4), 0, 0, 0);
                acPoly.AddVertexAt(1, new Point2d(4, 2), 0, 0, 0);
                acPoly.AddVertexAt(2, new Point2d(6, 4), 0, 0, 0);

                acPoly.CreateOffset(SidesOfCentre.Left, 2);

                return false;
            }
            catch (ArgumentOutOfRangeException)
            {
                return true;
            }
        }

        public bool VerifyCurveExtensionPolyLineOffsetRightInvalidResident()
        {
            try
            {
                var acPoly = new Polyline();

                acPoly.AddVertexAt(0, new Point2d(2, 4), 0, 0, 0);
                acPoly.AddVertexAt(1, new Point2d(4, 2), 0, 0, 0);
                acPoly.AddVertexAt(2, new Point2d(6, 4), 0, 0, 0);

                acPoly.CreateOffset(SidesOfCentre.Right, 2);

                return false;
            }
            catch (ArgumentOutOfRangeException)
            {
                return true;
            }
        }
    }
}
