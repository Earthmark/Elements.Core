using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Elements.Core
{
    public class BinaryWriterX : BinaryWriter
    {
        // needs its own copy unfortunatelly, the parent one isn't accessible
        byte[] _buffer = new byte[16];

        public virtual Stream TargetStream
        {
            get => BaseStream;
            set
            {
                OutStream?.Flush();
                OutStream = value;
            }
        }

        public BinaryWriterX() : base()
        {
            
        }

        public BinaryWriterX(Stream stream) : base(stream)
        {

        }

        public BinaryWriterX(Stream stream, Encoding encoding) : base(stream, encoding)
        {

        }

        public override void Write(float value)
        {
            unsafe
            {
                uint num = *(uint*)&value;

                _buffer[0] = (byte)num;
                _buffer[1] = (byte)(num >> 8);
                _buffer[2] = (byte)(num >> 16);
                _buffer[3] = (byte)(num >> 24);

                OutStream.Write(_buffer, 0, 4);
            }
        }

        public override void Write(double value)
        {
            unsafe
            {
                ulong num = (ulong)*(long*)&value;

                _buffer[0] = (byte)num;
                _buffer[1] = (byte)(num >> 8);
                _buffer[2] = (byte)(num >> 16);
                _buffer[3] = (byte)(num >> 24);
                _buffer[4] = (byte)(num >> 32);
                _buffer[5] = (byte)(num >> 40);
                _buffer[6] = (byte)(num >> 48);
                _buffer[7] = (byte)(num >> 56);

                OutStream.Write(_buffer, 0, 8);
            }
        }
    }
}
