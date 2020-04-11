namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class ModMeleeAttackPowerHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            if (this.EffectValue > 0)
                this.m_aura.Auras.Owner.MeleeAttackPowerModsPos += this.EffectValue;
            else
                this.m_aura.Auras.Owner.MeleeAttackPowerModsNeg += this.EffectValue;
        }

        protected override void Remove(bool cancelled)
        {
            if (this.EffectValue > 0)
                this.m_aura.Auras.Owner.MeleeAttackPowerModsPos -= this.EffectValue;
            else
                this.m_aura.Auras.Owner.MeleeAttackPowerModsNeg -= this.EffectValue;
        }
    }
}