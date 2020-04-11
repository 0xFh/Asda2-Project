using System;
using WCell.Constants.Factions;
using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells
{
    [Serializable]
    public class SpellSummonEntry
    {
        /// <summary>If set to false, the amount determines health</summary>
        public bool DetermineAmountBySpellEffect = true;

        public SummonType Id;
        public SummonGroup Group;
        public FactionTemplateId FactionTemplateId;
        public SummonPropertyType Type;
        public uint Slot;
        public SummonFlags Flags;
        public SpellSummonHandler Handler;
    }
}