namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class ModDecreaseSpeedHandler : AuraEffectHandler
    {
        public float Value;

        protected override void Apply()
        {
            this.m_aura.Auras.Owner.SpeedFactor += this.Value = (float) this.EffectValue / 100f;
        }

        protected override void Remove(bool cancelled)
        {
            this.m_aura.Auras.Owner.SpeedFactor -= this.Value;
        }
    }
}