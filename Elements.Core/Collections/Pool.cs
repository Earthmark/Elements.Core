using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.IO;

namespace Elements.Core;

public interface IPoolable
{
    void Clean();
}

// TODO!!! Add pool manager or some other management method which will periodically (or on demand) trim it
// to prevent accumulating too many resources

// TODO!!! Protection against returning same element multiple times?

public static class Pool
{
    public static int TRIM_INTERVAL_SECONDS => 30;
    public static int TRIM_HISTORY_COUNT => 2;
    public static float TRIM_MIN_RATIO => 1.05f;

    public readonly struct ActivePool
    {
        public readonly Type PoolType;
        public readonly Func<int> GetObjectCount;
        public readonly Action Trim;
        public readonly Action Clear;

        public ActivePool(Type poolType, Func<int> getObjectCount, Action trim, Action clear)
        {
            this.PoolType = poolType;
            this.GetObjectCount = getObjectCount;
            this.Trim = trim;
            this.Clear = clear;
        }
    }

    static List<ActivePool> _activePools = new List<ActivePool>();
    static object _statsLock = new object();

    static Pool()
    {
        Task.Run(PoolTrimWorker);
    }

    static async Task PoolTrimWorker()
    {
        List<ActivePool> pools = new List<ActivePool>();

        for (; ; )
        {
            await Task.Delay(TimeSpan.FromSeconds(TRIM_INTERVAL_SECONDS));

            lock (_statsLock)
            {
                foreach (var pool in _activePools)
                    pools.Add(pool);
            }

            foreach (var pool in pools)
                pool.Trim();

            pools.Clear();
        }
    }

    internal static void RegisterActivePool(in ActivePool activePool)
    {
        lock (_statsLock)
            _activePools.Add(activePool);
    }

    public static void GetActivePools(List<ActivePool> list)
    {
        lock (_statsLock)
            list.AddRange(_activePools);
    }

    public static void BorrowIfNull<T>(ref List<T> list)
    {
        if (list == null)
            list = BorrowList<T>();
    }

    public static List<T> BorrowList<T>() => Pool<List<T>>.Borrow();
    public static RawList<T> BorrowRawList<T>() => Pool<RawList<T>>.Borrow();

    public static RawValueList<T> BorrowRawValueList<T>()
        where T : struct
    {
        return Pool<RawValueList<T>>.Borrow();
    }

    public static Dictionary<TKey, TValue> BorrowDictionary<TKey, TValue>() => Pool<Dictionary<TKey, TValue>>.Borrow();
    public static HashSet<T> BorrowHashSet<T>() => Pool<HashSet<T>>.Borrow();
    public static Queue<T> BorrowQueue<T>() => Pool<Queue<T>>.Borrow();
    public static BitQueue BorrowBitQueue() => Pool<BitQueue>.Borrow();
    public static StringBuilder BorrowStringBuilder() => Pool<StringBuilder>.Borrow();
    public static KeyCounter<T> BorrowKeyCounter<T>() => Pool<KeyCounter<T>>.Borrow();

    public static DictionaryList<TKey, TValue> BorrowDictionaryList<TKey, TValue>()
    {
        var dict = Pool<DictionaryList<TKey, TValue>>.Borrow();
        return dict;
    }

    public static BinaryReaderX BorrowBinaryReader(Stream stream)
    {
        var reader = Pool<BinaryReaderX>.Borrow();
        reader.TargetStream = stream;
        return reader;
    }

    public static T Borrow<T>() where T : IPoolable, new() => Pool<T>.Borrow();

    static Encoding _defaultStringEncoding = new UTF8Encoding(false, false);
    static BinaryWriterX ConstructBinaryWriter() => new BinaryWriterX(new MemoryStream(), _defaultStringEncoding);
    static Func<BinaryWriterX> _binaryWriterConstructor = ConstructBinaryWriter;

    public static BinaryWriterX BorrowBinaryWriter(Stream stream)
    {
        var writer = Pool<BinaryWriterX>.Borrow(_binaryWriterConstructor);
        writer.TargetStream = stream;
        return writer;
    }

    public static BitBinaryReaderX BorrowBitBinaryReader(BitReaderStream stream)
    {
        var reader = Pool<BitBinaryReaderX>.Borrow();
        reader.TargetStream = stream;
        return reader;
    }

    public static BitBinaryWriterX BorrowBitBinaryWriter(BitWriterStream stream)
    {
        var writer = Pool<BitBinaryWriterX>.Borrow();
        writer.TargetStream = stream;
        return writer;
    }

    public static DataSegmentChain BorrowDataSegmentChain() => Pool<DataSegmentChain>.Borrow();

    // Returns

    public static void Return<T>(ref List<T> list)
    {
        list.Clear();
        Pool<List<T>>.ReturnCleaned(ref list);
    }

    public static void ReturnUnsafe<T>(List<T> list)
    {
        var copy = list;
        Pool.Return(ref copy);
    }

    public static void Return<T>(ref RawList<T> list)
    {
        list.Clear();
        Pool<RawList<T>>.ReturnCleaned(ref list);
    }

    public static void Return<T>(ref RawValueList<T> list)
        where T : struct
    {
        list.Clear();
        Pool<RawValueList<T>>.ReturnCleaned(ref list);
    }

