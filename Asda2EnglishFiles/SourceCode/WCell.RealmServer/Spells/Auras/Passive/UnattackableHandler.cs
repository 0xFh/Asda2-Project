namespace WCell.RealmServer.Spells.Auras.Passive
{
    public class UnattackableHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            ++this.Owner.Invulnerable;
        }

        protected override void Remove(bool cancelled)
        {
            --this.Owner.Invulnerable;
        }
    }
}