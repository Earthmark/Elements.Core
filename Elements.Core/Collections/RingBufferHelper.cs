using System;
using System.Collections.Generic;
using System.Text;

namespace Elements.Core
{
    public static class RingBufferHelper
    {
        public static void LinearizeRingBuffer<T>(this Span<T> ringBuffer, int start, int count)
        {
            if (start == 0)
                throw new InvalidOperationException("start at 0 means the ring buffer is already linear and this method should not be called");

            if (count == 0)
                throw new InvalidOperationException("The buffer is empty");

            if (start < 0)
                throw new ArgumentException("start must be non-negative");

            if (count < 0)
                throw new ArgumentException("count must be non-negative");

            if (count > ringBuffer.Length)
                throw new ArgumentException("count cannot be larger than the ring buffer capacity");

            while (count > 0 && start > 0)
            {
                // Determine how many elements we can move around in this pass
                // We are limited by
                // - How much space is available before the current start (the cursor)
                // - How many elements total we want to move
                // - How many elements there are until the current linear end of the buffer before we wrap the cursor around

                int linearRemainderCount = ringBuffer.Length - start;
                int countToProcess = MathX.Min(count, linearRemainderCount, start);

                for (int i = 0; i < countToProcess; i++)
                {
                    var sourcePos = start + i;
                    var targetPos = i;

                    // Swap the data at the two positions
                    (ringBuffer[sourcePos], ringBuffer[targetPos]) = (ringBuffer[targetPos], ringBuffer[sourcePos]);
                }

                // The elements got moved, now we update our data accordingly and do the next pass

                // We need to determine where to place the cursor for the next iteration of this
                // If the cursor has reached the end, the new smallest element in the buffer is at the point
                // where we originally started, so we start from there - we just keep the cursor position and
                // adjust it for the new offset
                var reachedEnd = countToProcess == linearRemainderCount;

                if(!reachedEnd)
                {
                    // We have not reached the end, which means we need to offset the start cursor
                    // to where it would be and process the remainder of the buffer in the next pass
                    start += countToProcess;
                }
                else
                {
                    // We have reached the end, meaning the smallest element of the buffer is now at the
                    // point we have started processing this section, because all the other smallest elements
                    // have been moved to the front now
                    // We don't actually do anything here, just keep the cursor offset where it is
                }

                // The beginning of the ring buffer is now linear, so we can slice it off and process the remainder!
                ringBuffer = ringBuffer.Slice(countToProcess);

                // Offset the cursor as well to match the new buffer size
                start -= countToProcess;

                // Subtract the processed count as well from the buffer
                count -= countToProcess;
            }
        }
    }
}