    public static void Return<TKey, TValue>(ref Dictionary<TKey, TValue> dictionary)
    {
        dictionary.Clear();
        Pool<Dictionary<TKey, TValue>>.ReturnCleaned(ref dictionary);
    }

    public static void Return<TKey, TValue>(ref DictionaryList<TKey, TValue> dictionary)
    {
        dictionary.Clear();
        Pool<DictionaryList<TKey, TValue>>.ReturnCleaned(ref dictionary);
    }

    public static void Return<T>(ref HashSet<T> hashSet)
    {
        hashSet.Clear();
        Pool<HashSet<T>>.ReturnCleaned(ref hashSet);
    }

    public static void Return<T>(ref Queue<T> queue)
    {
        queue.Clear();
        Pool<Queue<T>>.ReturnCleaned(ref queue);
    }

    public static void Return(ref BitQueue bitqueue)
    {
        bitqueue.Clear();
        Pool<BitQueue>.ReturnCleaned(ref bitqueue);
    }

    public static void Return(ref StringBuilder stringBuilder)
    {
        stringBuilder.Clear();
        Pool<StringBuilder>.ReturnCleaned(ref stringBuilder);
    }

    public static void Return<T>(ref KeyCounter<T> counter)
    {
        counter.Clear();
        Pool<KeyCounter<T>>.ReturnCleaned(ref counter);
    }

    public static string ReturnToString(ref StringBuilder stringBuilder)
    {
        var str = stringBuilder.ToString();
        Return(ref stringBuilder);
        return str;
    }

    public static void Return(ref BinaryReaderX reader)
    {
        reader.TargetStream.Flush();
        reader.TargetStream = null;

        Pool<BinaryReaderX>.ReturnCleaned(ref reader);
    }

    public static void Return(ref BinaryWriterX writer)
    {
        writer.TargetStream.Flush();
        writer.TargetStream = null;

        Pool<BinaryWriterX>.ReturnCleaned(ref writer);
    }

    public static void Return(ref BitBinaryReaderX reader)
    {
        reader.TargetStream.Flush();
        reader.TargetStream = null;

        Pool<BitBinaryReaderX>.ReturnCleaned(ref reader);
    }

    public static void Return(ref BitBinaryWriterX writer)
    {
        writer.TargetStream.Flush();
        writer.TargetStream = null;

        Pool<BitBinaryWriterX>.ReturnCleaned(ref writer);
    }

    public static void Return(ref DataSegmentChain chain)
    {
        chain.Clear();
        Pool<DataSegmentChain>.ReturnCleaned(ref chain);
    }

    public static void Return<T>(ref T poolable)
        where T : IPoolable, new()
    {
        poolable.Clean();
        Pool<T>.ReturnCleaned(ref poolable);
    }
}

public static class Pool<T>
    where T : new()
{
    static ConcurrentStack<T> pool = new();

    static List<int> countHistory = new List<int>();

    static int borrowedCount;
    static int maxBorrowedCount;

    static Pool()
    {
        Pool.RegisterActivePool(new Pool.ActivePool(typeof(T), GetObjectCount, Trim, Clear));
    }

    public static int GetObjectCount() => pool.Count;

    public static void Trim()
    {
        countHistory.Add(maxBorrowedCount);

        // reset tracking
        maxBorrowedCount = 0;

        if (countHistory.Count > Pool.TRIM_HISTORY_COUNT)
            countHistory.RemoveAt(0);

        if (countHistory.Count == Pool.TRIM_HISTORY_COUNT)
        {
            int max = 0;

            foreach (var c in countHistory)
                max = MathX.Max(max, c);

            max = MathX.CeilToInt(max * Pool.TRIM_MIN_RATIO);

            if (max < pool.Count)
            {
                // reset borrow count tracking
                borrowedCount = 0;

                // perform the actual trim
                while (pool.Count > max)
                    pool.TryPop(out _);
            }
        }
    }

    public static void Clear() => pool.Clear();

    // TODO!!! Allow borrowing dirty resources? Might be a source of bugs if the has some
    // leftover objects that shouldn't be accessed by someone who forgets to clean the resource
    // This will force to add cleaning support for any new types

    public static T Borrow(Func<T> constructor = null)
    {
        Interlocked.Increment(ref borrowedCount);

        // it's ok to have some race conditions here, since it doesn't have to be super exact
        if (borrowedCount > maxBorrowedCount)
            maxBorrowedCount = borrowedCount;

        if (pool.TryPop(out T resource))
            return resource;

        if (constructor != null)
            return constructor();

        return new T();
    }

    static void Clean(T resource)
    {
        // try to clean the resource

        switch (resource)
        {
            case IList list:
                list.Clear();
                return;

            case IDictionary dict:
                dict.Clear();
                return;

            case IPoolable poolable:
                poolable.Clean();
                return;

            default:
                throw new Exception($"Pool doesn't support cleaning objects of type {typeof(T)}");
        }
    }

    public static void Return(ref T resource)
    {
        Clean(resource);
        ReturnCleaned(ref resource);
    }

    public static void ReturnCleanedUnsafe(T resource) => ReturnCleaned(ref resource);

    public static void ReturnCleaned(ref T resource)
    {
        if (resource.GetType() != typeof(T))
            throw new ArgumentException("Returned resource is of type " + resource.GetType() + ", but the pool is for type: " + typeof(T));

        Interlocked.Decrement(ref borrowedCount);

        pool.Push(resource);

        resource = default;
    }
}
