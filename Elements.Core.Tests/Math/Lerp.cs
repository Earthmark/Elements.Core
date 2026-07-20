using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Elements.Core.Tests
{
    [TestClass]
    public class Lerp
    {
        [TestMethod]
        public void TestConstantLerpHandlesNaN()
        {
            Assert.IsTrue(float.IsNaN(MathX.ConstantLerp(float.NaN, float.NaN, float.NaN)));

            var f2 = new float2(float.NaN, float.NaN);
            var f3 = new float3(float.NaN, float.NaN, float.NaN);
            var f4 = new float4(float.NaN, float.NaN, float.NaN, float.NaN);
            Assert.IsTrue(MathX.ConstantLerp(f2, f2, float.NaN).IsNaN);
            Assert.IsTrue(MathX.ConstantLerp(f3, f3, float.NaN).IsNaN);
            Assert.IsTrue(MathX.ConstantLerp(f4, f4, float.NaN).IsNaN);

            Assert.IsTrue(double.IsNaN(MathX.ConstantLerp(double.NaN, double.NaN, double.NaN)));

            var d2 = new double2(double.NaN, double.NaN);
            var d3 = new double3(double.NaN, double.NaN, double.NaN);
            var d4 = new double4(double.NaN, double.NaN, double.NaN, double.NaN);
            Assert.IsTrue(MathX.ConstantLerp(d2, d2, double.NaN).IsNaN);
            Assert.IsTrue(MathX.ConstantLerp(d3, d3, double.NaN).IsNaN);
            Assert.IsTrue(MathX.ConstantLerp(d4, d4, double.NaN).IsNaN);

            // Colors can lerp
            Assert.IsTrue(MathX.ConstantLerp(colorX.NaN, colorX.NaN, float.NaN).IsNaN);
            Assert.IsTrue(MathX.ConstantLerp(color.NaN, color.NaN, float.NaN).IsNaN);

        }
    }
}
