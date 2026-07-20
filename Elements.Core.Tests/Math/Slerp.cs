using Microsoft.VisualStudio.TestTools.UnitTesting;
using Elements.Core; using Elements.Data;
using System;

namespace Elements.Core.Tests
{
    [TestClass]
    public class Slerp
    {
        [TestMethod]
        public void TestStability()
        {
            // Sample values from "flamingo leg" issue
            var from = new floatQ(-0.3079502f, -0.6658229f, -0.6510023f, 0.1950482f);
            var to = new floatQ(-0.3079502f, -0.6658229f, -0.6510023f, 0.1950482f);
            var delta = 0.070058f;

            var lerped = MathX.Slerp(from, to, delta);

            Assert.IsTrue(lerped.IsValid);
        }
    }
}
