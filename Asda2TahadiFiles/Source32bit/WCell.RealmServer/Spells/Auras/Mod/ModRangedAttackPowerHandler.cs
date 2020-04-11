namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class ModRangedAttackPowerHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      if(EffectValue > 0)
        m_aura.Auras.Owner.RangedAttackPowerModsPos += EffectValue;
      else
        m_aura.Auras.Owner.RangedAttackPowerModsNeg += EffectValue;
    }

    protected override void Remove(bool cancelled)
    {
      if(EffectValue > 0)
        m_aura.Auras.Owner.RangedAttackPowerModsPos -= EffectValue;
      else
        m_aura.Auras.Owner.RangedAttackPowerModsNeg -= EffectValue;
    }
  }
}