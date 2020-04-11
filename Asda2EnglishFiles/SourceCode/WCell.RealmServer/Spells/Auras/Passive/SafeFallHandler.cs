namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>
    /// Reduces the falling damage and allows to fall longer without taking any damage at all.
    /// The amount of yards the has been falling for is reduced by this number for the damage formular
    /// </summary>
    public class SafeFallHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            this.m_aura.Auras.Owner.SafeFall += this.EffectValue;
        }

        protected override void Remove(bool cancelled)
        {
            this.m_aura.Auras.Owner.SafeFall -= this.EffectValue;
        }
    }
}