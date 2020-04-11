using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class StunHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      if(SpellEffect.MiscValueB == 10)
      {
        m_aura.Auras.Owner.IncMechanicCount(SpellMechanic.Frozen, false);
        m_aura.Auras.Owner.IncMechanicCount(SpellMechanic.Invulnerable, false);
        m_aura.Auras.Owner.IncMechanicCount(SpellMechanic.Invulnerable_2, false);
      }

      m_aura.Auras.Owner.IncMechanicCount(SpellMechanic.Stunned, false);
    }

    protected override void Remove(bool cancelled)
    {
      if(SpellEffect.MiscValueB == 10)
      {
        m_aura.Auras.Owner.DecMechanicCount(SpellMechanic.Frozen, false);
        m_aura.Auras.Owner.DecMechanicCount(SpellMechanic.Invulnerable, false);
        m_aura.Auras.Owner.DecMechanicCount(SpellMechanic.Invulnerable_2, false);
      }

      m_aura.Auras.Owner.DecMechanicCount(SpellMechanic.Stunned, false);
    }

    public override bool IsPositive
    {
      get { return false; }
    }
  }
}