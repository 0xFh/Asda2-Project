using System.Collections.Generic;
using System.Threading;

namespace WCell.Util.Collections
{
  /// <summary>Allows reusable uint Ids</summary>
  public class IdQueue
  {
    private int m_currentId;
    private readonly LockfreeQueue<uint> m_freeIds;

    public IdQueue()
    {
      m_freeIds = new LockfreeQueue<uint>();
    }

    public uint NextId()
    {
      if(m_freeIds.Count > 0)
        return m_freeIds.Dequeue();
      return (uint) Interlocked.Increment(ref m_currentId);
    }

    public void RecycleId(uint id)
    {
      m_freeIds.Enqueue(id);
    }

    public void Load(List<uint> usedIds)
    {
      uint num = 0;
      for(uint id = 0; (long) id < (long) usedIds.Count; ++id)
      {
        if(!usedIds.Contains(id))
          RecycleId(id);
        else if(id > num)
          num = id;
      }
    }
  }
}