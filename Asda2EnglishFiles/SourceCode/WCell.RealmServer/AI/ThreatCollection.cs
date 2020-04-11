using System;
using System.Collections;
using System.Collections.Generic;
using WCell.RealmServer.AI.Groups;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.AI
{
    /// <summary>
    /// Collection representing threat values of an NPC against its foes
    /// TODO: Implement as priority list (B-Tree or Fibonacci Heap)
    /// </summary>
    public class ThreatCollection : IEnumerable<KeyValuePair<Unit, int>>, IEnumerable
    {
        /// <summary>
        /// Percentage of the current highest threat to become the new one.
        /// </summary>
        public static int RequiredHighestThreatPct = 110;

        public readonly List<KeyValuePair<Unit, int>> AggressorPairs;
        private Unit m_CurrentAggressor;
        private int m_highestThreat;
        private Unit m_taunter;
        protected internal AIGroup m_group;

        public ThreatCollection()
        {
            this.AggressorPairs = new List<KeyValuePair<Unit, int>>(5);
        }

        public int Size
        {
            get { return this.AggressorPairs.Count; }
        }

        /// <summary>The NPC is forced to only attack the taunter</summary>
        public Unit Taunter
        {
            get { return this.m_taunter; }
            set
            {
                if (value != null)
                {
                    this.m_taunter = this.m_CurrentAggressor = value;
                    this.m_highestThreat = int.MaxValue;
                    this.OnNewAggressor(value);
                }
                else
                {
                    this.m_taunter = (Unit) null;
                    this.FindNewAggressor();
                }
            }
        }

        public Unit CurrentAggressor
        {
            get { return this.m_CurrentAggressor; }
        }

        /// <summary>The AIGroup the owner of this collection belongs to</summary>
        public AIGroup Group
        {
            get { return this.m_group; }
            internal set { this.m_group = value; }
        }

        /// <summary>
        /// Use this indexer to get or set absolute values of Threat.
        /// Returns -1 for non-aggressor Units.
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public int this[Unit unit]
        {
            get
            {
                foreach (KeyValuePair<Unit, int> aggressorPair in this.AggressorPairs)
                {
                    if (aggressorPair.Key == unit)
                        return aggressorPair.Value;
                }

                return -1;
            }
            set
            {
                if (!unit.CanGenerateThreat)
                    return;
                int index1 = this.GetIndex(unit);
                int index2 = index1;
                if (index1 == -1)
                {
                    index2 = this.AggressorPairs.Count;
                    while (index2 - 1 >= 0 && this.AggressorPairs[index2 - 1].Value < value)
                        --index2;
                    KeyValuePair<Unit, int> keyValuePair = new KeyValuePair<Unit, int>(unit, value);
                    this.AggressorPairs.Insert(index2, keyValuePair);
                }
                else
                {
                    KeyValuePair<Unit, int> aggressorPair = this.AggressorPairs[index1];
                    if (value == aggressorPair.Value)
                        return;
                    if (value > aggressorPair.Value)
                    {
                        while (index2 - 1 >= 0 && this.AggressorPairs[index2 - 1].Value < value)
                            --index2;
                    }
                    else
                    {
                        while (index2 + 1 < this.AggressorPairs.Count && this.AggressorPairs[index2 + 1].Value > value)
                            ++index2;
                    }

                    this.AggressorPairs.RemoveAt(index1);
                    this.AggressorPairs.Insert(index2, new KeyValuePair<Unit, int>(aggressorPair.Key, value));
                }

                if (this.m_taunter != null)
                    return;
                if (unit == this.m_CurrentAggressor)
                {
                    this.m_highestThreat = value;
                    if (index2 == 0 || !this.IsNewHighestThreat(this.AggressorPairs[0].Value))
                        return;
                    this.m_CurrentAggressor = this.AggressorPairs[0].Key;
                    this.m_highestThreat = this.AggressorPairs[0].Value;
                    this.OnNewAggressor(this.m_CurrentAggressor);
                }
                else
                {
                    if ((index2 != 0 || !this.IsNewHighestThreat(value)) && this.m_CurrentAggressor != null)
                        return;
                    this.m_CurrentAggressor = unit;
                    this.m_highestThreat = value;
                    this.OnNewAggressor(this.m_CurrentAggressor);
                }
            }
        }

        /// <summary>Call this method when encountering a new Unit</summary>
        /// <param name="unit"></param>
        public void AddNewIfNotExisted(Unit unit)
        {
            ThreatCollection threatCollection;
            Unit index;
            (threatCollection = this)[index = unit] = threatCollection[index];
        }

        private void OnNewAggressor(Unit unit)
        {
            if (this.m_group == null)
                return;
            this.m_group.Aggro(unit);
        }

        public bool HasAggressor(Unit unit)
        {
            return this[unit] >= 0;
        }

        public KeyValuePair<Unit, int> GetThreat(Unit unit)
        {
            foreach (KeyValuePair<Unit, int> aggressorPair in this.AggressorPairs)
            {
                if (aggressorPair.Key == unit)
                    return aggressorPair;
            }

            return new KeyValuePair<Unit, int>();
        }

        public int GetIndex(Unit unit)
        {
            for (int index = 0; index < this.AggressorPairs.Count; ++index)
            {
                if (this.AggressorPairs[index].Key == unit)
                    return index;
            }

            return -1;
        }

        private void FindNewAggressor()
        {
            if (this.AggressorPairs.Count == 0)
            {
                this.m_CurrentAggressor = (Unit) null;
                this.m_highestThreat = 0;
            }
            else
            {
                KeyValuePair<Unit, int> aggressorPair = this.AggressorPairs[0];
                this.m_CurrentAggressor = aggressorPair.Key;
                this.m_highestThreat = aggressorPair.Value;
                this.OnNewAggressor(aggressorPair.Key);
            }
        }

        /// <summary>
        /// Whether the given amount is at least RequiredHighestThreatPct more than the current highest Threat
        /// </summary>
        /// <param name="threat"></param>
        /// <returns></returns>
        public bool IsNewHighestThreat(int threat)
        {
            return threat > (this.m_highestThreat * ThreatCollection.RequiredHighestThreatPct + 50) / 100;
        }

        /// <summary>
        /// Returns an array of size 0-max, containing the Units with the highest Threat and their amount.
        /// </summary>
        /// <param name="max"></param>
        public KeyValuePair<Unit, int>[] GetHighestThreatAggressorPairs(int max)
        {
            KeyValuePair<Unit, int>[] keyValuePairArray =
                new KeyValuePair<Unit, int>[Math.Min(max, this.AggressorPairs.Count)];
            for (int length = keyValuePairArray.Length; length >= 0; --length)
                keyValuePairArray[length] = this.AggressorPairs[length];
            return keyValuePairArray;
        }

        /// <summary>
        /// Returns an array of size 0-max, containing the Units with the highest Threat.
        /// </summary>
        /// <param name="max"></param>
        public Unit[] GetHighestThreatAggressors(int max)
        {
            Unit[] unitArray = new Unit[Math.Min(max, this.AggressorPairs.Count)];
            for (int length = unitArray.Length; length >= 0; --length)
                unitArray[length] = this.AggressorPairs[length].Key;
            return unitArray;
        }

        /// <summary>
        /// Returns the aggressor at the given 0-based index within the collection.
        /// Selects the least one in the list, if there is no such low rank
        /// Note: The aggressor with Rank = 0 is usually the CurrentAggressor
        /// </summary>
        public Unit GetAggressorByThreatRank(int rank)
        {
            if (this.AggressorPairs.Count <= rank)
                return this.AggressorPairs[this.AggressorPairs.Count - 1].Key;
            return this.AggressorPairs[rank].Key;
        }

        public void Remove(Unit unit)
        {
            for (int index = 0; index < this.AggressorPairs.Count; ++index)
            {
                if (this.AggressorPairs[index].Key == unit)
                    this.AggressorPairs.RemoveAt(index);
            }

            if (this.m_taunter == unit)
            {
                this.Taunter = (Unit) null;
            }
            else
            {
                if (this.m_CurrentAggressor != unit)
                    return;
                this.FindNewAggressor();
            }
        }

        /// <summary>Removes all Threat</summary>
        public void Clear()
        {
            this.AggressorPairs.Clear();
            this.m_CurrentAggressor = (Unit) null;
            this.m_highestThreat = -1;
            this.m_taunter = (Unit) null;
        }

        public IEnumerator<KeyValuePair<Unit, int>> GetEnumerator()
        {
            return (IEnumerator<KeyValuePair<Unit, int>>) this.AggressorPairs.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator) this.GetEnumerator();
        }
    }
}