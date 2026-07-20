using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Elements.Core
{
    /// <summary>
    /// This stream catches any exceptions coming from the inner stream, so they do not propagate outside.
    /// This is necessary for when Streams are used for an interop with native code, because exceptions cannot
    /// be propagated through native stack on certain platforms (e.g. Linux), which results them being treated as
    /// unhandled exceptions. See here:
    /// https://www.mono-project.com/docs/advanced/pinvoke/#runtime-exception-propagation
    /// </summary>
    public class ExceptionWrapperStream : Stream
    {
        public Exception CaughtException { get; private set; }

        Stream _innerStream;

        public ExceptionWrapperStream(Stream stream)
        {
            _innerStream = stream;
        }

        public void ClearException() => CaughtException = null;

        public override bool CanRead
        {
            get
            {
                if (CaughtException != null)
                    return false;

                try
                {
                    return _innerStream.CanRead;
                }
                catch(Exception ex)
                {
                    CaughtException = ex;
                    return false;
                }
            }
        }

        public override bool CanSeek
        {
            get
            {
                if (CaughtException != null)
                    return false;

                try
                {
                    return _innerStream.CanSeek;
                }
                catch (Exception ex)
                {
                    CaughtException = ex;
                    return false;
                }
            }
        }

        public override bool CanWrite
        {
            get
            {
                if (CaughtException != null)
                    return false;

                try
                {
                    return _innerStream.CanWrite;
                }
                catch (Exception ex)
                {
                    CaughtException = ex;
                    return false;
                }
            }
        }

        public override long Length
        {
            get
            {
                if (CaughtException != null)
                    return 0;

                try
                {
                    return _innerStream.Length;
                }
                catch (Exception ex)
                {
                    CaughtException = ex;
                    return 0;
                }
            }
        }

        public override long Position
        {
            get
            {
                if (CaughtException != null)
                    return 0;

                try
                {
                    return _innerStream.Position;
                }
                catch (Exception ex)
                {
                    CaughtException = ex;
                    return 0;
                }
            }

            set
            {
                try
                {
                    _innerStream.Position = value;
                }
                catch (Exception ex)
                {
                    CaughtException = ex;
                }
            }
        }

        public override void Flush()
        {
            if (CaughtException != null)
                return;

            try
            {
                _innerStream.Flush();
            }
            catch(Exception ex)
            {
                CaughtException = ex;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (CaughtException != null)
                return 0;

            try
            {
                return _innerStream.Read(buffer, offset, count);
            }
            catch (Exception ex)
            {
                CaughtException = ex;
            }

            return 0;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (CaughtException != null)
                return 0;

            try
            {
                return _innerStream.Seek(offset, origin);
            }
            catch (Exception ex)
            {
                CaughtException = ex;
            }

            return 0;
        }

        public override void SetLength(long value)
        {
            if (CaughtException != null)
                return;

            try
            {
                _innerStream.SetLength(value);
            }
            catch (Exception ex)
            {
                CaughtException = ex;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (CaughtException != null)
                return;

            try
            {
                _innerStream.Write(buffer, offset, count);
            }
            catch (Exception ex)
            {
                CaughtException = ex;
            }
        }

        protected override void Dispose(bool disposing)
        {
            _innerStream.Dispose();
        }
    }
}
