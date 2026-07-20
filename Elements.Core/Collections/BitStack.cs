using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Elements.Core
{
    public class BitStack
    {
        List<byte> data;

        public int Count { get; private set; }

        public BitStack()
        {
            data = new List<byte>();
        }

        public BitStack(int capacity)
        {
            data = new List<byte>(capacity / 8 + 1);
        }

        public void Push(bool state)
        {
            // add empty byte if it's full
            if (data.Count < (Count / 8 + 1))
                data.Add(0);

            data[data.Count - 1] |= (byte)((state?1:0) << (Count % 8));
            Count++;
        }

        public byte[] ToArray()
        {
            return data.ToArray();
        }

        public bool Pop()
        {
            if (Count == 0)
                throw new Exception("BitStack is empty!");

            var state = Peek();

            Count--;    // decrement

            // remove unecessary elements
            if (data.Count > (Count / 8 + 1))
                data.RemoveAt(data.Count - 1);

            return state;
        }

        public bool Peek()
        {
            int top = Count - 1;

            var segment = data[Count / 8];  // read appropriate segment
            segment &= (byte)(1 << (Count % 8)); // mask all other bits

            return segment != 0;
        }

        public void Clear()
        {
            data.Clear();
            Count = 0;
        }

        // Encoding

        public void Encode(BinaryWriter writer)
        {
            writer.Write7BitEncoded((ulong)Count);
            foreach (var b in data)
                writer.Write(b);
        }

        public void Decode(BinaryReader reader)
        {
            Clear();

            Count = (int)reader.Read7BitEncoded();
            data.Capacity = Count / 8 + 1;

            for (int i = 0; i < (Count / 8 + 1); i++)
                data.Add(reader.ReadByte());
        }
    }

    public static class BitStackExtensions
    {
        public static void Write(this BinaryWriter writer, BitStack stack)
        {
            stack.Encode(writer);
        }

        public static BitStack ReadBitStack(this BinaryReader reader)
        {
            BitStack stack = new BitStack();
            stack.Decode(reader);

            return stack;
        }
    }
}
