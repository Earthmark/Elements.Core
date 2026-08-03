using Renderite.Shared;

namespace Elements.Core.Tests
{
    [TestClass]
    public class ColorProfileTest
    {
        [TestMethod]
        public void Linear_Linear_RoundTrip()
        {
            RoundTrip(ColorProfile.Linear, ColorProfile.Linear);
        }

        [TestMethod]
        public void sRGB_sRGB_RoundTrip()
        {
            RoundTrip(ColorProfile.sRGB, ColorProfile.sRGB);
        }

        [TestMethod]
        public void Linear_sRGB_RoundTrip()
        {
            RoundTrip(ColorProfile.Linear, ColorProfile.sRGB);
        }

        [TestMethod]
        public void sRGB_Linear_RoundTrip()
        {
            RoundTrip(ColorProfile.sRGB, ColorProfile.Linear);
        }

        void RoundTrip(ColorProfile source, ColorProfile target)
        {
            for (float r = 0f; r < 1f; r += 0.01f)
                for (float g = 0f; g < 1f; g += 0.01f)
                    for (float b = 0f; b < 1f; b += 0.01f)
                    {
                        var original = new colorX(r, g, b, 1f, source);
                        var srgb = original.ConvertProfile(target);
                        var roundtrip = srgb.ConvertProfile(source);

                        for (int i = 0; i < 3; i++)
                            Assert.IsTrue(MathX.Approximately(original[i], roundtrip[i]), $"Converting {source}->{target}->{source} results in a different color.\n" +
                                $"Original: {original}, After round trip: {roundtrip}");
                    }
        }
    }
}
