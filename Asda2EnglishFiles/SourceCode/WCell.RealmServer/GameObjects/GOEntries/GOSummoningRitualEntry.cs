using NLog;
using WCell.Constants.Spells;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOSummoningRitualEntry : GOEntry
    {
        private static readonly Logger sLog = LogManager.GetCurrentClassLogger();

        /// <summary>Amount of casters?</summary>
        public int CasterCount
        {
            get { return this.Fields[0]; }
        }

        /// <summary>SpellId</summary>
        public SpellId SpellId
        {
            get { return (SpellId) this.Fields[1]; }
        }

        /// <summary>SpellId</summary>
        public SpellId AnimSpellId
        {
            get { return (SpellId) this.Fields[2]; }
        }

        /// <summary>???</summary>
        public bool RitualPersistent
        {
            get { return this.Fields[3] != 0; }
        }

        /// <summary>SpellId</summary>
        public SpellId CasterTargetSpellId
        {
            get { return (SpellId) this.Fields[4]; }
        }

        /// <summary>??? Not sure if this is actually a bool</summary>
        public bool CasterTargetSpellTargets
        {
            get
            {
                if (this.Fields[5] < 2)
                    return this.Fields[5] == 1;
                GOSummoningRitualEntry.sLog.Error(
                    "GOSummoningRitualEntry: Invalid value found for CasterTargetSpellTargets: {0}, defaulting to false.",
                    this.Fields[5]);
                return false;
            }
            set { this.Fields[5] = value ? 1 : 0; }
        }

        /// <summary>
        /// Whether or not the Casters of this SummoningRitual are in the same Group
        /// </summary>
        public bool CastersGrouped
        {
            get { return this.Fields[6] > 0; }
        }

        public bool RitualNoTargetCheck
        {
            get { return this.Fields[7] != 0; }
        }
    }
}