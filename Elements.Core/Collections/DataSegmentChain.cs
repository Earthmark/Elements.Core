using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Elements.Core; using Elements.Data;
using System.Collections;

namespace Elements.Core
{
    public class DataSegmentChain : IEnumerable<DataSegment>
    {
        public DataSegment Origin { get; private set; }

        internal RawValueList<DataSegmentData> _dataSegments = new RawValueList<DataSegmentData>();

        internal int AddSegmentData(DataSegmentData data)
        {
            _dataSegments.Add(data);
            return _dataSegments.Count - 1;
        }

        internal int AddSegmentData() => AddSegmentData(new DataSegmentData() { nextSegment = -1 });

        #region ENUMERABLE

        public Enumerator GetEnumerator() => new Enumerator(this);

        IEnumerator<DataSegment> IEnumerable<DataSegment>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // Enumerator

        public struct Enumerator : IEnumerator<DataSegment>
        {
            DataSegmentChain segmentChain;
            DataSegment currentSegment;
            bool initialized;

            public DataSegment Current { get { return currentSegment; } }

            object IEnumerator.Current { get { return Current; } }

            public Enumerator(DataSegmentChain chain)
            {
                this.segmentChain = chain;
                this.currentSegment = default;
                initialized = false;
            }

            public void Dispose()
            {
                currentSegment = default;
            }

            public bool MoveNext()
            {
                if (initialized)
                    currentSegment = currentSegment.NextSegment;
                else
                {
                    currentSegment = segmentChain.Origin;
                    initialized = true;
                }

                return !currentSegment.IsNull;
            }

            public void Reset()
            {
                initialized = false;
                currentSegment = default;
            }
        }

        #endregion

        #region HELPER FUNCTIONS

        public void Clear()
        {
            _dataSegments.Clear();
            Origin = default;
        }

        public int ComputeLength()
        {
            if (Origin.IsNull)
                return 0;

            DataSegment segment = Origin;

            while (!segment.NextSegment.IsNull)
                segment = segment.NextSegment;

            return segment.End;
        }

        public override string ToString()
        {
            StringBuilder str = new StringBuilder();
            foreach (var s in this)
                str.Append(s.SegmentType + " (" + s.Begin + " - " + s.End + "), ");
            return str.ToString();
        }

        public void SetUnmodified(int length)
        {
            _dataSegments.Clear();
            Origin = DataSegment.Unmodified(this, length);
        }

        public DataSegment SegmentAtPosition(int position)
        {
            DataSegment segment = Origin;

            while (!segment.IsNull && !(position >= segment.Begin && position < segment.End))
                segment = segment.NextSegment;

            return segment;
        }

        public void Encode(BinaryWriter writer)
        {
            // go through the whole chain, encoding the individual types
            DataSegment segment = default;

            do
            {
                // fetch the first or next segment
                if (segment.IsNull)
                    segment = Origin;
                else
                    segment = segment.NextSegment;

                if (segment.IsNull)
                    writer.WriteEnumBinary(DataSegment.Type.NONE); // this indicates last segment
                else
                {
                    writer.WriteEnumBinary(segment.SegmentType);

                    // Do not write begin, that can be calculated from the end
                    writer.Write7BitEncoded((ulong)segment.Length);

                    if (segment.SegmentType == DataSegment.Type.OriginalData)
                        writer.Write7BitEncoded((ulong)segment.OriginalDataBegin);
                }

            } while (!segment.IsNull);
        }

        public void Decode(BinaryReader reader)
        {
            Clear();

            DataSegment last = default;
            int position = 0;

            DataSegment.Type segmentType;

            do
            {
                segmentType = reader.ReadEnumBinary<DataSegment.Type>();

                if(segmentType != DataSegment.Type.NONE)
                {
                    // decode the segment end
                    int length = (int)reader.Read7BitEncoded();
                    int origDataBegin = 0;
                    if (segmentType == DataSegment.Type.OriginalData)
                        origDataBegin = (int)reader.Read7BitEncoded();

                    var segment = new DataSegment(this, segmentType, position, position + length, origDataBegin);

                    if (last.IsNull)
                        Origin = segment;
                    else
                        last.NextSegment = segment;

                    last = segment;

                    position += length;
                }

            } while (segmentType != DataSegment.Type.NONE);
        }

