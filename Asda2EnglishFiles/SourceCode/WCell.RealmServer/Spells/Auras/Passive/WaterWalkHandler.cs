namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>Allows to walk on water (Jesus!)</summary>
    public class WaterWalkHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            ++this.m_aura.Auras.Owner.WaterWalk;
        }

        protected override void Remove(bool cancelled)
        {
            --this.m_aura.Auras.Owner.WaterWalk;
        }
    }
}