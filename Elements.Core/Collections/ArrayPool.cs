using System;
using System.Collections.Concurrent;

namespace Elements.Core;

public class ArrayPool<T>
{
    int[] _sizeGroups;
    ConcurrentStack<T[]>[] _pools;

    public int MaxGroupSize = 1024 * 50; // 50 kB should be enough for everybody

    public ArrayPool(params int[] _sizeGroups)
    {
        // sort just to be sure
        Array.Sort(_sizeGroups);

        this._sizeGroups = _sizeGroups;
        this._pools = new ConcurrentStack<T[]>[_sizeGroups.Length];

        for (int i = 0; i < _sizeGroups.Length; i++)
            _pools[i] = new ConcurrentStack<T[]>();
    }

    public T[] GetArray(int size)
    {
        for (int i = 0; i < _sizeGroups.Length; i++)
            if (_sizeGroups[i] >= size)
                return GetArray(_pools[i], _sizeGroups[i]);

        // too large, just make a new one, should be rare if properly configured
        return new T[size];
    }

    public void ReturnArray(T[] array)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));

        for (int i = 0; i < _sizeGroups.Length; i++)
            if (_sizeGroups[i] == array.Length)
            {
                // just throw it away, there's too many of them already
                if ((_pools[i].Count + 1) * _sizeGroups[i] > MaxGroupSize)
                    break;

                Array.Clear(array, 0, array.Length);
                _pools[i].Push(array);

                OnReturn?.Invoke(array.Length);

                return;
            }

        OnThrowAway?.Invoke(array.Length);
    }

    T[] GetArray(ConcurrentStack<T[]> pool, int size)
    {
        if (pool.TryPop(out T[] array))
        {
            OnBorrow?.Invoke(size);
            return array;
        }

        OnAllocate?.Invoke(size);

        return new T[size];
    }

    public event Action<int> OnAllocate;
    public event Action<int> OnBorrow;
    public event Action<int> OnReturn;
    public event Action<int> OnThrowAway;
}
