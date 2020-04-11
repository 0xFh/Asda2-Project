using WCell.Constants;
using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class RootHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            if (this.m_aura.Spell.SchoolMask == DamageSchoolMask.Frost)
                this.m_aura.Auras.Owner.IncMechanicCount(SpellMechanic.Frozen, false);
            this.m_aura.Auras.Owner.IncMechanicCount(SpellMechanic.Rooted, false);
        }

        protected override void Remove(bool cancelled)
        {
            if (this.m_aura.Spell.SchoolMask == DamageSchoolMask.Frost)
                this.m_aura.Auras.Owner.DecMechanicCount(SpellMechanic.Frozen, false);
            this.m_aura.Auras.Owner.DecMechanicCount(SpellMechanic.Rooted, false);
        }

        public override bool IsPositive
        {
            get { return false; }
        }
    }
}