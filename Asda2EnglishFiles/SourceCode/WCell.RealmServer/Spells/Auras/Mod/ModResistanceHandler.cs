using WCell.Constants;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    public class ModResistanceHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            for (int index = 0; index < this.m_spellEffect.MiscBitSet.Length; ++index)
                this.m_aura.Auras.Owner.AddResistanceBuff((DamageSchool) this.m_spellEffect.MiscBitSet[index],
                    this.EffectValue);
        }

        protected override void Remove(bool cancelled)
        {
            for (int index = 0; index < this.m_spellEffect.MiscBitSet.Length; ++index)
                this.m_aura.Auras.Owner.RemoveResistanceBuff((DamageSchool) this.m_spellEffect.MiscBitSet[index],
                    this.EffectValue);
        }
    }
}