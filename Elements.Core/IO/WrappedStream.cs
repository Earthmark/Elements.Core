using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elements.Core; using Elements.Data;
using System.IO;
using System.Threading;

namespace Elements.Core
{
    public class WrappedStream : Stream
    {
        public Stream TargetStream { get; set; }

        public WrappedStream()
        {

        }

        public WrappedStream(Stream targetStream)
        {
            TargetStream = targetStream;
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return TargetStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return TargetStream.BeginWrite(buffer, offset, count, callback, state);
        }

        public override bool CanRead => TargetStream.CanRead;
        public override bool CanSeek => TargetStream.CanSeek;
        public override bool CanTimeout => TargetStream.CanTimeout;
        public override bool CanWrite => TargetStream.CanWrite;

        public override void Close() => TargetStream.Close();

        public override System.Threading.Tasks.Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return TargetStream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override int EndRead(IAsyncResult asyncResult) => TargetStream.EndRead(asyncResult);
        public override void EndWrite(IAsyncResult asyncResult) => TargetStream.EndWrite(asyncResult);
        public override void Flush() => TargetStream.Flush();

        public override System.Threading.Tasks.Task FlushAsync(CancellationToken cancellationToken) => TargetStream.FlushAsync(cancellationToken);

        public override long Length => TargetStream.Length;

        public override long Position { get => TargetStream.Position; set => TargetStream.Position = value; }

        public override int Read(byte[] buffer, int offset, int count) => TargetStream.Read(buffer, offset, count);

        public override System.Threading.Tasks.Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return TargetStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override int ReadByte()
        {
            return TargetStream.ReadByte();
        }

        public override int ReadTimeout { get => TargetStream.ReadTimeout; set => TargetStream.ReadTimeout = value; }

        public override long Seek(long offset, SeekOrigin origin) => TargetStream.Seek(offset, origin);
        public override void SetLength(long value) => TargetStream.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => TargetStream.Write(buffer, offset, count);

        public override System.Threading.Tasks.Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return TargetStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override void WriteByte(byte value)
        {
            TargetStream.WriteByte(value);
        }

        public override int WriteTimeout { get => TargetStream.WriteTimeout; set => TargetStream.WriteTimeout = value; }
    }
}
