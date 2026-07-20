using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Elements.Core
{
    public class BitBinaryReaderX : BinaryReaderX
    {
        BitReaderStream bitStream;

        public override Stream TargetStream
        {
            get => base.TargetStream;
            set
            {
                var newBitStream = value as BitReaderStream;

                if (newBitStream == null && value != null)
                    throw new ArgumentException("Stream must be BitReaderStream");

                bitStream = newBitStream;
                base.TargetStream = newBitStream;
            }
        }

        public BitBinaryReaderX() : base()
        {

        }

        public BitBinaryReaderX(BitReaderStream readerStream)
        {
            this.bitStream = readerStream;
            base.TargetStream = readerStream;
        }

        public bool ReadBit()
        {
            return bitStream.Read(1) != 0;
        }

        public ulong ReadBits(int bits)
        {
            ulong data = 0;
            while (bits > 0)
            {
                int subbits = bits > 8 ? 8 : bits;
                data <<= subbits;
                data |= bitStream.Read(subbits);
                bits -= subbits;
            }

            return data;
        }

        public uint2 ReadBits2D(int bits)
        {
            return new uint2(
                (uint)ReadBits(bits),
                (uint)ReadBits(bits));
        }

        public uint3 ReadBits3D(int bits)
        {
            return new uint3(
                (uint)ReadBits(bits),
                (uint)ReadBits(bits),
                (uint)ReadBits(bits));
        }

        public uint4 ReadBits4D(int bits)
        {
            return new uint4(
                (uint)ReadBits(bits),
                (uint)ReadBits(bits),
                (uint)ReadBits(bits),
                (uint)ReadBits(bits));
        }
    }
}
