using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Elements.Core.Tests
{
    [TestClass]
    public class RingBufferLinearizationTests
    {
        // DataTestRow cannot be used with color directly so we parse it.
        [TestMethod]
        public void TestRingBufferLinearization()
        {
            Span<int> buffer = stackalloc int[16];

            for(int count = 1; count <= buffer.Length; count++)
                for(int start = 1; start < buffer.Length; start++)
                {
                    SetBufferData(buffer, start, count);

                    var startBuffer = string.Join(", ", buffer.ToArray());

                    buffer.LinearizeRingBuffer(start, count);

                    var linearizedBuffer = string.Join(", ", buffer.ToArray());

                    Assert.IsTrue(IsRingBufferLinear(buffer, count), $"Start: {start}, Count: {count}\n" +
                        $"StarBuffer: {startBuffer}\n" +
                        $"LinearizedBuffer: {linearizedBuffer}");
                }
        }

        static void SetBufferData(Span<int> buffer, int start, int count)
        {
            buffer.Fill(-1);

            for (int i = 0; i < count; i++)
                buffer[(start + i) % buffer.Length] = i;
        }

        static bool IsRingBufferLinear(Span<int> buffer, int count)
        {
            for (int i = 0; i < count; i++)
                if (buffer[i] != i)
                    return false;

            return true;
        }
    }
}
