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
      if(count < 1)
        throw new ArgumentOutOfRangeException(nameof(count));
      if(comparer == null)
        throw new ArgumentNullException(nameof(comparer));
      _n = count;
      _topNKeys = new List<TKey>(count);
      _topNValues = new List<TValue>(count);
      _comparer = comparer;
    }

    public bool Add(KeyValuePair<TKey, TValue> item)
    {
      return Add(item.Key, item.Value);
    }

    public bool Add(TKey key, TValue value)
    {
      int index = _topNKeys.BinarySearch(key, _comparer);
      if(index < 0)
        index = ~index;
      if(_topNKeys.Count >= _n && index == 0)
        return false;
      if(_topNKeys.Count == _n)
      {
        _topNKeys.RemoveAt(0);
        _topNValues.RemoveAt(0);
        --index;
      }

      if(index < _n)
      {
        _topNKeys.Insert(index, key);
        _topNValues.Insert(index, value);
      }
      else
      {
        _topNKeys.Add(key);
        _topNValues.Add(value);
      }

      return true;
    }

    public IEnumerable<TValue> Values
    {
      get
      {
        for(int i = _topNKeys.Count - 1; i >= 0; --i)
          yield return _topNValues[i];
      }
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
      for(int i = _topNKeys.Count - 1; i >= 0; --i)
        yield return new KeyValuePair<TKey, TValue>(_topNKeys[i], _topNValues[i]);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}