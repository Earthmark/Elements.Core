using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Elements.Core
{
    public class BitWriterStream : Stream
    {
        Stream baseStream;

        byte bitBuffer;
        int bufferPosition;

        public override bool CanRead { get { return false; } }
        public override bool CanSeek { get { return false; } }

        public override bool CanWrite { get { return true; } }

        public override long Length { get { return baseStream.Length; } }

        public override long Position
        {
            get { return baseStream.Position; }
            set { Seek(value, SeekOrigin.Begin); }
        }

        public BitWriterStream(Stream baseStream)
        {
            this.baseStream = baseStream;
        }

        public void Write(ulong data, int bits)
        {
            if (bits > 64)
                throw new Exception("Number of bits to write cannot be larger than 64!");

            // make the MSB match ulong MSB
            // This shifts the part that should be encoded to be in the MSB and automatically gets rid of any
            // extra bits after that, so they don't interfere
            data <<= 64 - bits;

            while(bits > 0)
            {
                if (bits >= 8)
                {
                    Write((byte)(data >> 56));
                    bits -= 8;
                    data <<= 8;
                }
                else
                {
                    Write((byte)(data >> 56 + (8 - bits)), bits);
                    bits = 0;
                }
            }
        }

        public void Write(byte data, int bits = 8)
        {
            if (bits > 8)
                throw new Exception("Number if bits to write cannot be larger than 8!");

            if(bits == 8 && bufferPosition == 0)
            {
                bitBuffer = data;
                bufferPosition += 8; // shift it, because flushing bit buffer shifts it back
                FlushBitBuffer();
            }
            else
            {
                // shift the data, so the MSB of the source range matches MSB of the byte
                // this also masks the source data as a side effect (which is good)
                data <<= 8 - bits;

                // write to the bit buffer, shifted by the current buffer position
                bitBuffer |= (byte)(data >> bufferPosition);

                bufferPosition += bits;

                if(bufferPosition >= 8)
                {
                    FlushBitBuffer();
                    if(bufferPosition > 0)
                    {
                        // write the leftover
                        data <<= bits - bufferPosition;
                        bitBuffer |= data;
                    }
                }
            }
        }

        public override void WriteByte(byte value)
        {
            Write(value);
        }

        public void FlushBitBuffer()
        {
            if (bufferPosition > 0)
            {
                baseStream.WriteByte(bitBuffer);
                bitBuffer = 0;

                if (bufferPosition > 8)
                    bufferPosition -= 8;
                else
                    bufferPosition = 0;
            }
                                                          }

        public override void Flush()
        {
            FlushBitBuffer();
            baseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("Cannot read from the BitWriterStream!");
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("Cannot seek the BitWriterStream!");
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("Cannot seek the BitWriterStream!");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (bufferPosition == 0)
                baseStream.Write(buffer, offset, count);
            else
            {
                // slow method, to preserve bit offset
                for (int i = 0; i < count; i++)
                    Write(buffer[i + offset]);
            }
        }


        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }

        public override void Close()
        {
            FlushBitBuffer();

            baseStream.Close();
            base.Close();
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            throw new NotSupportedException();
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            throw new NotSupportedException();
        }

        public override int ReadByte()
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                baseStream.Dispose();
        }
    }
}
