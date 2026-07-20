using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Elements.Core.Tests
{
    [TestClass]
    public class Division
    {
        (int, int, bool)[] intTests =
        {
            (0, 0, false),
            (1, 0, false),
            (1, 1, true),
            (42, 2, true),
            (int.MinValue, -1, false),
        };

        (long, long, bool)[] longTests =
        {
            (0, 0, false),
            (1, 0, false),
            (1, 1, true),
            (42, 2, true),
            (long.MinValue, -1, false),
        };

        (int2, int, bool)[] int2Tests =
        {
            (new int2(0, 0), 0, false),
            (new int2(1, 1), 0, false),
            (new int2(1, 1), 1, true),
            (new int2(42, 42), 2, true),
            (new int2(int.MinValue, int.MinValue), -1, false),
        };

        [TestMethod]
        public void TestDivision()
        {
            foreach ((int,int, bool) t in intTests) {
                Assert.AreEqual(t.Item3, MathX.CanDivide(t.Item1, t.Item2), $"A division of ${t.Item1} and ${t.Item2} should be {possible(t.Item3)}");
            }

            foreach ((long, long, bool) t in longTests)
            {
                Assert.AreEqual(t.Item3, MathX.CanDivide(t.Item1, t.Item2), $"A division of ${t.Item1} and ${t.Item2} should be {possible(t.Item3)}");
            }

            foreach ((int2, int, bool) t in int2Tests)
            {
                Assert.AreEqual(t.Item3, MathX.CanDivide(t.Item1, t.Item2), $"A division of ${t.Item1} and ${t.Item2} should be {possible(t.Item3)}");
            }
        }

        private string possible(bool b)
        {
            return b ? "possible" : "not possible";
        }
    }
}
