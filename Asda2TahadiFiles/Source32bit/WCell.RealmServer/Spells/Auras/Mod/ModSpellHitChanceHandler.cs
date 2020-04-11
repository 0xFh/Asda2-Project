using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  /// <summary>Mods Spell crit chance in %</summary>
  public class ModSpellHitChanceHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      Unit owner = Owner;
      if(m_spellEffect.MiscValue == 0)
      {
        for(DamageSchool school = DamageSchool.Physical; school < DamageSchool.Count; ++school)
          owner.ModSpellHitChance(school, EffectValue);
      }
      else
        owner.ModSpellHitChance(m_spellEffect.MiscBitSet, EffectValue);
    }

    protected override void Remove(bool cancelled)
    {
      Unit owner = Owner;
      if(m_spellEffect.MiscValue == 0)
      {
        for(DamageSchool school = DamageSchool.Physical; school < DamageSchool.Count; ++school)
          owner.ModSpellHitChance(school, -EffectValue);
      }
      else
        owner.ModSpellHitChance(m_spellEffect.MiscBitSet, -EffectValue);
    }
  }
}