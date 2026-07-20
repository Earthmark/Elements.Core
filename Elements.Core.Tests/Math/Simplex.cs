using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Elements.Core.Tests
{
    /// <summary>
    /// Summary description for Simplex
    /// </summary>
    [TestClass]
    public class Simplex
    {

        // Positive Values
        [DataRow(0f,1f, -0.6789227f)]
        [DataRow(1f, 0f, -0.3678161f)]
        [DataRow(1f, 1f, 0.2796498f)]

        //Negative values
        [DataRow(-1f, -1f, 0.7687789f)]
        [DataRow(-1f, 0f, 0.1131009f)]
        [DataRow(0f, -1f, 0.6789228f)]
        [DataTestMethod]
        public void TestSimplex2D(float x, float y, float expected)
        {
            var simplex = SimplexNoise.Noise.Generate(x, y);
            // We use MathX here because it more acurate reflects what our users would see in Resonite
            // Assert.AreEqual does have an epsilion value but its better to use ours
            var result = MathX.Approximately(expected, simplex);

            Assert.IsTrue(result, $"Simplex 2D Noise for ({x},{y}) should be {expected} and is {simplex}");
        }

        // Positive Values
        [DataRow(0.1f, 0.3030851f)]
        [DataRow(0.5f, 0.4374317f)]
        [DataRow(0.8f, -0.02463769f)]
        [DataRow(0f, 0f)]
        [DataRow(1f, 0f)]

        //Negative values
        [DataRow(-1f, 0f)]
        [DataRow(-0.2f, -0.5102491f)]
        [DataRow(-0.5f, -0.1874707f)]
        [DataRow(-0.8f, 0.2930312f)]

        [DataTestMethod]
        public void TestSimplex1D(float x, float expected)
        {
            var simplex = SimplexNoise.Noise.Generate(x);
            // We use MathX here because it more accurate reflects what our users would see in Resonite
            // Assert.AreEqual does have an epsilon value but its better to use ours
            var result = MathX.Approximately(expected, simplex);

            Assert.IsTrue(result, $"Simplex 2D Noise for ({x}) should be {expected} and is {simplex}");
        }
    }
}
