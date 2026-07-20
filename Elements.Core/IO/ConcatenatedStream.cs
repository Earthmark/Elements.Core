using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Elements.Core
{
    // Based on: https://stackoverflow.com/questions/3879152/how-do-i-concatenate-two-system-io-stream-instances-into-one
    public class ConcatenatedStream : Stream
    {
        Queue<Stream> _streams = new Queue<Stream>();
        long _position;
        long _length;

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => _length;

        public override long Position { get => _position; set => throw new NotSupportedException(); }


        public ConcatenatedStream()
        {

        }

        public ConcatenatedStream(IEnumerable<Stream> streams)
        {
            foreach (var stream in streams)
                EnqueueStream(stream);
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_streams.Count == 0)
                return 0;

            int bytesRead = _streams.Peek().Read(buffer, offset, count);
            if (bytesRead == 0)
            {
                _streams.Dequeue().Dispose();
                bytesRead += Read(buffer, offset + bytesRead, count - bytesRead);
            }

            _position += bytesRead;

            return bytesRead;
        }

        public void EnqueueStream(Stream stream)
        {
            _length += stream.Length - stream.Position;
            _streams.Enqueue(stream);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            foreach (var stream in _streams)
                stream.Dispose();

            base.Dispose(disposing);
        }
    }
}
