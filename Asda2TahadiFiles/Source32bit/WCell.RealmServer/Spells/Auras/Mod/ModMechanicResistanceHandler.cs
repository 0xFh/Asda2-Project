namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class ModMechanicResistanceHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      m_aura.Auras.Owner.ModMechanicResistance(m_aura.Spell.Mechanic, EffectValue);
    }

    protected override void Remove(bool cancelled)
    {
      m_aura.Auras.Owner.ModMechanicResistance(m_aura.Spell.Mechanic, -EffectValue);
    }
  }
}