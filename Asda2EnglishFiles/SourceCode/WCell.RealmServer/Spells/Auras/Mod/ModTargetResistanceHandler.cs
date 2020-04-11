namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>Makes one ignore target's resistances</summary>
    public class ModTargetResistanceHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            this.Owner.ModTargetResistanceMod(this.EffectValue, this.m_spellEffect.MiscBitSet);
        }

        protected override void Remove(bool cancelled)
        {
            this.Owner.ModTargetResistanceMod(-this.EffectValue, this.m_spellEffect.MiscBitSet);
        }
    }
}