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
      lockable = m_cast.SelectedTarget == null
        ? m_cast.TargetItem
        : (ILockable) (m_cast.SelectedTarget as GameObject);
      if(lockable == null)
        return SpellFailedReason.BadTargets;
      LockEntry lockEntry = lockable.Lock;
      Character casterChar = m_cast.CasterChar;
      if(lockEntry == null)
      {
        log.Warn("Using OpenLock on object without Lock: " + lockable);
        return SpellFailedReason.Error;
      }

      if(casterChar == null)
      {
        log.Warn("Using OpenLock without Character: " + casterChar);
        return SpellFailedReason.Error;
      }

      SpellFailedReason spellFailedReason = SpellFailedReason.Ok;
      if(!lockEntry.IsUnlocked)
      {
        LockInteractionType miscValue = (LockInteractionType) Effect.MiscValue;
        if(lockEntry.Keys.Length > 0 && m_cast.CasterItem != null)
        {
          if(!lockEntry.Keys.Contains(
            key => key.KeyId == m_cast.CasterItem.Template.ItemId))
            return SpellFailedReason.ItemNotFound;
        }
        else if(!lockEntry.Supports(miscValue))
          return SpellFailedReason.BadTargets;

        if(miscValue != LockInteractionType.None)
        {
          foreach(LockOpeningMethod openingMethod in lockEntry.OpeningMethods)
          {
            if(openingMethod.InteractionType == miscValue)
            {
              if(openingMethod.RequiredSkill != SkillId.None)
              {
                skill = casterChar.Skills[openingMethod.RequiredSkill];
                if(skill == null || skill.ActualValue < openingMethod.RequiredSkillValue)
                  spellFailedReason = SpellFailedReason.MinSkill;
              }

              method = openingMethod;
              break;
            }
          }

          if(method == null)
            spellFailedReason = SpellFailedReason.BadTargets;
        }
      }

      if(spellFailedReason != SpellFailedReason.Ok && lockable is GameObject &&
         ((GameObject) lockable).Entry.IsConsumable)
        ((GameObject) lockable).State = GameObjectState.Enabled;
      return spellFailedReason;
    }

    public override void Apply()
    {
      if(skill != null)
      {
        uint requiredSkillValue = method.RequiredSkillValue;
        uint diff = skill.ActualValue - (requiredSkillValue == 1U ? 0U : requiredSkillValue);
        if(!CheckSuccess(diff))
        {
          m_cast.Cancel(SpellFailedReason.TryAgain);
          return;
        }

        skill.CurrentValue = (ushort) Math.Min(skill.ActualValue + (ushort) Gain(diff),
          skill.MaxValue);
      }

      Character chr = m_cast.CasterChar;
      chr.AddMessage(() =>
      {
        if(lockable is ObjectBase && !((ObjectBase) lockable).IsInWorld)
          return;
        LockEntry.Handle(chr, lockable,
          method != null ? method.InteractionType : LockInteractionType.None);
      });
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
      if(diff >= SkillAbility.GreyDiff || Utility.Random() % 1000 >= (diff < SkillAbility.GreenDiff
           ? (diff < SkillAbility.YellowDiff ? SkillAbility.GainChanceOrange : SkillAbility.GainChanceYellow)
           : SkillAbility.GainChanceGreen))
        return 0;
      return SkillAbility.GainAmount;
    }

    public bool CheckSuccess(uint diff)
    {
      int num;
      if(diff >= SkillAbility.GreyDiff)
        num = SkillAbility.SuccessChanceGrey;
      else if(diff >= SkillAbility.GreenDiff)
        num = SkillAbility.SuccessChanceGreen;
      else if(diff >= SkillAbility.YellowDiff)
      {
        num = SkillAbility.SuccessChanceYellow;
      }
      else
      {
        if(diff < 0U)
          return false;
        num = SkillAbility.SuccessChanceOrange;
      }

      return Utility.Random() % 1000 < num;
    }
  }
}