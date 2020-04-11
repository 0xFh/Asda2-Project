namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class ModIncreaseHealthHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            this.Owner.Health += this.EffectValue;
            this.Owner.MaxHealthModFlat += this.EffectValue;
        }

        protected override void Remove(bool cancelled)
        {
            this.Owner.MaxHealthModFlat -= this.EffectValue;
        }
    }
}