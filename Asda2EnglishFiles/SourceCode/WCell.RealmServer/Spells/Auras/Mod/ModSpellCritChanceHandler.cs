using WCell.RealmServer.Entities;
using WCell.Util;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>Mods Spell crit chance in %</summary>
    public class ModSpellCritChanceHandler : AuraEffectHandler
    {
        private static uint[] AllDamageSchoolSet = Utility.GetSetIndices((uint) sbyte.MaxValue);

        protected override void Apply()
        {
            Character owner = this.Owner as Character;
            if (owner == null)
                return;
            if (this.m_spellEffect.MiscValue == 0)
                owner.ModCritMod(ModSpellCritChanceHandler.AllDamageSchoolSet, this.EffectValue);
            else
                owner.ModCritMod(this.m_spellEffect.MiscBitSet, this.EffectValue);
        }

        protected override void Remove(bool cancelled)
        {
            Character owner = this.Owner as Character;
            if (owner == null)
                return;
            if (this.m_spellEffect.MiscValue == 0)
                owner.ModCritMod(ModSpellCritChanceHandler.AllDamageSchoolSet, -this.EffectValue);
            else
                owner.ModCritMod(this.m_spellEffect.MiscBitSet, -this.EffectValue);
        }
    }
}