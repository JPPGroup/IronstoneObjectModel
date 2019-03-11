using System;
using System.Reflection;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Jpp.AcTestFramework;
using Jpp.Ironstone.Highways.ObjectModel.Extensions;
using NUnit.Framework;

namespace Jpp.Ironstone.Highways.ObjectModel.Tests.Extensions
{
    [TestFixture]
    public class ArcExtensionTests : BaseNUnitTestFixture
    {
        public ArcExtensionTests() : base(Assembly.GetExecutingAssembly(), typeof(ArcExtensionTests)) { }        

        [Test]
        public void VerifyAntiClockwiseArc()
        {
            var result = RunTest<bool>("VerifyAntiClockwiseArcResident");
            Assert.IsFalse(result, "Arc is not anti clockwise.");
        }

        public bool VerifyAntiClockwiseArcResident()
        {
            const int radius = 6;
            const double startAngle = 0;
            const double endAngle = Math.PI;

            var centre = new Point3d(0, 0, 0);

            var arc = new Arc(centre, radius, startAngle, endAngle);

            return arc.Clockwise();
        }

        [Test]
        public void VerifyAntiClockwiseArcReverseCurve()
        {
            var result = RunTest<bool>("VerifyAntiClockwiseArcReverseCurveResident");
            Assert.IsTrue(result, "Arc is not clockwise.");
        }

        public bool VerifyAntiClockwiseArcReverseCurveResident()
        {
            const int radius = 6;
            const double startAngle = 0;
            const double endAngle = Math.PI;

            var centre = new Point3d(0, 0, 0);

            var arc = new Arc(centre, radius, startAngle, endAngle);
            arc.ReverseCurve();

            return arc.Clockwise();
        }

        [Test]
        public void VerifyClockwiseArc()
        {
            var result = RunTest<bool>("VerifyClockwiseArcResident");
            Assert.IsTrue(result, "Arc is not clockwise.");
        }

        public bool VerifyClockwiseArcResident()
        {
            const int radius = 6;
            const double startAngle = Math.PI;
            const double endAngle = 0;

            var centre = new Point3d(0, 0, 0);

            var arc = new Arc(centre, radius, startAngle, endAngle);

            return arc.Clockwise();
        }

        [Test]
        public void VerifyClockwiseArcReverseCurve()
        {
            var result = RunTest<bool>("VerifyClockwiseArcReverseCurveResident");
            Assert.IsFalse(result, "Arc is not anti-clockwise.");
        }

        public bool VerifyClockwiseArcReverseCurveResident()
        {
            const int radius = 6;
            const double startAngle = Math.PI;
            const double endAngle = 0;

            var centre = new Point3d(0, 0, 0);

            var arc = new Arc(centre, radius, startAngle, endAngle);
            arc.ReverseCurve();

            return arc.Clockwise();
        }
    }
}