        #endregion

        #region PUBLIC INTERFACE

        public void EnsureLength(int newLength)
        {
            var oldLength = ComputeLength();

            // it's already long enough, skip
            if (oldLength >= newLength)
                return;

            int expandedLength = newLength - oldLength;

            // find the last segment and append the new one
            if (Origin.IsNull)
            {
                _dataSegments.Clear();

                var newSegment = DataSegment.Unmodified(this, expandedLength, oldLength);
                Origin = newSegment;
            }
            else
            {
                var newSegment = DataSegment.Unmodified(this, expandedLength, oldLength);

                var last = this.Last();
                last.NextSegment = newSegment;
            }
        }

        public void WriteNew(int index, int length)
        {
            if (Origin.IsNull && index != 0)
                throw new Exception("Index is beyond the end of existing data segments");

            // check if it would write into or at the end of a new data segment
            var existingSegment = SegmentAtPosition(Math.Max(0, index - 1));

            if (!existingSegment.IsNull && existingSegment.SegmentType == DataSegment.Type.NewData)
            {
                // try to generate expanded segment and splice the new one into the chain if the length was changed
                var expanded = existingSegment.TryExpand(index + length);
                if (!expanded.IsNull)
                    SpliceChain(expanded, expanded);
            }
            else
            {
                // create a new data segment
                if (Origin.IsNull)
                {
                    _dataSegments.Clear();

                    var newSegment = DataSegment.NewData(this, index, length);
                    Origin = newSegment;
                }
                else
                {
                    var newSegment = DataSegment.NewData(this, index, length);
                    SpliceChain(newSegment, newSegment);
                }
            }
        }

        public void WriteCopy(int index, int length, int originalIndex)
        {
            // build a segment chain corresponding to the copied over region
            DataSegment source = SegmentAtPosition(originalIndex);

            int copiedLength = 0;
            DataSegment firstLink = default;
            DataSegment lastLink = default;

            while (copiedLength < length)
            {
                if (source.IsNull)
                    throw new Exception("Source doesn't contain enough data for the copy!");

                int sourceIndex = copiedLength + originalIndex;
                int targetIndex = index + copiedLength;
                int sourceLength = source.End - sourceIndex;

                // limit the length by the remaining length needed to copy
                sourceLength = Math.Min(sourceLength, length - copiedLength);

                DataSegment segment;

                // determine the type
                switch (source.SegmentType)
                {
                    case DataSegment.Type.Unmodified:
                        segment = DataSegment.OriginalData(this, targetIndex, sourceLength,
                            sourceIndex);
                        break;

                    case DataSegment.Type.OriginalData:
                        // it is already copied original data, adjust the origin index

                        int offset = sourceIndex - source.Begin;
                        int origData = source.OriginalDataBegin + offset;

                        segment = DataSegment.OriginalData(this, targetIndex, sourceLength, origData);

                        break;

                    case DataSegment.Type.NewData:
                        // it is simply new data, do not track duplication at this point
                        segment = DataSegment.NewData(this, targetIndex, sourceLength);
                        break;

                    default:
                        throw new Exception("Invalid DataSegment Type");
                }

                // set it as first link if there's none
                if (firstLink.IsNull)
                    firstLink = segment;

                // link the last one (if there's one) to this one
                if (!lastLink.IsNull)
                    lastLink.NextSegment = segment;

                // save it as last link
                lastLink = segment;

                // advance the copied length and move on to next source link
                copiedLength += sourceLength;
                source = source.NextSegment;
            }

            // splice the result into original segment chain
            SpliceChain(firstLink, lastLink);
        }

        public void Shorten(int newLength)
        {
            if (newLength == 0)
            {
                _dataSegments.Clear();
                Origin = default;
                return;
            }

            var targetSegment = SegmentAtPosition(Math.Max(0, newLength - 1));

            if (targetSegment.IsNull)
                throw new Exception("Data length is smaller than the desired shortened legnth");

            targetSegment.CutBack(newLength);
            targetSegment.NextSegment = default;
        }

