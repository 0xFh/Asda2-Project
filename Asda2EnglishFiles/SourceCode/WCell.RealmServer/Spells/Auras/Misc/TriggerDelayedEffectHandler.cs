using NLog;
using System;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;

namespace WCell.RealmServer.Spells.Auras.Misc
{
    /// <summary>
    /// Triggers the TriggerSpell of the SpellEffect after a delay of EffectValue on the Owner
    /// </summary>
    public class TriggerDelayedEffectHandler : AuraEffectHandler
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private OneShotObjectUpdateTimer timer;

        protected internal override void CheckInitialize(SpellCast creatingCast, ObjectReference casterReference,
            Unit target, ref SpellFailedReason failReason)
        {
            if (this.m_spellEffect.TriggerSpell != null)
                return;
            failReason = SpellFailedReason.Error;
            TriggerDelayedEffectHandler.log.Warn("Tried to cast Spell \"{0}\" which has invalid TriggerSpellId {1}",
                (object) this.m_spellEffect.Spell, (object) this.m_spellEffect.TriggerSpellId);
        }

        protected override void Apply()
        {
            this.timer = this.Owner.CallDelayed(this.EffectValue, new Action<WorldObject>(this.TriggerSpell));
        }

        protected override void Remove(bool cancelled)
        {
            if (this.timer == null)
                return;
            this.Owner.RemoveUpdateAction((ObjectUpdateTimer) this.timer);
        }

        private void TriggerSpell(WorldObject owner)
        {
            this.timer = (OneShotObjectUpdateTimer) null;
            SpellCast.ValidateAndTriggerNew(this.m_spellEffect.TriggerSpell, this.m_aura.CasterReference, this.Owner,
                (WorldObject) this.Owner, this.m_aura.Controller as SpellChannel, this.m_aura.UsedItem,
                (IUnitAction) null, (SpellEffect) null);
        }
    }
}