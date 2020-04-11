using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.Util;
using WCell.Util.Data;

namespace WCell.RealmServer.Spells
{
    /// <summary>
    /// Represents a row from UDB's spell_proc_event database table.
    /// Defines some corrections for spells
    /// </summary>
    public class SpellProcEventEntry : IDataHolder
    {
        public SpellId SpellId;
        public DamageSchoolMask SchoolMask;
        public SpellClassSet SpellClassSet;
        [Persistent(3)] public uint[] SpellFamilyMask0;
        [Persistent(3)] public uint[] SpellFamilyMask1;
        [Persistent(3)] public uint[] SpellFamilyMask2;
        public ProcTriggerFlags ProcFlags;
        public ProcFlagsExLegacy ProcFlagsEx;
        public float PpmRate;
        public float CustomChance;
        public uint Cooldown;

        public uint[] GetSpellFamilyMask(EffectIndex index)
        {
            switch (index)
            {
                case EffectIndex.Zero:
                    return this.SpellFamilyMask0;
                case EffectIndex.One:
                    return this.SpellFamilyMask1;
                case EffectIndex.Two:
                    return this.SpellFamilyMask2;
                default:
                    return (uint[]) null;
            }
        }

        [NotPersistent] public DataHolderState DataHolderState { get; set; }

        public void FinalizeDataHolder()
        {
            ProcEventHelper.Entries.Add(this.SpellId, this);
            if (((IEnumerable<uint>) this.SpellFamilyMask0).Sum() == 0U)
                this.SpellFamilyMask0 = (uint[]) null;
            if (((IEnumerable<uint>) this.SpellFamilyMask1).Sum() == 0U)
                this.SpellFamilyMask1 = (uint[]) null;
            if (((IEnumerable<uint>) this.SpellFamilyMask2).Sum() != 0U)
                return;
            this.SpellFamilyMask2 = (uint[]) null;
        }
    }
}