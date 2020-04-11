using WCell.Constants;
using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
  public class RootHandler : AuraEffectHandler
  {
    protected override void Apply()
    {
      if(m_aura.Spell.SchoolMask == DamageSchoolMask.Frost)
        m_aura.Auras.Owner.IncMechanicCount(SpellMechanic.Frozen, false);
      m_aura.Auras.Owner.IncMechanicCount(SpellMechanic.Rooted, false);
    }

    protected override void Remove(bool cancelled)
    {
      if(m_aura.Spell.SchoolMask == DamageSchoolMask.Frost)
        m_aura.Auras.Owner.DecMechanicCount(SpellMechanic.Frozen, false);
      m_aura.Auras.Owner.DecMechanicCount(SpellMechanic.Rooted, false);
    }

    public override bool IsPositive
    {
      get { return false; }
    }
  }
}