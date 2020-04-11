namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class ModAttackerCritChancePercentHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            this.Owner.AttackerPhysicalCritChancePercentMod += this.EffectValue;
        }

        protected override void Remove(bool cancelled)
        {
            this.Owner.AttackerPhysicalCritChancePercentMod -= this.EffectValue;
        }
    }
}