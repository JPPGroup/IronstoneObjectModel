using System;
using Jpp.Ironstone.Highways.ObjectModel.Helpers;
using NUnit.Framework;

namespace Jpp.Ironstone.Highways.ObjectModel.Tests
{
    [TestFixture]
    public class RadiansHelperTests
    {
        [Test]
        public void Verify360Degrees()
        {
            const double expected = 2 * Math.PI;
            const double result = RadiansHelper.DEGREES_360;
            Assert.AreEqual(expected, result, "Unexpected value for 360 degrees.");
        }

        [Test]
        public void Verify180Degrees()
        {
            const double expected = Math.PI;
            const double result = RadiansHelper.DEGREES_180;
            Assert.AreEqual(expected, result, "Unexpected value for 180 degrees.");
        }

        [Test]
        public void Verify90Degrees()
        {
            const double expected = Math.PI / 2;
            const double result = RadiansHelper.DEGREES_90;
            Assert.AreEqual(expected, result, "Unexpected value for 90 degrees.");
        }

        [Test]
        public void VerifyTolerance()
        {
            const double expected = Math.PI / 360;
            const double result = RadiansHelper.ANGLE_TOLERANCE;
            Assert.AreEqual(expected, result, "Unexpected value for angle tolerance.");
        }


        [Test]
        public void VerifyAngleForRightSideGreaterThan90()
        {
            const double expected = Math.PI / 2;
            const double initialAngle = Math.PI; 

            var result = RadiansHelper.AngleForRightSide(initialAngle);
            Assert.AreEqual(expected, result, "Unexpected value for RHS angle.");
        }

        [Test]
        public void VerifyAngleForRightSideLessThan90()
        {
            const double expected = Math.PI * 1.5; 
            const double initialAngle = 0; 

            var result = RadiansHelper.AngleForRightSide(initialAngle);
            Assert.AreEqual(expected, result, "Unexpected value for RHS angle.");
        }

        [Test]
        public void VerifyAngleForLeftSideGreaterThan270()
        {
            const double expected = 0;
            const double initialAngle = Math.PI * 1.5; 

            var result = RadiansHelper.AngleForLeftSide(initialAngle);
            Assert.AreEqual(expected, result, "Unexpected value for LHS angle.");
        }

        [Test]
        public void VerifyAngleForLeftSideLessThan270()
        {
            const double expected = Math.PI;
            const double initialAngle = Math.PI / 2;

            var result = RadiansHelper.AngleForLeftSide(initialAngle);
            Assert.AreEqual(expected, result, "Unexpected value for LHS angle.");
        }


        [Test]
        public void VerifyAnglesAreEqualAddTolerance()
        {
            const double tolerance = RadiansHelper.ANGLE_TOLERANCE / 2;
            const double angle1 = Math.PI;
            const double angle2 = angle1 + tolerance;

            var result = RadiansHelper.AnglesAreEqual(angle1, angle2);
            Assert.True(result, "Angles should be equal.");
        }

        [Test]
        public void VerifyAnglesAreNotEqualAddTolerance()
        {
            const double tolerance = RadiansHelper.ANGLE_TOLERANCE * 1.5;
            const double angle1 = Math.PI;
            const double angle2 = angle1 + tolerance;

            var result = RadiansHelper.AnglesAreEqual(angle1, angle2);
            Assert.False(result, "Angles should not be equal.");
        }


        [Test]
        public void VerifyAnglesAreEqualMinusTolerance()
        {
            const double tolerance = RadiansHelper.ANGLE_TOLERANCE / 2;
            const double angle1 = Math.PI;
            const double angle2 = angle1 - tolerance;

            var result = RadiansHelper.AnglesAreEqual(angle1, angle2);
            Assert.True(result, "Angles should be equal.");
        }

        [Test]
        public void VerifyAnglesAreNotEqualMinusTolerance()
        {
            const double tolerance = RadiansHelper.ANGLE_TOLERANCE * 1.5;
            const double angle1 = Math.PI;
            const double angle2 = angle1 - tolerance;

            var result = RadiansHelper.AnglesAreEqual(angle1, angle2);
            Assert.False(result, "Angles should not be equal.");
        }
    }
}
