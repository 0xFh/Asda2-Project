namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>Modifies MaxHealth</summary>
    public class ModMaxHealthHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            this.m_aura.Auras.Owner.MaxHealthModFlat += this.EffectValue;
        }

        protected override void Remove(bool cancelled)
        {
            this.m_aura.Auras.Owner.MaxHealthModFlat -= this.EffectValue;
        }
    }
}