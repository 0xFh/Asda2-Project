namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>Flying mount speed effect.</summary>
    public class ModSpeedMountedFlightHandler : AuraEffectHandler
    {
        private float val;

        protected override void Apply()
        {
            this.m_aura.Auras.Owner.FlightSpeedFactor += this.val = (float) this.EffectValue / 100f;
        }

        protected override void Remove(bool cancelled)
        {
            this.m_aura.Auras.Owner.FlightSpeedFactor -= this.val;
        }
    }
}