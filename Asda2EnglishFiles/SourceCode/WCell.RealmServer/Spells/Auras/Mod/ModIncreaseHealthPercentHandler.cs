namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class ModIncreaseHealthPercentHandler : AuraEffectHandler
    {
        private int health;

        protected override void Apply()
        {
            this.health = (this.Owner.MaxHealth * this.EffectValue + 50) / 100;
            this.Owner.Health += this.health;
            this.Owner.MaxHealthModScalar += (float) this.EffectValue / 100f;
        }

        protected override void Remove(bool cancelled)
        {
            this.Owner.Health -= this.health;
            this.Owner.MaxHealthModScalar -= (float) this.EffectValue / 100f;
        }
    }
}