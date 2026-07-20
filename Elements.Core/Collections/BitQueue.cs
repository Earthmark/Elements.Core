using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Collections;

namespace Elements.Core
{
    public class BitQueue
    {
        List<byte> data;

        public int Count { get; private set; }

        int readOffset;

        int SegmentIndex(int bitIndex)
        {
            return bitIndex / 8;
        }

        public BitQueue()
        {
            data = new List<byte>();
        }

        public BitQueue(int capacity)
        {
            data = new List<byte>(capacity / 8 + 1);
        }

        public void Enqueue(bool state)
        {
            int index = Count + readOffset;

            // add empty byte if it's full
            if (data.Count == SegmentIndex(index))
                data.Add(0);

            data[SegmentIndex(index)] |= (byte)((state ? 1 : 0) << (index % 8));

            Count++;
        }

        public bool Dequeue()
        {
            if (Count == 0)
                throw new Exception("BitQueue is empty!");

            var state = Peek();

            Count--;

            // increment the offset
            readOffset++;
            if(readOffset == 8)
            {
                // remove the first one since it's completely read out
                readOffset = 0;
                data.RemoveAt(0);
            }

            return state;
        }

        public bool Peek()
        {
            var segment = data[0];
            segment &= (byte)(1 << (readOffset));

            return segment != 0;
        }

        public void Clear()
        {
            data.Clear();
            Count = 0;
            readOffset = 0;
        }

        // Encoding

        public void Encode(BinaryWriter writer)
        {
            writer.Write7BitEncoded((ulong)Count);
            writer.Write((byte)readOffset);

            foreach (var b in data)
                writer.Write(b);
        }

        public void Decode(BinaryReader reader)
        {
            Clear();

            Count = (int)reader.Read7BitEncoded();
            readOffset = reader.ReadByte();

            int index = Count + readOffset - 1; // need to subtract 1, to compensate for the last enqueue
            data.Capacity = SegmentIndex(index)+1;

            if (Count > 0)
            {
                for (int i = 0; i < data.Capacity; i++)
                    data.Add(reader.ReadByte());
            }
        }

        public override string ToString()
        {
            string str = "";
            foreach (var b in data)
                str += b.ToString("X2");

            return str;                
        }
    }

    public static class BitQueueExtensions
    {
        public static void Write(this BinaryWriter writer, BitQueue queue)
        {
            queue.Encode(writer);
        }

        public static BitQueue ReadBitQueue(this BinaryReader reader)
        {
            BitQueue queue = new BitQueue();
            queue.Decode(reader);

            return queue;
        }
    }
}
