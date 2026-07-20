using Microsoft.VisualStudio.TestTools.UnitTesting;
using Elements.Core; using Elements.Data;
using System;

namespace Elements.Core.Tests.Math
{
    [TestClass]
    public class Factorial
    {
        [TestMethod]
        public void TestFactorialInfinity()
        {
            Assert.AreEqual(MathX.FactorialDouble(171), double.PositiveInfinity);
            Assert.AreEqual(MathX.FactorialFloat(171), float.PositiveInfinity);
        }

        [TestMethod]
        public void TestFactorialInvalidValues()
        {
            Assert.AreEqual(double.PositiveInfinity, MathX.FactorialDouble(int.MaxValue));
            Assert.AreEqual(double.NaN, MathX.FactorialDouble(int.MinValue));

            Assert.AreEqual(float.PositiveInfinity, MathX.FactorialFloat(int.MaxValue));
            Assert.AreEqual(float.NaN, MathX.FactorialFloat(int.MinValue));
        }
    }
}
