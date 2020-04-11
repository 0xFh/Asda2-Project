using System;
using System.Collections;
using System.Collections.Generic;

namespace WCell.Util.Collections
{
    public sealed class BaseImmutableDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
    {
        private Dictionary<TKey, TValue> _dictionary;

        public TValue this[TKey key]
        {
            get { return this._dictionary[key]; }
        }

        public BaseImmutableDictionary()
        {
            this._dictionary = new Dictionary<TKey, TValue>();
        }

        public BaseImmutableDictionary(int capacity)
        {
            this._dictionary = new Dictionary<TKey, TValue>(capacity);
        }

        public int Count
        {
            get { return this._dictionary.Count; }
        }

        public BaseImmutableDictionary<TKey, TValue> Add(TKey key, TValue value)
        {
            return this.CopyAndMutate((Action<Dictionary<TKey, TValue>>) (dictionary => dictionary.Add(key, value)));
        }

        public BaseImmutableDictionary<TKey, TValue> Remove(TKey key)
        {
            return this.CopyAndMutate((Action<Dictionary<TKey, TValue>>) (dictionary => dictionary.Remove(key)));
        }

        public BaseImmutableDictionary<TKey, TValue> Clear()
        {
            return this.CopyAndMutate((Action<Dictionary<TKey, TValue>>) (dictionary => dictionary.Clear()));
        }

        public bool ContainsKey(TKey key)
        {
            return this._dictionary.ContainsKey(key);
        }

        private BaseImmutableDictionary<TKey, TValue> CopyAndMutate(Action<Dictionary<TKey, TValue>> mutator)
        {
            BaseImmutableDictionary<TKey, TValue> immutableDictionary = new BaseImmutableDictionary<TKey, TValue>();
            foreach (KeyValuePair<TKey, TValue> keyValuePair in this._dictionary)
                immutableDictionary._dictionary.Add(keyValuePair.Key, keyValuePair.Value);
            mutator(immutableDictionary._dictionary);
            return immutableDictionary;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return (IEnumerator<KeyValuePair<TKey, TValue>>) this._dictionary.GetEnumerator();
        }

        public BaseImmutableDictionary<TKey, TValue> SetValue(TKey key, TValue value)
        {
            return this.CopyAndMutate((Action<Dictionary<TKey, TValue>>) (dictionary => dictionary[key] = value));
        }

        public Dictionary<TKey, TValue>.KeyCollection Keys
        {
            get { return this._dictionary.Keys; }
        }

        public Dictionary<TKey, TValue>.ValueCollection Values
        {
            get { return this._dictionary.Values; }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return this._dictionary.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator) this.GetEnumerator();
        }
    }
}