        public void ClearChanges()
        {
            int length = ComputeLength();
            _dataSegments.Clear();
            Origin = DataSegment.Unmodified(this, length);
        }

        #endregion

        #region DEBUGGING

        public string ToSimpleDebugString()
        {
            string debug = "";

            foreach (var s in this)
            {
                int length = s.Length;

                for (int i = 0; i < length; i++)
                {
                    switch (s.SegmentType)
                    {
                        case DataSegment.Type.Unmodified:
                            debug += "-";
                            break;

                        case DataSegment.Type.OriginalData:
                            debug += (s.OriginalDataBegin + i).ToString("X");
                            break;

                        case DataSegment.Type.NewData:
                            debug += "#";
                            break;
                    }
                }
            }

            return debug;
        }

        #endregion

        #region PRIVATE OPERATIONS

        void SpliceChain(DataSegment chainStart, DataSegment chainEnd)
        {
            int spliceBegin = chainStart.Begin;
            int spliceEnd = chainEnd.End;

            DataSegment precedingSegment = default;

            if(spliceBegin != 0)
            {
                // find the segment that contains or ends with the target write position
                precedingSegment = SegmentAtPosition(Math.Max(0, spliceBegin - 1));

                if (precedingSegment.IsNull)
                    throw new Exception("Write position must be within the segment chain!");
            }

            // find if there's any segment at the end of the write position
            var followingSegment = SegmentAtPosition(chainEnd.End);

            if (precedingSegment == followingSegment && !precedingSegment.IsNull)
            {
                // it's the same segment and needs to be split in two
                followingSegment = precedingSegment.Split(spliceBegin, spliceEnd);

                precedingSegment.NextSegment = chainStart;
                chainEnd.NextSegment = followingSegment;
            }
            else
            {
                if (spliceBegin != 0)
                {
                    precedingSegment.CutBack(spliceBegin);
                    precedingSegment.NextSegment = chainStart;
                }
                else
                    Origin = chainStart;

                // join it with the segment chain continuing at the end of there's one
                if (!followingSegment.IsNull)
                {
                    followingSegment.CutFront(spliceEnd);
                    chainEnd.NextSegment = followingSegment;
                }
            }
        }

        #endregion
    }

    struct DataSegmentData
    {
        public DataSegment.Type type;

        public int nextSegment;

        public int begin;
        public int end;

        public int originalDataBegin;
    }

    public struct DataSegment : IEquatable<DataSegment>
    {
        public enum Type
        {
            NONE,

            Unmodified,
            OriginalData,
            NewData,
        }

        readonly int _index;
        readonly DataSegmentChain _chain;

        DataSegmentData GetData() => _chain._dataSegments[_index];
        void SetData(DataSegmentData data) => _chain._dataSegments[_index] = data;

        public Type SegmentType
        {
            get => GetData().type;
            private set
            {
                var data = GetData();
                data.type = value;
                SetData(data);
            }
        }

        public DataSegment NextSegment //{ get; internal set; }
        {
            get
            {
                var index = GetData().nextSegment;
                if (index < 0)
                    return default;
                return new DataSegment(_chain, index);
            }

            internal set
            {
                int nextIndex;
                if (value._chain == null)
                    nextIndex = -1;
                else
                    nextIndex = value._index;

                var data = GetData();
                data.nextSegment = nextIndex;
                SetData(data);
            }
        }

        public int Begin
        {
            get => GetData().begin;
            private set
            {
                var data = GetData();
                data.begin = value;
                SetData(data);
            }
        }

        public int End
        {
            get => GetData().end;
            private set
            {
                var data = GetData();
                data.end = value;
                SetData(data);
            }
        }

        public int OriginalDataBegin
        {
            get => GetData().originalDataBegin;
            private set
            {
                var data = GetData();
                data.originalDataBegin = value;
                SetData(data);
            }
        }

        public int Length { get { return End - Begin; } }

        public bool IsNull => _chain == null;

        #region CONSTRUCTORS

        internal DataSegment(DataSegmentChain chain, int index)
        {
            _chain = chain;
            _index = index;
        }

