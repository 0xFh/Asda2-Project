namespace WCell.RealmServer.Spells.Auras.Mod
{
    public class ModAOEDamagePercentHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            this.Owner.AoEDamageModifierPct += this.EffectValue;
        }

        protected override void Remove(bool cancelled)
        {
            this.Owner.AoEDamageModifierPct -= this.EffectValue;
        }
    }
}