namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class PeriodicHealHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            if (this.Owner == null || this.m_aura == null || this.m_aura.CasterUnit == null)
                return;
            if (this.SpellEffect.MiscValueB == 0)
                this.Owner.Heal(this.SpellEffect.MiscValue, this.m_aura.CasterUnit, this.m_spellEffect);
            if (this.SpellEffect.MiscValueB != 1)
                return;
            this.Owner.Heal(
                (int) ((double) this.SpellEffect.MiscValue * (double) this.m_aura.CasterUnit.RandomMagicDamage / 100.0),
                this.m_aura.CasterUnit, this.m_spellEffect);
        }
    }
}