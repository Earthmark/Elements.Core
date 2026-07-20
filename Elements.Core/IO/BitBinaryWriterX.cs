using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Elements.Core
{
    public class BitBinaryWriterX : BinaryWriterX
    {
        BitWriterStream bitStream;

        public override Stream TargetStream
        {
            get => base.TargetStream;
            set
            {
                var newBitStream = value as BitWriterStream;

                if (newBitStream == null && value != null)
                    throw new Exception("Stream must be BitWriterStream");

                bitStream = newBitStream;
                base.TargetStream = newBitStream;
            }
        }

        public BitBinaryWriterX() : base()
        {
        }

        public BitBinaryWriterX(BitWriterStream writerStream) 
            : base(writerStream)
        {
            this.bitStream = writerStream;
        }

        public void WriteBit(bool bit)
        {
            bitStream.Write((byte)(bit ? 1 : 0), 1);
        }

        public void WriteBits(byte data, int bits)
        {
            bitStream.Write(data, bits);
        }

        public void WriteBits(ulong data, int bits)
        {
            bitStream.Write(data, bits);
        }

        public void WriteBits(uint2 data, int bits)
        {
            bitStream.Write(data.x, bits);
            bitStream.Write(data.y, bits);
        }

        public void WriteBits(uint3 data, int bits)
        {
            bitStream.Write(data.x, bits);
            bitStream.Write(data.y, bits);
            bitStream.Write(data.z, bits);
        }

        public void WriteBits(uint4 data, int bits)
        {
            bitStream.Write(data.x, bits);
            bitStream.Write(data.y, bits);
            bitStream.Write(data.z, bits);
            bitStream.Write(data.w, bits);
        }

        public override void Flush()
        {
            base.Flush();
            bitStream.Flush();
        }

        public override void Close()
        {
            base.Close();
            bitStream.Close();
        }
    }
}
