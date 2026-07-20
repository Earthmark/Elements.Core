using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Elements.Core.Tests.Math
{
    [TestClass]
    public class Easing
    {
        [TestMethod]
        public void TestEasingNaNFloat()
        {
            Assert.IsTrue(float.IsNaN(MathX.EaseInSineFloat(float.NaN, float.NaN, float.NaN, float.NaN)));
            Assert.IsTrue(float.IsNaN(MathX.EaseOutSineFloat(float.NaN, float.NaN, float.NaN, float.NaN)));
            Assert.IsTrue(float.IsNaN(MathX.EaseInOutSineFloat(float.NaN, float.NaN, float.NaN, float.NaN)));

            Assert.IsTrue(float.IsNaN(MathX.EaseInQuadraticFloat(float.NaN, float.NaN, float.NaN, float.NaN)));
            Assert.IsTrue(float.IsNaN(MathX.EaseOutQuadraticFloat(float.NaN, float.NaN, float.NaN, float.NaN)));
            Assert.IsTrue(float.IsNaN(MathX.EaseInOutQuadraticFloat(float.NaN, float.NaN, float.NaN, float.NaN)));

            Assert.IsTrue(float.IsNaN(MathX.EaseInCubicFloat(float.NaN, float.NaN, float.NaN, float.NaN)));
            Assert.IsTrue(float.IsNaN(MathX.EaseOutCubicFloat(float.NaN, float.NaN, float.NaN, float.NaN)));
            Assert.IsTrue(float.IsNaN(MathX.EaseInOutCubicFloat(float.NaN, float.NaN, float.NaN, float.NaN)));

            Assert.IsTrue(float.IsNaN(MathX.EaseInQuarticFloat(float.NaN, float.NaN, float.NaN, float.NaN)));
            Assert.IsTrue(float.IsNaN(MathX.EaseOutQuarticFloat(float.NaN, float.NaN, float.NaN, float.NaN)));
            Assert.IsTrue(float.IsNaN(MathX.EaseInOutQuarticFloat(float.NaN, float.NaN, float.NaN, float.NaN)));

            Assert.IsTrue(float.IsNaN(MathX.EaseInQuinticFloat(float.NaN, float.NaN, float.NaN, float.NaN)));
            Assert.IsTrue(float.IsNaN(MathX.EaseOutQuinticFloat(float.NaN, float.NaN, float.NaN, float.NaN)));
            Assert.IsTrue(float.IsNaN(MathX.EaseInOutQuinticFloat(float.NaN, float.NaN, float.NaN, float.NaN)));

            Assert.IsTrue(float.IsNaN(MathX.EaseInExponentialFloat(float.NaN, float.NaN, float.NaN, float.NaN)));
            Assert.IsTrue(float.IsNaN(MathX.EaseOutExponentialFloat(float.NaN, float.NaN, float.NaN, float.NaN)));
            Assert.IsTrue(float.IsNaN(MathX.EaseInOutExponentialFloat(float.NaN, float.NaN, float.NaN, float.NaN)));

            Assert.IsTrue(float.IsNaN(MathX.EaseInCircularFloat(float.NaN, float.NaN, float.NaN, float.NaN)));
            Assert.IsTrue(float.IsNaN(MathX.EaseOutCircularFloat(float.NaN, float.NaN, float.NaN, float.NaN)));
            Assert.IsTrue(float.IsNaN(MathX.EaseInOutCircularFloat(float.NaN, float.NaN, float.NaN, float.NaN)));

            Assert.IsTrue(float.IsNaN(MathX.EaseInReboundFloat(float.NaN, float.NaN, float.NaN, float.NaN, 0f)));
            Assert.IsTrue(float.IsNaN(MathX.EaseOutReboundFloat(float.NaN, float.NaN, float.NaN, float.NaN, 0f)));
            Assert.IsTrue(float.IsNaN(MathX.EaseInOutReboundFloat(float.NaN, float.NaN, float.NaN, float.NaN, 0f)));

            Assert.IsTrue(float.IsNaN(MathX.EaseInReboundFloat(float.NaN, float.NaN, float.NaN, float.NaN, 1f)));
            Assert.IsTrue(float.IsNaN(MathX.EaseOutReboundFloat(float.NaN, float.NaN, float.NaN, float.NaN, 1f)));
            Assert.IsTrue(float.IsNaN(MathX.EaseInOutReboundFloat(float.NaN, float.NaN, float.NaN, float.NaN, 1f)));

            Assert.IsTrue(float.IsNaN(MathX.EaseInElasticFloat(float.NaN, float.NaN, float.NaN, float.NaN, 0f, 0f)));
            Assert.IsTrue(float.IsNaN(MathX.EaseOutElasticFloat(float.NaN, float.NaN, float.NaN, float.NaN, 0f, 0f)));
            Assert.IsTrue(float.IsNaN(MathX.EaseInOutElasticFloat(float.NaN, float.NaN, float.NaN, float.NaN, 0f, 0f)));

            Assert.IsTrue(float.IsNaN(MathX.EaseInElasticFloat(float.NaN, float.NaN, float.NaN, float.NaN, 1f, 1f)));
            Assert.IsTrue(float.IsNaN(MathX.EaseOutElasticFloat(float.NaN, float.NaN, float.NaN, float.NaN, 1f, 1f)));
            Assert.IsTrue(float.IsNaN(MathX.EaseInOutElasticFloat(float.NaN, float.NaN, float.NaN, float.NaN, 1f, 1f)));

            Assert.IsTrue(float.IsNaN(MathX.EaseInBounceFloat(float.NaN, float.NaN, float.NaN, float.NaN)));
            Assert.IsTrue(float.IsNaN(MathX.EaseOutBounceFloat(float.NaN, float.NaN, float.NaN, float.NaN)));
            Assert.IsTrue(float.IsNaN(MathX.EaseInOutBounceFloat(float.NaN, float.NaN, float.NaN, float.NaN)));
        }

        [TestMethod]
        public void TestEasingNaNDouble()
        {
            Assert.IsTrue(double.IsNaN(MathX.EaseInSineDouble(double.NaN, double.NaN, double.NaN, double.NaN)));
            Assert.IsTrue(double.IsNaN(MathX.EaseOutSineDouble(double.NaN, double.NaN, double.NaN, double.NaN)));
            Assert.IsTrue(double.IsNaN(MathX.EaseInOutSineDouble(double.NaN, double.NaN, double.NaN, double.NaN)));

            Assert.IsTrue(double.IsNaN(MathX.EaseInQuadraticDouble(double.NaN, double.NaN, double.NaN, double.NaN)));
            Assert.IsTrue(double.IsNaN(MathX.EaseOutQuadraticDouble(double.NaN, double.NaN, double.NaN, double.NaN)));
            Assert.IsTrue(double.IsNaN(MathX.EaseInOutQuadraticDouble(double.NaN, double.NaN, double.NaN, double.NaN)));

            Assert.IsTrue(double.IsNaN(MathX.EaseInCubicDouble(double.NaN, double.NaN, double.NaN, double.NaN)));
            Assert.IsTrue(double.IsNaN(MathX.EaseOutCubicDouble(double.NaN, double.NaN, double.NaN, double.NaN)));
            Assert.IsTrue(double.IsNaN(MathX.EaseInOutCubicDouble(double.NaN, double.NaN, double.NaN, double.NaN)));

            Assert.IsTrue(double.IsNaN(MathX.EaseInQuarticDouble(double.NaN, double.NaN, double.NaN, double.NaN)));
            Assert.IsTrue(double.IsNaN(MathX.EaseOutQuarticDouble(double.NaN, double.NaN, double.NaN, double.NaN)));
            Assert.IsTrue(double.IsNaN(MathX.EaseInOutQuarticDouble(double.NaN, double.NaN, double.NaN, double.NaN)));

            Assert.IsTrue(double.IsNaN(MathX.EaseInQuinticDouble(double.NaN, double.NaN, double.NaN, double.NaN)));
            Assert.IsTrue(double.IsNaN(MathX.EaseOutQuinticDouble(double.NaN, double.NaN, double.NaN, double.NaN)));
            Assert.IsTrue(double.IsNaN(MathX.EaseInOutQuinticDouble(double.NaN, double.NaN, double.NaN, double.NaN)));

            Assert.IsTrue(double.IsNaN(MathX.EaseInExponentialDouble(double.NaN, double.NaN, double.NaN, double.NaN)));
            Assert.IsTrue(double.IsNaN(MathX.EaseOutExponentialDouble(double.NaN, double.NaN, double.NaN, double.NaN)));
            Assert.IsTrue(double.IsNaN(MathX.EaseInOutExponentialDouble(double.NaN, double.NaN, double.NaN, double.NaN)));

            Assert.IsTrue(double.IsNaN(MathX.EaseInCircularDouble(double.NaN, double.NaN, double.NaN, double.NaN)));
            Assert.IsTrue(double.IsNaN(MathX.EaseOutCircularDouble(double.NaN, double.NaN, double.NaN, double.NaN)));
            Assert.IsTrue(double.IsNaN(MathX.EaseInOutCircularDouble(double.NaN, double.NaN, double.NaN, double.NaN)));

            Assert.IsTrue(double.IsNaN(MathX.EaseInReboundDouble(double.NaN, double.NaN, double.NaN, double.NaN, 0f)));
            Assert.IsTrue(double.IsNaN(MathX.EaseOutReboundDouble(double.NaN, double.NaN, double.NaN, double.NaN, 0f)));
            Assert.IsTrue(double.IsNaN(MathX.EaseInOutReboundDouble(double.NaN, double.NaN, double.NaN, double.NaN, 0f)));

            Assert.IsTrue(double.IsNaN(MathX.EaseInReboundDouble(double.NaN, double.NaN, double.NaN, double.NaN, 1f)));
            Assert.IsTrue(double.IsNaN(MathX.EaseOutReboundDouble(double.NaN, double.NaN, double.NaN, double.NaN, 1f)));
            Assert.IsTrue(double.IsNaN(MathX.EaseInOutReboundDouble(double.NaN, double.NaN, double.NaN, double.NaN, 1f)));

            Assert.IsTrue(double.IsNaN(MathX.EaseInElasticDouble(double.NaN, double.NaN, double.NaN, double.NaN, 0f, 0f)));
            Assert.IsTrue(double.IsNaN(MathX.EaseOutElasticDouble(double.NaN, double.NaN, double.NaN, double.NaN, 0f, 0f)));
            Assert.IsTrue(double.IsNaN(MathX.EaseInOutElasticDouble(double.NaN, double.NaN, double.NaN, double.NaN, 0f, 0f)));

            Assert.IsTrue(double.IsNaN(MathX.EaseInElasticDouble(double.NaN, double.NaN, double.NaN, double.NaN, 1f, 1f)));
            Assert.IsTrue(double.IsNaN(MathX.EaseOutElasticDouble(double.NaN, double.NaN, double.NaN, double.NaN, 1f, 1f)));
            Assert.IsTrue(double.IsNaN(MathX.EaseInOutElasticDouble(double.NaN, double.NaN, double.NaN, double.NaN, 1f, 1f)));

            Assert.IsTrue(double.IsNaN(MathX.EaseInBounceDouble(double.NaN, double.NaN, double.NaN, double.NaN)));
            Assert.IsTrue(double.IsNaN(MathX.EaseOutBounceDouble(double.NaN, double.NaN, double.NaN, double.NaN)));
            Assert.IsTrue(double.IsNaN(MathX.EaseInOutBounceDouble(double.NaN, double.NaN, double.NaN, double.NaN)));
        }
    }
}
