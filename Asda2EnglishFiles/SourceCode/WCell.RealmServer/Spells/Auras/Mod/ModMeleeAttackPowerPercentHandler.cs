namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class ModMeleeAttackPowerPercentHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            this.m_aura.Auras.Owner.MeleeAttackPowerMultiplier += (float) this.EffectValue / 100f;
        }

        protected override void Remove(bool cancelled)
        {
            this.m_aura.Auras.Owner.MeleeAttackPowerMultiplier -= (float) this.EffectValue / 100f;
        }
    }
}