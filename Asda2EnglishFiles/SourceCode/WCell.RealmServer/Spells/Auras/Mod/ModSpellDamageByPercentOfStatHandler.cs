using WCell.Constants;

namespace WCell.RealmServer.Spells.Auras.Mod
{
    public class ModSpellDamageByPercentOfStatHandler : AuraEffectHandler
    {
        private int value;

        protected override void Apply()
        {
            this.value =
                (this.Owner.GetTotalStatValue((StatType) this.SpellEffect.MiscValueB) * this.EffectValue + 50) / 100;
            this.Owner.AddDamageDoneMod(this.m_spellEffect.MiscBitSet, this.value);
        }

        protected override void Remove(bool cancelled)
        {
            this.Owner.RemoveDamageDoneMod(this.m_spellEffect.MiscBitSet, this.value);
        }
    }
}