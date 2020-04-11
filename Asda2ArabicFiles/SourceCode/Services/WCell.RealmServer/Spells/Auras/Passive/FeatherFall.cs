namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class FeatherFallHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            m_aura.Auras.Owner.FeatherFalling++;
        }

        protected override void Remove(bool cancelled)
        {
            m_aura.Auras.Owner.FeatherFalling--;
        }
    }
}