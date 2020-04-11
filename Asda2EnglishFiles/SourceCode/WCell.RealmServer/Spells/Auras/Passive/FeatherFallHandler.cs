namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class FeatherFallHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            ++this.m_aura.Auras.Owner.FeatherFalling;
        }

        protected override void Remove(bool cancelled)
        {
            --this.m_aura.Auras.Owner.FeatherFalling;
        }
    }
}