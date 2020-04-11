namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class ModMechanicResistanceHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            this.m_aura.Auras.Owner.ModMechanicResistance(this.m_aura.Spell.Mechanic, this.EffectValue);
        }

        protected override void Remove(bool cancelled)
        {
            this.m_aura.Auras.Owner.ModMechanicResistance(this.m_aura.Spell.Mechanic, -this.EffectValue);
        }
    }
}