namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class ModRangedAttackPowerHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            if (this.EffectValue > 0)
                this.m_aura.Auras.Owner.RangedAttackPowerModsPos += this.EffectValue;
            else
                this.m_aura.Auras.Owner.RangedAttackPowerModsNeg += this.EffectValue;
        }

        protected override void Remove(bool cancelled)
        {
            if (this.EffectValue > 0)
                this.m_aura.Auras.Owner.RangedAttackPowerModsPos -= this.EffectValue;
            else
                this.m_aura.Auras.Owner.RangedAttackPowerModsNeg -= this.EffectValue;
        }
    }
}