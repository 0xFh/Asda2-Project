using NLog;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Auras.Handlers
{
    /// <summary>Periodically makes the holder cast a Spell</summary>
    public class PeriodicTriggerSpellHandler : AuraEffectHandler
    {
        protected override void Apply()
        {
            this.TriggerSpell(this.m_spellEffect.TriggerSpell);
        }

        protected void TriggerSpell(Spell spell)
        {
            SpellCast spellCast = this.m_aura.SpellCast;
            if (spell == null)
                LogManager.GetCurrentClassLogger().Warn("Found invalid periodic TriggerSpell in Spell {0} ({1}) ",
                    (object) this.m_aura.Spell, (object) this.m_spellEffect.TriggerSpellId);
            else
                SpellCast.ValidateAndTriggerNew(spell, this.m_aura.CasterReference, this.Owner,
                    (WorldObject) this.Owner, this.m_aura.Controller as SpellChannel, spellCast?.TargetItem,
                    (IUnitAction) null, this.m_spellEffect);
        }

        protected override void Remove(bool cancelled)
        {
        }
    }
}