using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Elements.Core
{
    public static class DataCoding
    {
        public static void WriteDeltaCodedSequence(this BinaryWriter writer, IList<ulong> sequence)
        {
            // write the number of values
            writer.Write7BitEncoded((ulong)sequence.Count);

            if (sequence.Count > 0)
            {
                // write the first one raw
                writer.Write7BitEncoded(sequence[0]);

                // write the rest as deltas
                for (int i = 1; i < sequence.Count; i++)
                    writer.Write7BitEncoded(sequence[i] - sequence[i - 1]);
            }
        }

        public static void ReadDeltaCodedSequence(this BinaryReader reader, IList<ulong> sequence)
        {
            // read the number of values first
            int count = (int)reader.Read7BitEncoded();

            if (count > 0)
            {
                // read the first value
                sequence.Add(reader.Read7BitEncoded());

                // read the rest as delta
                for (int i = 1; i < count; i++)
                    sequence.Add(sequence[i - 1] + reader.Read7BitEncoded());
            }
        }
    }
}
