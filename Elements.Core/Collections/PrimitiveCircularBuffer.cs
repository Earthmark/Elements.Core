using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Elements.Core
{
    public class PrimitiveCircularBuffer<T>
        where T : struct, IConvertible
    {
        int head;
        int tail;
        int capacity;
        int size;

        static int elementSize;

        public ulong LinearHead { get; private set; }
        public ulong LinearTail { get; private set; }

        public bool CanOverflow { get; set; }
        public bool CanExpand { get; set; }
        public int MaxCapacity { get; set; }

        public int ElementSize { get { return elementSize; } }

        public int Capacity
        {
            get { return capacity; }
            set { SetCapacity(value); }
        }

        public int Size { get { return size; } }
        public int Free { get { return capacity - size; } }

        T[] buffer;

        public PrimitiveCircularBuffer(int capacity)
        {
            if (capacity <= 0)
                throw new Exception("Capacity must be greater than 0");

            this.capacity = capacity;
            buffer = new T[capacity];

            // compute element size (because can't use sizeof)
            if (elementSize == 0)
                elementSize = Buffer.ByteLength(buffer) / buffer.Length;
        }

        public void Put(T[] data, bool allowOverwrite)
        {
            InternalPut(data, 0, data.Length, allowOverwrite);
        }

        public void Put(T[] data, int offset, int count, bool allowOverwrite)
        {
            InternalPut(data, offset * elementSize, count, allowOverwrite);
        }

        public void Put(byte[] data, bool allowOverwrite)
        {
            InternalPut(data, 0, data.Length / elementSize, allowOverwrite);
        }

        public void Put(byte[] data, int offset, int count, bool allowOverwrite)
        {
            InternalPut(data, offset, count, allowOverwrite);
        }

        // offset is in the source granularity
        // count is in the target granularity
        void InternalPut(Array data, int offset, int count, bool overwrite = false)
        {
            if (count > capacity)
            {
                if (CanExpand)
                    SetCapacity(count);
                else if (!overwrite)
                    throw new Exception("Capacity of the buffer is too small!");
            }

            if (count > Free)
            {
                // expand first if it can
                int targetCapacity = capacity;

                if (CanExpand)
                {
                    targetCapacity = capacity + (count - Free);
                    if (MaxCapacity > 0)
                        targetCapacity = Math.Min(targetCapacity, MaxCapacity);
                }

                if (targetCapacity < count)
                {
                    if (overwrite)
                    {
                        int sourceExcess = count - targetCapacity;

                        // shift both linear positions forward to account for skipped source data
                        LinearHead += (ulong)sourceExcess;
                        LinearTail += (ulong)sourceExcess;

                        count = targetCapacity;
                        offset = sourceExcess * elementSize;
                    }
                    else
                        throw new Exception("The maximum capacity if smaller than the amount of data being written");
                }

                // check if  it would overflow with the new capacity
                bool wouldOverflow = count > (targetCapacity - Size);

                if (wouldOverflow && !CanOverflow)
                    throw new Exception("Buffer overflow!");

                // expand the capacity if it's larger
                if (targetCapacity > capacity)
                    SetCapacity(targetCapacity);

                if (wouldOverflow)
                {
                    // shift the tail and free capacity that way
                    int excess = count - Free;
                    tail += excess;
                    LinearTail += (uint)excess;
                    tail %= capacity;
                    size -= excess;
                }
            }

            while (count > 0)
            {
                // find how much space is free on the right of the head
                int rightFree;
                if (head >= tail)
                    rightFree = capacity - head;
                else
                    rightFree = Free; // it's simply equal to free capacity

                // clamp it by the count
                int write = Math.Min(rightFree, count);

                // copy continuous area
                Buffer.BlockCopy(data, offset, buffer, head * elementSize, write * elementSize);

                // shift the variables by the amount written
                count -= write;
                offset += write * elementSize;
                head += write;
                LinearHead += (ulong)write;
                head %= capacity;
                size += write;
            }
        }

        public T[] Get()
        {
            // get all data
            return Get(Size);
        }

        public T[] Get(int count)
        {
            T[] data = new T[count];
            Get(data, 0, count);
            return data;
        }

        public byte[] GetBytes()
        {
            return GetBytes(Size);
        }

        public byte[] GetBytes(int count)
        {
            byte[] data = new byte[elementSize * count];
            Get(data, 0, count);
            return data;
        }

        public int Read(T[] target, int offset, int count, int readOffset = 0)
        {
            return InternalRead(target, offset, count, readOffset, false);
        }

        public int Get(T[] target, int offset, int count)
        {
            return InternalRead(target, offset, count, 0, true);
        }

        public int Get(byte[] target, int offset, int count)
        {
            return InternalRead(target, offset, count, 0, true) * ElementSize;
        }

        public void Trim()
        {
            SetCapacity(Size);
        }

        int InternalRead(Array target, int offset, int count, int readOffset, bool advanceTail)
        {
            count = MathX.Clamp(count, 0, Math.Max(0, size - readOffset));

            int readCount = count;

            int pos = (tail + readOffset) % capacity;

            while (count > 0)
            {
                // find how much data is on the right of the tail
                int rightSize;
                if (pos >= head)
                    rightSize = capacity - pos;
                else
                    rightSize = head - pos;

                // clamp it by the count
                int read = Math.Min(rightSize, count);

                // copy continuous area
                Buffer.BlockCopy(buffer, pos * elementSize, target, offset, elementSize * read);

                // shift the variables by the amount read
                count -= read;
                offset += read * elementSize;
                pos += read;
                pos %= capacity;

                if (advanceTail)
                {
                    tail += read;
                    tail %= capacity;
                    LinearTail += (ulong)read;
                    size -= read;
                }
            }

            return readCount;
        }

        void SetCapacity(int newCapacity)
        {
            if (newCapacity < Size)
                throw new Exception("Cannot set capacity below the size of the data in the buffer");

            // create new buffer
            T[] newBuf = new T[newCapacity];

            int oldSize = Size;

            // copy the data
            Get(newBuf, 0, Size);

            // setup the new buffer
            buffer = newBuf;
            tail = 0;
            head = oldSize;
            capacity = newCapacity;
            size = oldSize; // got set to zero because of the Get operation
        }
    }
}