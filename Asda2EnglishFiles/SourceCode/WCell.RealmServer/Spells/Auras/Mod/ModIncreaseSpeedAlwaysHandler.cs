namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class ModIncreaseSpeedAlwaysHandler : AuraEffectHandler
    {
        private float amount;

        protected override void Apply()
        {
            this.amount = (float) this.EffectValue / 100f;
            this.m_aura.Auras.Owner.SpeedFactor += this.amount;
        }

        protected override void Remove(bool cancelled)
        {
            this.m_aura.Auras.Owner.SpeedFactor -= this.amount;
        }
    }
}