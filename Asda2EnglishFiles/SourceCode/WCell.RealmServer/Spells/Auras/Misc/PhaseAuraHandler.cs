namespace WCell.RealmServer.Spells.Auras.Misc
{
    public class PhaseAuraHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            this.m_aura.Auras.Owner.Phase = (uint) this.m_spellEffect.MiscValue;
        }

        protected override void Remove(bool cancelled)
        {
            this.m_aura.Auras.Owner.Phase = 1U;
        }
    }
}