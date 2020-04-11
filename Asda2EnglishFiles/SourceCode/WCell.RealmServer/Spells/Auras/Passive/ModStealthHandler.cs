namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>
    /// Activates Stealth (renders caster invisible for others and makes him/her sneaky)
    /// </summary>
    public class ModStealthHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            this.m_aura.Auras.Owner.Stealthed += this.EffectValue;
        }

        protected override void Remove(bool cancelled)
        {
            this.m_aura.Auras.Owner.Stealthed -= this.EffectValue;
        }
    }
}