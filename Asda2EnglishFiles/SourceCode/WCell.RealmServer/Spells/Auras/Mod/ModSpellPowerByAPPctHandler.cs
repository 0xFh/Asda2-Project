using WCell.Constants;
using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Mod
{
    /// <summary>TODO: Reapply when AP changes</summary>
    public class ModSpellPowerByAPPctHandler : AuraEffectHandler
    {
        private int[] values;

        protected override void Apply()
        {
            Unit owner = this.Owner;
            this.values = new int[this.m_spellEffect.MiscBitSet.Length];
            for (int index = 0; index < this.m_spellEffect.MiscBitSet.Length; ++index)
            {
                uint miscBit = this.m_spellEffect.MiscBitSet[index];
                int delta = (owner.GetDamageDoneMod((DamageSchool) miscBit) * this.EffectValue + 50) / 100;
                this.values[index] = delta;
                owner.AddDamageDoneModSilently((DamageSchool) miscBit, delta);
            }
        }

        protected override void Remove(bool cancelled)
        {
            Unit owner = this.Owner;
            for (int index = 0; index < this.m_spellEffect.MiscBitSet.Length; ++index)
            {
                uint miscBit = this.m_spellEffect.MiscBitSet[index];
                owner.RemoveDamageDoneModSilently((DamageSchool) miscBit, this.values[index]);
            }
        }
    }
}