using C5;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Elements.Core
{
    public class WeightedList<T> : IEnumerable<T>
    {
        readonly struct WeightedItem : IComparable<WeightedItem>
        {
            public readonly T item;
            public readonly float cumulativeWeight;

            public WeightedItem(T item, float cumulativeWeight)
            {
                this.item = item;
                this.cumulativeWeight = cumulativeWeight;
            }

            public int CompareTo(WeightedItem other) => cumulativeWeight.CompareTo(other.cumulativeWeight);
        }

        TreeBag<WeightedItem> _tree = new TreeBag<WeightedItem>();

        public float TotalWeight { get; private set; }
        public int Count => _tree.Count;

        public void Add(T item, float weight)
        {
            if(weight <= 0)
                throw new ArgumentOutOfRangeException(nameof(weight));

            TotalWeight += weight;

            _tree.Add(new WeightedItem(item, TotalWeight));
        }

        public void Clear()
        {
            TotalWeight = 0f;
            _tree.Clear();
        }

        public T GetItemAt(float position)
        {
            if (position < 0 || position > TotalWeight)
                throw new ArgumentOutOfRangeException(nameof(position), position, $"TotalWeight: {TotalWeight}");

            return _tree.WeakSuccessor(new WeightedItem(default, position)).item;
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in _tree)
                yield return item.item;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
