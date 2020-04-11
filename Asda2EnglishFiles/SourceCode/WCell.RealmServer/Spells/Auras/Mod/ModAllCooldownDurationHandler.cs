namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>Only in: Noxious Breath (Id: 24818)</summary>
    public class ModAllCooldownDurationHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            this.Owner.IntMods[33] += this.SpellEffect.MiscValue;
        }

        protected override void Remove(bool cancelled)
        {
            this.Owner.IntMods[33] -= this.SpellEffect.MiscValue;
        }

        /// <summary>
        /// If the amount of duration to be applied is negative, we have a positive effect
        /// (because we decrease cooldown)
        /// </summary>
        public override bool IsPositive
        {
            get { return this.EffectValue <= 0; }
        }
    }
}