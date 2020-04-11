using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Mod
{
  public class ModDamageDoneVersusCreatureTypeHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      Character owner = Owner as Character;
      if(owner == null)
        return;
      owner.ModDmgBonusVsCreatureTypePct(m_spellEffect.MiscBitSet, EffectValue);
    }

    protected override void Remove(bool cancelled)
    {
      Character owner = Owner as Character;
      if(owner == null)
        return;
      owner.ModDmgBonusVsCreatureTypePct(m_spellEffect.MiscBitSet, -EffectValue);
    }
  }
}