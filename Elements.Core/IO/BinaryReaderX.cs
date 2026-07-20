using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Elements.Core
{
    public class BinaryReaderX : BinaryReader
    {
        public virtual Stream TargetStream
        {
            get => _wrappedStream.TargetStream;
            set => _wrappedStream.TargetStream = value;
        }

        WrappedStream _wrappedStream;

        public BinaryReaderX() : base(new WrappedStream(new NullStream()))
        {
            _wrappedStream = BaseStream as WrappedStream;
        }
    }
}
