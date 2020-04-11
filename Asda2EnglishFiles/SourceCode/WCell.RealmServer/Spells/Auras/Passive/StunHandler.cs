using WCell.Constants.Spells;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class StunHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            if (this.SpellEffect.MiscValueB == 10)
            {
                this.m_aura.Auras.Owner.IncMechanicCount(SpellMechanic.Frozen, false);
                this.m_aura.Auras.Owner.IncMechanicCount(SpellMechanic.Invulnerable, false);
                this.m_aura.Auras.Owner.IncMechanicCount(SpellMechanic.Invulnerable_2, false);
            }

            this.m_aura.Auras.Owner.IncMechanicCount(SpellMechanic.Stunned, false);
        }

        protected override void Remove(bool cancelled)
        {
            if (this.SpellEffect.MiscValueB == 10)
            {
                this.m_aura.Auras.Owner.DecMechanicCount(SpellMechanic.Frozen, false);
                this.m_aura.Auras.Owner.DecMechanicCount(SpellMechanic.Invulnerable, false);
                this.m_aura.Auras.Owner.DecMechanicCount(SpellMechanic.Invulnerable_2, false);
            }

            this.m_aura.Auras.Owner.DecMechanicCount(SpellMechanic.Stunned, false);
        }

        public override bool IsPositive
        {
            get { return false; }
        }
    }
}