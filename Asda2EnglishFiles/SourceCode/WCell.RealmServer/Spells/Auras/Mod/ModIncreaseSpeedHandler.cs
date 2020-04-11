namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>
    /// Increases (or decreases) overall speed
    /// TODO: If ShapeshiftMask is set, it only applies to the given form(s)
    /// </summary>
    public class ModIncreaseSpeedHandler : AuraEffectHandler
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