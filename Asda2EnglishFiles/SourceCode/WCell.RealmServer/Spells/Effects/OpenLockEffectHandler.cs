using NLog;
using System;
using System.Collections.Generic;
using WCell.Constants;
using WCell.Constants.GameObjects;
using WCell.Constants.Skills;
using WCell.Constants.Spells;
using WCell.Constants.Updates;
using WCell.RealmServer.Entities;
using WCell.RealmServer.Misc;
using WCell.RealmServer.Skills;
using WCell.Util;

namespace WCell.RealmServer.Spells.Effects
{
    /// <summary>Tries to open a GameObject or Item or disarm a trap</summary>
    public class OpenLockEffectHandler : SpellEffectHandler
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();
        private ILockable lockable;
        private LockOpeningMethod method;
        private Skill skill;

        public OpenLockEffectHandler(SpellCast cast, SpellEffect effect)
            : base(cast, effect)
        {
        }

        public override SpellFailedReason Initialize()
        {
            this.lockable = this.m_cast.SelectedTarget == null
                ? (ILockable) this.m_cast.TargetItem
                : (ILockable) (this.m_cast.SelectedTarget as GameObject);
            if (this.lockable == null)
                return SpellFailedReason.BadTargets;
            LockEntry lockEntry = this.lockable.Lock;
            Character casterChar = this.m_cast.CasterChar;
            if (lockEntry == null)
            {
                OpenLockEffectHandler.log.Warn("Using OpenLock on object without Lock: " + (object) this.lockable);
                return SpellFailedReason.Error;
            }

            if (casterChar == null)
            {
                OpenLockEffectHandler.log.Warn("Using OpenLock without Character: " + (object) casterChar);
                return SpellFailedReason.Error;
            }

            SpellFailedReason spellFailedReason = SpellFailedReason.Ok;
            if (!lockEntry.IsUnlocked)
            {
                LockInteractionType miscValue = (LockInteractionType) this.Effect.MiscValue;
                if (lockEntry.Keys.Length > 0 && this.m_cast.CasterItem != null)
                {
                    if (!((IEnumerable<LockKeyEntry>) lockEntry.Keys).Contains<LockKeyEntry>(
                        (Func<LockKeyEntry, bool>) (key => key.KeyId == this.m_cast.CasterItem.Template.ItemId)))
                        return SpellFailedReason.ItemNotFound;
                }
                else if (!lockEntry.Supports(miscValue))
                    return SpellFailedReason.BadTargets;

                if (miscValue != LockInteractionType.None)
                {
                    foreach (LockOpeningMethod openingMethod in lockEntry.OpeningMethods)
                    {
                        if (openingMethod.InteractionType == miscValue)
                        {
                            if (openingMethod.RequiredSkill != SkillId.None)
                            {
                                this.skill = casterChar.Skills[openingMethod.RequiredSkill];
                                if (this.skill == null || this.skill.ActualValue < openingMethod.RequiredSkillValue)
                                    spellFailedReason = SpellFailedReason.MinSkill;
                            }

                            this.method = openingMethod;
                            break;
                        }
                    }

                    if (this.method == null)
                        spellFailedReason = SpellFailedReason.BadTargets;
                }
            }

            if (spellFailedReason != SpellFailedReason.Ok && this.lockable is GameObject &&
                ((GameObject) this.lockable).Entry.IsConsumable)
                ((GameObject) this.lockable).State = GameObjectState.Enabled;
            return spellFailedReason;
        }

        public override void Apply()
        {
            if (this.skill != null)
            {
                uint requiredSkillValue = this.method.RequiredSkillValue;
                uint diff = this.skill.ActualValue - (requiredSkillValue == 1U ? 0U : requiredSkillValue);
                if (!this.CheckSuccess(diff))
                {
                    this.m_cast.Cancel(SpellFailedReason.TryAgain);
                    return;
                }

                this.skill.CurrentValue = (ushort) Math.Min(this.skill.ActualValue + (uint) (ushort) this.Gain(diff),
                    (uint) this.skill.MaxValue);
            }

            Character chr = this.m_cast.CasterChar;
            chr.AddMessage((Action) (() =>
            {
                if (this.lockable is ObjectBase && !((ObjectBase) this.lockable).IsInWorld)
                    return;
                LockEntry.Handle(chr, this.lockable,
                    this.method != null ? this.method.InteractionType : LockInteractionType.None);
            }));
        }

        protected override void Apply(WorldObject target, ref DamageAction[] actions)
        {
        }

        public override ObjectTypes CasterType
        {
            get { return ObjectTypes.Player; }
        }

        public override bool HasOwnTargets
        {
            get { return false; }
        }

        public int Gain(uint diff)
        {
            if (diff >= SkillAbility.GreyDiff || Utility.Random() % 1000 >= (diff < SkillAbility.GreenDiff
                    ? (diff < SkillAbility.YellowDiff ? SkillAbility.GainChanceOrange : SkillAbility.GainChanceYellow)
                    : SkillAbility.GainChanceGreen))
                return 0;
            return SkillAbility.GainAmount;
        }

        public bool CheckSuccess(uint diff)
        {
            int num;
            if (diff >= SkillAbility.GreyDiff)
                num = SkillAbility.SuccessChanceGrey;
            else if (diff >= SkillAbility.GreenDiff)
                num = SkillAbility.SuccessChanceGreen;
            else if (diff >= SkillAbility.YellowDiff)
            {
                num = SkillAbility.SuccessChanceYellow;
            }
            else
            {
                if (diff < 0U)
                    return false;
                num = SkillAbility.SuccessChanceOrange;
            }

            return Utility.Random() % 1000 < num;
        }
    }
}