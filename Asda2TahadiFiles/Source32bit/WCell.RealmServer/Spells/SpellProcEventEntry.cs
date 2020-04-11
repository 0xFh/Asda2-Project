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
    [Persistent(3)]public uint[] SpellFamilyMask0;
    [Persistent(3)]public uint[] SpellFamilyMask1;
    [Persistent(3)]public uint[] SpellFamilyMask2;
    public ProcTriggerFlags ProcFlags;
    public ProcFlagsExLegacy ProcFlagsEx;
    public float PpmRate;
    public float CustomChance;
    public uint Cooldown;

    public uint[] GetSpellFamilyMask(EffectIndex index)
    {
      switch(index)
      {
        case EffectIndex.Zero:
          return SpellFamilyMask0;
        case EffectIndex.One:
          return SpellFamilyMask1;
        case EffectIndex.Two:
          return SpellFamilyMask2;
        default:
          return null;
      }
    }

    [NotPersistent]
    public DataHolderState DataHolderState { get; set; }

    public void FinalizeDataHolder()
    {
      ProcEventHelper.Entries.Add(SpellId, this);
      if(SpellFamilyMask0.Sum() == 0U)
        SpellFamilyMask0 = null;
      if(SpellFamilyMask1.Sum() == 0U)
        SpellFamilyMask1 = null;
      if(SpellFamilyMask2.Sum() != 0U)
        return;
      SpellFamilyMask2 = null;
    }
  }
}