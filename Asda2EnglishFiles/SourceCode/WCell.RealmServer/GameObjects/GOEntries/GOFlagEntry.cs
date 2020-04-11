using NLog;
using WCell.Constants;
using WCell.Constants.Spells;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public abstract class GOFlagEntry : GOEntry
    {
        private static readonly Logger sLog = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Represents the side owning this entry.
        /// Default value: Horde
        /// </summary>
        public BattlegroundSide Side = BattlegroundSide.Horde;

        protected int m_damageIndex;

        /// <summary>LockId from Lock.dbc</summary>
        public int LockId
        {
            get { return this.Fields[0]; }
        }

        public abstract bool NoDamageImmune { get; }

        public abstract int OpenTextId { get; }

        public abstract SpellId PickupSpellId { get; }
    }
}