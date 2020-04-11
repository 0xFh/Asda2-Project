using System;
using WCell.Constants;
using WCell.Constants.Spells;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Factions;
using WCell.RealmServer.Spells;
using WCell.Util;

namespace WCell.RealmServer.Misc
{
    /// <summary>Default implementation for IProcHandler</summary>
    public class ProcHandler : IProcHandler, IDisposable
    {
        public static ProcValidator DodgeBlockOrParryValidator = (ProcValidator) ((target, action) =>
        {
            DamageAction damageAction = action as DamageAction;
            if (damageAction == null)
                return false;
            if (damageAction.VictimState != VictimState.Dodge && damageAction.VictimState != VictimState.Parry)
                return damageAction.Blocked > 0;
            return true;
        });

        public static ProcValidator DodgeValidator = (ProcValidator) ((target, action) =>
        {
            DamageAction damageAction = action as DamageAction;
            if (damageAction == null)
                return false;
            return damageAction.VictimState == VictimState.Dodge;
        });

        public static ProcValidator StunValidator = (ProcValidator) ((target, action) =>
        {
            DamageAction damageAction = action as DamageAction;
            if (damageAction == null || damageAction.Spell == null ||
                (!damageAction.Spell.IsAura || !action.Attacker.MayAttack((IFactionMember) action.Victim)))
                return false;
            return damageAction.Spell.Attributes.HasAnyFlag(SpellAttributes.MovementImpairing);
        });

        public readonly WeakReference<Unit> CreatorRef;
        public readonly ProcHandlerTemplate Template;
        private int m_stackCount;

        public ProcHandler(Unit creator, Unit owner, ProcHandlerTemplate template)
        {
            this.CreatorRef = new WeakReference<Unit>(creator);
            this.Owner = owner;
            this.Template = template;
            this.m_stackCount = template.StackCount;
        }

        public Unit Owner { get; private set; }

        /// <summary>The amount of times that this Aura has been applied</summary>
        public int StackCount
        {
            get { return this.m_stackCount; }
            set { this.m_stackCount = value; }
        }

        public ProcTriggerFlags ProcTriggerFlags
        {
            get { return this.Template.ProcTriggerFlags; }
        }

        public ProcHitFlags ProcHitFlags
        {
            get { return this.Template.ProcHitFlags; }
        }

        public Spell ProcSpell
        {
            get { return (Spell) null; }
        }

        /// <summary>Chance to proc in %</summary>
        public uint ProcChance
        {
            get { return this.Template.ProcChance; }
        }

        public int MinProcDelay
        {
            get { return this.Template.MinProcDelay; }
        }

        public DateTime NextProcTime { get; set; }

        /// <param name="active">Whether the triggerer is the attacker/caster (true), or the victim (false)</param>
        public bool CanBeTriggeredBy(Unit triggerer, IUnitAction action, bool active)
        {
            if (this.Template.Validator != null)
                return this.Template.Validator(triggerer, action);
            return true;
        }

        public void TriggerProc(Unit triggerer, IUnitAction action)
        {
            if (!this.CreatorRef.IsAlive)
            {
                this.Dispose();
            }
            else
            {
                if (!this.Template.ProcAction((Unit) this.CreatorRef, triggerer, action) || this.m_stackCount <= 0)
                    return;
                --this.m_stackCount;
            }
        }

        public void Dispose()
        {
            this.Owner.RemoveProcHandler((IProcHandler) this);
        }
    }
}