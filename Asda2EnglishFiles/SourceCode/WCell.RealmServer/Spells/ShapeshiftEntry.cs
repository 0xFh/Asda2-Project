using WCell.Constants;
using WCell.Constants.NPCs;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.NPCs;

namespace WCell.RealmServer.Spells
{
    public class ShapeshiftEntry
    {
        public PowerType PowerType = PowerType.End;
        public ShapeshiftForm Id;
        public uint BarOrder;
        public string Name;
        public ShapeshiftInfoFlags Flags;
        public CreatureType CreatureType;

        /// <summary>In millis</summary>
        public int AttackTime;

        public uint ModelIdAlliance;
        public uint ModelIdHorde;
        public SpellId[] DefaultActionBarSpells;

        public UnitModelInfo ModelAlliance
        {
            get { return UnitMgr.GetModelInfo(this.ModelIdAlliance); }
        }

        public UnitModelInfo ModelHorde
        {
            get { return UnitMgr.GetModelInfo(this.ModelIdHorde); }
        }
    }
}