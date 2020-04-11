namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>
    /// Decreases cost for spells of certain schools (percent)
    /// </summary>
    public class ModPowerCostHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            this.m_aura.Auras.Owner.ModPowerCostPct(this.m_spellEffect.MiscBitSet, this.EffectValue);
        }

        protected override void Remove(bool cancelled)
        {
            this.m_aura.Auras.Owner.ModPowerCostPct(this.m_spellEffect.MiscBitSet, -this.EffectValue);
        }

        public override bool IsPositive
        {
            get { return this.EffectValue <= 0; }
        }
    }
}