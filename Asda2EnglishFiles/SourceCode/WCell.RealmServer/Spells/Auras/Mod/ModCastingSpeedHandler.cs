namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class ModCastingSpeedHandler : AuraEffectHandler
    {
        private float val;

        protected override void Apply()
        {
            this.m_aura.Auras.Owner.CastSpeedFactor += this.val = (float) -this.EffectValue / 100f;
        }

        protected override void Remove(bool cancelled)
        {
            this.m_aura.Auras.Owner.CastSpeedFactor -= this.val;
        }
    }
}