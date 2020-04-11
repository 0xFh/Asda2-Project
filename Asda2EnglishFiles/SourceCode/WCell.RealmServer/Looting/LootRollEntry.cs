using System;
using WCell.Constants.Looting;

namespace WCell.RealmServer.Looting
{
    /// <summary>
    /// A LootRollEntry consists of the number that was rolled and the type of Roll that the looter decided
    /// </summary>
    public struct LootRollEntry : IComparable
    {
        private readonly int m_num;
        private readonly LootRollType m_type;

        public LootRollEntry(int num, LootRollType type)
        {
            this.m_num = num;
            this.m_type = type;
        }

        public int Number
        {
            get { return this.m_num; }
        }

        public LootRollType Type
        {
            get { return this.m_type; }
        }

        public override int GetHashCode()
        {
            return this.m_num + (this.m_type == LootRollType.Need ? 100 : 0);
        }

        public override bool Equals(object obj)
        {
            if (obj is LootRollEntry && ((LootRollEntry) obj).m_type == this.m_type)
                return ((LootRollEntry) obj).m_num == this.m_num;
            return false;
        }

        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings: Value Meaning Less than zero This instance is less than <paramref name="obj" />. Zero This instance is equal to <paramref name="obj" />. Greater than zero This instance is greater than <paramref name="obj" />.
        /// </returns>
        /// <param name="obj">An object to compare with this instance. </param>
        /// <exception cref="T:System.ArgumentException"><paramref name="obj" /> is not the same type as this instance. </exception>
        /// <filterpriority>2</filterpriority>
        public int CompareTo(object obj)
        {
            if (obj is LootRollEntry)
                return this.GetHashCode() - ((LootRollEntry) obj).GetHashCode();
            throw new ArgumentException("obj must be of type is LootRollEntry, but is of: " + (object) obj.GetType());
        }

        public override string ToString()
        {
            return ((int) this.m_type).ToString() + " " + (object) this.m_num;
        }
    }
}