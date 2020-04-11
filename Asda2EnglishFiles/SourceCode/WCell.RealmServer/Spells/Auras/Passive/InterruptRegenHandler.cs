namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>Interrupts Regeneration while applied</summary>
    public class InterruptRegenHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            this.m_aura.Auras.Owner.Regenerates = false;
        }

        protected override void Remove(bool cancelled)
        {
            this.m_aura.Auras.Owner.Regenerates = true;
        }
    }
}