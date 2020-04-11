using System;
using System.Collections;
using System.Collections.Generic;
using WCell.Constants.Updates;
using WCell.RealmServer.AI.Brains;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI.Groups
{
    [Serializable]
    public class AIGroup : IList<NPC>, ICollection<NPC>, IEnumerable<NPC>, IEnumerable
    {
        private NPC m_Leader;
        private readonly List<NPC> groupList;

        public AIGroup()
        {
            this.groupList = new List<NPC>();
        }

        public AIGroup(NPC leader)
            : this()
        {
            this.m_Leader = leader;
            if (leader == null || this.Contains(leader))
                return;
            this.Add(leader);
        }

        public AIGroup(IEnumerable<NPC> mobs)
        {
            this.groupList = new List<NPC>(mobs);
        }

        public NPC Leader
        {
            get { return this.m_Leader; }
            set
            {
                this.m_Leader = value;
                if (value == null || this.Contains(value))
                    return;
                this.Add(value);
            }
        }

        public virtual BrainState DefaultState
        {
            get { return BaseBrain.DefaultBrainState; }
        }

        public virtual UpdatePriority UpdatePriority
        {
            get { return UpdatePriority.Background; }
        }

        public void Aggro(Unit unit)
        {
            foreach (NPC npc in this)
                npc.ThreatCollection.AddNewIfNotExisted(unit);
        }

        public IEnumerator<NPC> GetEnumerator()
        {
            return (IEnumerator<NPC>) this.groupList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator) this.GetEnumerator();
        }

        /// <summary>Adds the given NPC to this group</summary>
        public void Add(NPC npc)
        {
            if (!npc.IsAlive)
                return;
            this.groupList.Add(npc);
            npc.Group = this;
            if (this.Leader == null)
            {
                this.m_Leader = npc;
            }
            else
            {
                if (npc == this.Leader)
                    return;
                Unit currentAggressor = this.Leader.ThreatCollection.CurrentAggressor;
                if (currentAggressor == null)
                    return;
                npc.ThreatCollection[currentAggressor] = 2 * npc.ThreatCollection[currentAggressor] + 1;
                foreach (KeyValuePair<Unit, int> threat in this.m_Leader.ThreatCollection)
                    npc.ThreatCollection.AddNewIfNotExisted(threat.Key);
            }
        }

        public void Clear()
        {
            this.groupList.Clear();
        }

        public bool Contains(NPC item)
        {
            return this.groupList.Contains(item);
        }

        void ICollection<NPC>.CopyTo(NPC[] array, int arrayIndex)
        {
            this.groupList.CopyTo(array, arrayIndex);
        }

        public bool Remove(NPC npc)
        {
            if (!this.groupList.Remove(npc))
                return false;
            if (npc == this.m_Leader)
                this.m_Leader = (NPC) null;
            npc.Group = (AIGroup) null;
            return true;
        }

        public int Count
        {
            get { return this.groupList.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int IndexOf(NPC item)
        {
            return this.groupList.IndexOf(item);
        }

        void IList<NPC>.Insert(int index, NPC item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            this.groupList.RemoveAt(index);
        }

        public NPC this[int index]
        {
            get { return this.groupList[index]; }
            set { this.groupList[index] = value; }
        }
    }
}