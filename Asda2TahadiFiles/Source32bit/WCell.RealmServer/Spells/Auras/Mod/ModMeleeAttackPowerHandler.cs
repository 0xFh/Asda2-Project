namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class ModMeleeAttackPowerHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      if(EffectValue > 0)
        m_aura.Auras.Owner.MeleeAttackPowerModsPos += EffectValue;
      else
        m_aura.Auras.Owner.MeleeAttackPowerModsNeg += EffectValue;
    }

    protected override void Remove(bool cancelled)
    {
      if(EffectValue > 0)
        m_aura.Auras.Owner.MeleeAttackPowerModsPos -= EffectValue;
      else
        m_aura.Auras.Owner.MeleeAttackPowerModsNeg -= EffectValue;
    }
  }
}