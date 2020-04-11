using WCell.Constants;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>Same as ModResistance?</summary>
    public class ModResistanceExclusiveHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            foreach (DamageSchool miscBit in this.m_spellEffect.MiscBitSet)
                this.m_aura.Auras.Owner.AddResistanceBuff(miscBit, this.EffectValue);
        }

        protected override void Remove(bool cancelled)
        {
            foreach (DamageSchool miscBit in this.m_spellEffect.MiscBitSet)
                this.m_aura.Auras.Owner.RemoveResistanceBuff(miscBit, this.EffectValue);
        }
    }
}