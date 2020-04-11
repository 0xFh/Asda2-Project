using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Handlers;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>Periodically damages the holder</summary>
    public class PeriodicDamageHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            Unit owner = this.Owner;
            if (!owner.IsAlive)
                return;
            float num = (float) ((double) this.SpellEffect.MiscValue * (this.m_aura.CasterUnit != null
                                     ? (this.m_aura.CasterUnit.Class == ClassId.AtackMage ||
                                        this.m_aura.CasterUnit.Class == ClassId.SupportMage ||
                                        this.m_aura.CasterUnit.Class == ClassId.HealMage
                                         ? (double) this.m_aura.CasterUnit.RandomMagicDamage
                                         : (double) this.m_aura.CasterUnit.RandomDamage)
                                     : 666.0) / 100.0 * 3.0);
            if (this.m_aura.Spell.Mechanic == SpellMechanic.Bleeding)
            {
                int bleedBonusPercent = this.m_aura.Auras.GetBleedBonusPercent();
                num += (float) (((double) num * (double) bleedBonusPercent + 50.0) / 100.0);
                this.m_aura.Owner.IncMechanicCount(SpellMechanic.Bleeding, false);
            }

            DamageAction damageAction = owner.DealSpellDamage(this.m_aura.CasterUnit, this.m_spellEffect, (int) num,
                true, true, false, false);
            if (damageAction == null)
                return;
            Asda2SpellHandler.SendMonstrTakesDamageSecondaryResponse(this.m_aura.CasterUnit as Character,
                this.Owner as Character, this.Owner as NPC, damageAction.ActualDamage);
            damageAction.OnFinished();
        }

        protected override void Remove(bool cancelled)
        {
            if (this.m_aura.Spell.Mechanic != SpellMechanic.Bleeding)
                return;
            this.m_aura.Owner.DecMechanicCount(SpellMechanic.Bleeding, false);
        }
    }
}