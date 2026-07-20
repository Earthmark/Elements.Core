using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Elements.Core
{
    public class KeyCounter<T> : IEnumerable<KeyValuePair<T, int>>
    {
        readonly Dictionary<T, int> keys = new Dictionary<T, int>();

        public Dictionary<T, int>.KeyCollection Keys => keys.Keys;

        public int KeyCount => keys.Count;

        public bool AllowNegativeValues { get; private set; }

        public KeyCounter()
        {
            AllowNegativeValues = false;
        }

        public KeyCounter(bool allowNegativeValues = false)
        {
            AllowNegativeValues = allowNegativeValues;
        }

        public int this[T key]
        {
            get
            {
                keys.TryGetValue(key, out int count);
                return count;
            }

            set
            {
                if (keys.ContainsKey(key))
                    keys[key] = value;
                else
                    keys.Add(key, value);
            }
        }

        public T FindItemWithHighestCount()
        {
            int max = int.MinValue;
            T maxItem = default;

            foreach(var key in keys)
                if(key.Value > max)
                {
                    max = key.Value;
                    maxItem = key.Key;
                }

            return maxItem;
        }

        public T FindItemWithLowestCount()
        {
            int min = int.MaxValue;
            T minItem = default;

            foreach (var key in keys)
                if (key.Value < min)
                {
                    min = key.Value;
                    minItem = key.Key;
                }

            return minItem;
        }

        public int Increment(T key) => Add(key, 1);
        public int Decrement(T key) => Subtract(key, 1);

        public int Add(T key, int amount)
        {
            if (!keys.TryGetValue(key, out int count))
            {
                keys.Add(key, amount);
                return amount;
            }
            else
            {
                count += amount;
                keys[key] = count;

                return count;
            }
        }

        public int Subtract(T key, int amount)
        {
            if (!keys.TryGetValue(key, out int count))
            {
                if(!AllowNegativeValues)
                    throw new Exception("Current count of the given key is zero, cannot subtract! Key: " + key);

                count = 0;
            }

            count -= amount;

            if (!AllowNegativeValues)
            {
                if (count < 0)
                    throw new Exception("Resulting count of given key is less than zero! Key: " + key);
            }

            if (count == 0)
                keys.Remove(key);
            else
                keys[key] = count;

            return count;
        }

        public int Take(T key)
        {
            if (!keys.TryGetValue(key, out var count))
                return 0;

            keys.Remove(key);

            return count;
        }

        public int Sum
        {
            get
            {
                int count = 0;

                foreach (var group in this)
                    count += group.Value;

                return count;
            }
        }

        public void Clear() => keys.Clear();

        public IEnumerator<KeyValuePair<T, int>> GetEnumerator() => ((IEnumerable<KeyValuePair<T, int>>)keys).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<KeyValuePair<T, int>>)keys).GetEnumerator();
    }
}
