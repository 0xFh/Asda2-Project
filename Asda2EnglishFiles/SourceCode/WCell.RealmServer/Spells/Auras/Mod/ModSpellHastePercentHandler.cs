namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>Modidifies casting speed</summary>
    public class ModSpellHastePercentHandler : AuraEffectHandler
    {
        private float val;

        protected override void Apply()
        {
            this.m_aura.Auras.Owner.CastSpeedFactor += this.val = (float) this.EffectValue / 100f;
        }

        protected override void Remove(bool cancelled)
        {
            this.m_aura.Auras.Owner.CastSpeedFactor -= this.val;
        }
    }
}