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
      get { return _dictionary[key]; }
    }

    public BaseImmutableDictionary()
    {
      _dictionary = new Dictionary<TKey, TValue>();
    }

    public BaseImmutableDictionary(int capacity)
    {
      _dictionary = new Dictionary<TKey, TValue>(capacity);
    }

    public int Count
    {
      get { return _dictionary.Count; }
    }

    public BaseImmutableDictionary<TKey, TValue> Add(TKey key, TValue value)
    {
      return CopyAndMutate(dictionary => dictionary.Add(key, value));
    }

    public BaseImmutableDictionary<TKey, TValue> Remove(TKey key)
    {
      return CopyAndMutate(dictionary => dictionary.Remove(key));
    }

    public BaseImmutableDictionary<TKey, TValue> Clear()
    {
      return CopyAndMutate(dictionary => dictionary.Clear());
    }

    public bool ContainsKey(TKey key)
    {
      return _dictionary.ContainsKey(key);
    }

    private BaseImmutableDictionary<TKey, TValue> CopyAndMutate(Action<Dictionary<TKey, TValue>> mutator)
    {
      BaseImmutableDictionary<TKey, TValue> immutableDictionary = new BaseImmutableDictionary<TKey, TValue>();
      foreach(KeyValuePair<TKey, TValue> keyValuePair in _dictionary)
        immutableDictionary._dictionary.Add(keyValuePair.Key, keyValuePair.Value);
      mutator(immutableDictionary._dictionary);
      return immutableDictionary;
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
      return _dictionary.GetEnumerator();
    }

    public BaseImmutableDictionary<TKey, TValue> SetValue(TKey key, TValue value)
    {
      return CopyAndMutate(dictionary => dictionary[key] = value);
    }

    public Dictionary<TKey, TValue>.KeyCollection Keys
    {
      get { return _dictionary.Keys; }
    }

    public Dictionary<TKey, TValue>.ValueCollection Values
    {
      get { return _dictionary.Values; }
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
      return _dictionary.TryGetValue(key, out value);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}