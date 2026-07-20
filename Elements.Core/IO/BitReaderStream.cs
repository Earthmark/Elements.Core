using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Elements.Core
{
    public class BitReaderStream : Stream
    {
        Stream baseStream;

        ushort bitBuffer;
        int remainingBits;

        public override bool CanRead { get { return true; } }
        public override bool CanSeek { get { return false; } }

        public override bool CanWrite { get { return false; } }

        public override long Length { get { return baseStream.Length; } }

        public override long Position
        {
            get { return baseStream.Length; }
            set { Seek(Position, SeekOrigin.Begin); }
        }

        public override void Flush()
        {
            baseStream.Flush();
        }

        public BitReaderStream(Stream baseStream)
        {
            this.baseStream = baseStream;
        }

        public byte Read(int bits = 8)
        {
            // read another byte into the stream if necessary
            if (bits > remainingBits)
            {
                bitBuffer |= (ushort)(baseStream.ReadByte() << (8 - remainingBits));
                remainingBits += 8;
            }

            byte data = (byte)(bitBuffer >> (16 - bits));

            bitBuffer <<= bits;
            remainingBits -= bits;

            return data;
        }

        public override int ReadByte()
        {
            return Read();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (remainingBits % 8 == 0)
            {
                // Do a fast byte-aligned read
                int read = 0;

                // First read in the remaining bytes in the buffer
                while(remainingBits > 0 && count > 0)
                {
                    buffer[offset++] = Read();
                    count--;
                    read++;
                }

                // If there's still stuff left, then use a fast direct read
                if (count > 0)
                    read += baseStream.Read(buffer, offset, count);

                return read;
            }
            else
            {
                // do the slow read, respecting the bit offsets
                for (int i = 0; i < count; i++)
                {
                    // check if we reached the end
                    if (baseStream.Position == baseStream.Length && remainingBits < 8)
                        return i;

                    buffer[i + offset] = Read();
                }

                return count;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException("BitReaderStream doesn't support seeking");
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("BitReaderStream doesn't support setting length");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new Exception("BitReaderStream doesn't support writing");
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

        public override void WriteByte(byte value)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
                baseStream.Dispose();
        }
    }
}
