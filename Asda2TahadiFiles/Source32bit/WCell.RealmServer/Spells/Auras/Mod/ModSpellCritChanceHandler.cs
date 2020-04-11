using WCell.RealmServer.Entities;
using WCell.Util;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>Mods Spell crit chance in %</summary>
  public class ModSpellCritChanceHandler : AuraEffectHandler
  {
    private static uint[] AllDamageSchoolSet = Utility.GetSetIndices((uint) sbyte.MaxValue);

    protected override void Apply()
    {
      Character owner = Owner as Character;
      if(owner == null)
        return;
      if(m_spellEffect.MiscValue == 0)
        owner.ModCritMod(AllDamageSchoolSet, EffectValue);
      else
        owner.ModCritMod(m_spellEffect.MiscBitSet, EffectValue);
    }

    protected override void Remove(bool cancelled)
    {
      Character owner = Owner as Character;
      if(owner == null)
        return;
      if(m_spellEffect.MiscValue == 0)
        owner.ModCritMod(AllDamageSchoolSet, -EffectValue);
      else
        owner.ModCritMod(m_spellEffect.MiscBitSet, -EffectValue);
    }
  }
}