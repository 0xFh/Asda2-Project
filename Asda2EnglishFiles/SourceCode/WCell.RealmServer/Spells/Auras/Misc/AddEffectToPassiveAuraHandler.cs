namespace WCell.RealmServer.Spells.Auras.Misc
{
    /// <summary>Adds an extra effect to a passive Aura</summary>
    public class AddEffectToPassiveAuraHandler : AuraEffectHandler
    {
        public SpellEffect ExtraEffect { get; set; }

        protected override void Apply()
        {
            PlayerAuraCollection auras = this.m_aura.Auras as PlayerAuraCollection;
        }

        protected override void Remove(bool cancelled)
        {
        }
    }
}