        internal DataSegment(DataSegmentChain chain, Type segmentType, int begin, int end, int originalDataBegin)
        {
            _chain = chain;
            _index = chain.AddSegmentData(new DataSegmentData()
            {
                type = segmentType,
                begin = begin,
                end = end,
                originalDataBegin = originalDataBegin,
                nextSegment = -1
            });
        }

        internal static DataSegment NewData(DataSegmentChain chain, int begin, int length)
        {
            return new DataSegment(chain, Type.NewData, begin, begin + length, 0);
        }

        internal static DataSegment OriginalData(DataSegmentChain chain, int begin, int length, int originalDataBegin)
        {
            // if the data matches the original unmodified data, simply consider it unmodified
            if (begin == originalDataBegin)
                return new DataSegment(chain, Type.Unmodified, begin, begin+length, 0);

            return new DataSegment(chain, Type.OriginalData, begin, begin + length, originalDataBegin);
        }

        internal static DataSegment Unmodified(DataSegmentChain chain, int length, int index = 0)
        {
            return new DataSegment(chain, Type.Unmodified, index, index + length, 0);
        }

        #endregion

        #region DETECT FUNCTIONS

        public bool Overwrites(DataSegment segment)
        {
            if (segment.SegmentType != Type.OriginalData)
                throw new ArgumentException("Only OriginalData segments can be checked for overwrites");

            // unmodified segments cannot overwrite anything from principle
            if (this.SegmentType == Type.Unmodified)
                return false;

            // check if the write region touches the original data region of the target segment
            int originalBegin = segment.OriginalDataBegin;
            int originalEnd = segment.OriginalDataBegin + segment.Length;

            // check of begin/end is within this segment
            if (originalBegin >= Begin && originalBegin < End)
                return true;
            if (originalEnd > Begin && originalEnd <= End)
                return true;

            // check if this segment is within the original data region
            if (Begin >= originalBegin && Begin < originalEnd)
                return true;
            if (End > originalBegin && End <= originalEnd)
                return true;

            // all tests have failed, no overlap, no ovewrite
            return false;
        }

        #endregion

        #region OPERATIONS FOR THE DATA SEGMENT CHAIN

        // returns the second segment
        internal DataSegment Split(int splitStart, int splitEnd)
        {
            // create a duplicate segment
            var second = new DataSegment(_chain, SegmentType, Begin, End, OriginalDataBegin);

            second.NextSegment = NextSegment;
            
            // cut end of self
            CutBack(splitStart);

            // cut the front of the duplicate
            second.CutFront(splitEnd);

            return second;
        }

        internal void CutFront(int newBegin)
        {
            if (newBegin >= End)
                throw new Exception("Cut position is beyond the end of the segment!");

            int cut = newBegin - Begin;

            Begin = newBegin;

            if (SegmentType == Type.OriginalData)
                OriginalDataBegin += cut;
        }

        internal void CutBack(int newEnd)
        {
            if (newEnd <= Begin && (newEnd != 0 || Begin != 0))
                throw new Exception("Cut position is before or at the beginning of the segment!");

            End = newEnd;
        }

        internal DataSegment TryExpand(int newEnd)
        {
            if (SegmentType != Type.NewData)
                throw new Exception("Only NewData segments can be expanded!");

            if (newEnd <= Begin)
                throw new Exception("New End cannot be at or before the start of the segment");

            if (newEnd <= End)
                return default;

            return new DataSegment(_chain, Type.NewData, Begin, newEnd, 0);
        }

        public bool Equals(DataSegment other)
        {
            return _chain == other._chain && _index == other._index;
        }

        public override bool Equals(object obj)
        {
            if (obj is DataSegment)
                return this.Equals((DataSegment)obj);

            return false;
        }

        public override int GetHashCode()
        {
            if (_chain == null)
                return 0;
            return _chain.GetHashCode() ^ _index.GetHashCode();
        }

        public static bool operator ==(DataSegment a, DataSegment b) => a.Equals(b);
        public static bool operator !=(DataSegment a, DataSegment b) => !(a == b);

        #endregion
    }
}
