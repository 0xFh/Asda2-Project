using NLog;
using WCell.Constants.Spells;

namespace WCell.RealmServer.GameObjects.GOEntries
{
    public class GOAuraGeneratorEntry : GOEntry
    {
        private static readonly Logger sLog = LogManager.GetCurrentClassLogger();

        /// <summary>???</summary>
        public bool StartOpen
        {
            get
            {
                if (this.Fields[0] < 2)
                    return this.Fields[0] == 1;
                GOAuraGeneratorEntry.sLog.Error(
                    "GOAuraGeneratorEntry: Invalid value found for StartOpen: {0}, defaulting to false.",
                    this.Fields[0]);
                return false;
            }
            set { this.Fields[0] = value ? 1 : 0; }
        }

        /// <summary>Area of effect (?)</summary>
        public int Radius
        {
            get { return this.Fields[1]; }
            set { this.Fields[1] = value; }
        }

        /// <summary>SpellId from Spells.dbc</summary>
        public SpellId AuraId1
        {
            get { return (SpellId) this.Fields[2]; }
        }

        /// <summary>???</summary>
        public int ConditionId1
        {
            get { return this.Fields[3]; }
            set { this.Fields[3] = value; }
        }

        /// <summary>SpellId from Spells.dbc</summary>
        public SpellId AuraId2
        {
            get { return (SpellId) this.Fields[4]; }
        }

        /// <summary>???</summary>
        public int ConditionId2
        {
            get { return this.Fields[5]; }
            set { this.Fields[5] = value; }
        }

        /// <summary>???</summary>
        public bool ServerOnly
        {
            get
            {
                if (this.Fields[6] < 2)
                    return this.Fields[6] == 1;
                GOAuraGeneratorEntry.sLog.Error(
                    "GOButtonEntry: Invalid value found for StartOpen: {0}, defaulting to false.", this.Fields[6]);
                return false;
            }
            set { this.Fields[6] = value ? 1 : 0; }
        }
    }
}