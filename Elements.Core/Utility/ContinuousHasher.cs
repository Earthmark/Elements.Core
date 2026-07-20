using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Elements.Core
{
    public class ContinuousHasher<T> : Stream
        where T : HashAlgorithm, new()
    {
        T _hash;

        byte[] _buffer;
        int _dataLength;

        public int WrittenBytes { get; private set; }
        public int HashedBytes { get; private set; }

        public ContinuousHasher(int bufferSize = 4096)
        {
            _hash = new T();
            _hash.Initialize();

            _buffer = new byte[bufferSize];
        }

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => WrittenBytes;
        public override long Position { get => HashedBytes; set => throw new NotSupportedException(); }

        public override void Flush() { }

        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count)
        {
            while(count > 0)
            {
                if(_dataLength > 0 || count < _buffer.Length)
                {
                    // We've already written some data in, so write more and then see where we're at
                    // We also want to write the data if it's not enough to fill the whole buffer
                    int toWrite = Math.Min(_buffer.Length - _dataLength, count);

                    Buffer.BlockCopy(buffer, offset, _buffer, _dataLength, toWrite);

                    _dataLength += toWrite;
                    count -= toWrite;
                    offset += toWrite;

                    WrittenBytes += toWrite;

                    // If we filled the buffer, this will hash it and clear it out
                    CheckFlush();
                }
                else
                {
                    // We don't have any data pending in the buffer and the incoming data is bigger than our buffer
                    // We can just run the hashing directly to avoid unnecessary copies
                    _hash.TransformBlock(buffer, offset, count, null, 0);

                    HashedBytes += count;
                    WrittenBytes += count;

                    offset += count;
                    count = 0;
                }
            }
        }

        public byte[] FinalizeHash()
        {
            _hash.TransformFinalBlock(_buffer, 0, _dataLength);

            return _hash.Hash;
        }

        void CheckFlush()
        {
            if (_dataLength != _buffer.Length)
                return;

            _hash.TransformBlock(_buffer, 0, _dataLength, null, 0);

            HashedBytes += _dataLength;

            _dataLength = 0;
        }

        protected override void Dispose(bool disposing)
        {
            _hash.Dispose();

            base.Dispose(disposing);
        }
    }
}
