using WCell.RealmServer.Entities;

namespace WCell.RealmServer.Spells.Auras.Mod
{
    public class ModDamageDoneVersusCreatureTypeHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            Character owner = this.Owner as Character;
            if (owner == null)
                return;
            owner.ModDmgBonusVsCreatureTypePct(this.m_spellEffect.MiscBitSet, this.EffectValue);
        }

        protected override void Remove(bool cancelled)
        {
            Character owner = this.Owner as Character;
            if (owner == null)
                return;
            owner.ModDmgBonusVsCreatureTypePct(this.m_spellEffect.MiscBitSet, -this.EffectValue);
        }
    }
}