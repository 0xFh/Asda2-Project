using System;
using System.Collections;
using System.Collections.Generic;

namespace WCell.Util
{
    internal class SortedTopN<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
    {
        private int _n;
        private List<TKey> _topNKeys;
        private List<TValue> _topNValues;
        private IComparer<TKey> _comparer;

        public SortedTopN(int count, IComparer<TKey> comparer)
        {
            if (count < 1)
                throw new ArgumentOutOfRangeException(nameof(count));
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));
            this._n = count;
            this._topNKeys = new List<TKey>(count);
            this._topNValues = new List<TValue>(count);
            this._comparer = comparer;
        }

        public bool Add(KeyValuePair<TKey, TValue> item)
        {
            return this.Add(item.Key, item.Value);
        }

        public bool Add(TKey key, TValue value)
        {
            int index = this._topNKeys.BinarySearch(key, this._comparer);
            if (index < 0)
                index = ~index;
            if (this._topNKeys.Count >= this._n && index == 0)
                return false;
            if (this._topNKeys.Count == this._n)
            {
                this._topNKeys.RemoveAt(0);
                this._topNValues.RemoveAt(0);
                --index;
            }

            if (index < this._n)
            {
                this._topNKeys.Insert(index, key);
                this._topNValues.Insert(index, value);
            }
            else
            {
                this._topNKeys.Add(key);
                this._topNValues.Add(value);
            }

            return true;
        }

        public IEnumerable<TValue> Values
        {
            get
            {
                for (int i = this._topNKeys.Count - 1; i >= 0; --i)
                    yield return this._topNValues[i];
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            for (int i = this._topNKeys.Count - 1; i >= 0; --i)
                yield return new KeyValuePair<TKey, TValue>(this._topNKeys[i], this._topNValues[i]);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator) this.GetEnumerator();
        }
    }
}