namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>Allows to hover</summary>
    public class HoverHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            ++this.m_aura.Auras.Owner.Hovering;
        }

        protected override void Remove(bool cancelled)
        {
            --this.m_aura.Auras.Owner.Hovering;
        }
    }
}