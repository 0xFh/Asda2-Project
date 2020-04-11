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
            this.m_freeIds = new LockfreeQueue<uint>();
        }

        public uint NextId()
        {
            if (this.m_freeIds.Count > 0)
                return this.m_freeIds.Dequeue();
            return (uint) Interlocked.Increment(ref this.m_currentId);
        }

        public void RecycleId(uint id)
        {
            this.m_freeIds.Enqueue(id);
        }

        public void Load(List<uint> usedIds)
        {
            uint num = 0;
            for (uint id = 0; (long) id < (long) usedIds.Count; ++id)
            {
                if (!usedIds.Contains(id))
                    this.RecycleId(id);
                else if (id > num)
                    num = id;
            }
        }
    }
}