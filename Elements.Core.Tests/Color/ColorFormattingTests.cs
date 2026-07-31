using System.Globalization;

namespace Elements.Core.Tests
{
    [TestClass]
    public class ColorFormattingTests
    {
        // DataTestRow cannot be used with color directly so we parse it.
        [DataRow("[1;0;0;0]", "#F00")]
        [DataRow("[0;1;0;0]", "#0F0")]
        [DataRow("[0;0;1;0]", "#00F")]
        [DataRow("[1;1;1;0]", "#FFF")]
        [DataRow("[0;0;0;0]", "#000")]
        [DataRow("[0.5;.5;0.5;0.5]", "#888")]
        [DataRow("[0.5;.5;0.5;0.5]", "#8888", true)]
        [DataRow("[0;0;0;1]", "#000F", true)]
        [DataRow("[0;0;0;0]", "#0000", true)]
        [DataRow("[0;0;0;1]", "#000", false)]
        [DataTestMethod]
        public void ColorShortHexTests(string strFloat4, string expected, bool alpha = false)
        {
            if (!color.TryParse(strFloat4, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
                Assert.Fail("Invalid color");

            Assert.AreEqual(expected, result.ToShortHexString(alpha), $"{strFloat4} should result in {expected}");

            if (!colorX.TryParse(strFloat4, NumberStyles.Float, CultureInfo.InvariantCulture, out var resultX))
                Assert.Fail("Invalid color");

            Assert.AreEqual(expected, resultX.ToShortHexString(alpha), $"{strFloat4} should result in {expected}");

        }
    }
}
